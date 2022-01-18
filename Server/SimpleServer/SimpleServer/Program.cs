using MySql;
using SimpleServer.Net;
using System;
namespace SimpleServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("启动服务器\n");
            MySqlMgr.Instance.Init();
            ServerSocket.Instance.Init();
            Console.ReadLine();
        }
    }
}
