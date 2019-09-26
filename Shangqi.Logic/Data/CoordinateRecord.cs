using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Data
{


    public class CoordinateRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CarId { get; set; }
        

        public Coordinate[] Coordinates { get; set; }
    }
}
