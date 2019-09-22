using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using Shangqi.Logic.Configuration;
using Shangqi.Logic.Data;

namespace Shangqi.Logic.Services
{
    public class CarDbService
    {
        //Private Constructor.

        private IMongoCollection<RegisteredCarData> _cars;


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

        }
    }
}