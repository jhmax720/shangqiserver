using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shangqi.Logic.Model;

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
        public int RouteStatus { get; set; }

        [BsonElement("longitude")]
        public double TriggerLongitude { get; set; }

        [BsonElement("latitude")]
        public double TriggerLatitude { get; set; }

        public DateTime Created { get; set; }
    }
}
