using System;
using System.Collections.Generic;
using System.Text;

namespace Shangqi.Logic.Model
{
    public class RegisteredCarModel
    {
        public string Id { get; set; }
        public int status { get; set; }
        public string ipAddress { get; set; }
        public string longitude { get; set; }

        public string latitude { get; set; }
    }
}
