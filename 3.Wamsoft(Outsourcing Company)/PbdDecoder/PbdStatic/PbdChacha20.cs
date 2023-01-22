using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PbdStatic
{
    /// <summary>
    /// Pbd二进制立绘文件Chacha20加密
    /// </summary>
    internal class PbdChacha20
    {
        /// <summary>
        /// 小加密表 16字节
        /// </summary>
        private byte[] mSmallTable;

        /// <summary>
        /// 加密块
        /// </summary>
        private byte[] mCryptoBlock;
        /// <summary>
        /// 当前未使用块的起始位置
        /// </summary>
        private int mBlockPosition;
        /// <summary>
        /// 块数量  以64字节为一块计算
        /// </summary>
        private int mBlockCount;
        /// <summary>
        /// 种子
        /// </summary>
        private uint mSeed;
        /// <summary>
        /// 加密轮次数
        /// </summary>
        private int mRound;
        /// <summary>
        /// 加密状态变量
        /// </summary>
        private byte[] mState;
        /// <summary>
        /// Key
        /// </summary>
        private ulong mKey;

        /// <summary>
        /// 变换一次块
        /// </summary>
        private void TransformBlock()
        {
            Span<byte> ctxS = stackalloc byte[64];
            this.mState.CopyTo(ctxS);

            {
                Span<ulong> ctxSPack8 = MemoryMarshal.Cast<byte, ulong>(ctxS);
                ctxSPack8[6] = ~this.mKey;
                ++this.mKey;
            }
            Span<byte> block = this.mCryptoBlock;
            this.TransformState(block, ctxS);       //变换64字节
            int blockCount = this.mBlockCount;
            //块大于64字节
            if (blockCount > 1)
            {
                Span<byte> lastBlock = block[64..];

                Span<uint> blockPack4 = MemoryMarshal.Cast<byte, uint>(block);
                Span<uint> lastBlockPack4 = MemoryMarshal.Cast<byte, uint>(lastBlock);

                for(int i = 0; i < (blockCount - 1) * 16; ++i)
                {
                    uint s = blockPack4[i];
                    s = (((s << 13) ^ s) >> 17) ^ (s << 13) ^ s;
                    s = (32 * s) ^ s;
                    s = s == 0 ? this.mSeed : s;
                    lastBlockPack4[i] = s;
                }
            }

        }

        /// <summary>
        /// 变换一次加密状态
        /// </summary>
        /// <param name="output">输出缓冲区 (64字节)</param>
        /// <param name="input">输入缓冲区 (64字节)</param>
        private void TransformState(Span<byte> output, Span<byte> input)
        {
            Span<byte> buffer = stackalloc byte[64];

            input.CopyTo(buffer);

            //进行一次反转
            for (int i = 0; i < 64; ++i)
            {
                buffer[i] = (byte)~buffer[i];
            }

            Span<uint> inputPack4 = MemoryMarshal.Cast<byte, uint>(input);
            Span<uint> srcPack4 = MemoryMarshal.Cast<byte, uint>(buffer);
            Span<uint> destPack4 = MemoryMarshal.Cast<byte, uint>(output);

            uint z0, z1, z2, z3, z4, z5, z6, z7, z8, z9, za, zb, zc, zd, ze, zf;
            z0 = srcPack4[0];
            z1 = srcPack4[1];
            z2 = srcPack4[2];
            z3 = srcPack4[3];
            z4 = srcPack4[4];
            z5 = srcPack4[5];
            z6 = srcPack4[6];
            z7 = srcPack4[7];
            z8 = srcPack4[8];
            z9 = srcPack4[9];
            za = srcPack4[10];
            zb = srcPack4[11];
            zc = srcPack4[12];
            zd = srcPack4[13];
            ze = srcPack4[14];
            zf = srcPack4[15];

            int round = ((this.mRound - 1) >> 1) + 1;

            for (int i = 0; i < round; ++i)
            {
                // QUARTER(z0, z4, z8, zc);
                z0 += z4; zc = BitOperations.RotateLeft(zc ^ z0, 16);
                z8 += zc; z4 = BitOperations.RotateLeft(z4 ^ z8, 12);
                z0 += z4; zc = BitOperations.RotateLeft(zc ^ z0, 8);
                z8 += zc; z4 = BitOperations.RotateLeft(z4 ^ z8, 7);
                // QUARTER(z1, z5, z9, zd);
                z1 += z5; zd = BitOperations.RotateLeft(zd ^ z1, 16);
                z9 += zd; z5 = BitOperations.RotateLeft(z5 ^ z9, 12);
                z1 += z5; zd = BitOperations.RotateLeft(zd ^ z1, 8);
                z9 += zd; z5 = BitOperations.RotateLeft(z5 ^ z9, 7);
                // QUARTER(z2, z6, za, ze);
                z2 += z6; ze = BitOperations.RotateLeft(ze ^ z2, 16);
                za += ze; z6 = BitOperations.RotateLeft(z6 ^ za, 12);
                z2 += z6; ze = BitOperations.RotateLeft(ze ^ z2, 8);
                za += ze; z6 = BitOperations.RotateLeft(z6 ^ za, 7);
                // QUARTER(z3, z7, zb, zf);
                z3 += z7; zf = BitOperations.RotateLeft(zf ^ z3, 16);
                zb += zf; z7 = BitOperations.RotateLeft(z7 ^ zb, 12);
                z3 += z7; zf = BitOperations.RotateLeft(zf ^ z3, 8);
                zb += zf; z7 = BitOperations.RotateLeft(z7 ^ zb, 7);
                // QUARTER(z0, z5, za, zf);
                z0 += z5; zf = BitOperations.RotateLeft(zf ^ z0, 16);
                za += zf; z5 = BitOperations.RotateLeft(z5 ^ za, 12);
                z0 += z5; zf = BitOperations.RotateLeft(zf ^ z0, 8);
                za += zf; z5 = BitOperations.RotateLeft(z5 ^ za, 7);
                // QUARTER(z1, z6, zb, zc);
                z1 += z6; zc = BitOperations.RotateLeft(zc ^ z1, 16);
                zb += zc; z6 = BitOperations.RotateLeft(z6 ^ zb, 12);
                z1 += z6; zc = BitOperations.RotateLeft(zc ^ z1, 8);
                zb += zc; z6 = BitOperations.RotateLeft(z6 ^ zb, 7);
                // QUARTER(z2, z7, z8, zd);
                z2 += z7; zd = BitOperations.RotateLeft(zd ^ z2, 16);
                z8 += zd; z7 = BitOperations.RotateLeft(z7 ^ z8, 12);
                z2 += z7; zd = BitOperations.RotateLeft(zd ^ z2, 8);
                z8 += zd; z7 = BitOperations.RotateLeft(z7 ^ z8, 7);
                // QUARTER(z3, z4, z9, ze);
                z3 += z4; ze = BitOperations.RotateLeft(ze ^ z3, 16);
                z9 += ze; z4 = BitOperations.RotateLeft(z4 ^ z9, 12);
                z3 += z4; ze = BitOperations.RotateLeft(ze ^ z3, 8);
                z9 += ze; z4 = BitOperations.RotateLeft(z4 ^ z9, 7);
            }

            destPack4[0] = z0;
            destPack4[1] = z1;
            destPack4[2] = z2;
            destPack4[3] = z3;
            destPack4[4] = z4;
            destPack4[5] = z5;
            destPack4[6] = z6;
            destPack4[7] = z7;
            destPack4[8] = z8;
            destPack4[9] = z9;
            destPack4[10] = za;
            destPack4[11] = zb;
            destPack4[12] = zc;
            destPack4[13] = zd;
            destPack4[14] = ze;
            destPack4[15] = zf;

            for (int i = 0; i < 16; ++i)
            {
                destPack4[i] += ~inputPack4[i];
            }
        }

        /// <summary>
        /// 初始化块
        /// </summary>
        private void InitializeBlock()
        {
            this.mCryptoBlock = new byte[this.mBlockCount * 64];
            this.mBlockPosition = this.mCryptoBlock.Length;
        }

        /// <summary>
        /// 初始化加密环境变量
        /// </summary>
        /// <param name="key">32字节 key</param>
        /// <param name="seed1">种子1</param>
        /// <param name="seed2">种子2</param>
        /// <param name="seed3">种子3</param>
        /// <param name="seed4">种子4</param>
        private void InitializeState(Span<byte> key, uint seed1, uint seed2, uint seed3, uint seed4)
        {
            this.mState = new byte[64];

            Span<byte> state = this.mState;
            Span<uint> statePack4 = MemoryMarshal.Cast<byte, uint>(state);

            this.mSmallTable.CopyTo(state.Slice(0, 16));
            for(int i = 16; i < 48; ++i)
            {
                state[i] = (byte)~key[i - 16];
            }

            statePack4[14] = ~seed1;
            statePack4[15] = ~seed2;
            statePack4[12] = ~seed3;
            statePack4[13] = ~seed4;
        }

        /// <summary>
        /// 加密初始化
        /// </summary>
        /// <param name="seed">种子</param>
        /// <param name="iv">加密向量</param>
        /// <param name="round">加密轮次数</param>
        /// <param name="blockCount">块数量</param>
        public void Initialize(uint seed, Span<byte> iv, int round, int blockCount)
        {
            Span<byte> key = stackalloc byte[32];

            //生成key
            {
                Span<byte> salt = stackalloc byte[32];
                salt[0] = 0x20;
                salt[1] = 0x04;
                salt[2] = 0x01;
                salt[3] = 0x01;

                Span<byte> seedArray = stackalloc byte[64];
                Span<uint> seedArrayPack4 = MemoryMarshal.Cast<byte, uint>(seedArray);
                seedArrayPack4[0] = seed;

                PbdSHA256 sha256 = new(salt);
                sha256.Update(seedArray);
                sha256.Update(iv);
                sha256.Final(key);
            }

            this.mKey = 0;
            this.mSeed = 0xFFFFFFFF;
            this.mRound = round;
            this.mBlockCount = blockCount;

            uint s = PbdChacha20.GenerateSeed(iv, seed);
            this.InitializeState(key, s, seed, 0, 0);
            this.InitializeBlock();

            if (blockCount > 1)
            {
                if (s != seed)
                {
                    this.mSeed = s ^ seed;
                }
                else if (seed != 0)
                {
                    this.mSeed = seed;
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">数据</param>
        public void Decrypt(Span<byte> data)
        {
            for(int i = 0; i < data.Length; ++i)
            {
                if (this.mBlockPosition >= this.mCryptoBlock.Length)
                {
                    this.TransformBlock();
                    this.mBlockPosition = 0;
                }
                data[i] ^= this.mCryptoBlock[this.mBlockPosition];
                ++this.mBlockPosition;
            }
        }

        /// <summary>
        /// chacha20构造函数
        /// </summary>
        /// <param name="smallTable">16字节小表</param>
        public PbdChacha20(byte[] smallTable)
        {
            this.mSmallTable = smallTable;
        }

        /// <summary>
        /// 生成种子
        /// </summary>
        /// <param name="iv">加密向量</param>
        /// <param name="seed">种子</param>
        /// <returns></returns>
        public static uint GenerateSeed(Span<byte> iv,uint seed)
        {
            int ivLen = iv.Length;
            int pos = 0;

            uint key;

            //16字节对齐
            if (ivLen < 16)
            {
                key = seed + 0x165667B1;
            }
            else
            {
                uint a = seed;
                uint b = seed + 0x24234428;
                uint c = seed - 0x7A143589;
                uint d = seed + 0x61C8864F;

                Span<byte> alignIV = iv.Slice(0, ivLen / 16 * 16);

                do
                {
                    Span<uint> ivPack4 = MemoryMarshal.Cast<byte, uint>(alignIV.Slice(pos, 16));

                    b = 0x9E3779B1 * BitOperations.RotateLeft(b - 0x7A143589 * ivPack4[0], 13);
                    c = 0x9E3779B1 * BitOperations.RotateLeft(c - 0x7A143589 * ivPack4[1], 13);
                    a = 0x9E3779B1 * BitOperations.RotateLeft(a - 0x7A143589 * ivPack4[2], 13);
                    d = 0x9E3779B1 * BitOperations.RotateLeft(d - 0x7A143589 * ivPack4[3], 13);

                    pos += 16;
                }
                while (pos <= ivLen - 16);
                key = BitOperations.RotateLeft(b, 1) + BitOperations.RotateLeft(c, 7) + BitOperations.RotateLeft(a, 12) + BitOperations.RotateLeft(d, 18);
            }
            key += (uint)ivLen;

            //4字节对齐
            while (pos <= ivLen - 4)
            {
                key = 0x27D4EB2F * BitOperations.RotateLeft(key - 0x3D4D51C3 * BitConverter.ToUInt32(iv.Slice(pos, 4)), 17);
                pos += 4;
            }

            //1字节对齐
            while (pos < ivLen)
            {
                key = 0x9E3779B1 * BitOperations.RotateLeft(key + 0x165667B1 * (uint)iv[pos], 11);
                ++pos;
            }

            key = 0xC2B2AE3D * ((0x85EBCA77 * (key ^ (key >> 15))) ^ ((0x85EBCA77 * (key ^ (key >> 15))) >> 13));

            return key ^ (key >> 16);
        }
    }
}
