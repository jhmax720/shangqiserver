using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

using Shangqi.Logic;

namespace ShangqiApi
{
    public class MyEchoConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<MyEchoConnectionHandler> _logger;

        public MyEchoConnectionHandler(ILogger<MyEchoConnectionHandler> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            
            _logger.LogInformation(connection.ConnectionId + " connected");


            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, connection);
                RedisHelper.Instance.SetNormalCache("client", stream.ToArray());
            }
            //await RedisHelper.Instance.SetCache<SocketConnection>("client", myConnection);

            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    await connection.Transport.Output.WriteAsync(segment);
                }

                if (result.IsCompleted)
                {
                    break;
                }

                connection.Transport.Input.AdvanceTo(buffer.End);
            }

            _logger.LogInformation(connection.ConnectionId + " disconnected");
        }
    }
}