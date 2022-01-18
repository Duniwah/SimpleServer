using System;
using UnityEngine;
/// <summary>
/// 所有协议收发的单独类
/// </summary>
public class ProtocolMgr
{
    /// <summary>
    /// 连接服务器的第一个请求
    /// </summary>
    public static void SecretRequest()
    {
        MsgSecret msg = new MsgSecret();
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resMsg) =>
        {
            NetManager.Instance.SetKey(((MsgSecret) resMsg).Srcret);
            Debug.Log("获取密钥：" + ((MsgSecret) resMsg).Srcret);
        });
    }

    public static void SocketTest()
    {
        MsgTest msg = new MsgTest();
        msg.ReqContent = "Ocean";
        // + " dsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwer";
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgTest, (resMsg) =>
        {
            Debug.Log("测试回调：" + ((MsgTest) resMsg).RecContent);
        });
    }

    /// <summary>
    /// 注册协议请求
    /// </summary>
    /// <param name="registerType"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="code"></param>
    /// <param name="callback"></param>
    public static void Register(RegisterType registerType, string userName, string password, string code, Action<RegisterResult> callback)
    {
        MsgRegister msg = new MsgRegister();
        msg.RegisterType = registerType;
        msg.Account = userName;
        msg.Password = password;
        msg.Code = code;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgRegister, (resMsg) =>
        {
            MsgRegister msgRegister = (MsgRegister) resMsg;
            callback(msgRegister.Result);
        });
    }
    
    /// <summary>
    /// 登录协议请求
    /// </summary>
    /// <param name="loginType"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="callback"></param>
    public static void Login(LoginType loginType, string userName, string password, Action<LoginResult,string> callback)
    {
        MsgLogin msg = new MsgLogin();
        msg.LoginType = loginType;
        msg.Account = userName;
        msg.Password = password;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgLogin, (resMsg) =>
        {
            MsgLogin msgLogin = (MsgLogin) resMsg;
            callback(msgLogin.Result, msgLogin.Token);
        });
    }
}
