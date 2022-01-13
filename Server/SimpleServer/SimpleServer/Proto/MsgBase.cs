﻿using ProtoBuf;
using ServerBase;
using SimpleServer.Net;
using System;
using System.IO;
namespace SimpleServer.Proto
{
    public class MsgBase
    {
        public virtual ProtocolEnum ProtoType { get; set; }

        /// <summary>
        /// 编码协议名
        /// </summary>
        /// <param name="msgBase"></param>
        /// <returns></returns>
        public static byte[] EncodeName(MsgBase msgBase)
        {
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
            Int16 len = (Int16)nameBytes.Length;
            byte[] bytes = new byte[2 + len];
            bytes[0] = (byte) (len % 256);
            bytes[1] = (byte) (len / 256);
            Array.Copy(nameBytes,0,bytes,2,len);
            return bytes;
        }

        /// <summary>
        /// 解码协议名
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static ProtocolEnum DecodeName(byte[] bytes,int offset,out int count)
        {
            count = 0;
            if (offset + 2 > bytes.Length) return ProtocolEnum.None;
            //获得协议长度
            Int16 len = (Int16) (bytes[offset + 1] << 8 | bytes[offset]);
            if (offset + 2 + len > bytes.Length) return ProtocolEnum.None;
            count = 2 + len;
            try
            {
                string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
                return (ProtocolEnum)Enum.Parse(typeof(ProtocolEnum), name);
            }
            catch (Exception ex)
            {
                Debug.LogError("不存在的协议:" + ex);
                return ProtocolEnum.None;
            }
        }

        /// <summary>
        /// 协议序列化以及加密
        /// </summary>
        /// <param name="msgBase"></param>
        /// <returns></returns>
        public static byte[] Encond(MsgBase msgBase)
        {
            using (var memory = new MemoryStream())
            {
                //将协议类进行序列号并转换成数组
                Serializer.Serialize(memory, msgBase);
                byte[] bytes = memory.ToArray();
                //默认使用密钥加密
                string secret = ServerSocket.SecretKey;
                //如果请求密钥，则用公钥加密
                if (msgBase is MsgSecret)
                {
                    secret = ServerSocket.PublicKey;
                }
                //对数组进行加密
                bytes = AES.AESEncrypt(bytes, secret);
                return bytes;
            }
        }

        /// <summary>
        /// 协议解密
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int offset, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("协议解密出错，数据长度为0");
                return null;
            }
            try
            {
                byte[] newBytes = new byte[count];
                Array.Copy(bytes,offset,newBytes,0,count);
                string secret = ServerSocket.SecretKey;
                if (protocol == ProtocolEnum.MsgSecret)
                {
                    secret = ServerSocket.PublicKey;
                }
                //解密
                newBytes = AES.AESDecrypt(newBytes, secret);
                using (var memory = new MemoryStream(newBytes, 0, newBytes.Length))
                {
                    Type t = Type.GetType(protocol.ToString());
                    return (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("协议解密出错:" + ex);
                return null;
            }
        }
    }
}