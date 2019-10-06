using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Shangqi.Logic.Model
{
    [DataContract]
    public class HeartBeatModel
    {
        // {"type": "heart", “tcp/ip” :192.168.0.100:5050, "msg_count": 42, "robot_status":0, "error" : 10, "rtk_qual": 14, "route_id": 1, "route_status" :3, "longitude": 2231.30538, "latitude": 11353.76058, "battery":90, "check": 8}
        [JsonProperty(PropertyName = "type")]
        public string type { get; set; }

        [JsonProperty(PropertyName = "tcp/ip")]
        public string tcpip { get; set; }

        //  robot_status 含义
        //0 待机模式
        //1 遥控器模式
        //2 ⾃动驾驶模式
        //3 远程控制模式
        //4 路径传输模式
        [JsonProperty(PropertyName = "robot_status")]
        public int Status { get; set; }

        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}
