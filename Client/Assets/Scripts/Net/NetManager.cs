using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
public class NetManager : Singleton<NetManager>
{
    public enum NetEvent
    {
        ConnectSuccess = 1,
        ConnectFail = 2,
        Close = 3
    }

    public string PublicKey = "OceanSever";
    public string SecretKey { get; private set; }
    private Socket mSocket;
    private ByteArray mReadBuff;

    private string mIp;
    private int mPoet;

    ///是否正在链接中
    private bool mConnecting = false;
    ///是否正在关闭 
    private bool mClosing = false;

    //处理消息的线程（后台也能运行）
    private Thread mMsgThread;
    //处理心跳包的线程（后台也能运行）
    private Thread mHeartThread;

    //最后一次发送心跳包的时间
    private static long _lastPingTime;
    //最后一次收到心跳包的时间
    private static long _lastPongTime;

    private Queue<ByteArray> mWriteQueue;

    private List<MsgBase> mMsgList;
    private List<MsgBase> mUnityMsgList;
    //消息长度（不包括心跳包）
    private int mMsgCount = 0;
    //心跳包间隔时间
    private const long PING_INTERVAL = 30;

    //简易事件
    public delegate void EventListener(string str);

    private Dictionary<NetEvent, EventListener> mListenersDic = new Dictionary<NetEvent, EventListener>();

    public delegate void ProtoListener(MsgBase msg);

    private Dictionary<ProtocolEnum, ProtoListener> mProtoDic = new Dictionary<ProtocolEnum, ProtoListener>();

