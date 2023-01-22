using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PbdStatic;
using PbdStatic.Database;

namespace PbdGalleryMerger
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.InitializeGameTitleSelector();
            this.InitializeBinder();
        }

        private GalleryInformation mGalleryInformation;         //立绘信息
        private Dictionary<ToolStripMenuItem, PictureType> mSetTypeButtonBinder;   //设置类型按钮绑定

        /// <summary>
        /// 初始化游戏选择器
        /// </summary>
        private void InitializeGameTitleSelector()
        {
            this.cbGameTitle.Items.Clear();
            foreach (string title in DataManager.SDataBase.Keys)
            {
                this.cbGameTitle.Items.Add(title);
            }
            this.cbGameTitle.SelectedIndex = 0;
        }

        private void InitializeBinder()
        {
            this.mSetTypeButtonBinder = new(3)
            {
                { this.btnSetNone, PictureType.None },
                { this.btnSetBackground, PictureType.Character },
                { this.btnSetEmote, PictureType.Emote }
            };
        }

        /// <summary>
        /// 初始化图像信息
        /// </summary>
        private void InitializePictureInformation()
        {
            List<ImageInformation> pictureInfos = this.mGalleryInformation.GetImagePictureInformations();
            this.lbImageInformation.Items.Clear();
            for(int i = 0; i < pictureInfos.Count; ++i)
            {
                var p = pictureInfos[i];
                string itemStr = null;
                if (p.IsPictureTypeSet)
                {
                    itemStr = string.Format("{0} ---> {1}", p.Name, p.PictureStringType);
                }
                else
                {
                    itemStr = p.Name;
                }
                this.lbImageInformation.Items.Add(itemStr);
            }
        }

        private void btnSelectPbdFile_Click(object sender, EventArgs e)
        {
            if (DataManager.SDataBase.TryGetValue(this.cbGameTitle.SelectedItem.ToString(), out GameInformationBase gameinfo))
            {
                OpenFileDialog fileDialog = new()
                {
                    Title = "请选择pbd文件",
                    Filter = "pbd File(*.pbd)|*.pbd",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = false,
                    FilterIndex = 0,
                };
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.mGalleryInformation = GalleryInformation.Create(fileDialog.FileName, gameinfo);
                        this.InitializePictureInformation();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择游戏", "Error");
            }
        }

        private void lbImageInformation_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = sender as ListBox;
            int index = lb.SelectedIndex;
            if (lb.SelectedIndex >= 0)
            {
                string path = this.mGalleryInformation.GetPictureFilePath(this.mGalleryInformation.GetImagePictureInformations()[index]);
                if (File.Exists(path))
                {
                    this.pictureBoxPreview.Image?.Dispose();
                    this.pictureBoxPreview.Image = Image.FromFile(path);
                    
                    this.labelStatus.Text = string.Empty;
                }
                else
                {
                    this.labelStatus.Text = "文件不存在";
                }
            }
        }

        private void lbImageInformation_MouseUp(object sender, MouseEventArgs e)
        {
            ListBox lb = sender as ListBox;
            //右键
            if (e.Button == MouseButtons.Right)
            {
                int selectIndex = lb.IndexFromPoint(e.Location);
                if (selectIndex >= 0)
                {
                    lb.SetSelected(selectIndex, true);
                    this.lbPictureRightClickMenu.Show(lb, e.Location);
                }
            }
        }

        private void btnRightClickMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem btn = sender as ToolStripMenuItem;
            this.mGalleryInformation.GetImagePictureInformations()[this.lbImageInformation.SelectedIndex].PictureType = this.mSetTypeButtonBinder[btn];
            this.InitializePictureInformation();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (this.mGalleryInformation != null)
            {
                Button btn = sender as Button;
                btn.Enabled = false;
                new Thread(new ThreadStart(() =>
                {
                    GalleryProcess.MergeStandGallery(this.mGalleryInformation);
                    this.BeginInvoke(() =>
                    {
                        btn.Enabled = true;
                    });
                    MessageBox.Show("合并成功", "Information");
                })).Start();
            }
            else
            {
                MessageBox.Show("请先加载pbd立绘文件", "Error");
            }
        }
    }
}