using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model.Outbound
{
    [Serializable()]
    public class msg_map_route
    {
        public int msg_count { get; set; }
        public int msg_total { get; set; }
        public string longitude { get; set; }

        public string latitude { get; set; }

        public string route_id { get; set; }
        public string check { get; set; }
    }
}
