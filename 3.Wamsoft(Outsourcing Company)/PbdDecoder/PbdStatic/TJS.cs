using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TJS
{
    /// <summary>
    /// 对象类型
    /// </summary>
    public enum TJSVariantType : byte
    {
        /// <summary>
        /// 不操作
        /// </summary>
        Empty = 0x00,
        /// <summary>
        /// 对象
        /// </summary>
        Object = 0x01,
        /// <summary>
        /// 字符串
        /// </summary>
        String = 0x02,
        /// <summary>
        /// 二进制数据
        /// </summary>
        Bytes = 0x03,
        /// <summary>
        /// 64位数据
        /// </summary>
        SignInt64 = 0x04,
        /// <summary>
        /// 双精度浮点
        /// </summary>
        Double = 0x05,
        /// <summary>
        /// 数组
        /// </summary>
        ArrayObject = 0x81,
        /// <summary>
        /// 字典
        /// </summary>
        DictionaryObject = 0xC1,
    }
    public class TJSVariant
    {
        /// <summary>
        /// 类型
        /// </summary>
        public TJSVariantType Type { get; private set; }
        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="value">值</param>
        public TJSVariant(TJSVariantType type, object value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <returns></returns>
        public string String()
        {
            if (Type != TJSVariantType.String)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return Value as string;
        }
        /// <summary>
        /// 获取二进制流
        /// </summary>
        /// <returns></returns>
        public byte[] Bytes()
        {
            if (Type != TJSVariantType.Bytes)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return Value as byte[];
        }
        /// <summary>
        /// 获取64位整数
        /// </summary>
        /// <returns></returns>
        public long SignInt64()
        {
            if (Type != TJSVariantType.SignInt64)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return (long)Value;
        }
        /// <summary>
        /// 获取64位浮点
        /// </summary>
        /// <returns></returns>
        public double Double()
        {
            if (Type != TJSVariantType.Double)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return (double)Value;
        }
        /// <summary>
        /// 获取TJS数组
        /// </summary>
        /// <returns></returns>
        public List<TJSVariant> TJSArray()
        {
            if (Type != TJSVariantType.ArrayObject)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return Value as List<TJSVariant>;
        }
        /// <summary>
        /// 获取TJS字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, TJSVariant> TJSDictionary()
        {
            if (Type != TJSVariantType.DictionaryObject)
            {
                throw TJSVariantException.ErrorType(Type);
            }
            return Value as Dictionary<string, TJSVariant>;
        }
    }

    public class TJSVariantException : Exception
    {
        public TJSVariantException(string message) : base(message)
        {
        }

        /// <summary>
        /// 抛出默认异常
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static TJSVariantException New(string message)
        {
            return new TJSVariantException(message);
        }
        /// <summary>
        /// 抛出类型错误异常
        /// </summary>
        /// <param name="nowType">当前类型</param>
        /// <returns></returns>
        public static TJSVariantException ErrorType(TJSVariantType nowType)
        {
            return new TJSVariantException("类型错误, 当前类型为" + nowType.ToString());
        }
    }

}