using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Data
{
    public class RegisteredCarData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public bool IsMainCar { get; set; }
        public string IpAddress { get; set; }
        public double Battery { get; set; }

        //[BsonElement("Status")]
        //public int Status { get; set; }

        
    }
}
