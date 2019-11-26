using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Shangqi.Logic.Model.Outbound
{
    [Serializable()]
    public class msg_map_route
    {
        [JsonProperty]
        public string type
        {
            get { return "route"; }
        }
        public int msg_count { get; set; }
        public int msg_total { get; set; }
        public double longitude { get; set; }

        public double latitude { get; set; }

        public string route_id { get; set; }
        public string check { get; set; }
    }
}
