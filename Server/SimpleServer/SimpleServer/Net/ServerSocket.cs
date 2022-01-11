using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace SimpleServer.Net
{
    public class ServerSocket : Singleton<ServerSocket>
    {
        //公钥
        public static string PublicKey = "OceanSever";
        //密钥，后续可随时间进行变化
        public static string SecretKey = "Ocean_Up&&NB!!";

#if DEBUG
        private const string IP_STR = "172.12.12.30";
#else
        //对应阿里云或腾讯云的本地ip地址（不是公共ip地址）
        private string mIpstr = "172.45.756.54";
#endif
        //服务器监听socket
        private const int PORT = 8011;
        
        //服务器监听socket
        private static Socket _listenSocket;
        
        //客户端socket集合
        private static List<Socket> _checkReadList = new List<Socket>();

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(IP_STR);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, PORT);
            _listenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(ipEndPoint);
            _listenSocket.Listen(50000);
            
        }
    }
}
