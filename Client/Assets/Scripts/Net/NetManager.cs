using System;
using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
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
            FirstEvent(NetEvent.ConnectSuccess,"");
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
        FirstEvent(NetEvent.Close,normal.ToString());
        Debug.LogError("Close Socket");
    }

    public void SetKey(string key)
    {
        SecretKey = key;
    }
}
