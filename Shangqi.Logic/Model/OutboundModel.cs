using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{
    [Serializable()]
    public class OutboundModel
    {
        public int CarName { get; set; }

        public string Type { get; set; }

        public IList<object> Data { get; set; }

        public string Ip { get; set; }
        
    }
}
