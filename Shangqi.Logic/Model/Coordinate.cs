using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Shangqi.Logic.Model
{
    [Serializable()]
    public class Coordinate
    {
        public Coordinate(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
