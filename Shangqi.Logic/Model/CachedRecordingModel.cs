using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{
    public class CachedRecordingModel
    {
        public string CarIp { get; set; }
        public string CarId { get; set; }
        public bool IsReturn { get; set; }


        public int CarStatus { get; set; }

        public CoordinateModel TriggerPoint { get; set; }
        public CoordinateModel CurrentPosition { get; set; }
        public IList<CoordinateModel> CachedCoordinates { get; set; }

        public CachedRecordingModel()
        {
            CachedCoordinates = new List<CoordinateModel>();
        }
    }
}