    public void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (mListenersDic.ContainsKey(netEvent))
        {
            mListenersDic[netEvent] += listener;
        }
        else
        {
            mListenersDic[netEvent] = listener;
        }
    }
    public void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (mListenersDic.ContainsKey(netEvent))
        {
            mListenersDic[netEvent] -= listener;
            if (mListenersDic[netEvent] == null)
            {
                mListenersDic.Remove(netEvent);
            }
        }
    }
    public void FirstEvent(NetEvent netEvent, string str)
    {
        if (mListenersDic.ContainsKey(netEvent))
        {
            mListenersDic[netEvent](str);
        }
    }

    /// <summary>
    /// 一个协议只有一个监听
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="listener"></param>
    public void AddProtoListener(ProtocolEnum protocolEnum, ProtoListener listener)
    {
        mProtoDic[protocolEnum] = listener;
    }

    public void FirstProto(ProtocolEnum protocolEnum, MsgBase msgBase)
    {
        if (mProtoDic.ContainsKey(protocolEnum))
        {
            mProtoDic[protocolEnum](msgBase);
        }
    }

    public void Update()
    {
        MsgUpdate();
    }

    private void MsgUpdate()
    {
        if (mSocket != null && mSocket.Connected)
        {
            if (mMsgCount == 0) return;
            MsgBase msgBase = null;
            lock (mUnityMsgList)
            {
                if (mUnityMsgList.Count > 0)
                {
                    msgBase = mUnityMsgList[0];
                    mUnityMsgList.RemoveAt(0);
                    mMsgCount--;
                }
            }
            if (msgBase != null)
            {
                FirstProto(msgBase.ProtoType, msgBase);
            }
        }
    }

    /// <summary>
    /// 消息线程处理函数
    /// </summary>
    private void MsgThread()
    {
        while (mSocket != null && mSocket.Connected)
        {
            lock (mMsgList)
            {
                if (mMsgList.Count <= 0) continue;
            }
            MsgBase msgBase = null;
            lock (mMsgList)
            {
                if (mMsgList.Count > 0)
                {
                    msgBase = mMsgList[0];
                    mMsgList.RemoveAt(0);
                }
            }
            if (msgBase != null)
            {
                if (msgBase is MsgPing)
                {
                    _lastPongTime = GetTimeStamp();
                    Debug.Log("收到心跳包：" + _lastPongTime);
                    mMsgCount--;
                }
                else
                {
                    //Unity消息处理
                    lock (mUnityMsgList)
                    {
                        mUnityMsgList.Add(msgBase);
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 心跳包处理
    /// </summary>
    private void PingThead()
    {
        while (mSocket != null && mSocket.Connected)
        {
            long timeNow = GetTimeStamp();
            if (timeNow - _lastPingTime > PING_INTERVAL)
            {
                MsgPing msgPing = new MsgPing();
                SendMessage(msgPing);
                _lastPingTime = GetTimeStamp();
            }
            //如果心跳包过长时间没收到
            if (timeNow - _lastPongTime > PING_INTERVAL * 4)
            {
                Close(false);
            }
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port)
    {
        if (mSocket != null && mSocket.Connected)
        {

            Debug.LogError("连接失败：已经链接");
            return;
        }
        if (mConnecting)
        {
            Debug.LogError("连接失败：正在连接中");
            return;
        }
        InitState();
        mSocket.NoDelay = true;
        mConnecting = true;
        mSocket.BeginConnect(ip, port, ConnectCallback, mSocket);
        mIp = ip;
        mPoet = port;
    }

    /// <summary>
    /// 初始化状态
    /// </summary>
    private void InitState()
    {
        //初始化变量
        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        mReadBuff = new ByteArray();
        mWriteQueue = new Queue<ByteArray>();
        mConnecting = false;
        mClosing = false;
        mMsgList = new List<MsgBase>();
        mUnityMsgList = new List<MsgBase>();
        mMsgCount = 0;
        _lastPongTime = GetTimeStamp();
        _lastPingTime = GetTimeStamp();

    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="ar"></param>
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Success");
            //消息的派发
            FirstEvent(NetEvent.ConnectSuccess, "");

            mMsgThread = new Thread(MsgThread)
            {
                IsBackground = true
            };
            mMsgThread.Start();

            mHeartThread = new Thread(PingThead)
            {
                IsBackground = true
            };
            mHeartThread.Start();

            mConnecting = false;
            //获得密钥
            ProtocolMgr.SecretRequest();
            mSocket.BeginReceive(mReadBuff.Bytes, mReadBuff.WriteIdx, mReadBuff.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket Connect fail:" + e);
            mConnecting = false;
        }
    }

    /// <summary>
    /// 接受数据回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                //关闭连接
                Close();
                return;
            }
            mReadBuff.WriteIdx += count;
            OnReceiveData();
            if (mReadBuff.Remain < 8)
            {
                //扩充ReadBuff
                mReadBuff.MoveBytes();
                mReadBuff.ReSize(mReadBuff.Length * 2);
            }
            socket.BeginReceive(mReadBuff.Bytes, mReadBuff.WriteIdx, mReadBuff.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.LogError("Socket ReceiveCallback fail:" + e);
            Close();
        }
    }

    /// <summary>
    /// 处理接受到的数据  
    /// </summary>
    private void OnReceiveData()
    {
        if (mReadBuff.Length <= 4 || mReadBuff.ReadIdx < 0) return;
        int readIdx = mReadBuff.ReadIdx;
        byte[] bytes = mReadBuff.Bytes;
        int bodyLength = BitConverter.ToInt32(bytes, readIdx);
        //如果消息长度小于头中的记录的消息长度，消息不完整，可能分包
        if (mReadBuff.Length < bodyLength + 4) return;
        mReadBuff.ReadIdx += 4;
        int nameCount = 0;
        ProtocolEnum protocol = MsgBase.DecodeName(mReadBuff.Bytes, mReadBuff.ReadIdx, out nameCount);
        if (protocol == ProtocolEnum.None)
        {
            Debug.LogError("OnReceiveData MsgBase.DecodeName fail");
            Close();
            return;
        }
        mReadBuff.ReadIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        try
        {
            MsgBase msgBase = MsgBase.Decode(protocol, mReadBuff.Bytes, mReadBuff.ReadIdx, bodyCount);
            if (msgBase == null)
            {
                Debug.LogError($"接收到的{protocol.ToString()}协议内容解析出差");
                Close();
                return;
            }
            mReadBuff.ReadIdx += bodyCount;
            mReadBuff.CheckAndMoveBytes();

            //协议具体的操作 (在多线程里面，所以Unity不能操作数据)
            lock (mMsgList)
            {
                mMsgList.Add(msgBase);
            }
            mMsgCount++;

            if (mReadBuff.Length > 4)
            {
                //处理粘包
                OnReceiveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket OnReceiveData error:" + e);
            Close();
        }
    }

    public void SendMessage(MsgBase msgBase)
    {
        if (mSocket == null || !mSocket.Connected)
        {
            return;
        }

        if (mConnecting)
        {
            Debug.LogError("正在连接服务器中，无法发送消息");
            return;
        }

        if (mClosing)
        {
            Debug.LogError("正在关闭连接中，无法发送消息");
            return;
        }

        try
        {
            byte[] nameBytes = MsgBase.EncodeName(msgBase);
            byte[] bodyBytes = MsgBase.Encond(msgBase);
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] byteHead = BitConverter.GetBytes(len);
            byte[] sendBytes = new byte[byteHead.Length + len];
            Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);
            Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);
            Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);
            ByteArray ba = new ByteArray(sendBytes);
            int count = 0;
            lock (mWriteQueue)
            {
                mWriteQueue.Enqueue(ba);
                count = mWriteQueue.Count;
            }
            if (count == 1)
            {
                mSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, mSocket);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SendMessage Error:" + e);
            Close();
        }
    }

    /// <summary>
    /// 发送结束回调
    /// </summary>
    /// <param name="ar"></param>
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            if (socket == null || !socket.Connected) return;
            int count = socket.EndSend(ar);
            //判断是否发送完整
            ByteArray ba;
            lock (mWriteQueue)
            {
                ba = mWriteQueue.First();
            }
            ba.ReadIdx += count;

            if (ba.Length == 0)//说明发送完整了
            {
                lock (mWriteQueue)
                {
                    mWriteQueue.Dequeue();
                    if (mWriteQueue.Count > 0)//说明还有没有发送的数据
                    {
                        ba = mWriteQueue.First();
                    }
                    else
                    {
                        ba = null;
                    }
                }
            }

            //说明发送不完整或者发送完整且存在第二条数据
            if (ba != null)
            {
                socket.BeginSend(ba.Bytes, ba.ReadIdx, ba.Length, 0, SendCallback, socket);
            }
            //确保关闭连接前，把消息都发送完
            else if (mClosing)
            {
                RealClose();
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SendCallback Error:" + e);
            Close();
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="normal">是否正常关闭</param>
    public void Close(bool normal = true)
    {
        if (mSocket == null || mConnecting || !mSocket.Connected)
        {
            return;
        }
        if (mConnecting) return;
        lock (mWriteQueue)
        {
            if (mWriteQueue.Count > 0)
            {
                mClosing = true;
                return;
            }
        }
        RealClose(normal);
    }

    private void RealClose(bool normal = true)
    {
        SecretKey = "";
        mSocket.Close();
        FirstEvent(NetEvent.Close, normal.ToString());
        if (mHeartThread != null && mHeartThread.IsAlive)
        {
            mHeartThread.Abort();
            mHeartThread = null;
        }
        if (mMsgThread != null && mMsgThread.IsAlive)
        {
            mMsgThread.Abort();
            mMsgThread = null;
        }
        Debug.LogError("Close Socket");
    }

    public void SetKey(string key)
    {
        SecretKey = key;
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}
