using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shangqi.Logic.Configuration;
using Shangqi.Logic.Data;
using Shangqi.Logic.Model;

namespace Shangqi.Logic.Services
{
    public class CarDbService
    {
        //Private Constructor.

        private IMongoCollection<RegisteredCarData> _cars;
        private IMongoCollection<CoordinateRecord> _coordinateRecordsCollection;
        private IMongoCollection<Route> _routes;

        public CarDbService(IOptions<DBSettings>  settings)
        {
            var st = settings.Value;
            Setup(st.ConnectionString, st.DatabaseName);
        }

        private void Setup(string cs, string db)
        {
            var client = new MongoClient(cs);
            var _db = client.GetDatabase(db);
            _cars = _db.GetCollection<RegisteredCarData>(Const.DB_CARS);
            _coordinateRecordsCollection = _db.GetCollection<CoordinateRecord>(Const.DB_COORDINATE_RECORD);
            _routes = _db.GetCollection<Route>(Const.DB_ROUTE);
        }

        //public CarDbService(string cs, string db)
        //{
        //    Setup(cs, db);
        //}


        public IList<RegisteredCarData> List()
        {

            var list = _cars.Find(car => car.Id !=null).ToList();
            return list;
        }

        private RegisteredCarData GetCar(string carId)
        {
            return _cars.Find(car => car.Id == carId).FirstOrDefault();
        }

        public RegisteredCarData GetCarByName(int carName)
        {
            return _cars.Find(car => car.CarName == carName).FirstOrDefault();
        }

        public void AddCarIfNotExist(CachedRecordingModel model)
        {
            var dbCar = new RegisteredCarData
            {
                Battery = model.Battery,
                CarName = model.CarName
            };
            var count = _cars.Find<RegisteredCarData>(Builders<RegisteredCarData>.Filter.Eq(r => r.CarName, model.CarName)).CountDocuments();
            if (count == 0)
            {
                _cars.InsertOne(dbCar);
            }
            //_cars.fino(Builders<RegisteredCarData>.Filter.Eq(r => r.IpAddress, model.CarIp), dbCar);
        }

        public void AddNewCarRecord(string carId)
        {
            var record = new CoordinateRecord() {
                CarId = carId,
                Coordinates = new Coordinate[] { }
            };

            IList<FilterDefinition<CoordinateRecord>> filters = new List<FilterDefinition<CoordinateRecord>>();

            //add this to db
            var count = _coordinateRecordsCollection.Find<CoordinateRecord>(
                Builders<CoordinateRecord>.Filter.Eq(r => r.CarId, carId) & 
                Builders<CoordinateRecord>.Filter.Eq(r => r.IsComplete, false))
                .CountDocuments();
            if (count == 0)
            {
                _coordinateRecordsCollection.InsertOne(record);
            }


            //_coordinateRecordsCollection.FindOneAndReplace(Builders<CoordinateRecord>.Filter.Eq(r => r.CarId, carId),record);
            
        }

        public void EndCarRecording(string carId, IList<Coordinate> coordinates)
        {
            var lcr = _coordinateRecordsCollection.Find(c => c.CarId == carId && !c.IsComplete).SortByDescending(c => c.Id)
                .FirstOrDefault();
            

            _coordinateRecordsCollection.UpdateOne(Builders<CoordinateRecord>.Filter
                .Eq(r => r.Id, lcr.Id),
                Builders<CoordinateRecord>.Update
                .Set(x => x.Coordinates, coordinates.ToArray())
                .Set(x => x.IsComplete, true));
        }

        public List<CoordinateRecord> GetCoordinateRecordsforCar(string carId)
        {
            var list = _coordinateRecordsCollection.Find(c => c.CarId == carId).SortByDescending(c => c.Id).ToList();                
            return list;
        }

        public CoordinateRecord GetCoordinateRecord(string recordId)
        {
            var lcr = _coordinateRecordsCollection.Find(c => c.Id == recordId).SortByDescending(c => c.Id)
                .FirstOrDefault();
            return lcr;
        }

        public void AddRoute(string carId, Coordinate[] importedCoordinates )
        {
            var route = new Route();
            route.ImportedCarTrack = importedCoordinates;
            route.CarId = carId;
            route.RouteStatus = 1;
            route.Created = DateTime.Now;

            _routes.InsertOne(route);

        }

        public void UpdateRouteWithCoordinates(Coordinate[] coordinates, bool isReturn = false)
        {

        }


        public void UpdateRouteWithStatus(int status)
        {

        }
        public async Task UpdateRouteWithTriggerPoint(string recordId, double longtitude, double latitude, double speed)
        {
            await _routes.FindOneAndUpdateAsync(
                                Builders<Route>.Filter.Eq(r => r.Id, recordId),
                                Builders<Route>.Update
                                    .Set(x => x.TriggerLatitude, latitude)
                                    .Set(x => x.TriggerLongitude, longtitude)
                                    .Set(x=>x.StartingSpeed, speed)
                                );
        }

        public List<Route> Routes(string carId)
        {
            var list = _routes.Find(r=>r.CarId == carId).ToList();
            return list;
        }

        public Route LatestRoute(string carId)
        {
            var route = _routes.Find(r => r.CarId == carId).SortByDescending(r => r.Created).FirstOrDefault();

            return route;
        }

        public async Task SyncRoute(CachedRecordingModel model)
        {
            if(model.RouteStatus == 2)
            {
                var result = await _routes.FindOneAndUpdateAsync(
                                Builders<Route>.Filter.Eq(r => r.Id, model.RouteId),
                                Builders<Route>.Update.Set(x => x.CarTrack, model.CachedCoordinates.ToArray()).Set(x => x.RouteStatus, model.RouteStatus)
                                );
            }
            else
            {
                var result = await _routes.FindOneAndUpdateAsync(
                                Builders<Route>.Filter.Eq(r => r.Id, model.RouteId),
                                Builders<Route>.Update.Set(x => x.CarTrackReturn, model.CachedCoordinates.ToArray()).Set(x=>x.RouteStatus, model.RouteStatus)
                                );
            }

            //await _routes.FindOneAndUpdateAsync(
            //                    Builders<Route>.Filter.Eq(r => r.CarId, model.CarId),
            //                    Builders<Route>.Update.Set(x => x.RouteStatus, model.RouteStatus).Set(x=>x.)
            //                    );
        }
    }
}