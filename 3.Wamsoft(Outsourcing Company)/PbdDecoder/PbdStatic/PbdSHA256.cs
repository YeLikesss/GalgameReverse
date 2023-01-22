using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PbdStatic
{
    internal class PbdSHA256
    {
        /// <summary>
        /// 初始默认状态
        /// </summary>
        public static uint[] Magic = new uint[8]
        {
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A,
            0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

        private byte[] mState = new byte[32];       //当前Hash状态
        private byte[] mData = new byte[64];        //数据块

        private uint mCalculatedSize = 0;           //已计算数据大小
        private uint mLessDataBlockSizeTimes = 0;   //小于数据块大小的次数
        private uint mHashFinalFlag = 0;            //Final标志
        private uint mSeed = 0;

        private int mUnAlignDataSize = 0;           //数据块未对齐大小
        private int mNeedLength = 0;            //需要获取Hash的长度 

        private bool mEnableSeedFlag = false;      //盐启用标志

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            Span<uint> statePack4 = MemoryMarshal.Cast<byte, uint>(this.mState.AsSpan());
            PbdSHA256.Magic.CopyTo(statePack4);
        }

        /// <summary>
        /// 初始化 (带盐)
        /// </summary>
        /// <param name="salt"></param>
        private void InitializeWithSalt(Span<byte> salt)
        {
            this.Initialize();
            if (salt.Length >= 32)
            {
                Span<ulong> statePack8 = MemoryMarshal.Cast<byte, ulong>(this.mState.AsSpan());
                Span<ulong> saltPack8 = MemoryMarshal.Cast<byte, ulong>(salt);
                for (int i = 0; i < 4; ++i)
                {
                    statePack8[i] ^= saltPack8[i];
                }
            }
        }


        /// <summary>
        /// 计算轮
        /// </summary>
        private void CalculateRound(Span<uint> ctx, uint data1, uint data2, uint seed1, uint seed2, uint seed3, uint seed4)
        {
            Span<uint> temp = stackalloc uint[8];

            temp[0] = data1 + seed1 + seed2;
            temp[1] = BitOperations.RotateLeft(temp[0] ^ seed3, 16);
            temp[2] = temp[1] + seed4;
            temp[3] = BitOperations.RotateRight(temp[2] ^ seed1, 12);
            temp[4] = temp[3] + data2 + temp[0];
            temp[5] = BitOperations.RotateRight(temp[4] ^ temp[1], 8);
            temp[6] = temp[5] + temp[2];
            temp[7] = BitOperations.RotateRight(temp[3] ^ temp[6], 7);


            for (int i = 0; i < 4; i++)
            {
                ctx[i] = temp[i + 4];
            }
        }

        /// <summary>
        /// 提交数据并计算
        /// </summary>
        /// <param name="data">待计算的数据</param>
        private void Transform(ReadOnlySpan<byte> data)
        {
            ReadOnlySpan<uint> dataPack4 = MemoryMarshal.Cast<byte, uint>(data);
            Span<uint> statePack4 = MemoryMarshal.Cast<byte, uint>(this.mState);
            Span<uint> extra = stackalloc uint[4];
            extra[0] = this.mCalculatedSize;
            extra[1] = this.mLessDataBlockSizeTimes;
            extra[2] = this.mHashFinalFlag;
            extra[3] = this.mSeed;


            Span<uint> ctxS = stackalloc uint[16];
            Span<uint> ctxD = stackalloc uint[16];

            //-----------------------------------> Round-1

            for (int i = 0; i < 4; ++i)
            {
                this.CalculateRound(ctxD.Slice(4 * i, 4), dataPack4[2 * i + 0], dataPack4[2 * i + 1], statePack4[i + 4], statePack4[i + 0], extra[i] ^ PbdSHA256.Magic[i + 4], PbdSHA256.Magic[i + 0]);
            }
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-2

            for (int i = 0; i < 4; ++i)
            {
                this.CalculateRound(ctxD.Slice(4 * i, 4), dataPack4[2 * i + 8], dataPack4[2 * i + 9], ctxS[(4 * (i + 1) + 3) % 16], ctxS[(4 * (i + 0) + 0) % 16], ctxS[(4 * (i + 3) + 1) % 16], ctxS[(4 * (i + 2) + 2) % 16]);
            }
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-3

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[14], dataPack4[10], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[4], dataPack4[8], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[9], dataPack4[15], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[13], dataPack4[6], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-4

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[1], dataPack4[12], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[0], dataPack4[2], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[11], dataPack4[7], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[5], dataPack4[3], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-5

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[11], dataPack4[8], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[12], dataPack4[0], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[5], dataPack4[2], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[15], dataPack4[13], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-6

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[10], dataPack4[14], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[3], dataPack4[6], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[7], dataPack4[1], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[9], dataPack4[4], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-7

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[7], dataPack4[9], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[3], dataPack4[1], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[13], dataPack4[12], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[11], dataPack4[14], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-8

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[2], dataPack4[6], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[5], dataPack4[10], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[4], dataPack4[0], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[15], dataPack4[8], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-9

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[9], dataPack4[0], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[5], dataPack4[7], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[2], dataPack4[4], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[10], dataPack4[15], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-10

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[14], dataPack4[1], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[11], dataPack4[12], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[6], dataPack4[8], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[3], dataPack4[13], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-11

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[2], dataPack4[12], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[6], dataPack4[10], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[0], dataPack4[11], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[8], dataPack4[3], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-12

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[4], dataPack4[13], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[7], dataPack4[5], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[15], dataPack4[14], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[1], dataPack4[9], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-13

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[12], dataPack4[5], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[1], dataPack4[15], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[14], dataPack4[13], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[4], dataPack4[10], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-14

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[0], dataPack4[7], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[6], dataPack4[3], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[9], dataPack4[2], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[8], dataPack4[11], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-15

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[13], dataPack4[11], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[7], dataPack4[14], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[12], dataPack4[1], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[3], dataPack4[9], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-16

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[5], dataPack4[0], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[15], dataPack4[4], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[8], dataPack4[6], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[2], dataPack4[10], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-17

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[6], dataPack4[15], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[14], dataPack4[9], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[11], dataPack4[3], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[0], dataPack4[8], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-18

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[12], dataPack4[2], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[13], dataPack4[7], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[1], dataPack4[4], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[10], dataPack4[5], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-19

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[10], dataPack4[2], ctxS[15], ctxS[0], ctxS[5], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[8], dataPack4[4], ctxS[3], ctxS[4], ctxS[9], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[7], dataPack4[6], ctxS[7], ctxS[8], ctxS[13], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[1], dataPack4[5], ctxS[11], ctxS[12], ctxS[1], ctxS[6]);
            ctxD.CopyTo(ctxS);

            //-----------------------------------> Round-20

            this.CalculateRound(ctxD.Slice(0, 4), dataPack4[15], dataPack4[11], ctxS[7], ctxS[0], ctxS[13], ctxS[10]);
            this.CalculateRound(ctxD.Slice(4, 4), dataPack4[9], dataPack4[14], ctxS[11], ctxS[4], ctxS[1], ctxS[14]);
            this.CalculateRound(ctxD.Slice(8, 4), dataPack4[3], dataPack4[12], ctxS[15], ctxS[8], ctxS[5], ctxS[2]);
            this.CalculateRound(ctxD.Slice(12, 4), dataPack4[13], dataPack4[0], ctxS[3], ctxS[12], ctxS[9], ctxS[6]);
            ctxD.CopyTo(ctxS);


            statePack4[0] ^= ctxS[10] ^ ctxS[0];
            statePack4[1] ^= ctxS[14] ^ ctxS[4];
            statePack4[2] ^= ctxS[2] ^ ctxS[8];
            statePack4[3] ^= ctxS[6] ^ ctxS[12];
            statePack4[4] ^= ctxS[5] ^ ctxS[15];
            statePack4[5] ^= ctxS[9] ^ ctxS[3];
            statePack4[6] ^= ctxS[13] ^ ctxS[7];
            statePack4[7] ^= ctxS[1] ^ ctxS[11];
        }

        /// <summary>
        /// 添加数据计算
        /// </summary>
        /// <param name="data"></param>
        public void Update(ReadOnlySpan<byte> data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            Span<byte> ctxData = this.mData.AsSpan();

            int dataPos = 0;           //当前输入数据位置
            int dataLength = data.Length;      //当前数据长度

            if (dataLength > 64 - this.mUnAlignDataSize)
            {
                //补齐Hash数据快中剩余的数据
                data.Slice(0, 64 - this.mUnAlignDataSize).CopyTo(ctxData.Slice(this.mUnAlignDataSize));
                this.mCalculatedSize += 64;

                dataPos += 64 - this.mUnAlignDataSize;      //数据索引前移

                //检查当前数据总数是否小于最大数据块  是则记录
                if (this.mCalculatedSize < 64)
                {
                    this.mLessDataBlockSizeTimes++;
                }

                //使用Hash数据块计算一次Hash
                this.Transform(ctxData);

                //计算对齐部分数据
                while (dataLength - dataPos > 64)
                {
                    this.mCalculatedSize += 64;
                    //检查当前数据总数是否小于最大数据块  是则记录
                    if (this.mCalculatedSize < 64)
                    {
                        this.mLessDataBlockSizeTimes++;
                    }

                    //使用输入数据计算一次Hash
                    this.Transform(data.Slice(dataPos, 64));

                    dataPos += 64;   //数据索引前移
                }
                this.mUnAlignDataSize = 0;        //对齐完毕
            }

            if (dataPos < dataLength)
            {
                //添加数据到Hash数据块的尾部
                data.Slice(dataPos).CopyTo(ctxData.Slice(this.mUnAlignDataSize));
                this.mUnAlignDataSize += dataLength - dataPos;
            }

        }

        /// <summary>
        /// 获取Hash结果
        /// </summary>
        /// <param name="retValue"></param>
        /// <returns></returns>
        public bool Final(Span<byte> retValue)
        {
            int retLength = retValue.Length;
            if (this.mNeedLength == 0 || retLength < this.mNeedLength || this.mHashFinalFlag != 0)
            {
                return false;
            }

            this.mCalculatedSize += (uint)this.mUnAlignDataSize;      //剩下数据个数加到已计算部分
            if (this.mCalculatedSize < this.mUnAlignDataSize)
            {
                this.mLessDataBlockSizeTimes++;
            }

            //种子标志
            if (this.mEnableSeedFlag)
            {
                this.mSeed = 0xFFFFFFFF;
            }
            this.mHashFinalFlag = 0xFFFFFFFF;

            Span<byte> ctxData = this.mData.AsSpan();
            ctxData.Slice(this.mUnAlignDataSize).Fill(0);

            this.Transform(ctxData);

            this.mState.AsSpan().Slice(0, Math.Min(retLength, this.mState.Length)).CopyTo(retValue);

            return true;
        }

        /// <summary>
        /// 无盐构造函数
        /// </summary>
        public PbdSHA256()
        {
            this.Initialize();
        }

        /// <summary>
        /// 带盐构造函数
        /// </summary>
        /// <param name="salt">盐</param>
        public PbdSHA256(Span<byte> salt)
        {
            this.InitializeWithSalt(salt);
            this.mNeedLength = salt[0];
        }
    }
}
