using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Utils.PathProcess;
using Utils.ArchiveInformation;

namespace CxHashDecoder
{
    public partial class HashDecoderForm : Form
    {
        /// <summary>
        /// Hash计算器模式
        /// </summary>
        protected enum HasherMode
        {
            /// <summary>
            /// 文件夹路径
            /// </summary>
            DirectoryMode = 1,
            /// <summary>
            /// 文件名模式
            /// </summary>
            FileMode = 2,
        }
        /// <summary>
        /// 处理进度
        /// </summary>
        protected enum ProcessStatus
        {
            /// <summary>
            /// 准备就绪
            /// </summary>
            Ready,
            /// <summary>
            /// Hash解码中
            /// </summary>
            Decoding,
            /// <summary>
            /// 提交到磁盘
            /// </summary>
            CommitToDisk
        }

        private Dictionary<ProcessStatus, string> mStatusMap = new(5);      //状态绑定
        private IHashDecoder mHashDecoder = null;       //hash解码器
        private HasherMode mHasherMode = HasherMode.FileMode;     //当前hash计算模式

        /// <summary>
        /// 设置处理UI可用性
        /// </summary>
        /// <param name="enable"></param>
        private void SetProcessUIEnable(bool enable)
        {
            this.mTextViewer.Enabled = enable;
            this.mTextCreator.Enabled = enable;
            this.btnPathEnumerator.Enabled = enable;
            this.btnUseHashDump.Enabled = enable;
            this.btnBackupDecodedName.Enabled = enable;
            this.btnHashModeSwitch.Enabled = enable;
        }

        /// <summary>
        /// 设置处理状态的UI
        /// </summary>
        /// <param name="status"></param>
        private void SetProcessUIStatus(ProcessStatus status)
        {
            this.lbStatus.Text = this.mStatusMap[status];
        }

        /// <summary>
        /// 检查Hash处理例程合法性
        /// </summary>
        /// <returns></returns>
        private bool IsHasherProcessVaild()
        {
            DynamicProcess dp = DynamicProcess.Instance;
            return DecoderConfig.IsStaticDecodeMode || (dp.IsDynamicMode() && !dp.IsExit());
        }


        /// <summary>
        /// Hash处理进程
        /// </summary>
        private void HasherProcess(object callback)
        {
            if (this.IsHasherProcessVaild())
            {
                this.Invoke(this.SetProcessUIEnable, false);         //锁定UI
                this.BeginInvoke(this.SetProcessUIStatus, ProcessStatus.Decoding);
                //获取文件表
                Dictionary<string, List<ArchiveInformation>> arcListMaps = ArchiveInformationProcessV2.ReadArchiveInformationMulti(DecoderConfig.ArchivePath);

                //文本处理
                if (callback is Func<IEnumerable<List<string>>> textProc)
                {
                    //生成文本并计算hash解码
                    foreach (var textLst in textProc())
                    {
                        if (DecoderConfig.IsStaticDecodeMode)
                        {
                            //CxEncrypt.Hash.IStringHasher stringHasher = null;
                            //if (this.mHasherMode == HasherMode.DirectoryMode)
                            //{
                            //    stringHasher = CxEncrypt.Hash.DirectoryNameHasher.Instance;
                            //}
                            //else if (this.mHasherMode == HasherMode.FileMode)
                            //{
                            //    stringHasher = CxEncrypt.Hash.FileNameHasher.Instance;
                            //}

                            ////获得游戏信息并计算
                            //CxEncrypt.Games.GameInfomation gi = CxEncrypt.Games.DataManager.GetFactoryMaps()[DecoderConfig.Factory][DecoderConfig.GameTitle];
                            //var maps = stringHasher.CalculateMulit(textLst, gi.HashSeed, gi.HashSalt);

                            //Parallel.ForEach(arcListMaps.Keys, s =>
                            //{
                            //    this.mHashDecoder.Decode(arcListMaps[s], maps);
                            //});
                        }
                        else
                        {
                            if (!DynamicProcess.Instance.IsExit())
                            {
                                foreach (var maps in DynamicProcess.Instance.RemoteHashCalculate(textLst, (uint)this.mHasherMode))
                                {
                                    foreach (var infos in arcListMaps.Values)
                                    {
                                        this.mHashDecoder.Decode(infos, maps);
                                    }
                                }
                            }
                            else { break; }
                        }
                    }
                }

                //回写文件表到磁盘  刷新资源文件系统
                this.BeginInvoke(this.SetProcessUIStatus, ProcessStatus.CommitToDisk);

                ArchiveInformationProcessV2.CommitToHardDrive(DecoderConfig.ArchivePath, arcListMaps);

                this.BeginInvoke(this.SetProcessUIStatus, ProcessStatus.Ready);        //恢复准备就绪状态
                this.BeginInvoke(this.SetProcessUIEnable, true);         //解锁UI
            }
        }

