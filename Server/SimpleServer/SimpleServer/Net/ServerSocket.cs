using ServerBase;
using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
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

        //心跳包间隔时间
        public const long PING_INTERVAL = 30;

        //服务器监听socket
        private static Socket _listenSocket;

        //临时保存所有socket集合
        private static List<Socket> _checkReadList = new List<Socket>();

        //所有客户端的一个字典
        public static Dictionary<Socket, ClientSocket> ClientDic = new Dictionary<Socket, ClientSocket>();

        //TODO 需要改善 心跳用临时列表
        public static List<ClientSocket> TempList = new List<ClientSocket>();

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(IP_STR);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, PORT);
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(ipEndPoint);
            _listenSocket.Listen(50000);
            Debug.LogInfo("服务器启动监听{0}成功", _listenSocket.LocalEndPoint.ToString());

            while (true)
            {
                //检查是否有读取的socket

                //处理找出所有socket
                ResetCheckRead();

                try
                {
                    //最后等待时间单位是微妙
                    Socket.Select(_checkReadList, null, null, 1000);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                for (int i = _checkReadList.Count - 1; i >= 0; i--)
                {
                    Socket s = _checkReadList[i];
                    if (s == _listenSocket)
                    {
                        //说明有客户端连接到服务器了，所以服务器socket可读
                        ReadListen(s);
                    }
                    else
                    {
                        //说明连接的客户端可读，证明有信息传上来了
                        ReadClient(s);
                    }
                }

                //检查是否有心跳包超时的计算
                long timeNow = GetTimeStamp();
                TempList.Clear();
                foreach (var clientSocket in ClientDic.Values)
                {
                    if (timeNow - clientSocket.LastPingTime > PING_INTERVAL * 4)
                    {
                        Debug.Log("Ping Close" + clientSocket.Socket.RemoteEndPoint.ToString());
                        // CloseClient(clientSocket);
                        TempList.Add(clientSocket);
                    }
                }

                foreach (var clientSocket in TempList)
                {
                    CloseClient(clientSocket);
                }
                TempList.Clear();
            }
        }

        public static void ResetCheckRead()
        {
            _checkReadList.Clear();
            _checkReadList.Add(_listenSocket);
            foreach (var s in ClientDic.Keys)
            {
                _checkReadList.Add(s);
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="listen"></param>
        public void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = new ClientSocket
                {
                    Socket = client,
                    LastPingTime = GetTimeStamp(),
                    ReadBuff = new ByteArray()
                };
                ClientDic.Add(client, clientSocket);
                Debug.Log("一个客户端连接：{0}，当前有{1}个客户端在线", client.LocalEndPoint.ToString(), ClientDic.Count);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Accept fail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// 接受客户端消息
        /// </summary>
        /// <param name="client"></param>
        public void ReadClient(Socket client)
        {
            ClientSocket clientSocket = ClientDic[client];
            //接受信息，根据信息解析协议
            //根据处理下发给客户端
            ByteArray readBuff = clientSocket.ReadBuff;
            int count = 0;

            //如果上一次接受数据刚好占满1024的数组
            if (readBuff.Remain <= 0)
            {
                //数据移动到index = 0为止
                OnReceiveData(clientSocket);
                readBuff.CheckAndMoveBytes();
                //如果数据长度大于默认长度，扩充数据长度，保证信息的正常接收
                while (readBuff.Remain <= 0)
                {
                    int expandSize = readBuff.Length < ByteArray.DefaultSize ? ByteArray.DefaultSize : readBuff.Length;
                    readBuff.ReSize(expandSize * 2);
                }
            }
            try
            {
                count = client.Receive(readBuff.Bytes, readBuff.WriteIdx, readBuff.Remain, 0);

            }
            catch (SocketException ex)
            {
                Debug.LogError("Receive fail: " + ex);
                CloseClient(clientSocket);
                return;
            }

            //客户端断开连接
            if (count <= 0)
            {
                CloseClient(clientSocket);
                return;
            }

            readBuff.WriteIdx += count;
            //解析信息
            OnReceiveData(clientSocket);
            readBuff.CheckAndMoveBytes();
        }

        /// <summary>
        /// 接收信息处理
        /// </summary>
        /// <param name="clientSocket"></param>
        public void OnReceiveData(ClientSocket clientSocket)
        {
            ByteArray readBuff = clientSocket.ReadBuff;
            //基本消息长度判断
            if (readBuff.Length <= 4 || readBuff.ReadIdx < 0)
            {
                return;
            }
            int readIdx = readBuff.ReadIdx;
            byte[] bytes = readBuff.Bytes;
            //读取bytes的四个字节，得到长度(读取数据头字符中信息长度）
            int bodyLength = BitConverter.ToInt32(bytes, readIdx);
            //判断接受到了信息长度是否小于包体的长度+包体头长度（小于-信息不全，大于-有可能有粘包存在）
            if (readBuff.Length < bodyLength + 4)
            {
                //信息不全 有可能是分包
                return;
            }
            readBuff.ReadIdx += 4;
            //解析协议名
            int nameCount = 0;
            ProtocolEnum proto = ProtocolEnum.None;
            try
            {
                //包名字的解析
                proto = MsgBase.DecodeName(readBuff.Bytes, readBuff.ReadIdx, out nameCount);
            }
            catch (Exception e)
            {
                Debug.LogError("解析协议名出错：" + e);
                CloseClient(clientSocket);
                return;
            }

            if (proto == ProtocolEnum.None)
            {
                Debug.LogError("OnReceiveData MsgBase.DecodeName fail");
                CloseClient(clientSocket);
                return;
            }
            readBuff.ReadIdx += nameCount;

            //解析协议体
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = null;
            try
            {
                msgBase = MsgBase.Decode(proto, readBuff.Bytes, readBuff.ReadIdx, bodyCount);
                if (msgBase == null)
                {
                    Debug.LogError("{0}协议内容解析错误", proto);
                    CloseClient(clientSocket);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("接受数据协议内容解析错误" + e);
                CloseClient(clientSocket);
                return;
            }
            readBuff.ReadIdx += bodyCount;
            readBuff.CheckAndMoveBytes();
            
            //分发消息
            //通过反射调用处理函数
            MethodInfo mi = typeof(MsgHandle).GetMethod(proto.ToString());
            object[] o = {clientSocket, msgBase};
            if (mi != null)
            {
                mi.Invoke(null, o);
            }
            else
            {
                Debug.LogError("OnReceiveData Invoke fail" + proto);
            }
            
            //继续读取消息
            if (readBuff.Length > 4)
            {
                OnReceiveData(clientSocket);
            }
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cs"></param>
        /// <param name="msgBase"></param>
        public static void Send(ClientSocket cs, MsgBase msgBase)
        {
            if (cs == null || !cs.Socket.Connected) return;
            try
            {
                //编码名字
                byte[] nameBytes = MsgBase.EncodeName(msgBase);
                //编码协议内容
                byte[] bodyBytes = MsgBase.Encond(msgBase);
                int len = nameBytes.Length + bodyBytes.Length;
                //把长度写到协议头长度
                byte[] byteHead = BitConverter.GetBytes(len);
                byte[] sendBytes = new byte[byteHead.Length + len];
                Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);
                Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);
                Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);
                try
                {
                    //开始发送数据
                    cs.Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
                }
                catch (Exception e)
                {
                    Debug.LogError("Socket BeginSend Error：" + e);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Socket发送数据失败：" + ex);
            }
        }

        public void CloseClient(ClientSocket client)
        {
            client.Socket.Close();
            ClientDic.Remove(client.Socket);
            Debug.Log("有一个客户端断开连接，当前有{0}个客户端在线", ClientDic.Count);
        }

        // public void

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}
