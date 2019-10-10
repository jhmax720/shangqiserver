using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{
    [Serializable()]
    public class OutboundModel
    {
        public string IpAddress { get; set; }

        public string Type { get; set; }

        public IList<object> Data { get; set; }
        
    }
}
