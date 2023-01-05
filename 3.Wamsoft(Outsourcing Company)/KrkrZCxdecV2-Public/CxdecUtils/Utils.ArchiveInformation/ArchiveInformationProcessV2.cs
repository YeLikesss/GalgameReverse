using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Utils.PathProcess;
using Utils.Binary;

namespace Utils.ArchiveInformation
{
    /// <summary>
    /// 文件列表 文本格式
    /// </summary>
    public class ArchiveInformation
    {
        /// <summary>
        /// 文件夹路径
        /// </summary>
        public string DirectoryPath;
        /// <summary>
        /// 文件夹路径Hash
        /// </summary>
        public string DirectoryPathHash;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;
        /// <summary>
        /// 文件名Hash
        /// </summary>
        public string FileNameHash;

        /// <summary>
        /// 文件夹路径已解码标志
        /// </summary>
        public bool IsDirectoryDecode => this.DirectoryPath != this.DirectoryPathHash;
        /// <summary>
        /// 文件名已解码标志
        /// </summary>
        public bool IsFileNameDecode => this.FileName != this.FileNameHash;

        /// <summary>
        /// 文本扩展名
        /// </summary>
        public static string Extension => ".alst";

        /// <summary>
        /// 文件遍历参数
        /// </summary>
        public static string SearchPattern => string.Concat("*",Extension);

        /// <summary>
        /// 字符串切割标记
        /// </summary>
        public static string SplitString => "##YSig##";

        /// <summary>
        /// 换行符 (LF)
        /// </summary>
        public static string NewLine => "\n";

        /// <summary>
        /// 空字符串替换符
        /// </summary>
        public static string EmptyString => "%EmptyString%";

        /// <summary>
        /// 该结构字符串项数
        /// </summary>
        public static int StringCount => 4;
    }

    public interface IHashDecoder
    {
        /// <summary>
        /// Hash表后缀名
        /// </summary>
        public string Extension { get; }
        /// <summary>
        /// 文件遍历参数
        /// </summary>
        public string SearchPattern { get; }
        /// <summary>
        /// 获取HashDump表文件名
        /// </summary>
        /// <returns></returns>
        public string HashDumpMapFileName { get; }
        /// <summary>
        /// 解码文件表
        /// </summary>
        /// <param name="infos">文件信息数组</param>
        /// <param name="maps">字典类型 Key->Hash值(字符串型) Value->字符串原数据</param>
        public void Decode(List<ArchiveInformation> infos, Dictionary<string, string> maps);
    }

    /// <summary>
    /// 文件夹名解码
    /// </summary>
    public class DirectoryPathDecoder : IHashDecoder
    {
        private static DirectoryPathDecoder smInstance = null;
        /// <summary>
        /// 对象实例
        /// </summary>
        public static DirectoryPathDecoder Instance 
        { 
            get
            {
                smInstance ??= new();
                return smInstance;
            } 
        }

        /// <summary>
        /// 扩展名 (静态)
        /// </summary>
        public static string SExtension => ".dlst";
        public string Extension => DirectoryPathDecoder.SExtension;
        public string SearchPattern => string.Concat("*", DirectoryPathDecoder.SExtension);
        public string HashDumpMapFileName => "_dumpDirectoryHash.log";
        public void Decode(List<ArchiveInformation> infos, Dictionary<string, string> maps)
        {
            Span<ArchiveInformation> archives = CollectionsMarshal.AsSpan(infos);
            for (int i = 0; i < archives.Length; ++i)
            {
                ArchiveInformation data = archives[i];      //引用类型  无需重新赋值
                //未解码
                if (!data.IsDirectoryDecode)
                {
                    //查表
                    if (maps.TryGetValue(data.DirectoryPathHash, out string directoryPath))
                    {
                        data.DirectoryPath = directoryPath;
                    }
                }
            }
        }

    }

    public class FileNameDecoder : IHashDecoder
    {
        private static FileNameDecoder smInstance = null;
        /// <summary>
        /// 对象实例
        /// </summary>
        public static FileNameDecoder Instance
        {
            get
            {
                smInstance ??= new();
                return smInstance;
            }
        }

        /// <summary>
        /// 扩展名 (静态)
        /// </summary>
        public static string SExtension => ".flst";
        public string Extension => FileNameDecoder.SExtension;
        public string SearchPattern => string.Concat("*", FileNameDecoder.SExtension);
        public string HashDumpMapFileName => "_dumpFileHash.log";

