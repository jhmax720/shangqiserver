using Shangqi.Logic.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{

    public enum RouteStatusValue
    {
        default_value=0,
        csv_imported =1,



    }
    [Serializable()]

    public class CachedRecordingModel
    {
        public string CarIp { get; set; }
        public string CarId { get; set; }
        public bool IsReturn { get; set; }

        //0 default
        //1 csv imported, ready to go
        //2 car started route
        //3 route completed
        //4 csv (return imported), ready to go
        //5 return route triggered manually
        //6 return route completed
        public int RouteStatus { get; set; }
        public int RobotStatus { get; set; }

        public double Battery { get; set; }

        public Coordinate TriggerPoint { get; set; }

        public Coordinate EndPoint { get; set; }
        public Coordinate CurrentPosition { get; set; }
        public IList<Coordinate> CachedCoordinates { get; set; }
        public IList<Coordinate> ImpotedCoordinates { get; set; }
        public bool IsMainVehicle {
            get {
                return this.CarIp == Const.MAIN_CAR_IP;
            }
        }

        public bool IsDirty { get; set; }
        public CachedRecordingModel()
        {
            CachedCoordinates = new List<Coordinate>();
        }
    }
}
