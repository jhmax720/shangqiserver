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

        public CoordinateModel[] ImportedCarTrack { get; set; }
        public CoordinateModel[] CarTrack { get; set; }
        public CoordinateModel[] ImportedCarTrackReturn { get; set; }
        public CoordinateModel[] CarTrackReturn { get; set; }
        public int RouteStatus { get; set; }

        [BsonElement("longitude")]
        public string TriggerLongitude { get; set; }

        [BsonElement("latitude")]
        public string TriggerLatitude { get; set; }
    }
}
