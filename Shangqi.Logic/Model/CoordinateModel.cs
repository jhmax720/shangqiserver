using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Model
{
    public class CoordinateModel
    {
        public CoordinateModel(string longitude, string latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
        public string Longitude { get; set; }

        public string Latitude { get; set; }
    }
}
