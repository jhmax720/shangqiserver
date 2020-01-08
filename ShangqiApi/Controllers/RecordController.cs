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
            

        [HttpPost("recording/start")]
        [Produces("application/json")]
        //STEP 1 START RECORDING CAR COORDINATES
        public async Task<ActionResult> StartCoordinateRecording(int carName, string recordId)
        {

            if (recordId == null)
            {
                //get car info from db
                var car = _carService.GetCarByName(carName);

                //update robot status in cache
                var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.CarName}");

                if (carInCache == null) return BadRequest();
                carInCache.RobotStatus = Const.ROBOT_STATUS_REMOTE;
                await RedisHelper.Instance.SetCache($"car_{car.CarName}", carInCache);

                //add to db
                _carService.AddNewCarRecord(car.Id);


                //send to robot to start recording
                var msg = new msg_control_cmd()
                {
                    cmd = 1,
                    cmd_slave = 0,
                    route_id = 0
                };


                var outbound = new OutboundModel()
                {
                    Ip = carInCache.Ip,
                    CarName = carInCache.CarName,
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
        [Produces("application/json")]
        //STEP 2 END RECORDING AND SAVE DATA FROM CACHE
        public async Task<ActionResult> EndCarRecord(int carName)
        {
            //get car info from db
            var car = _carService.GetCarByName(carName);
            //save cached coordinates to database
            var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{car.CarName}");
            _carService.EndCarRecording(car.Id, carInCache.CachedCoordinates);

            //update robot status in cache
            carInCache.RobotStatus = 0;
            carInCache.CachedCoordinates = new List<Coordinate>();
            await RedisHelper.Instance.SetCache($"car_{car.CarName}", carInCache);

            //send to robot to end recording
            var msg = new msg_control_cmd()
            {
                cmd = 0,
                cmd_slave = 0,
                route_id = 0
            };


            var outbound = new OutboundModel()
            {
                Ip = carInCache.Ip,
                CarName = carInCache.CarName,
                Data = new List<object> { msg }
            };
            await RedisHelper.Instance.SetCache<OutboundModel>("command", outbound);

            return Ok();
        }



        [HttpGet("recording/list")]
        [Produces("application/json")]
        public IList<CoordinateRecord> GetRecordingList(string carId)
        {
            
            var l = _carService.GetCoordinateRecordsforCar(carId);
            return l;
        }

        [HttpGet("recording/export")]
        [Produces("application/json")]
        //STEP 3 EXPORT THE COORDINATES IN CSV
        public ActionResult ExportCoordinateRecord(string recordId)
        {
            //var coordinates = "1,2,3,4";
            var coordinateRecord = _carService.GetCoordinateRecord(recordId);
            var sb = new StringBuilder();

            foreach (var coordinate in coordinateRecord.Coordinates)
            {
                sb.AppendLine($"{coordinate.Longitude}, {coordinate.Latitude}");
            }


            var result = new FileContentResult(System.Text.Encoding.ASCII.GetBytes(sb.ToString()), "application/octet-stream");
            result.FileDownloadName = "export_route.csv";
            return result;
        }

        

    }
}