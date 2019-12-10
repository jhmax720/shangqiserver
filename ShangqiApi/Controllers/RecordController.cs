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
    public class RecordController : ControllerBase
    {
        private readonly CarDbService _carService;
        public RecordController(CarDbService carService)
        {
            _carService = carService;
        }
        
        [HttpGet("car/cache/list")]
        public async Task<IList<CachedRecordingModel>> Get()
        {
            //var l = _carService.List();
            var l = await RedisHelper.Instance.GetCurrentCarsInCache();
            

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
                var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.CarName}");

                if (carInCache == null) return BadRequest();
                carInCache.RobotStatus = Const.ROBOT_STATUS_REMOTE;
                await RedisHelper.Instance.SetCache($"car_{car.CarName}", carInCache);

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
                    Ip = carInCache.Ip,
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
            var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.CarName}");
            _carService.EndCarRecording(carId, carInCache.CachedCoordinates);

            //update robot status in cache
            carInCache.RobotStatus = 0;
            await RedisHelper.Instance.SetCache($"car_{car.CarName}", carInCache);

            //send to robot to end recording
            var msg = new msg_control_cmd()
            {
                cmd = "0",
                cmd_slave = "x",
                route_id = "x"
            };


            var outbound = new OutboundModel()
            {
                Ip = carInCache.Ip,
                Data = new List<object> { msg }
            };
            await RedisHelper.Instance.SetCache<OutboundModel>("command", outbound);

            return Ok();
        }



        [HttpGet("recording/list")]
        public IList<CoordinateRecord> GetRecordingList(string carId)
        {
            
            var l = _carService.GetCoordinateRecordsforCar(carId);
            return l;
        }

        [HttpGet("recording/export")]
        //STEP 3 EXPORT THE COORDINATES IN CSV
        public ActionResult ExportCoordinateRecord(string recordId)
        {
            //var coordinates = "1,2,3,4";
            var coordinateRecord = _carService.GetCoordinateRecord(recordId);
            var sb = new StringBuilder();

            foreach (var coordinate in coordinateRecord.Coordinates)
            {
                sb.AppendLine($"{coordinate.Latitude}, {coordinate.Longitude}");
            }


            var result = new FileContentResult(System.Text.Encoding.ASCII.GetBytes(sb.ToString()), "application/octet-stream");
            result.FileDownloadName = "export_route.csv";
            return result;
        }



        

       

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