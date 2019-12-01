﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shangqi.Logic;
using Shangqi.Logic.Configuration;
using Shangqi.Logic.Model;
using Shangqi.Logic.Model.Outbound;
using Shangqi.Logic.Services;

namespace ShangqiSocket
{

   
    class Program
    {
    
        //private static byte[] result = new byte[1024];
        private const int port = 3388;
        private static string IpStr = "127.0.0.1";
        private static TcpListener listener;
        private static List<TcpClient> clients = new List<TcpClient>();
        private static List<CachedRecordingModel> _cars = new List<CachedRecordingModel>();

        private static CarDbService _carDbService;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    // do the work in the loop
                    //string newData = DateTime.Now.ToLongTimeString();

                    //// update the UI on the UI thread
                    //Console.WriteLine("tick tock");
                    foreach(var _carDirty in _cars)
                    {
                        if(_carDirty.IsDirty)
                        {
                            await _carDbService.SyncRoute(_carDirty);
                            _carDirty.IsDirty = false;
                        }
                    }

                    // don't run again for at least 200 milliseconds
                    await Task.Delay(5000);
                }
            });

            var dbSettings = new DBSettings()
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "Shangqidb"
            };
            

            _carDbService = new CarDbService(Options.Create(dbSettings));

            IPAddress ip = IPAddress.Parse(IpStr);
            IPEndPoint ip_end_point = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ip_end_point);
            listener.Start();
            Console.WriteLine("tcp server started");

            //异步接收 递归循环接收多个客户端
            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), listener);


            




            while (true)
            {
                try
                {
                    //SEND COMMAND TO ROBOT
                    if (clients.Count > 0)
                    {
                        var model = RedisHelper.Instance.GetCacheItem<OutboundModel>("command").Result;
                        if (model != null)
                        {
                            Logger.Instance.Log(LogLevel.Information, $"sending command to robot ip: {model.IpAddress}");
                            var client = clients.FirstOrDefault(c=>c.Client.RemoteEndPoint.ToString() == model.IpAddress);
                            //var client = clients.FirstOrDefault(); //todo remove this after local testing
                            if(client!=null)
                            {
                                var stream = client.GetStream();
                            
                                foreach (var obj in model.Data)
                                {
                                    var jsonStr = JsonConvert.SerializeObject(obj);


                                    var result = Encoding.ASCII.GetBytes(jsonStr);

                                    //TODO RESET CACHED COORDINATES



                                    stream.Write(result, 0, result.Length);
                                    stream.Flush();
                                }
                            }
                            
                          
                            RedisHelper.Instance.ClearKey("command");
                        }

                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine("error:" + e.ToString());
                    break;
                }

            }
            Console.ReadKey();
        }

        private static void DoAcceptTcpclient(IAsyncResult State)
        {
            /*                   */
            /* 处理多个客户端接入*/
            /*                   */
            TcpListener listener = (TcpListener)State.AsyncState;

            TcpClient client = listener.EndAcceptTcpClient(State);

            

            Console.WriteLine("\n new client connected:{0}", client.Client.RemoteEndPoint.ToString());
            //开启线程用来持续收来自客户端的数据
            Thread myThread = new Thread(new ParameterizedThreadStart(printReceiveMsg));




             myThread.Start(client);
            //Task.Run(async () => await printReceiveMsg(client));
            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), listener);

            clients.Add(client);
        }

        private static void printReceiveMsg(object reciveClient)
        {
            /*                   */
            /* 用来打印接收的消息*/
            /*                   */
            
            TcpClient client = reciveClient as TcpClient;
            if (client == null)
            {
                Console.WriteLine("client error");
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();
                while (true)
                {
                    byte[] result = new byte[1024];
                    int num = stream.Read(result, 0, result.Length); //将数据读到result中，并返回字符长度                  
                    if (num != 0)
                    {
                        string str = Encoding.UTF8.GetString(result, 0, num);//把字节数组中流存储的数据以字符串方式赋值给str
                                                                             
                        Console.WriteLine("From: " + client.Client.RemoteEndPoint.ToString() + " : " + str);
                        //Logger.Instance.Log("From: " + client.Client.RemoteEndPoint.ToString() + " : " + str);


                        try
                        {
                            var rows = str.Split(new string[] { "}{" }, StringSplitOptions.None);
                            foreach (var aRow in rows)
                            {
                                var validatedStr = aRow.FormatValidJson();
                                HeartBeatModel _heartBeat = JsonConvert.DeserializeObject<HeartBeatModel>(validatedStr);
                                if (_heartBeat != null)
                                {

                                    //Check if the car is in cache
                                    //get cache car list
                                    var carInCache = RedisHelper.Instance.TryGetFromCarList($"car_{client.Client.RemoteEndPoint.ToString()}").Result;
                                    if (carInCache == null)
                                    {

                                        
                                        var newCarModel = _heartBeat.ToCachedRecordModel(client.Client.RemoteEndPoint.ToString());
                                        
                                        //ADD IF NOT EXIST
                                        _carDbService.AddCar(newCarModel);

                                        //READ FROM DB 
                                        var carInDb = _carDbService.GetCarByIp(client.Client.RemoteEndPoint.ToString());

                                        if(carInDb != null)
                                        {
                                            newCarModel.CarId = carInDb.Id;
                                            Console.WriteLine("attempt loading car from db: " + client.Client.RemoteEndPoint.ToString());
                                            //todo load the latest route from db to cache if possible
                                        }
                                        else
                                        {                                            
                                            //throw err?
                                        }
                                       
                                        //add to the cache                                
                                        RedisHelper.Instance.SetCache($"car_{client.Client.RemoteEndPoint.ToString()}", newCarModel).Wait();
                                        carInCache = newCarModel;
                                        _cars.Add(carInCache);

                                        RedisHelper.Instance.UpdateCurrentCarsInCache(_cars).Wait();

                                        Logger.Instance.Log(LogLevel.Information, "new robot added successfully " + client.Client.RemoteEndPoint.ToString());

                                    }


                                    //if (carInCache.IsMainVehicle)
                                    //todo: FIX THIS AFTER LOCAL TEST
                                    //assume the 2nd car is the main car
                                    if(_cars.Count>1 && _cars[1].CarIp == carInCache.CarIp)
                                    {
                                        //update current postion
                                        carInCache.CurrentPosition = new Coordinate(_heartBeat.longitude, _heartBeat.latitude);
                                        //add to the coordinate list
                                        carInCache.CachedCoordinates.Add(new Coordinate(_heartBeat.longitude, _heartBeat.latitude));
                                        //trigger other route if possible
                                        foreach (var car in _cars)
                                        {
                                            if (car.RouteStatus == 1)
                                            {
                                                if (carInCache.TriggerPoint.IsInRange(_heartBeat.longitude,
                                                    _heartBeat.latitude))
                                                {
                                                    //trigger the route now
                                                    //go to auto pilot mode and send the coordinates to client

                                                    Logger.Instance.Log(LogLevel.Information, $"robot {car.CarIp} is getting triggered by main car");

                                                    var outbound = new OutboundModel();
                                                    outbound.IpAddress = carInCache.CarIp;
                                                    outbound.Data = new List<object>();
                                                    //切换⾄数据传输模式
                                                    outbound.Data.Add(new msg_control_cmd()
                                                    {
                                                        cmd = "1",
                                                        cmd_slave = "3",
                                                        route_id = "x"
                                                    });
                                                    //路径点与传输：

                                                    var msg_total = carInCache.ImpotedCoordinates.Count;
                                                    for (int i = 0; i < msg_total; i++)
                                                    {
                                                        outbound.Data.Add(new msg_map_route
                                                        {
                                                            msg_total = msg_total,
                                                            msg_count = i + 1, //it starts with 1
                                                            latitude = carInCache.ImpotedCoordinates[i].Latitude,
                                                            longitude = carInCache.ImpotedCoordinates[i].Longitude

                                                        });

                                                    }



                                                    RedisHelper.Instance.SetCache("command", outbound).Wait();
                                                    //update database with the  

                                                    //update cache status to action
                                                    car.RouteStatus =2 ;
                                                    car.IsDirty = true;
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {   //verify the car status in cache consitent with client
                                        if (_heartBeat.robot_status == carInCache.RobotStatus)
                                        {
                                            Console.WriteLine($"synced Robot {client.Client.RemoteEndPoint.ToString()} status {_heartBeat.robot_status}");
                                            //recording
                                            //// 1 == REMOTE CONTROL MODE
                                            if (_heartBeat.robot_status == Const.ROBOT_STATUS_REMOTE)
                                            {
                                                Console.WriteLine($"remote control request: update coordinate {_heartBeat.longitude}, {_heartBeat.latitude}");
                                                //add the coordinate to the cache
                                                carInCache.CachedCoordinates.Add(new Coordinate(_heartBeat.longitude, _heartBeat.latitude));
                                            }
                                            //2 == AUTO PILOT MODE
                                            else if (_heartBeat.robot_status == Const.ROBOT_STATUS_AUTO_PILOT)
                                            {
                                                //add the coordinates to the cache
                                                carInCache.CachedCoordinates.Add(new Coordinate(_heartBeat.longitude, _heartBeat.latitude));
                                                //update current position in cache
                                                carInCache.CurrentPosition = new Coordinate(_heartBeat.longitude, _heartBeat.latitude);

                                                //is the current position within the endpoint?
                                                if (carInCache.EndPoint.IsInRange(_heartBeat.longitude, _heartBeat.latitude))
                                                {
                                                    //send to client to stop auto pilot mode
                                                    var outbound = new OutboundModel();
                                                    RedisHelper.Instance.SetCache("command", outbound).Wait();
                                                    //update database with the cached coordinates 
                                                    _carDbService.UpdateRouteWithCoordinates(null, carInCache.IsReturn);
                                                    //update cache status to idle
                                                    carInCache.RouteStatus++;
                                                    //end of the process
                                                    if (carInCache.RouteStatus == 4)
                                                    {
                                                        RedisHelper.Instance.ClearKey("car_{ip}");

                                                    }
                                                }
                                            }

                                            //update cache
                                            RedisHelper.Instance.SetCache($"car_{client.Client.RemoteEndPoint.ToString()}", carInCache).Wait();
                                        }


                                    }

                                }
                            }

                        }
                        catch(Exception e)
                        {
                            Logger.Instance.Log(LogLevel.Error, e.Message);
                        }
                        
                


                        //TODO REMOVE
                        //服务器收到消息后并会给客户端一个消息。
                        string msg = str;
                        result = Encoding.UTF8.GetBytes(msg);
                        //stream = client.GetStream();
                        stream.Write(result, 0, result.Length);
                        stream.Flush();
                    }
                    else
                    {   //这里需要注意 当num=0时表明客户端已经断开连接，需要结束循环，不然会死循环一直卡住
                        Console.WriteLine("client closed");
                        stream.Dispose();
                        break;
                    }

                }

                stream.Dispose();
            }
            catch (Exception e)
            {
                clients.Remove(client);
                Console.WriteLine("error:" + e.ToString());
                Logger.Instance.Log(LogLevel.Error, e.Message);
                
            }
          

        }

        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
