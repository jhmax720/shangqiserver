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

                int port = 5000;
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

                    //Test
                    var sendStr = @"{""type"": ""heart"", ""tcp/ip"" :""127.0.0.1:49426"", ""msg_count"": 42, ""robot_status"": "+robotStatus+ @", ""error"" : 10, ""rtk_qual"": 14, ""route_id"": 1, ""route_status"" :3, ""longitude"": " + longitude + @", ""latitude"": " + latitude+  @", ""battery"":90, ""check"": 8}";
                    byte[] sendBytes = Encoding.ASCII.GetBytes(sendStr);
                    clientSocket.Send(sendBytes);

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
