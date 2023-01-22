using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PbdStatic
{
    /// <summary>
    /// Pbd二进制立绘加密环境
    /// </summary>
    internal class PbdCryptoFilter
    {
        private PbdChacha20 mPbdChacha20;

        /// <summary>
        /// Pbd信息
        /// </summary>
        public PbdInformation PbdInformation { get; private set; }
        

        /// <summary>
        /// 初始化加密环境
        /// </summary>
        /// <returns></returns>
        private bool InitializeFilter()
        {
            Span<byte> iv = this.PbdInformation.OuterIV;
            if (iv.Length == 0)
            {
                iv = this.PbdInformation.IV;
            }

            uint seed = this.PbdInformation.Seed;

            switch (this.PbdInformation.CryptoMode)
            {
                case 1:
                    this.mPbdChacha20.Initialize(seed, iv, 8, 16);
                    break;
                case 2:
                    this.mPbdChacha20.Initialize(seed, iv, 12, 8);
                    break;
                case 3:
                    this.mPbdChacha20.Initialize(seed, iv, 20, 4);
                    break;
                case 4:
                    this.mPbdChacha20.Initialize(seed, iv, 8, 1);
                    break;
                case 5:
                    this.mPbdChacha20.Initialize(seed, iv, 12, 1);
                    break;
                case 6:
                    this.mPbdChacha20.Initialize(seed, iv, 20, 1);
                    break;

                default:
                    return false;
            }
            return true;

        }

        private PbdCryptoFilter()
        {
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">数据</param>
        public void Decrypt(Span<byte> data)
        {
            this.mPbdChacha20.Decrypt(data);
        }

        /// <summary>
        /// Pbd二进制立绘加密环境
        /// </summary>
        /// <param name="pbdInfo">pbd信息</param>
        /// <param name="smallCryptoTable">加密表 16字节</param>
        /// <returns></returns>
        public static PbdCryptoFilter Create(PbdInformation pbdInfo, byte[] smallCryptoTable)
        {
            PbdCryptoFilter filter = new()
            {
                PbdInformation = pbdInfo,
                mPbdChacha20 = new(smallCryptoTable)
            };
            
            if (filter.InitializeFilter())
            {
                return filter;
            }
            else
            {
                return null;
            }
        }
    }
}
