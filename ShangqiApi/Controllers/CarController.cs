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
using Shangqi.Logic.Configuration;
using Shangqi.Logic.Data;
using Shangqi.Logic.Model.Outbound;

namespace ShangqiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly CarDbService _carService;
        public CarController(CarDbService carService)
        {
            _carService = carService;
        }
        // GET api/car/list
        [HttpGet("list")]
        public IList<RegisteredCarData> Get()
        {
            var l = _carService.List();
          
            

            return l;
        }

        [HttpPost("recording/start")]
        //STEP 1 START RECORDING CAR COORDINATES
        public async Task<ActionResult> StartCoordinateRecording(string carId, string recordId)
        {

            if (recordId == null)
            {
                //get car info from db
                var car = _carService.GetCar(carId);

                //update robot status in cache
                var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.IpAddress}");

                if (carInCache == null) return BadRequest();
                carInCache.RobotStatus = Const.ROBOT_STATUS_REMOTE;
                await RedisHelper.Instance.SetCache($"car_{car.IpAddress}", carInCache);

                //add to db
                _carService.AddNewCarRecord(carId);


                //send to robot to start recording
                var msg = new msg_control_cmd()
                {
                    cmd = "1",
                    cmd_slave = "0",
                    route_id = "x"
                };


                var outbound = new OutboundModel()
                {
                    IpAddress = carInCache.CarIp,
                    Data = new List<object> { msg }
                };
                await RedisHelper.Instance.SetCache<OutboundModel>("command", outbound);
                return Ok();
            }
            else
            {
                return BadRequest();
            }


        }
        [HttpPost("recording/end")]
        //STEP 2 END RECORDING AND SAVE DATA FROM CACHE
        public async Task<ActionResult> EndCarRecord(string carId)
        {
            //get car info from db
            var car = _carService.GetCar(carId);
            //save cached coordinates to database
            var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.IpAddress}");
            _carService.EndCarRecording(carId, carInCache.CachedCoordinates);

            //update robot status in cache
            carInCache.RobotStatus = 0;
            await RedisHelper.Instance.SetCache($"car_{car.IpAddress}", carInCache);

            //send to robot to end recording
            var msg = new msg_control_cmd()
            {
                cmd = "0",
                cmd_slave = "x",
                route_id = "x"
            };


            var outbound = new OutboundModel()
            {
                IpAddress = carInCache.CarIp,
                Data = new List<object> { msg }
            };
            await RedisHelper.Instance.SetCache<OutboundModel>("command", outbound);

            return Ok();
        }

        [HttpPost("recording/export")]
        //STEP 3 EXPORT THE COORDINATES IN CSV
        public ActionResult ExportCoordinateRecord(string carId)
        {
            //var coordinates = "1,2,3,4";
            var coordinateRecord = _carService.GetCoordinateRecord(carId);
            var sb = new StringBuilder();

            foreach (var coordinate in coordinateRecord.Coordinates)
            {
                sb.AppendLine($"{coordinate.Latitude}, {coordinate.Longitude}");
            }


            var result = new FileContentResult(System.Text.Encoding.ASCII.GetBytes(sb.ToString()), "application/octet-stream");
            result.FileDownloadName = "my-csv-file.csv";
            return result;
        }
        ////STEP 4 IMPORT THE COORDINATES FROM CSV AND GENERATE ROUTE 
        //public async Task ImportCoordinatesCreateRoute(string carId, bool isReturn = false )
        //{
        //    //get the coordinates from uploaded CSV
        //    var selected_Coordinates = new Coordinate[] { };

        //    //create a new route in db
        //    _carService.AddRoute(selected_Coordinates);



        //    //update the end position in cache
        //    var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>("carId_{ip}");
        //    cached.EndPoint = new Coordinate("","");



        //}

        ////STEP 5 ADD TRIGGER POINT FOR THE ROUTE
        //public async Task AddTriggerCoordinateForRoute(string routeId, string longitude, string latitude)
        //{
        //    //update the route in db
        //    _carService.UpdateRouteWithTriggerPoint(routeId, longitude, latitude);
        //    //update the route in cache
        //    var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>("carId_{ip}");
        //    cached.TriggerPoint = new Coordinate(longitude, latitude);

        //}

        ////STEP N.. TRIGGER RETURN ROUTE
        //public async Task xx()
        //{


        //    //go to auto pilot mode and send the coordinates to client
        //    var outbound = new OutboundModel();
        //    RedisHelper.Instance.SetCache("command", outbound).Wait();
        //    //update database with the cached coordinates 
        //    _carService.UpdateRouteWithStatus(4);
        //    //update cache status to return in progress
        //    var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>("carId_{ip}");
        //    cached.RouteStatus = 4;
        //    cached.CachedCoordinates = new List<Coordinate>();

        //}

    }
}