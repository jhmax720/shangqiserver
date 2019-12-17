using System;
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
        
        private static CarDbService _carDbService;

        static void Main(string[] args)
        {
            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        // do the work in the loop
            //        //string newData = DateTime.Now.ToLongTimeString();

            //        //// update the UI on the UI thread
            //        //Console.WriteLine("tick tock");
            //        foreach(var _carDirty in _cars)
            //        {
            //            if(_carDirty.IsDirty)
            //            {
            //                Logger.Instance.Log(LogLevel.Information, $"dirty car found, db sync {_carDirty.CarId}");
            //                await _carDbService.SyncRoute(_carDirty);
            //                _carDirty.IsDirty = false;
            //            }
            //        }

            //        // don't run again for at least 200 milliseconds
            //        await Task.Delay(5000);
            //    }
            //});

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
                            Logger.Instance.Log(LogLevel.Information, $"sending command to robot ip: {model.CarName}");
                            var client = clients.FirstOrDefault(c=>c.Client.RemoteEndPoint.ToString() == model.Ip);
                            //var client = clients.FirstOrDefault(); //todo remove this after local testing
                            if(client!=null)
                            {
                                var stream = client.GetStream();
                            
                                foreach (var obj in model.Data)
                                {
                                    var jsonStr = JsonConvert.SerializeObject(obj);


                                    var result = Encoding.ASCII.GetBytes(jsonStr);

                                    //TODO RESET CACHED COORDINATES

                                    Logger.Instance.Log(LogLevel.Information, $"send following command to robot: {model.CarName}, {jsonStr}");

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
                    int num = 0;
                    try
                    {
                        num = stream.Read(result, 0, result.Length); //将数据读到result中，并返回字符长度                  
                    }
                    catch (IOException ex)
                    {
                        Logger.Instance.Log(LogLevel.Warning, $"IO exception detected, client {client.Client.RemoteEndPoint.ToString()} might be disconnected");
                    }
                    
                    if (num != 0)
                    {
                        string str = Encoding.UTF8.GetString(result, 0, num);//把字节数组中流存储的数据以字符串方式赋值给str
                        
                        Logger.Instance.Log( LogLevel.Information, $"From: " + client.Client.RemoteEndPoint.ToString() + " : " + str);
                        


                        try
                        {
                            var rows = str.Split(new string[] { "}{" }, StringSplitOptions.None);
                            foreach (var aRow in rows)
                            {
                                var validatedStr = aRow.FormatValidJson();
                                HeartBeatModel heartBeat = JsonConvert.DeserializeObject<HeartBeatModel>(validatedStr);
                                if (heartBeat.robot_id == 0) continue;

                                if (heartBeat != null)
                                {

                                    

                                    //zhu che
                                    if (heartBeat.robot_id == Const.MAIN_CAR_ID)
                                    {
                                        //update current postion
                                        RedisHelper.Instance.SetCache<HeartBeatModel>("main", heartBeat).Wait();
                                        //carInCache.CurrentPosition = new Coordinate(_heartBeat.longitude, _heartBeat.latitude);
                                        //add to the coordinate list
                                        //carInCache.CachedCoordinates.Add(new Coordinate(_heartBeat.longitude, _heartBeat.latitude));
                                        //trigger other route if possible

                                        var _cars = RedisHelper.Instance.GetCurrentCarsInCache().Result;
                                        foreach (var cachedModel in _cars)
                                        {
                                            Logger.Instance.Log(LogLevel.Information, $"checking trigger point for robot {cachedModel.CarName}");

                                            if (cachedModel.RouteStatus == 1)
                                            {
                                                Logger.Instance.Log(LogLevel.Information, $"calculating robot {cachedModel.CarName} trigger point: {cachedModel.TriggerPoint.Longitude}, {cachedModel.TriggerPoint.Latitude}. Main car long:{heartBeat.longitude}, lat: {heartBeat.latitude}");
                                                if (cachedModel.TriggerPoint.IsInRange(heartBeat.longitude,
                                                    heartBeat.latitude))
                                                {
                                                    //trigger the route now
                                                    //go to auto pilot mode and send the coordinates to client

                                                    Logger.Instance.Log(LogLevel.Information, $"robot {cachedModel.CarName} is getting triggered by main car");

                                                    var outbound = new OutboundModel();
                                                    outbound.CarName = cachedModel.CarName;
                                                    outbound.Data = new List<object>();
                                                    outbound.Ip = cachedModel.Ip;

                                                    //切换⾄数据传输模式
                                                    outbound.Data.Add(new msg_control_cmd()
                                                    {
                                                        cmd = 1,
                                                        cmd_slave = 3,
                                                        route_id = 0
                                                    });

                                                    //循迹驾驶
                                                    outbound.Data.Add(new msg_control_cmd()
                                                    {
                                                        cmd = 1,
                                                        cmd_slave = 1,
                                                        route_id = 0,
                                                        check = 8,
                                                        speed = 2.1
                                                    });

                                                 



                                                    RedisHelper.Instance.SetCache("command", outbound).Wait();
                                                    //update database with the  

                                                    //update cache status to action
                                                    cachedModel.RouteStatus =2 ;
                                                    cachedModel.IsDirty = true;

                                                    Logger.Instance.Log(LogLevel.Information, $"dirty car found, db sync {cachedModel.CarId}");
                                                    _carDbService.SyncRoute(cachedModel).Wait();
                                                    cachedModel.IsDirty = false;

                                                }
                                            }
                                        }

                                    }
                                    //fu che
                                    else
                                    {

                                        //Check if the car is in cache
                                        //get cache car list
                                        var carInCache = RedisHelper.Instance.TryGetFromCarList($"car_{heartBeat.robot_id}").Result;
                                        if (carInCache == null)
                                        {


                                            var newCarModel = heartBeat.ToCachedRecordModel();

                                            //ADD IF NOT EXIST
                                            _carDbService.AddCarIfNotExist(newCarModel);

                                            //READ FROM DB 
                                            var carInDb = _carDbService.GetCarByName(heartBeat.robot_id);

                                            if (carInDb != null)
                                            {
                                                newCarModel.CarId = carInDb.Id;
                                                newCarModel.Ip = client.Client.RemoteEndPoint.ToString();
                                                //Console.WriteLine($"attempt loading car from db: {carInDb.CarName}, {client.Client.RemoteEndPoint.ToString()}");
                                                Logger.Instance.Log(LogLevel.Information, $"attempt loading car from db: {newCarModel.CarName}, {client.Client.RemoteEndPoint.ToString()}");

                                                var latestRouteIfAny = _carDbService.LatestRoute(carInDb.Id);
                                                //ready to be triggered by main car
                                                if (latestRouteIfAny != null && latestRouteIfAny.RouteStatus == 1)
                                                {
                                                    newCarModel.ImpotedCoordinates = latestRouteIfAny.ImportedCarTrack;
                                                }
                                            }
                                            else
                                            {
                                                //throw err?
                                            }

                                            //add to the cache                                


                                            carInCache = newCarModel;
                                            

                                            RedisHelper.Instance.AddCachedNameIndex(carInCache.CarName);
                                            RedisHelper.Instance.SetCache($"car_{carInDb.CarName}", carInCache).Wait();

                                            Logger.Instance.Log(LogLevel.Information, "new robot added successfully " + client.Client.RemoteEndPoint.ToString());

                                        }
                                        else
                                        {
                                            ///Ip will change randomly
                                            Logger.Instance.Log(LogLevel.Information, $"loaded robot '{heartBeat.robot_id}' successfully from cache:" + client.Client.RemoteEndPoint.ToString());

                                            if(carInCache.Ip != client.Client.RemoteEndPoint.ToString())
                                            {
                                                carInCache.Ip = client.Client.RemoteEndPoint.ToString();
                                                RedisHelper.Instance.SetCache($"car_{carInCache.CarName}", carInCache).Wait();

                                            }


                                        }

                                        //fu che
                                        //verify the car status in cache consitent with client
                                        if (heartBeat.robot_status == carInCache.RobotStatus)
                                        {
                                            Console.WriteLine($"synced Robot {client.Client.RemoteEndPoint.ToString()} status {heartBeat.robot_status}");
                                            //update current position in cache
                                            carInCache.CurrentPosition = new Coordinate(heartBeat.longitude, heartBeat.latitude);
                                            carInCache.Speed = heartBeat.speed;
                                            carInCache.Battery = heartBeat.battery;

                                            //recording
                                            //// 1 == REMOTE CONTROL MODE
                                            if (heartBeat.robot_status == Const.ROBOT_STATUS_REMOTE)
                                            {
                                                Console.WriteLine($"remote control request: update coordinate {heartBeat.longitude}, {heartBeat.latitude}");
                                                //add the coordinate to the cache
                                                carInCache.CachedCoordinates.Add(new Coordinate(heartBeat.longitude, heartBeat.latitude));
                                                
                                            }
                                            //2 == AUTO PILOT MODE
                                            else if (heartBeat.robot_status == Const.ROBOT_STATUS_AUTO_PILOT)
                                            {
                                                //add the coordinates to the cache
                                                carInCache.CachedCoordinates.Add(new Coordinate(heartBeat.longitude, heartBeat.latitude));
                                                

                                                //is the current position within the endpoint?
                                                if (carInCache.EndPoint.IsInRange(heartBeat.longitude, heartBeat.latitude))
                                                {
                                                    //send to client to stop auto pilot mode
                                                    var outbound = new OutboundModel( carInCache.Ip, carInCache.CarName);



                                                    RedisHelper.Instance.SetCache("command", outbound).Wait();
                                                    //update database with the cached coordinates 
                                                    _carDbService.UpdateRouteWithCoordinates(null, carInCache.IsReturn);
                                                    //update cache status to idle
                                                    carInCache.RouteStatus++;
                                                    //end of the process
                                                    //if (carInCache.RouteStatus == 4)
                                                    //{
                                                    //    RedisHelper.Instance.ClearKey("car_{ip}");

                                                    //}
                                                }
                                            }

                                            //update cache
                                            RedisHelper.Instance.SetCache($"car_{carInCache.CarName}", carInCache).Wait();
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

     
    }
}
