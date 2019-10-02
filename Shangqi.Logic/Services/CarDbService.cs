using System;
using System.Collections.Generic;
using System.Text;
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


        public CarDbService(DBSettings settings)
        {
            Setup(settings.ConnectionString, settings.DatabaseName, settings.CarCollectionName);
        }

        private void Setup(string cs, string db, string car)
        {
            var client = new MongoClient(cs);
            var _db = client.GetDatabase(db);
            _cars = _db.GetCollection<RegisteredCarData>(car);
        }

        public CarDbService(string cs, string db, string car)
        {
            Setup(cs, db, car);
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
    }
}