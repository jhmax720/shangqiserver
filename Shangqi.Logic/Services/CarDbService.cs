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

            var list = _cars.Find(car => !car.IsMainCar).ToList();
            return list;
        }

        public RegisteredCarData GetCar(string carId)
        {
            return _cars.Find(car => car.Id == carId).FirstOrDefault();
        }

        public RegisteredCarData GetCarByIp(string carIp)
        {
            return _cars.Find(car => car.IpAddress == carIp).FirstOrDefault();
        }

        public void AddCar(CachedRecordingModel model)
        {
            var dbCar = new RegisteredCarData
            {
                Battery = model.Battery,
                IpAddress = model.CarIp
            };
            var count = _cars.Find<RegisteredCarData>(Builders<RegisteredCarData>.Filter.Eq(r => r.IpAddress, model.CarIp)).CountDocuments();
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
            var list = _coordinateRecordsCollection.Find(c => c.Id == carId).SortByDescending(c => c.Id).ToList();                
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
            _routes.InsertOne(route);

        }

        public void UpdateRouteWithCoordinates(Coordinate[] coordinates, bool isReturn = false)
        {

        }


        public void UpdateRouteWithStatus(int status)
        {

        }
        public void UpdateRouteWithTriggerPoint(string recordId, string longtitude, string latitude)
        {

        }

        public async Task SyncRoute(CachedRecordingModel model)
        {
            if(model.RouteStatus == 2)
            {
                var result = await _routes.FindOneAndUpdateAsync(
                                Builders<Route>.Filter.Eq(r => r.CarId, model.CarId),
                                Builders<Route>.Update.Set(x => x.CarTrack, model.CachedCoordinates).Set(x => x.RouteStatus, model.RouteStatus)
                                );
            }
            else
            {
                var result = await _routes.FindOneAndUpdateAsync(
                                Builders<Route>.Filter.Eq(r => r.CarId, model.CarId),
                                Builders<Route>.Update.Set(x => x.CarTrackReturn, model.CachedCoordinates).Set(x=>x.RouteStatus, model.RouteStatus)
                                );
            }

            //await _routes.FindOneAndUpdateAsync(
            //                    Builders<Route>.Filter.Eq(r => r.CarId, model.CarId),
            //                    Builders<Route>.Update.Set(x => x.RouteStatus, model.RouteStatus).Set(x=>x.)
            //                    );
        }
    }
}