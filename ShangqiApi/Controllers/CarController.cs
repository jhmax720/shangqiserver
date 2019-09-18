using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shangqi.Logic.Model;
using Shangqi.Logic.Services;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.AspNetCore.Connections;
using Shangqi.Logic;

namespace ShangqiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private CarDbService _service;
        public CarController(CarDbService service)
        {
            _service = service;
        }
        // GET api/car/list
        [HttpGet("/list")]
        public List<RegisteredCarModel> Get()
        {
            var l = _service.List();
            var results = l.Select(x =>
            {
                var model = new RegisteredCarModel()
                {
                    status = x.Status,
                    Id = x.Id
                };
                return model;
            });
            

            return results.ToList();
        }

        [HttpPost("/command")]
        public async Task Command()
        {
            RedisHelper.Instance.SetNormalCache("command", Encoding.UTF8.GetBytes("fuck you thank you"));
            //Send redis a message
            //var model = new HeartBeatModel()
            //{
            //    tcpip = "123123",
            //    type = "max"
            //};


            //var raw = RedisHelper.Instance.SetCache<HeartBeatModel>("command", model);


            //using (var stream = new MemoryStream(raw))
            //{
            //    {
            //        var mf =  (ConnectionContext)new BinaryFormatter().Deserialize(stream);
            //        string msg = "hello world from max";

            //        byte[] bytes = Encoding.ASCII.GetBytes(msg);

            //        var sor = new ReadOnlyMemory<byte>(bytes);
            //        await mf.Transport.Output.WriteAsync(sor);
            //        foreach (var segment in bytes)
            //        {

            //        }

            //    }
            //}


        }



    }
}