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
using Shangqi.Logic.Data;

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
        [HttpGet("list")]
        public List<CachedRecordingModel> Get()
        {
            var l = _service.List();
          
            

            return null;
        }

        [HttpPost("command")]
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

        //STEP 1 START RECORDING CAR COORDINATES
        public async Task StartCoordinateRecording(string carId, string recordId)
        {

            if (recordId == null)
            {
                //add to db
                 _service.AddNewCarRecord();

                //setup cache layer
                await RedisHelper.Instance.SetCache<CachedRecordingModel>("recording_{carId}", new CachedRecordingModel());

                //send to client to start recording
                await RedisHelper.Instance.SetCache<OutboundModel>("command", new OutboundModel());
            }
            else
            {
                
            }
            

        }

        //STEP 2 END RECORDING AND SAVE DATA FROM CACHE
        public async Task EndCarRecord(string recordId, string carId)
        {
            //save cached coordinates to database
            var inCached = RedisHelper.Instance.TryGetFromCarList(carId);

            _service.EndCarRecord(recordId);

            //send to client to end recording
            await RedisHelper.Instance.SetCache<OutboundModel>("command", new OutboundModel());

            //update cache status = 0;
        }


        //STEP 3 EXPORT THE COORDINATES IN CSV
        public async void ExportCoordinateRecord(string recording)
        {

        }
        //STEP 4 IMPORT THE COORDINATES FROM CSV AND GENERATE ROUTE 
        public async Task ImportCoordinatesCreateRoute(string carId, bool isReturn = false )
        {
            //get the coordinates from uploaded CSV
            var selected_Coordinates = new Coordinate[] { };

            //create a new route in db
            _service.AddRoute(selected_Coordinates);

            //update the end position in cache
            var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>("carId_{ip}");
            cached.EndPoint = new CoordinateModel("","");

        }

        //STEP 5 ADD TRIGGER POINT FOR THE ROUTE
        public async Task AddTriggerCoordinateForRoute(string routeId, string longitude, string latitude)
        {
            //update the route in db
            _service.UpdateRouteWithTriggerPoint(routeId, longitude, latitude);
            //update the route in cache
            var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>("carId_{ip}");
            cached.TriggerPoint = new CoordinateModel(longitude, latitude);

        }


    }
}