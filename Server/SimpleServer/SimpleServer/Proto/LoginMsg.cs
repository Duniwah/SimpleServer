using ProtoBuf;
using SimpleServer.Business;
namespace SimpleServer.Proto
{
    [ProtoContract]
    public class MsgRegister : MsgBase
    {
        //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列号标记
        public MsgRegister()
        {
            ProtoType = ProtocolEnum.MsgRegister;
        }
        [ProtoMember(1)] 
        public sealed override ProtocolEnum ProtoType { get; set; }
        
        //client to server
        [ProtoMember(2)] 
        public string Account;
        [ProtoMember(3)] 
        public string Password;
        [ProtoMember(4)] 
        public string Code;
        [ProtoMember(5)] 
        public RegisterType RegisterType;
        
        //server to client
        [ProtoMember(6)] 
        public RegisterResult Result;
    }
    
    [ProtoContract]
    public class MsgLogin : MsgBase
    {
        //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列号标记
        public MsgLogin()
        {
            ProtoType = ProtocolEnum.MsgLogin;
        }
        [ProtoMember(1)] 
        public sealed override ProtocolEnum ProtoType { get; set; }
        
        //client to server
        [ProtoMember(2)] 
        public string Account;
        [ProtoMember(3)] 
        public string Password;
        [ProtoMember(4)] 
        public LoginType LoginType;
        
        //server to client
        [ProtoMember(5)] 
        public LoginResult Result;
        [ProtoMember(6)] 
        public string Token;
    }
}
