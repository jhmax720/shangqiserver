using System;
using System.Collections.Generic;
using System.IO;
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
using Shangqi.Logic.Model;

namespace ShangqiSocket
{

   
    class Program
    {
        //private static byte[] result = new byte[1024];
        private const int port = 5000;
        private static string IpStr = "127.0.0.1";
        private static TcpListener listener;
        public static List<TcpClient> clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse(IpStr);
            IPEndPoint ip_end_point = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ip_end_point);
            listener.Start();
            Console.WriteLine("tcp server started");

            //异步接收 递归循环接收多个客户端
            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), listener);


            //sending




            while (true)
            {
                try
                {
                    if (clients.Count > 0)
                    {
                        var model = RedisHelper.Instance.GetNormalItem("command");
                        if (model != null)
                        {
                            Console.WriteLine("found a command from redis");
                            var client = clients[0];
                            var stream = client.GetStream();
                            string msg = "static message from max";
                            byte[] result = new byte[1024];
                            result = Encoding.UTF8.GetBytes(msg);
                            Console.WriteLine("static message from max");
                            stream.Write(result, 0, result.Length);
                            stream.Flush();
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

                        HeartBeatModel model = null;

                        //model = JsonConvert.DeserializeObject<HeartBeatModel>(str);
                        //if(model!=null)
                        //{
                        //    RedisHelper.Instance.SetNormalCache("command", Encoding.UTF8.GetBytes("WTF!"));
                        //}
                        //cache.SetAsync<HeartBeatModel>(str, model, new DistributedCacheEntryOptions()).Wait();
                        //RedisHelper.Instance.SetCache<HeartBeatModel>(str, model).Wait();

                        //HeartBeatModel p = RedisHelper.Instance.GetCacheItem<HeartBeatModel>(str).Result;
                        //var result2 = RedisHelper.Instance.GetNormalItem("command");
                        //if(result2!=null)
                        //{
                        //    var p = System.Text.Encoding.Default.GetString(result2);

                        //    Console.WriteLine($"cached value:{p}");
                        //}
                        



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
                
                
            }
          

        }

    }
}
