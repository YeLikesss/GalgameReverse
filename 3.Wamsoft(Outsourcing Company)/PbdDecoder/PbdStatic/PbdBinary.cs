using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using K4os.Compression.LZ4.Streams;
using TJS;

namespace PbdStatic
{
    /// <summary>
    /// pbd立绘信息二进制流
    /// </summary>
    public class PbdBinary
    {
        /// <summary>
        /// 二进制数据
        /// </summary>
        private byte[] mData;
        /// <summary>
        /// pbd信息
        /// </summary>
        private PbdInformation mPbdInformation;

        public bool TryGetTJSVariantObject(out TJSVariant tjsGalleryTable)
        {
            using MemoryStream tableStream = new(this.mData, false);

            Span<byte> checkSum = stackalloc byte[4];

            PbdByteChecker checker = PbdByteChecker.Create(this.mPbdInformation);
            TJSDeserializer deserializer = new(tableStream, this.mPbdInformation.IsBigEndian, checker.IsVaildByte);
            tjsGalleryTable = deserializer.ReadSingleObject();

            tableStream.Read(checkSum);
            if (this.mPbdInformation.IsBigEndian)
            {
                checkSum.Reverse();
            }

            return checker.IsVaildFinal(BitConverter.ToUInt32(checkSum));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="binaryData">pbd立绘信息二进制流</param>
        /// <param name="pbdInfo">pbd信息</param>
        private PbdBinary(byte[] binaryData, PbdInformation pbdInfo)
        {
            this.mData = binaryData;
            this.mPbdInformation = pbdInfo;
        }

        /// <summary>
        /// 使用pbd全路径创建Pbd二进制流
        /// </summary>
        /// <param name="pbdFullPath">文件全路径</param>
        /// <param name="keyInformation">游戏信息</param>
        /// <returns></returns>
        public static PbdBinary Create(string pbdFullPath, IKeyInformation keyInformation)
        {
            using FileStream fs = File.OpenRead(pbdFullPath);
            return PbdBinary.Create(fs, keyInformation);
        }

        /// <summary>
        /// 使用二进制流创建Pbd二进制流
        /// </summary>
        /// <param name="pbdStream">流</param>
        /// <param name="keyInformation">游戏信息</param>
        /// <returns></returns>
        public static PbdBinary Create(Stream pbdStream, IKeyInformation keyInformation)
        {
            //获取pbd信息
            PbdInformation pbdInfo = PbdInformation.Create(pbdStream, keyInformation.CustomIV);
            if (pbdInfo == null)
            {
                return null;
            }

            //获取数据
            byte[] rawData = new byte[pbdStream.Length - pbdInfo.PbdHeaderSize];
            pbdStream.Position = pbdInfo.PbdHeaderSize;
            pbdStream.Read(rawData);

            //解密数据
            PbdCryptoFilter filter = PbdCryptoFilter.Create(pbdInfo, keyInformation.CryptoSmallTable);
            filter?.Decrypt(rawData);

            if (pbdInfo.CompressFlag != 0)
            {
                rawData = PbdBinary.Lz4Decompress(rawData);
            }

            //判断是否解压失败
            if (rawData == null)
            {
                return null;
            }

            return new(rawData, pbdInfo);
        }

        /// <summary>
        /// Lz4解压缩
        /// </summary>
        /// <param name="rawData">原数据</param>
        /// <returns></returns>
        public static byte[] Lz4Decompress(byte[] rawData)
        {
            byte[] dictionaryBuffer = ArrayPool<byte>.Shared.Rent(0x100000);    //1MB
            byte[] compressBuffer = ArrayPool<byte>.Shared.Rent(0x100000);      //1MB
            byte[] decompressBuffer = ArrayPool<byte>.Shared.Rent(0x100000);    //1MB

            //原始数据
            MemoryStream outputData = new(0x100000);          //1MB

            using MemoryStream dataMs = new(rawData, false)
            {
                Position = 0
            };
            using BinaryReader dataBr = new(dataMs);

            Span<byte> dictionaryMem = dictionaryBuffer;
            Span<byte> compressMem = compressBuffer;
            Span<byte> decompressMem = decompressBuffer;

            int compressLength = 0;     //当前块压缩长度
            int decodeLength = 0;       //当前块解压缩长度

            try
            {
                while (dataMs.Position < dataMs.Length)
                {
                    compressLength = dataBr.ReadUInt16();       //读取长度(2字节)

                    Span<byte> encMem = compressMem[..compressLength];

                    dataMs.Read(encMem);
                    //Lz4压缩
                    decodeLength = LZ4Codec.Decode(encMem, decompressMem, dictionaryMem[..decodeLength]);

                    Span<byte> decMem = decompressMem[..decodeLength];
                    outputData.Write(decMem);

                    //当前结果作为下一次解压字典
                    decMem.CopyTo(dictionaryMem[..decodeLength]);
                }

                outputData.Position = 0;
                return outputData.ToArray();
            }
            catch(Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
                outputData.Dispose();
                return null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(dictionaryBuffer);
                ArrayPool<byte>.Shared.Return(compressBuffer);
                ArrayPool<byte>.Shared.Return(decompressBuffer);
            }
        }
    }
}
