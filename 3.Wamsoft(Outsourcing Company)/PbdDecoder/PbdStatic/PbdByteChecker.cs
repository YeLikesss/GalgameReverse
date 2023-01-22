using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PbdStatic
{
    /// <summary>
    /// Pbd校验
    /// </summary>
    internal class PbdByteChecker
    {
        /// <summary>
        /// 检查标志
        /// <para>检查每个类型 == 1</para>
        /// <para>只检查最终结果 == 0</para>
        /// <para>检查失败 == -1</para>
        /// </summary>
        private int mCheckFlag;
        private uint mSeed;

        /// <summary>
        /// 计算轮
        /// </summary>
        /// <param name="seed">4字节种子</param>
        private void CalculateRound(Span<byte> seed)
        {
            byte a, b;
            b = a = (byte)(seed[0] ^ (byte)(seed[0] * 2));

            b >>= 2;
            b ^= seed[2];
            b >>= 3;
            b ^= seed[2];
            b ^= a;

            seed[0] = seed[1];
            seed[1] = seed[2];
            seed[2] = b;
        }

        /// <summary>
        /// 进行一次最后变换
        /// </summary>
        private void FinalTransform()
        {
            Span<byte> s = stackalloc byte[4];
            BitConverter.TryWriteBytes(s, this.mSeed);

            this.CalculateRound(s);
            this.CalculateRound(s);
            this.CalculateRound(s);

            s.Slice(0,3).Reverse();

            this.mSeed = BitConverter.ToUInt32(s);
        }

        /// <summary>
        /// 获取种子
        /// </summary>
        /// <param name="typeCode">TJS类型码</param>
        /// <returns></returns>
        public byte GetSeed(byte typeCode)
        {
            if (this.mCheckFlag == 0)
            {
                return 0;       //不检查
            }

            Span<byte> s = stackalloc byte[4];
            BitConverter.TryWriteBytes(s, this.mSeed);

            if (typeCode == 0)
            {
                return s[2];
            }

            this.CalculateRound(s);

            this.mSeed = BitConverter.ToUInt32(s);

            return s[2];
        }

        /// <summary>
        /// 检查当前字节是否合法
        /// </summary>
        /// <param name="valueNeedCheck">待检查字节</param>
        /// <param name="valueInTable">pbd二进制流读取的值</param>
        /// <returns></returns>
        public bool IsVaildByte(byte valueNeedCheck, byte valueInTable)
        {
            return this.IsVaild(valueInTable, this.GetSeed(valueNeedCheck));
        }

        /// <summary>
        /// 检查当前字节是否合法
        /// </summary>
        /// <param name="valueInTable">pbd二进制流读取的值</param>
        /// <param name="valueInSeed">检查器获取的值</param>
        /// <returns></returns>
        public bool IsVaild(byte valueInTable, byte valueInSeed)
        {
            if (this.mCheckFlag == 0 || valueInTable == valueInSeed)
            {
                return true;
            }
            this.mCheckFlag = -1;
            return false;
        }

        /// <summary>
        /// 检查整个二进制流是否合法
        /// </summary>
        /// <param name="checkSum">校验和</param>
        /// <returns></returns>
        public bool IsVaildFinal(uint checkSum)
        {
            this.FinalTransform();
            return this.mCheckFlag >= 0 && checkSum == this.mSeed;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="checkFlag">检查标志位</param>
        /// <param name="seed">种子</param>
        private PbdByteChecker(int checkFlag, uint seed)
        {
            this.mCheckFlag = checkFlag;
            this.mSeed = seed;
        }

        /// <summary>
        /// 使用pbd信息创建检查器
        /// <para>默认检查模式为1 检查每个类型</para>
        /// </summary>
        /// <param name="pbdInfo">pbd信息</param>
        /// <returns></returns>
        public static PbdByteChecker Create(PbdInformation pbdInfo)
        {
            return PbdByteChecker.Create(pbdInfo, 1);
        }

        /// <summary>
        /// 使用pbd信息创建检查器
        /// </summary>
        /// <param name="pbdInfo">pbd信息</param>
        /// <param name="checkMode">检查模式</param>
        /// <returns></returns>
        public static PbdByteChecker Create(PbdInformation pbdInfo, int checkMode)
        {
            Span<byte> s = stackalloc byte[4];
            BitConverter.TryWriteBytes(s, pbdInfo.Seed);
            s[0] ^= s[3];
            s[3] = 0;

            return new(checkMode, BitConverter.ToUInt32(s));
        }

    }
}
