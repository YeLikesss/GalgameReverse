using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Text.Encodings;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CxHashDecoder
{
    public partial class TextViewer : UserControl
    {
        protected enum TextEncode : uint
        {
            GBK = 0,
            Shift_JIS = 1,
            UTF8 = 2,
            UTF16LE = 3,
        }

        /// <summary>
        /// 文本处理回调
        /// </summary>
        public Action<object> TextProcessCallBack { get; set; }

        private List<string> mFileFullPath = new(256);      //临时储存全路径

        private Dictionary<TextEncode, Encoding> mEncoderMap = new(8);
        private Dictionary<RadioButton, TextEncode> mEncodingSelectBtns = new(8);

        private TextEncode mTextEncode;

        /// <summary>
        /// 初始化编码表
        /// </summary>
        private void InitMap()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            mEncoderMap.Add(TextEncode.GBK, Encoding.GetEncoding(936));
            mEncoderMap.Add(TextEncode.Shift_JIS, Encoding.GetEncoding(932));
            mEncoderMap.Add(TextEncode.UTF8, new UTF8Encoding(true));
            mEncoderMap.Add(TextEncode.UTF16LE, new UnicodeEncoding(false, true));

            mEncodingSelectBtns.Add(this.rbtGBK, TextEncode.GBK);
            mEncodingSelectBtns.Add(this.rbtShiftJIS, TextEncode.Shift_JIS);
            mEncodingSelectBtns.Add(this.rbtUTF8, TextEncode.UTF8);
            mEncodingSelectBtns.Add(this.rbtUTF16, TextEncode.UTF16LE);
        }
        /// <summary>
        /// 清空导入项
        /// </summary>
        private void ClearItems()
        {
            this.cbSelectTextFile.Items.Clear();
            this.mFileFullPath.Clear();
            this.tbPreview.Clear();
        }

        /// <summary>
        /// 文本生成函数
        /// </summary>
        /// <returns></returns>
        private IEnumerable<List<string>> TextGenerator()
        {
            List<string> sl = new(1024);
            //根据文件全路径缓冲器读取文件
            foreach (var filePath in this.mFileFullPath)
            {
                //输入流
                using StreamReader input = new(filePath, this.mEncoderMap[this.mTextEncode]);
                while (!input.EndOfStream)
                {
                    sl.Add(input.ReadLine().ToLower());     //转化为小写
                    //达量返回
                    if (sl.Count == sl.Capacity)
                    {
                        yield return sl;
                        sl.Clear();
                    }
                }
                input.Close();
            }

            //剩余部分
            if (sl.Count != 0)
            {
                yield return sl;
            }
            this.BeginInvoke(new Action(this.ClearItems));
        }

        public TextViewer()
        {
            InitializeComponent();
            InitMap();
        }

        //切换编码按钮事件
        private void EncodingBtn_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton bt = sender as RadioButton;
            if (bt.Checked)
            {
                this.mTextEncode = this.mEncodingSelectBtns[bt];
            }
        }

        //文件选择索引修改事件
        private void cbSelectTextFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedIndex >= 0)
            {
                //从全路径缓存选择文件路径
                using StreamReader sr = new(this.mFileFullPath[cb.SelectedIndex], this.mEncoderMap[this.mTextEncode]);

                StringBuilder sb = new(0x100000);
                while (!sr.EndOfStream)
                {
                    sb.AppendLine(sr.ReadLine());
                }
                this.tbPreview.Text = sb.ToString();
                sr.Close();
            }
        }

        private void btnIntput_Click(object sender, EventArgs e)
        {
            string[] files = null;
            OpenFileDialog fileDialog = new()
            {
                Multiselect = true,
                Title = "请选择文件",
                Filter = "文本文档(*.txt)|*.txt|所有文件(*.*)|*.*",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };
            this.cbSelectTextFile.Items.Clear();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                files = fileDialog.FileNames;
            }
            if (files != null)
            {
                foreach (var f in files)
                {
                    this.mFileFullPath.Add(f);
                    this.cbSelectTextFile.Items.Add(Path.GetFileName(f));
                }
            }
        }
        private void TextViewer_Load(object sender, EventArgs e)
        {
            this.rbtUTF16.Checked = true;
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (this.mFileFullPath.Count > 0)
            {
                new Thread(new ParameterizedThreadStart(this.TextProcessCallBack)).Start(new Func<IEnumerable<List<string>>>(this.TextGenerator));
            }
            else
            {
                MessageBox.Show("未导入任何文件", "Information");
            }
        }
    }
}
