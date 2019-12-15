using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Shangqi.Logic.Model.Outbound
{
    [Serializable()]
    public class msg_control_cmd
    {
        [JsonProperty]
        public string type
        {
            get { return "control"; }
        }
        
        public int msg_count { get; set; }
        public int cmd { get; set; }
        public int cmd_slave { get; set; }
        public int route_id { get; set; }
        public double speed { get; set; }
        public int check { get; set; }
    }
}
