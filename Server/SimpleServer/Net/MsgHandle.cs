using ServerBase;
using SimpleServer.Business;
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
            if (msgBase == null)
            {
                return;
            }
            MsgSecret msgSecret = (MsgSecret) msgBase;
            msgSecret.Srcret = ServerSocket.SecretKey;
            ServerSocket.Send(c, msgSecret);
        }

        /// <summary>
        /// 心跳包
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgPing(ClientSocket c, MsgBase msgBase)
        {
            c.LastPingTime = ServerSocket.GetTimeStamp();
            MsgPing msgPong = new MsgPing();
            ServerSocket.Send(c, msgPong);
        }
        
        /// <summary>
        /// 分包粘包测试
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgTest(ClientSocket c, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest) msgBase;
            Debug.Log(msgTest.ReqContent);
            msgTest.RecContent = "服务器放回的数据!";
            // +"Ocean dsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwerdsfsdffdsegvxcvxdfgawerrtwerwerwerwer";;
            ServerSocket.Send(c, msgTest);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgRegister(ClientSocket c, MsgBase msgBase)
        {
            MsgRegister msg = (MsgRegister) msgBase;
            var rst = UserManager.Instance.Register(msg.RegisterType, msg.Account, msg.Password, out string token);
            msg.Result = rst;
            ServerSocket.Send(c, msg);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgLogin(ClientSocket c, MsgBase msgBase)
        {
            MsgLogin msg = (MsgLogin) msgBase;
            var rst = UserManager.Instance.Login(msg.LoginType, msg.Account, msg.Password, out int userId, out string token);
            msg.Result = rst;
            msg.Token = token;
            c.UserId = userId;
            ServerSocket.Send(c, msg);
        }
    }
}
