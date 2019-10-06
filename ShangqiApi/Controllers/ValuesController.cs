//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Distributed;
//using Shangqi.Logic;
//using Shangqi.Logic.Model;

//namespace ShangqiApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ValuesController : ControllerBase
//    {
//        private IDistributedCache _cache;
//        public ValuesController(IDistributedCache cache)
//        {
//            _cache = cache;
//        }

//        [HttpPost("/command")]
//        public async Task Command()
//        {

//            //Send redis a message
//            var model = new HeartBeatModel()
//            {
//                tcpip = "123123",
//                type = "max"
//            };


//            var raw = RedisHelper.Instance.SetCache<HeartBeatModel>("command", model);




//        }

//        // GET api/values
//        [HttpGet]
//        public ActionResult<IEnumerable<string>> Get()
//        {
//            //var model = new HeartBeatModel()
//            //{
//            //    tcpip = "123123",
//            //    type = "max"
//            //};


//            RedisHelper.Instance.SetNormalCache("command", Encoding.UTF8.GetBytes("fuck you thank you"));


//            return new string[] { "value3", "value2" };
//        }

//        // GET api/values/5
//        [HttpGet("{key}")]
//        public ActionResult<string> Get(string key)
//        {
//            string value = "value";
//            var result = _cache.Get(key);
//            if (result != null)
//            {
//                value = System.Text.Encoding.Default.GetString(result);
//            }

//            var result2 = RedisHelper.Instance.GetNormalItem("command");
//            var result3 =System.Text.Encoding.Default.GetString(result2);
//            return value;
//        }

//        // POST api/values
//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//        }

//        // PUT api/values/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE api/values/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }


        

//    }
//}
