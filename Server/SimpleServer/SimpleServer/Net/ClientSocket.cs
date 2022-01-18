using System.Net.Sockets;
namespace SimpleServer.Net
{
    public class ClientSocket
    {
        public Socket Socket { get; set; }

        public long LastPingTime { get; set; } = 0;

        public ByteArray ReadBuff { get; set; }

        public int UserId { get; set; }
    }
}
