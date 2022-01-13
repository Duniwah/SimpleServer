using System;
using UnityEngine;
using System.Collections.Generic;
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

    private static long lastPingTime;
    private static long lastPongTime;

    private List<MsgBase> mMsgList;
    private List<MsgBase> mUnityMsgList;
    //消息长度（不包括心跳包）
    private int mMsgCount = 0;

    //简易事件
    public delegate void EventListener(string str);
    private Dictionary<NetEvent, EventListener> mListenersDic = new Dictionary<NetEvent, EventListener>();
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

    public void Update()
    {
        MsgUpdate();
    }
    
    private void MsgUpdate()
    {
        if (mSocket != null && mSocket.Connected)
        {
            if(mMsgCount == 0) return;
            
        }
    }
    
    /// <summary>
    /// 消息线程处理函数
    /// </summary>
    private void MsgThread()
    {
        while (mSocket!=null && mSocket.Connected)
        {
            if(mMsgList.Count<=0) continue;
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
                    lastPongTime = GetTimeStamp();
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
        mConnecting = false;
        mClosing = false;
        mMsgList = new List<MsgBase>();
        mUnityMsgList = new List<MsgBase>();
        mMsgCount = 0;
        lastPongTime = GetTimeStamp();
        lastPingTime = GetTimeStamp();
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
            
            mConnecting = false;
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
            //TODO 看下是不是错了
            mReadBuff.ReadIdx += count;
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

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="normal">是否正常关闭</param>
    public void Close(bool normal = true)
    {
        if (mSocket == null || mConnecting)
        {
            return;
        }
        SecretKey = "";
        mSocket.Close();
        FirstEvent(NetEvent.Close, normal.ToString());
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
