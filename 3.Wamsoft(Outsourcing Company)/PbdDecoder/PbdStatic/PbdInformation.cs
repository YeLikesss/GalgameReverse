using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace PbdStatic
{
    /// <summary>
    /// Pbd二进制立绘信息
    /// </summary>
    internal class PbdInformation
    {
        /// <summary>
        /// 压缩方法选择器
        /// </summary>
        private readonly byte[] CompressMethodSelecetor = new byte[] { 0x6E, 0x34 };

        /// <summary>
        /// 获取文件合法性
        /// </summary>
        public bool IsVaild { get; private set; }

        /// <summary>
        /// 不检查标记
        /// </summary>
        public bool NoCheck { get; private set; }

        /// <summary>
        /// 检查失败标记
        /// </summary>
        public bool CheckFail { get; private set; }

        /// <summary>
        /// 获取是否为大端模式
        /// </summary>
        public bool IsBigEndian { get; private set; }

        /// <summary>
        /// 压缩模式
        /// </summary>
        public uint CompressFlag { get; private set; }

        /// <summary>
        /// 种子
        /// </summary>
        public uint Seed { get; private set; }

        /// <summary>
        /// 加密模式
        /// </summary>
        public uint CryptoMode { get; private set; }

        /// <summary>
        /// 加密向量1
        /// </summary>
        public byte[] IV { get; private set; }

        /// <summary>
        /// 加密向量2
        /// </summary>
        public byte[] OuterIV { get; private set; }

        /// <summary>
        /// Pbd立绘信息头大小
        /// </summary>
        public long PbdHeaderSize { get; private set; } = 0;


        private PbdInformation()
        {
        }

        /// <summary>
        /// 创建Pbd二进制立绘信息
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="customIV">用户IV</param>
        /// <returns></returns>
        public static PbdInformation Create(Stream stream, byte[] customIV = null)
        {
            PbdInformation pbd = new();

            Span<byte> buffer = stackalloc byte[16];

            Span<byte> sign = buffer.Slice(0, 4);       //4字节
            Span<byte> check = buffer.Slice(4, 4);      //4字节
            Span<byte> seed = buffer.Slice(8, 4);       //4字节
            Span<byte> cryptoMode = buffer.Slice(12, 2);        //2字节
            Span<byte> ivLength = buffer.Slice(14, 2);          //2字节

            stream.Position = 0;
            stream.Read(buffer);    //读取头    

            //查Sign 
            {
                uint magic = BitConverter.ToUInt32(sign);
                if (magic == 0x5C534A54)
                {
                    pbd.IsBigEndian = true;
                }
                else if (magic == 0x2F534A54)
                {
                    pbd.IsBigEndian = false;
                }
                else
                {
                    return null;
                }
            }
            //查Check
            {
                if (check[1] != 0x73 || check[2] != 0x30 || check[3] != 0x00)
                {
                    return null;
                }

                //获取压缩方法    
                pbd.CompressFlag = 0;
                for (int i = 0; i < pbd.CompressMethodSelecetor.Length; ++i)
                {
                    if (check[0] == pbd.CompressMethodSelecetor[i])
                    {
                        pbd.CompressFlag = (uint)i;
                    }
                }
            }

            //获取种子
            if (pbd.IsBigEndian)
            {
                seed.Reverse();
            }
            pbd.Seed = BitConverter.ToUInt32(seed);

            //获取加密模式
            if (pbd.IsBigEndian)
            {
                cryptoMode.Reverse();
            }
            pbd.CryptoMode = BitConverter.ToUInt16(cryptoMode);


            //获取iv
            {
                if (pbd.IsBigEndian)
                {
                    ivLength.Reverse();
                }
                int ivLen = BitConverter.ToInt16(ivLength);

                if (ivLen != 0)
                {
                    pbd.IV = new byte[ivLen];
                    stream.Read(pbd.IV);
                }
                pbd.OuterIV = customIV;
            }

            pbd.NoCheck = false;
            pbd.CheckFail = false;

            pbd.PbdHeaderSize = stream.Position;

            return pbd;
        }
    }
}