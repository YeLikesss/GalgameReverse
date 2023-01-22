using PbdStatic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TJS
{
    /// <summary>
    /// Pbd立绘信息反序列化器
    /// </summary>
    public class TJSDeserializer
    {
        private Stream mPbdStream;
        private bool mIsBigEndian;
        private Func<byte, byte, bool> mCheckerFunction;      //校验函数指针

        /// <summary>
        /// 当前流是否结束
        /// </summary>
        public bool IsStreamEnd => mPbdStream.Position == mPbdStream.Length;

        /// <summary>
        /// 读取类型
        /// </summary>
        /// <returns></returns>
        private TJSVariantType ReadType()
        {
            byte type = (byte)mPbdStream.ReadByte();
            //校验检查
            bool? isVaild = this.mCheckerFunction?.Invoke(type, (byte)mPbdStream.ReadByte());

            return (TJSVariantType)type;
        }

        /// <summary>
        /// 读取Int32
        /// </summary>
        /// <returns></returns>
        private int ReadInt32()
        {
            Span<byte> temp = stackalloc byte[4];
            mPbdStream.Read(temp);
            if (this.mIsBigEndian)
            {
                temp.Reverse();
            }
            return BitConverter.ToInt32(temp);
        }
        /// <summary>
        /// 读取UInt32
        /// </summary>
        /// <returns></returns>
        private uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        /// <summary>
        /// 读取Int64
        /// </summary>
        /// <returns></returns>
        private long ReadInt64()
        {
            Span<byte> temp = stackalloc byte[8];
            mPbdStream.Read(temp);
            if (this.mIsBigEndian)
            {
                temp.Reverse();
            }
            return BitConverter.ToInt64(temp);
        }

        /// <summary>
        /// 读取UInt64
        /// </summary>
        /// <returns></returns>
        private ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        /// <summary>
        /// 读取双精度浮点
        /// </summary>
        /// <returns></returns>
        private double ReadDouble()
        {
            Span<byte> temp = stackalloc byte[8];
            mPbdStream.Read(temp);
            if (this.mIsBigEndian)
            {
                temp.Reverse();
            }
            return BitConverter.ToDouble(temp);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <returns></returns>
        private string ReadString()
        {
            int length = ReadInt32();
            byte[] buffer = new byte[sizeof(short) * length];

            mPbdStream.Read(buffer);
            string s = Encoding.Unicode.GetString(buffer);

            return s;
        }

        /// <summary>
        /// 读取二进制流
        /// </summary>
        /// <returns></returns>
        private byte[] ReadBytes()
        {
            int length = ReadInt32();
            byte[] buffer = new byte[length];
            mPbdStream.Read(buffer);
            return buffer;
        }

        /// <summary>
        /// 读取单个对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadSingleObject()
        {
            TJSVariantType tjsVarType = ReadType();
            switch (tjsVarType)
            {
                case TJSVariantType.Empty:
                case TJSVariantType.Object:
                    return ReadSingleObject();     //对象与空对象递归跳过
                case TJSVariantType.String:
                    return ReadTJSStringObject();
                case TJSVariantType.Bytes:
                    return ReadTJSBytesObject();
                case TJSVariantType.SignInt64:
                    return ReadTJSSignInt64Object();
                case TJSVariantType.Double:
                    return ReadTJSDoubleObject();
                case TJSVariantType.ArrayObject:
                    return ReadTJSArrayObject();
                case TJSVariantType.DictionaryObject:
                    return ReadTJSDictionaryObject();
                default:
                    throw TJSVariantException.New("读取到未知类型");
            }
        }

        /// <summary>
        /// 读取TJS字符串对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSStringObject()
        {
            return new TJSVariant(TJSVariantType.String, ReadString());
        }
        /// <summary>
        /// 读取TJS二进制流对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSBytesObject()
        {
            return new TJSVariant(TJSVariantType.Bytes, ReadBytes());
        }
        /// <summary>
        /// 读取TJS 64位整数对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSSignInt64Object()
        {
            return new TJSVariant(TJSVariantType.SignInt64, ReadInt64());
        }
        /// <summary>
        /// 读取TJS 64位浮点对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSDoubleObject()
        {
            return new TJSVariant(TJSVariantType.Double, ReadDouble());
        }
        /// <summary>
        /// 读取TJS数组对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSArrayObject()
        {
            int count = ReadInt32();       //读取数组个数
            List<TJSVariant> tjsArray = new(count);

            for (int i = 0; i < count; ++i)
            {
                tjsArray.Add(ReadSingleObject());
            }

            return new TJSVariant(TJSVariantType.ArrayObject, tjsArray);
        }
        /// <summary>
        /// 读取TJS字段对象
        /// </summary>
        /// <returns></returns>
        public TJSVariant ReadTJSDictionaryObject()
        {
            int count = ReadInt32();       //读取字典个数
            Dictionary<string, TJSVariant> tjsDictionary = new(count);

            for (int i = 0; i < count; ++i)
            {
                tjsDictionary.Add(ReadString(), ReadSingleObject());
            }

            return new TJSVariant(TJSVariantType.DictionaryObject, tjsDictionary);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="isDisposeStream">True 释放流 False不释放流</param>
        public void Dispose(bool isDisposeStream)
        {
            if (isDisposeStream)
            {
                mPbdStream?.Dispose();
            }
            mPbdStream = null;
        }

        /// <summary>
        /// TJS对象反序列器
        /// </summary>
        /// <param name="pbdBinaryStream">pbd立绘信息二进制流</param>
        /// <param name="bigEndian">True 大端 False 小端</param>
        /// <param name="checkerFunction">pbd校验函数指针</param>
        public TJSDeserializer(Stream pbdBinaryStream, bool bigEndian, Func<byte, byte, bool> checkerFunction = null)
        {
            mPbdStream = pbdBinaryStream;
            mIsBigEndian = bigEndian;
            mCheckerFunction = checkerFunction;
        }
    }
}