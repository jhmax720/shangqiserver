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

        public Coordinate[] ImportedCarTrack { get; set; }
        public Coordinate[] CarTrack { get; set; }
        public Coordinate[] ImportedCarTrackReturn { get; set; }
        public Coordinate[] CarTrackReturn { get; set; }


        [BsonElement("longitude")]
        public string TriggerLongitude { get; set; }

        [BsonElement("latitude")]
        public string TriggerLatitude { get; set; }
    }
}
