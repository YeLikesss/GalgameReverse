using System;
using System.Collections.Generic;
using System.Linq;

namespace PbdStatic
{
    /// <summary>
    /// 游戏Key信息
    /// </summary>
    public interface IKeyInformation 
    {
        /// <summary>
        /// 加密小表  (16字节)
        /// </summary>
        public byte[] CryptoSmallTable { get; }
        /// <summary>
        /// 用户加密向量
        /// </summary>
        public byte[] CustomIV { get; }
    }

    /// <summary>
    /// 默认值
    /// </summary>
    public abstract class GameInformationBase : IKeyInformation
    {
        public virtual byte[] CryptoSmallTable { get; } = new byte[]
        {
             0x9A, 0x87, 0x8F, 0x9E, 0x91, 0x9B, 0xDF, 0xCC, 0xCD, 0xD2, 0x9D, 0x86, 0x8B, 0x9A, 0xDF, 0x94
        };
        public virtual byte[] CustomIV { get; } = null;
    }

}
