//using System;
//using System.Collections.Generic;
//using System.Text;
//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;

//namespace Shangqi.Logic.Data
//{
//    public enum TrackType
//    {
//        Recording = 1,
//        RecordingReturn = 2,
//        Tracking = 3,
//        TrackingReturn = 4

//    }

//    public class CarTrack
//    {


//        [BsonId]
//        [BsonRepresentation(BsonType.ObjectId)]
//        public string Id { get; set; }

//        //public string CarId { get; set; }


//        public TrackType TrackType { get; set; }

//        public Coordinate TriggerPoint { get; set; }

//        public Coordinate[] CachedCoordinates { get; set; }
//    }
//}