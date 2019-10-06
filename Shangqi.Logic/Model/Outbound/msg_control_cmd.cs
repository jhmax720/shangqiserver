using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model.Outbound
{
    public class msg_control_cmd
    {
        public int msg_count { get; set; }
        public string cmd { get; set; }
        public string cmd_slave { get; set; }
        public string route_id { get; set; }
        public double speed { get; set; }
        public string check { get; set; }
    }
}