        public void Decode(List<ArchiveInformation> infos, Dictionary<string, string> maps)
        {
            Span<ArchiveInformation> archives = CollectionsMarshal.AsSpan(infos);
            for (int i = 0; i < archives.Length; ++i)
            {
                ArchiveInformation data = archives[i];
                //未解码
                if (!data.IsFileNameDecode)
                {
                    //查表
                    if (maps.TryGetValue(data.FileNameHash, out string fileName))
                    {
                        data.FileName = fileName;
                    }
                }
            }
        }

    }

    /// <summary>
    /// Hash文本表信息
    /// </summary>
    public class HashMapInformation
    {
        /// <summary>
        /// Hash文本表文件夹名
        /// </summary>
        public static string HashMapDirectory => "Hash_Output";
        /// <summary>
        /// 获取Hash文本表全路径
        /// </summary>
        /// <param name="directoryPath">游戏根目录</param>
        /// <param name="decoder">Hash解码对象</param>
        /// <returns></returns>
        public static string GetHashMapFullPath(string directoryPath, IHashDecoder decoder)
        {
            return Path.Combine(directoryPath, HashMapDirectory, decoder.HashDumpMapFileName);
        }
    }

    /// <summary>
    /// 资源信息处理
    /// </summary>
    public class ArchiveInformationProcessV2
    {
        /// <summary>
        /// 读取文本文件表
        /// </summary>
        /// <param name="path">文件表文本全路径</param>
        /// <returns>文件信息数组</returns>
        public static List<ArchiveInformation> ReadArchiveInformation(string path)
        {
            List<ArchiveInformation> infos = new();
            Encoding encoding = new UnicodeEncoding(false, true);      //UTF-16LE (Bom自动识别)
            using StreamReader reader = new(path, encoding, false);

            while (!reader.EndOfStream)
            {
                string[] s = reader.ReadLine().Split(ArchiveInformation.SplitString, StringSplitOptions.None);
                if (s.Length == ArchiveInformation.StringCount)
                {
                    ArchiveInformation arcInfo = new()
                    {
                        //装填信息
                        DirectoryPath = s[0],
                        DirectoryPathHash = s[1],
                        FileName = s[2],
                        FileNameHash = s[3]
                    };

                    infos.Add(arcInfo);
                }
            }
            reader.Close();
            return infos;
        }

        /// <summary>
        /// 批量读取文件表
        /// </summary>
        /// <param name="directoryPath">文件表文件夹路径</param>
        /// <returns>字典类型 Key->文件列表全路径 Value->文件信息数组 </returns>
        public static Dictionary<string, List<ArchiveInformation>> ReadArchiveInformationMulti(string directoryPath)
        {
            //读取文件表
            List<string> arcInfoPaths = PathUtil.EnumerateCurrentDirectoryFullPath(directoryPath, ArchiveInformation.SearchPattern);
            Dictionary<string, List<ArchiveInformation>> arcInfoDic = new(arcInfoPaths.Count);
            foreach (string p in arcInfoPaths)
            {
                arcInfoDic.Add(p, ArchiveInformationProcessV2.ReadArchiveInformation(p));
            }
            return arcInfoDic;
        }

