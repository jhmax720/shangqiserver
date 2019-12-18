using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shangqi.Logic;
using Shangqi.Logic.Data;
using Shangqi.Logic.Model;
using Shangqi.Logic.Model.Outbound;
using Shangqi.Logic.Services;

namespace ShangqiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : Controller
    {
        private readonly CarDbService _carService;
        public RouteController(CarDbService carService)
        {
            _carService = carService;
        }

        [HttpPost("import")]
        [Produces("application/json")]
        ////STEP 4 IMPORT THE COORDINATES FROM CSV AND GENERATE ROUTE 
        public async Task ImportCoordinatesCreateRoute(string carId, bool isReturn = false)
        {
            //get the coordinates from uploaded CSV

            var formFile = HttpContext.Request.Form.Files[0];

            if (formFile != null)
            {
                var list = new List<Coordinate>();
                using (var ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    ms.Position = 0;

                    StreamReader reader = new StreamReader(ms);

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        var anc = new Coordinate(double.Parse(values[0]), double.Parse(values[1]));
                        list.Add(anc);
                    }

                }
                //create a new route in db
                _carService.AddRoute(carId, list.ToArray());
                ////update the end position in cache
                // update the final endpoint in cache
                var carData = _carService.GetCar(carId);
                var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{carData.CarName}");
                cached.EndPoint = list.LastOrDefault();
                cached.ImpotedCoordinates = list.ToArray();
                cached.RouteStatus = 1;
                await RedisHelper.Instance.SetCache($"car_{carData.CarName}", cached);


                //send command to robot
                //路径点与传输：
                var outbound = new OutboundModel();
                outbound.CarName = cached.CarName;
                outbound.Data = new List<object>();
                outbound.Ip = cached.Ip;
                var msg_total = cached.ImpotedCoordinates.Count;
                for (int i = 0; i < msg_total; i++)
                {
                    outbound.Data.Add(new msg_map_route
                    {
                        msg_total = msg_total,
                        msg_count = i + 1, //it starts with 1
                        latitude = cached.ImpotedCoordinates[i].Latitude,
                        longitude = cached.ImpotedCoordinates[i].Longitude

                    });

                }
                RedisHelper.Instance.SetCache("command", outbound).Wait();
            }











        }


        ////STEP 5 ADD TRIGGER POINT FOR THE ROUTE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeId"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        /// 
        [HttpGet("items")]
        [Produces("application/json")]
        public List<Route> GetRouteIds(string carId)
        {
            var routes = _carService.Routes(carId);
            return routes;
        }


        [HttpPost("triggerpoint")]
        [Produces("application/json")]
        public async Task AddTriggerCoordinateForRoute(string carId, string routeId, double longitude, double latitude)
        {
            //update the route in db
            await _carService.UpdateRouteWithTriggerPoint(routeId, longitude, latitude);

            //update the route in cache                        
            var carData = _carService.GetCar(carId);
            var cached = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{carData.CarName}");
            cached.TriggerPoint = new Coordinate(longitude, latitude);
            await RedisHelper.Instance.SetCache($"car_{carData.CarName}", cached);

        }
    }
}