using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Data
{
    public class Route
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CarId { get; set; }

        public CarTrack ImportedCarTrack { get; set; }
        public CarTrack CarTrack { get; set; }
        public CarTrack ImportedCarTrackReturn { get; set; }
        public CarTrack CarTrackReturn { get; set; }


        [BsonElement("longitude")]
        public string Longitude { get; set; }

        [BsonElement("latitude")]
        public string Latitude { get; set; }
    }
}
