﻿using ProtoBuf;
namespace SimpleServer.Proto
{
    [ProtoContract]
    public class MsgSecret : MsgBase
    {
        //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列号标记
        public MsgSecret()
        {
            ProtoType = ProtocolEnum.MsgSecret;
        }
        [ProtoMember(1)] public sealed override ProtocolEnum ProtoType { get; set; }

        [ProtoMember(2)]
        public string Srcret;
    }
}