        /// <summary>
        /// 写入文本文件表
        /// </summary>
        /// <param name="infos">文件信息数组</param>
        /// <param name="path">文件表文本全路径</param>
        public static void WriteArchiveInformation(List<ArchiveInformation> infos, string path)
        {
            Encoding encoding = new UnicodeEncoding(false, false);      //UTF-16LE不带BOM
            using StreamWriter writer = new(path, false, encoding);
            writer.NewLine = ArchiveInformation.NewLine;

            //循环写入
            foreach(ArchiveInformation arcInfo in CollectionsMarshal.AsSpan(infos))
            {
                writer.Write(arcInfo.DirectoryPath);
                writer.Write(ArchiveInformation.SplitString);
                writer.Write(arcInfo.DirectoryPathHash);
                writer.Write(ArchiveInformation.SplitString);

                writer.Write(arcInfo.FileName);
                writer.Write(ArchiveInformation.SplitString);
                writer.Write(arcInfo.FileNameHash);
                writer.WriteLine();
            }

            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// 读取Hash映射文件
        /// </summary>
        /// <param name="path">Hash表文本全路径</param>
        /// <returns>字典类型 Key->Hash值(字符串型) Value->字符串原数据</returns>
        public static Dictionary<string, string> ReadStringHashMapFileDictionary(string path)
        {
            Dictionary<string, string> maps = new();
            Encoding encoding = new UnicodeEncoding(false, false);      //UTF-16LE (BOM自动识别)
            using StreamReader reader = new(path, encoding, false);

            while (!reader.EndOfStream)
            {
                string[] s = reader.ReadLine().Split(ArchiveInformation.SplitString, StringSplitOptions.None);
                if (s.Length == 2)
                {
                    //字符串[Sign]Hash串
                    //过滤重复字符串 不存在则添加
                    if (!maps.ContainsKey(s[1]))
                    {
                        //Hash -> 字符串映射
                        maps.Add(s[1], s[0]);
                    }
                }
            }
            reader.Close();

            return maps;
        }

        /// <summary>
        /// 批量解码文件表
        /// </summary>
        /// <param name="dictionaryPath">文件表文件夹路径</param>
        /// <param name="hashMapDirectoryPath">Hash表文件夹路径</param>
        /// <param name="decoder">hash解码器</param>
        /// <remarks>运行后会删除hash计算结果的文本文件</remarks>
        public static Dictionary<string, List<ArchiveInformation>> DecodeArchiveInformationMulti(string dictionaryPath, string hashMapDirectoryPath, IHashDecoder decoder)
        {
            //读取文件表
            Dictionary<string, List<ArchiveInformation>> arcInfoDic = ArchiveInformationProcessV2.ReadArchiveInformationMulti(dictionaryPath);

            //读取hash计算结果
            List<string> hashMapPaths = PathUtil.EnumerateCurrentDirectoryFullPath(hashMapDirectoryPath, decoder.SearchPattern);
            foreach (string p in hashMapPaths)
            {
                Dictionary<string, string> maps = ArchiveInformationProcessV2.ReadStringHashMapFileDictionary(p);      //获取hash映射表
                Parallel.ForEach(arcInfoDic.Keys, k =>
                {
                    decoder.Decode(arcInfoDic[k], maps);
                });
                File.Delete(p);       //删除计算后的临时文件
            }
            return arcInfoDic;
        }

        /// <summary>
        /// 提交文件表信息到已提取的资源
        /// </summary>
        /// <param name="infos">文件信息数组</param>
        /// <param name="directoryPath">资源文件夹目录</param>
        /// <param name="packageName">资源封包名</param>
        public static void CommitToArchiveFiles(List<ArchiveInformation> infos, string directoryPath, string packageName)
        {
            string arcPkgPath = Path.Combine(directoryPath, packageName);     //封包资源路径

            Parallel.ForEach(infos, arcInfo =>
            {
                //检查文件夹
                if (arcInfo.IsDirectoryDecode)
                {
                    //目标文件夹
                    string dir = Path.Combine(arcPkgPath, arcInfo.DirectoryPath == ArchiveInformation.EmptyString ? string.Empty : arcInfo.DirectoryPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string originalP = Path.Combine(arcPkgPath, arcInfo.DirectoryPathHash, arcInfo.FileName);
                    string newP = null;

                    if (File.Exists(originalP))
                    {
                        newP = Path.Combine(dir, arcInfo.FileName);
                    }
                    else
                    {
                        originalP = Path.Combine(arcPkgPath, arcInfo.DirectoryPathHash, arcInfo.FileNameHash);
                        if (File.Exists(originalP))
                        {
                            newP = Path.Combine(dir, arcInfo.FileNameHash);
                        }
                    }

                    if (newP != null)
                    {
                        File.Move(originalP, newP);
                    }
                }

                //检查文件名
                if (arcInfo.IsFileNameDecode)
                {
                    string dir = Path.Combine(arcPkgPath, arcInfo.DirectoryPath == ArchiveInformation.EmptyString ? string.Empty : arcInfo.DirectoryPath);

                    string originalP = Path.Combine(dir, arcInfo.FileNameHash);
                    string newP = null;

                    if (File.Exists(originalP))
                    {
                        newP = Path.Combine(dir, arcInfo.FileName);
                    }
                    else
                    {
                        dir = Path.Combine(arcPkgPath, arcInfo.DirectoryPathHash);
                        originalP = Path.Combine(dir, arcInfo.FileNameHash);

                        if (File.Exists(originalP))
                        {
                            newP = Path.Combine(dir, arcInfo.FileName);
                        }
                    }

                    if (newP != null)
                    {
                        File.Move(originalP, newP);
                    }
                }
            });
        }

        /// <summary>
        /// 提交结果到硬盘资源
        /// </summary>
        /// <param name="directoryPath">资源文件夹目录</param>
        /// <param name="pkgArcListsMap">封包与文件列表数组的映射字典  Key->文件表文本全路径 Value->文件列表信息数组</param>
        public static void CommitToHardDrive(string directoryPath, Dictionary<string, List<ArchiveInformation>> pkgArcListsMap)
        {
            foreach (var list in pkgArcListsMap)
            {
                string pkgName = Path.GetFileNameWithoutExtension(list.Key);
                ArchiveInformationProcessV2.WriteArchiveInformation(list.Value, list.Key);
                ArchiveInformationProcessV2.CommitToArchiveFiles(list.Value, directoryPath, pkgName);
                PathUtil.DeleteEmptyDirectory(Path.Combine(directoryPath, pkgName));
            }
        }

        /// <summary>
        /// 备份已解码Hash文本
        /// </summary>
        /// <param name="directoryPath">文件表根目录</param>
        public static void BackupDecodedHashMap(string directoryPath)
        {
            //获取文件表
            Dictionary<string, List<ArchiveInformation>> arcListMaps = ArchiveInformationProcessV2.ReadArchiveInformationMulti(directoryPath);
            string outPutDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup_Output");

            if (!Directory.Exists(outPutDir))
            {
                Directory.CreateDirectory(outPutDir);
            }

            //UTF-16LE With Bom CRLF输出Hash表
            using StreamWriter swDirectoryMapOut = new(Path.Combine(outPutDir, "DirectoryPathHashMap" + DirectoryPathDecoder.SExtension), false, Encoding.Unicode);
            using StreamWriter swFileNameMapOut = new(Path.Combine(outPutDir, "FileNameHashMap" + FileNameDecoder.SExtension), false, Encoding.Unicode);
            //UTF-16LE With Bom CRLF输出文本
            using StreamWriter swDirectoryOut = new(Path.Combine(outPutDir, "DirectoryPath.txt"), false, Encoding.Unicode);
            using StreamWriter swFileNameOut = new(Path.Combine(outPutDir, "FileName.txt"), false, Encoding.Unicode);

            Dictionary<string, string> directoryHashMap = new(256);
            Dictionary<string, string> fileNameHashMap = new(1024);

            foreach (List<ArchiveInformation> arcInfos in arcListMaps.Values)
            {
                foreach (ArchiveInformation arcInfo in CollectionsMarshal.AsSpan(arcInfos))
                {
                    //路径
                    if (arcInfo.IsDirectoryDecode)
                    {
                        if (directoryHashMap.TryAdd(arcInfo.DirectoryPath, arcInfo.DirectoryPathHash))
                        {
                            if (arcInfo.DirectoryPath == ArchiveInformation.EmptyString)
                            {
                                swDirectoryOut.WriteLine(string.Empty);
                            }
                            else
                            {
                                swDirectoryOut.WriteLine(arcInfo.DirectoryPath);
                            }
                        }
                    }
                    //文件名
                    if (arcInfo.IsFileNameDecode)
                    {
                        if(fileNameHashMap.TryAdd(arcInfo.FileName, arcInfo.FileNameHash))
                        {
                            swFileNameOut.WriteLine(arcInfo.FileName);
                        }
                    }
                }
            }
            
            foreach(var hashMap in directoryHashMap)
            {
                swDirectoryMapOut.Write(hashMap.Key);
                swDirectoryMapOut.Write(ArchiveInformation.SplitString);
                swDirectoryMapOut.WriteLine(hashMap.Value);
            }
            foreach (var hashMap in fileNameHashMap)
            {
                swFileNameMapOut.Write(hashMap.Key);
                swFileNameMapOut.Write(ArchiveInformation.SplitString);
                swFileNameMapOut.WriteLine(hashMap.Value);
            }

            swDirectoryMapOut.Flush();
            swDirectoryMapOut.Close();
            swFileNameMapOut.Flush();
            swFileNameMapOut.Close();
            swDirectoryOut.Flush();
            swDirectoryOut.Close();
            swFileNameOut.Flush();
            swFileNameOut.Close();
        }
    }
}
