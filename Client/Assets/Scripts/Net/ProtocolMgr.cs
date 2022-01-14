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
}
