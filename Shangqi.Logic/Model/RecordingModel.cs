using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{
    public class RecordingModel
    {
        public string CarIp { get; set; }
        public bool IsReturn { get; set; }

        public IList<CoordinateModel> Coordinates { get; set; }

        public RecordingModel()
        {
            Coordinates = new List<CoordinateModel>();
        }
    }
}
