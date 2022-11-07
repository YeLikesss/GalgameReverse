using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace NekoNyanStatic.Crypto.V1
{
    /// <summary>
    /// 封包加密类
    /// </summary>
    public class ArchiveCrypto
    {
        /// <summary>
        /// 文件表
        /// </summary>
        public struct FileEntry
        {
            /// <summary>
            /// 文件名
            /// </summary>
            public string FileName;
            /// <summary>
            /// 文件偏移
            /// </summary>
            public uint Offset;
            /// <summary>
            /// 文件大小
            /// </summary>
            public uint Size;
            /// <summary>
            /// Key
            /// </summary>
            public uint Key;
        }


        private FileStream mFileStream;
        private List<ArchiveCrypto.FileEntry> mFileEntries;         //文件表数组

        private string mPackageName;        //封包名字
        private string mExtractDir;          //提取路径

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            this.mFileStream.Position = 0;

            //读取并解密原封包信息
            Span<byte> rawPkgInfo = stackalloc byte[1024];
            this.mFileStream.Read(rawPkgInfo);

            int fileCount = 0;              //文件个数
            uint rawFileEntryKey = 0;        //原始文件表Key
            uint rawFileNamesKey = 0;        //原始文件名Key

            //解密封包信息
            {
                Span<int> rawPkgInfoPack4 = MemoryMarshal.Cast<byte, int>(rawPkgInfo);
                //获得文件个数
                for (int i = 4; i < 255; ++i)
                {
                    fileCount += rawPkgInfoPack4[i];
                }
                //获得原始文件表Key
                rawFileEntryKey = (uint)rawPkgInfoPack4[53];
                //获得原始文件名Key
                rawFileNamesKey = (uint)rawPkgInfoPack4[23];
            }

            //读取并解密原文件表信息
            byte[] rawEntryData = new byte[16 * fileCount];
            this.mFileStream.Read(rawEntryData);
            this.Decrypt(rawEntryData, rawFileEntryKey);

            //读取并解密原文件名信息
            byte[] rawFileNamesData = new byte[BitConverter.ToInt32(rawEntryData, 12) - (1024 + rawEntryData.Length)];
            this.mFileStream.Read(rawFileNamesData);
            this.Decrypt(rawFileNamesData, rawFileNamesKey);

            this.ParserFileEntry(rawEntryData, rawFileNamesData, fileCount);
        }

        /// <summary>
        /// 获得文件表
        /// </summary>
        /// <param name="rawEntryData">原文件表信息</param>
        /// <param name="rawFileNamesData">原文件名信息</param>
        /// <param name="fileCount">文件个数</param>
        private void ParserFileEntry(Span<byte> rawEntryData,Span<byte> rawFileNamesData,int fileCount)
        {
            Span<uint> rawEntryDataPack4 = MemoryMarshal.Cast<byte, uint>(rawEntryData);
            this.mFileEntries = new(fileCount);
            for (int i = 0; i < fileCount; ++i)
            {
                int pos = 4 * i;
                ArchiveCrypto.FileEntry entry = new()
                {
                    Size = rawEntryDataPack4[pos + 0],
                    Key = rawEntryDataPack4[pos + 2],
                    Offset = rawEntryDataPack4[pos + 3]
                };

                //获得文件名偏移与长度
                int fileNameOffset = (int)rawEntryDataPack4[pos + 1];
                int fileNameLen = rawFileNamesData.Slice(fileNameOffset).IndexOf((byte)0x00);

                //获得文件名
                entry.FileName = Encoding.ASCII.GetString(rawFileNamesData.Slice(fileNameOffset, fileNameLen));

                this.mFileEntries.Add(entry);
            }
        }


        /// <summary>
        /// 生成key  256字节长度
        /// </summary>
        /// <param name="tablePtr">表指针</param>
        /// <param name="key">key</param>
        private void KeyGenerator(Span<byte> tablePtr, uint key)
        {
            uint k1 = key * 0x00001CDF + 0x0000A74C;
            uint k2 = k1 << 0x11 ^ k1;

            for(int i = 0; i < 256; ++i)
            {
                k1 = k1 - key + k2;
                k2 = k1 + 0x38;
                k1 *= k2 & 0xEF;
                tablePtr[i] = (byte)k1;
                k1 >>= 1;
            }

        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="key">解密Key</param>
        private void Decrypt(Span<byte> data, uint key)
        {
            Span<byte> table = stackalloc byte[256];
            this.KeyGenerator(table, key);
            for (int i = 0; i < data.Length; ++i)
            {
                byte temp = data[i];
                temp ^= table[i % 253];
                temp += 0x03;
                temp += table[i % 89];
                temp ^= 0x99;
                data[i] = temp;
            }
        }

        /// <summary>
        /// 提取资源
        /// </summary>
        public void Extract()
        {

            foreach(ArchiveCrypto.FileEntry entry in CollectionsMarshal.AsSpan(this.mFileEntries))
            {
                string extractPath = Path.Combine(this.mExtractDir, entry.FileName);
                string archiveDir = Path.GetDirectoryName(extractPath);
                if (!Directory.Exists(archiveDir))
                {
                    Directory.CreateDirectory(archiveDir);
                }

                //读取并解密资源
                this.mFileStream.Position = entry.Offset;
                byte[] buffer = ArrayPool<byte>.Shared.Rent((int)entry.Size);

                Span<byte> data = buffer.AsSpan(0, (int)entry.Size);
                this.mFileStream.Read(data);
                this.Decrypt(data, entry.Key);

                //回写解密后资源
                using FileStream outStream = new(extractPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                outStream.Write(data);
                outStream.Flush();

                //释放
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }


        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            this.mFileEntries.Clear();

            this.mFileStream.Close();
            this.mFileStream.Dispose();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pkgPath">封包全路径</param>
        public ArchiveCrypto(string pkgPath)
        {
            this.mFileStream = File.OpenRead(pkgPath);
            this.mPackageName = Path.GetFileNameWithoutExtension(pkgPath);
            this.mExtractDir = Path.Combine(Path.GetDirectoryName(pkgPath), "Extract", this.mPackageName);
            this.Initialize();
        }



        /// <summary>
        /// 枚举封包路径
        /// </summary>
        /// <param name="dirPath">封包文件夹路径</param>
        /// <returns></returns>
        public static IEnumerable<string> EnumeratePackagePaths(string dirPath)
        {
            return Directory.EnumerateFiles(dirPath, "*.dat");
        }
        /// <summary>
        /// 检查是否合法封包
        /// </summary>
        /// <param name="path">封包路径</param>
        /// <returns></returns>
        public static bool IsVaildPackage(string path)
        {
            return Path.GetExtension(path) == ".dat";
        }
    }
}
