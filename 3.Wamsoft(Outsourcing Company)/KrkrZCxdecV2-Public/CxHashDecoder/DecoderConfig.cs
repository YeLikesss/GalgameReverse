using System;
using System.Collections.Generic;

namespace CxHashDecoder
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public class DecoderConfig
    {
        /// <summary>
        /// 获取是否为静态模式
        /// </summary>
        public static bool IsStaticDecodeMode { get; private set; } = false;

        /// <summary>
        /// 游戏厂商
        /// </summary>
        public static string Factory { get; private set; } = null;

        /// <summary>
        /// 游戏标题
        /// </summary>
        public static string GameTitle { get; private set; } = null;

        /// <summary>
        /// 解包后资源路径
        /// </summary>
        public static string ArchivePath { get; private set; } = null;
        /// <summary>
        /// 游戏路径
        /// </summary>
        public static string GamePath { get; private set; } = null;



        /// <summary>
        /// 设置处理目录
        /// </summary>
        /// <param name="archivePath">资源文件夹</param>
        /// <param name="gamePath">游戏文件夹</param>
        public static void SetProcessPath(string archivePath, string gamePath)
        {
            ArchivePath = archivePath;
            GamePath = gamePath;
        }

        /// <summary>
        /// 设置静态参数
        /// </summary>
        /// <param name="factory">厂商</param>
        /// <param name="title">游戏标题</param>
        public static void SetStaticArgs(string factory,string title)
        {
            if (!string.IsNullOrEmpty(factory) && !string.IsNullOrEmpty(title))
            {
                Factory = factory;
                GameTitle = title;
                IsStaticDecodeMode = true;
            }
        }

    }


}
