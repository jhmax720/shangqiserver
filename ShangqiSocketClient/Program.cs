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

                //int port = 5000;
                string host = "127.0.0.1";//服务器端ip地址
                int port = 8007;
                //string host = "localhost";//服务器端ip地址

                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);

                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("connection starting 。");
                clientSocket.Connect(ipe);

                while (true)
                {
                    Console.WriteLine("please enter something：");
                    string sendStr = Console.ReadLine();

                    if (sendStr == "exit")
                        break;

                    //Test
                    sendStr = @"{""type"": ""heart"", ""tcp/ip"" :""192.168.0.100:5050"", ""msg_count"": 42, ""robot_status"":0, ""error"" : 10, ""rtk_qual"": 14, ""route_id"": 1, ""route_status"" :3, ""longitude"": 2231.30538, ""latitude"": 11353.76058, ""battery"":90, ""check"": 8}";
                    byte[] sendBytes = Encoding.ASCII.GetBytes(sendStr);
                    clientSocket.Send(sendBytes);

                    //receive message
                    string recStr = "";
                    byte[] recBytes = new byte[4096];
                    int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                    recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
                    Console.WriteLine($"msg from server：{recStr}");
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
