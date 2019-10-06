using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        public CarDbService(DBSettings settings)
        {
            Setup(settings.ConnectionString, settings.DatabaseName);
        }

        private void Setup(string cs, string db)
        {
            var client = new MongoClient(cs);
            var _db = client.GetDatabase(db);
            _cars = _db.GetCollection<RegisteredCarData>(Const.DB_CARS);
            _coordinateRecordsCollection = _db.GetCollection<CoordinateRecord>(Const.DB_COORDINATE_RECORD);
            _routes = _db.GetCollection<Route>(Const.DB_ROUTE);
        }

        public CarDbService(string cs, string db)
        {
            Setup(cs, db);
        }


        public IList<RegisteredCarData> List()
        {

            var list = _cars.Find(car => !car.IsMainCar).ToList();
            return list;
        }

        public void AddCar()
        {

        }

        public void AddNewCarRecord()
        {
            var record = new CoordinateRecord();
            //add this to db
            _coordinateRecordsCollection.InsertOne(record);
        }

        public void EndCarRecord(string recordId)
        {

        }

        public void AddRoute(CoordinateModel[] importedCoordinates )
        {
            var route = new Route();
            route.ImportedCarTrack = importedCoordinates;
            //todo call db.save()
        }

        public void UpdateRouteWithCoordinates(CoordinateModel[] coordinates, bool isReturn = false)
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