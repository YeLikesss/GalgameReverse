﻿using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;

using NekoNyanStatic.Crypto;


namespace ExtractorGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Dictionary<string, CryptoVersion> mGameInfo;

        /// <summary>
        /// 初始化游戏信息
        /// </summary>
        private void InitializeGameInformation()
        {
            this.mGameInfo = DataManager.SGameInformations;

            this.cbGameTitle.Items.Clear();

            foreach(string title in this.mGameInfo.Keys)
            {
                this.cbGameTitle.Items.Add(title);
            }
        }

        /// <summary>
        /// 初始化日志输出
        /// </summary>
        private void InitializeLog()
        {
            Console.SetOut(new TextBoxLog(this.tbLog));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.InitializeGameInformation();
            this.InitializeLog();
        }

        private void FileDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void lbFilePath_DragDrop(object sender, DragEventArgs e)
        {
            ListBox lb = sender as ListBox;
            lb.Items.Clear();
            string[] resPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string path in resPaths)
            {
                lb.Items.Add(path);
            }
        }

        private void cbGameTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedIndex >= 0)
            {
                string title = cb.SelectedItem.ToString();

                this.labelCryptoVer.Text = string.Format("{0}版加密", this.mGameInfo[title].ToString());
            }
            else
            {
                this.labelCryptoVer.Text = string.Empty;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            this.tbLog.Clear();
            if (this.lbFilePath.Items.Count <= 0)
            {
                MessageBox.Show("请拖拽待解封包到指定位置", "Error");
                return;
            }
            if (this.cbGameTitle.SelectedIndex < 0)
            {
                MessageBox.Show("请选择游戏", "Error");
                return;
            }

            Button btn = sender as Button;
            if(this.mGameInfo.TryGetValue(this.cbGameTitle.SelectedItem.ToString(), out CryptoVersion ver))
            {
                btn.Enabled = false;
                List<string> fullPaths = this.lbFilePath.Items.Cast<string>().ToList();
                List<Thread> extractThreads = new(32);

                foreach (string pkgPath in fullPaths)
                {
                    ArchiveCryptoBase filter = ArchiveCryptoBase.Create(pkgPath, ver);
                    if (filter != null)
                    {
                        Thread thread = new(new ThreadStart(() =>
                        {
                            filter.Extract();
                            filter.Dispose();
                        }));
                        extractThreads.Add(thread);
                        thread.Start();
                    }
                    else
                    {
                        MessageBox.Show(string.Format("{0}加密对象创建失败", Path.GetFileName(pkgPath)), "Error");
                    }
                }

                new Thread(new ParameterizedThreadStart((object o) =>
                {
                    List<Thread> ts = o as List<Thread>;
                    while (ts.Any(t => t.IsAlive))
                    {
                        Thread.Sleep(10);
                    }
                    this.BeginInvoke(() => { btn.Enabled = true; });
                })).Start(extractThreads);
            }
            else
            {
                MessageBox.Show("获取不到游戏信息", "Error");
            }
        }

        private class TextBoxLog : TextWriter
        {
            private TextBox mTextBox;

            public override void Write(string? value)
            {
                this.mTextBox.BeginInvoke((string msg) =>
                {
                    this.mTextBox.AppendText(msg);
                }, value);
            }

            public override void WriteLine(string? value)
            {
                this.Write(value);
                this.mTextBox.BeginInvoke(() =>
                {
                    this.mTextBox.AppendText("\n");
                });
            }

            public override Encoding Encoding => Encoding.Unicode;

            public TextBoxLog(TextBox tb)
            {
                this.mTextBox = tb;
            }
        }

    }
}