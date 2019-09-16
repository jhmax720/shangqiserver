using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Data
{
    public class Coordinate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("longitude")]
        public string Longitude { get; set; }

        [BsonElement("latitude")]
        public string Latitude { get; set; }

    }
}
