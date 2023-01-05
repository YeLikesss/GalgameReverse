using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CxHashDecoder
{
    public partial class LoaderForm : Form
    {
        public LoaderForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取主窗体对象
        /// </summary>
        public Form MainForm { get; private set; }
        /// <summary>
        /// 获取初始化标志
        /// </summary>
        public bool Isinitialized { get; private set; }


        private Dictionary<Button, TextBox> mPathSetterBinding = new(2);    //路径选择器控件绑定

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
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            //控件绑定
            this.mPathSetterBinding.Add(this.btnViewArchivePath, this.tbArchivePath);
            this.mPathSetterBinding.Add(this.btnViewGamePath, this.tbGamePath);
        }

        private void LoaderForm_Load(object sender, EventArgs e)
        {
            this.Initialize();
            this.btnSetStaticMode.Visible = false;
        }

        private void ViewPathButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string path = SelectPath();
            TextBox tb = this.mPathSetterBinding[btn];
            if (path != null)
            {
                tb.Text = path;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string archivePath = this.tbArchivePath.Text;
            string gamePath = this.tbGamePath.Text;

            //检查路径
            if (string.IsNullOrEmpty(archivePath) || string.IsNullOrEmpty(gamePath))
            {
                this.Isinitialized = false;
                MessageBox.Show("路径不可以为空", "Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //检查模式
            DecoderConfig.SetProcessPath(archivePath, gamePath);
            this.MainForm = new HashDecoderForm(); ;
            this.Isinitialized = true;      //初始化完成

            this.Close();       //关闭Loader进入主窗口
        }

        private void btnSetStaticMode_Click(object sender, EventArgs e)
        {
            //CxStaticExtractor.GameSelector selector = new();
            //selector.CallBack = DecoderConfig.SetStaticArgs;
            //selector.ShowDialog();
        }
    }
}
