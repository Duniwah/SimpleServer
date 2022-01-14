using ServerBase;
using SimpleServer.Proto;
namespace SimpleServer.Net
{
    /// <summary>
    /// 协议处理函数 函数名 = 协议枚举名 = 类名
    /// </summary>
    public partial class MsgHandle
    {
        /// <summary>
        /// 密钥获取协议
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgSecret(ClientSocket c, MsgBase msgBase)
        {
            MsgSecret msgSecret = (MsgSecret) msgBase;
            msgSecret.Srcret = ServerSocket.SecretKey;
            ServerSocket.Send(c, msgSecret);
        }
        
        public static void MsgPing(ClientSocket c, MsgBase msgBase)
        {
            c.LastPingTime = ServerSocket.GetTimeStamp();
            MsgPing msgPong = new MsgPing();
            ServerSocket.Send(c, msgPong);
        }
        
        public static void MsgTest(ClientSocket c, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest) msgBase;
            Debug.Log(msgTest.ReqContent);
            msgTest.RecContent = "服务器放回的数据!";
             // +"Ocean dsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwer";;
            ServerSocket.Send(c, msgTest);
        }
    }
}
