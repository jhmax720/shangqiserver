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

        private readonly IMongoCollection<RegisteredCarData> _cars;

        public CarDbService(DBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var _db = client.GetDatabase(settings.DatabaseName);
            _cars = _db.GetCollection<RegisteredCarData>(settings.CarCollectionName);

        }

        public IList<RegisteredCarData> List()
        {

            var list = _cars.Find(car => !car.IsMainCar).ToList();
            return list;
        }


    }
}