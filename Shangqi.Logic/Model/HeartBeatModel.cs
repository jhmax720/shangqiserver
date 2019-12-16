using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Shangqi.Logic.Model
{
    [Serializable()]
    [DataContract]
    public class HeartBeatModel
    {
        [JsonProperty(PropertyName = "msg_count")]
        public int msg_count { get; set; }

        [JsonProperty]
        public int robot_id { get; set; }

        //  robot_status 含义
        //0 待机模式
        //1 遥控器模式
        //2 ⾃动驾驶模式
        //3 远程控制模式
        //4 路径传输模式
        [JsonProperty(PropertyName = "robot_status")]
        public int robot_status { get; set; }


        [JsonProperty(PropertyName = "error")]
        public int error { get; set; }

        [JsonProperty(PropertyName = "rtk_qual")]
        public int rtk_qual { get; set; }
        [JsonProperty(PropertyName = "route_id")]
        public int route_id { get; set; }
        [JsonProperty(PropertyName = "route_status")]
        public int route_status { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double longitude { get; set; }
        [JsonProperty(PropertyName = "latitude")]
        public double latitude { get; set; }
        [JsonProperty(PropertyName = "battery")]
        public int battery { get; set; }

        [JsonProperty(PropertyName = "check")]
        public string check { get; set; }

        [JsonProperty]
        public string type
        {
            get { return "heart"; }
        }

        [JsonProperty]
        public int speed { get; set; }
    }
}
