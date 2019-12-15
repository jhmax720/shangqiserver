﻿using System;
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
    public class RobotController : ControllerBase
    {
        private readonly CarDbService _carService;
        public RobotController(CarDbService carService)
        {
            _carService = carService;
        }
        
        [HttpGet("cache/list")]
        [Produces("application/json")]
        public async Task<IList<CachedRecordingModel>> Get()
        {
            //var l = _carService.List();
            var l = await RedisHelper.Instance.GetCurrentCarsInCache();
            

            return l;
        }

        [HttpPost("control")]
        [Produces("application/json")]
        public async Task<IActionResult> Control(int control, string carId)
        {
            
            var carInDb = _carService.GetCar(carId);
            var carInCache = await RedisHelper.Instance.GetCacheItem<CachedRecordingModel>($"car_{carInDb.CarName}");

            if (carInCache == null)
                return BadRequest();

            var outbound = new OutboundModel(carInCache.Ip, carInCache.CarName);
   

            switch (control)
            {
                //急停
                case 1:                    
                    outbound.Data.Add(new msg_control_cmd()
                    {
                        cmd = 0,
                        cmd_slave = 0,
                        route_id = 0,
                        check = 8,
                        speed = 2.1
                    });
                    return Ok();
                //遥控
                case 2:
                    outbound.Data.Add(new msg_control_cmd()
                    {
                        cmd = 1,
                        cmd_slave = 0,
                        route_id = 0,
                        check = 8,
                        speed = 2.1
                    });
                    return Ok();
                //测绘
                case 3:
                    outbound.Data.Add(new msg_control_cmd()
                    {
                        cmd = 2,
                        cmd_slave = 0,
                        route_id = 0,
                        check = 8,
                        speed = 2.1
                    });

                    return Ok();
                //循迹驾驶
                case 4:
                    outbound.Data.Add(new msg_control_cmd()
                    {
                        cmd = 1,
                        cmd_slave = 1,
                        route_id = 0,
                        check = 8,
                        speed = 2.1
                    });
                    return Ok();



            }
            return BadRequest();
        }

    }
}