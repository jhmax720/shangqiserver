using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ShangqiSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("starting socket client..");

                int port = 3388;
                string host = "127.0.0.1";//服务器端ip地址
                //int port = 8007;
                //string host = "localhost";//服务器端ip地址

                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);

                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("connection starting ...");
                clientSocket.Connect(ipe);
                Console.WriteLine("connection ok");
                while (true)
                {
                    byte[] recBytes = new byte[4096];
                    string recStr = "";
                  
                    


                    Console.WriteLine("please enter longitude：");
                    string longitude = Console.ReadLine();
                    Console.WriteLine("please enter latitude：");
                    string latitude = Console.ReadLine();
                    Console.WriteLine("please enter robot status: '1-5'：");
                    string robotStatus = Console.ReadLine();
                    Console.WriteLine("please enter robot ID(e.g. 100)：");
                    string robotId = Console.ReadLine();

                    //Scenaria 1 Test normal payload                   
                    //var sendStr = @"{""type"": ""heart"", ""check"": 0, ""longitude"": " + longitude +@", ""msg_count"": 2675, ""route_id"": 0, ""robot_status"": "+ robotStatus + @", ""robot_id"": 102, ""battery"": 0, ""error"": 0, ""rtk_qual"": 0, ""route_status"": 0, ""latitude"": "+ latitude +"}";
                    //byte[] sendBytes = Encoding.ASCII.GetBytes(sendStr);
                    //clientSocket.Send(sendBytes);

                    //Scenaria 2 Test bad payload
                    var sendStr2 = @"{""robot_id"":" + robotId + @", ""type"": ""heart"", ""check"": 0, ""longitude"": " + longitude + @", ""msg_count"": 2675, ""route_id"": 0, ""robot_status"": " + robotStatus + @", ""battery"": 0, ""error"": 0, ""rtk_qual"": 0, ""route_status"": 0, ""latitude"": " + latitude + "}";
                    ///var sendStr3 = @"{""type"": ""heart"", ""check"": 0, ""longitude"": " + longitude + @", ""msg_count"": 2675, ""route_id"": 0, ""robot_status"": " + robotStatus + @", ""robot_id"": 102, ""battery"": 0, ""error"": 0, ""rtk_qual"": 0, ""route_status"": 0, ""latitude"": " + latitude + "}";

                    byte[] sendBytes2 = Encoding.ASCII.GetBytes(sendStr2);
                    clientSocket.Send(sendBytes2);

                    //receive message

                    int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                    recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
                    Console.WriteLine("attempt to read from server");
                    if (recStr.Length > 0)
                    {
                        Console.WriteLine($"msg from server part 2：{recStr}");
                    }
                    //bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                    //recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
                    //Console.WriteLine($"msg from server：{recStr}");
                }
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

    
    }
}