        private string mSelectPath;
        /// <summary>
        /// 获取本地路径或文件名
        /// </summary>
        /// <returns></returns>
        private IEnumerable<List<string>> GetLocalPathOrNameString()
        {
            List<string> texts = new(0);
            //判断模式
            if (this.mHasherMode == HasherMode.DirectoryMode)
            {
                texts = PathUtil.EnumerateKirikiriRelativeDirectory(this.mSelectPath);
            }
            else if (this.mHasherMode == HasherMode.FileMode)
            {
                texts = PathUtil.EnumerateKirikiriFileName(this.mSelectPath);
            }
            yield return texts;
        }


        /// <summary>
        /// 选择路径
        /// </summary>
        private string SelectPath()
        {
            string path = null;
            FolderBrowserDialog dialog = new()
            {
                Description = "请选择文件夹",
                ShowNewFolderButton = false,
                AutoUpgradeEnabled = true,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.SelectedPath;
            }
            return path;
        }

        /// <summary>
        /// 设置Hash计算模式
        /// </summary>
        /// <param name="mode"></param>
        private void SetHashMode(HasherMode mode)
        {
            this.mHasherMode = mode;
            string title = string.Empty;
            if (mode == HasherMode.DirectoryMode)
            {
                this.mHashDecoder = DirectoryPathDecoder.Instance;
                title = "DirectoryHashDecoder - 文件夹路径Hash解码";
                this.btnHashModeSwitch.Text = "文件夹路径模式";
            }
            else if (mode == HasherMode.FileMode)
            {
                this.mHashDecoder = FileNameDecoder.Instance;
                title = "FileNameHashDecoder - 文件名Hash解码";
                this.btnHashModeSwitch.Text = "文件名模式";
            }

            if (DecoderConfig.IsStaticDecodeMode)
            {
                title += " - 静态模式";
            }
            else
            {
                title += " - 动态模式";
            }
            this.Text = title;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            this.InitializeEnvironment();
            this.SetHashMode(HasherMode.FileMode);      //默认为文件名模式

            this.mTextViewer.TextProcessCallBack = this.HasherProcess;
            this.mTextCreator.TextProcessCallBack = this.HasherProcess;
        }

        /// <summary>
        /// 初始化环境
        /// </summary>
        private void InitializeEnvironment()
        {
            this.mStatusMap.Add(ProcessStatus.Ready, "准备就绪");
            this.mStatusMap.Add(ProcessStatus.Decoding, "Hash解码中");
            this.mStatusMap.Add(ProcessStatus.CommitToDisk, "提交到磁盘资源文件");
        }

        public HashDecoderForm()
        {
            InitializeComponent();
        }

        private void HashDecoderForm_Load(object sender, EventArgs e)
        {
            this.Initialize();
            this.SetProcessUIStatus(ProcessStatus.Ready);
            this.SetProcessUIEnable(true);
        }

        private void btnPathEnumerator_Click(object sender, EventArgs e)
        {
            string path = this.SelectPath();
            if (path != null)
            {
                this.mSelectPath = path;
                new Thread(new ParameterizedThreadStart(this.HasherProcess)).Start(new Func<IEnumerable<List<string>>>(this.GetLocalPathOrNameString));
            }
        }

        private void btnUseHashDump_Click(object sender, EventArgs e)
        {
            string path = HashMapInformation.GetHashMapFullPath(DecoderConfig.GamePath, this.mHashDecoder);
            if (File.Exists(path))
            {
                this.SetProcessUIEnable(false);     //UI锁定
                this.SetProcessUIStatus(ProcessStatus.Decoding);

                //获取文件表
                Dictionary<string, List<ArchiveInformation>> arcListMaps = ArchiveInformationProcessV2.ReadArchiveInformationMulti(DecoderConfig.ArchivePath);

                //获取Hash映射表
                Dictionary<string, string> hashMaps = ArchiveInformationProcessV2.ReadStringHashMapFileDictionary(path);

                //解码
                Parallel.ForEach(arcListMaps.Keys, s =>
                {
                    this.mHashDecoder.Decode(arcListMaps[s], hashMaps);
                });

                this.SetProcessUIStatus(ProcessStatus.CommitToDisk);

                //回写文件表到磁盘  刷新资源文件系统
                ArchiveInformationProcessV2.CommitToHardDrive(DecoderConfig.ArchivePath, arcListMaps);

                this.SetProcessUIEnable(true);      //解锁UI
                this.SetProcessUIStatus(ProcessStatus.Ready);

                GC.Collect();
            }
        }

        private void btnBackupDecodedName_Click(object sender, EventArgs e)
        {
            ArchiveInformationProcessV2.BackupDecodedHashMap(DecoderConfig.ArchivePath);
            MessageBox.Show("备份完成", "Information");
        }

        private void btnHashModeSwitch_Click(object sender, EventArgs e)
        {
            if (this.mHasherMode == HasherMode.FileMode)
            {
                this.SetHashMode(HasherMode.DirectoryMode);
            }
            else if (this.mHasherMode == HasherMode.DirectoryMode)
            {
                this.SetHashMode(HasherMode.FileMode);
            }
        }

        private void HashDecoderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DynamicProcess.Instance.Dispose();
        }
    }
}
