using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TJS;
using System.Drawing.Imaging;

namespace PbdStatic
{
    /// <summary>
    /// 立绘处理
    /// </summary>
    public class GalleryProcess
    {
        /// <summary>
        /// 合成立绘
        /// </summary>
        /// <param name="galleryInfo">立绘信息</param>
        /// <returns></returns>
        public static bool MergeStandGallery(GalleryInformation galleryInfo)
        {
            //画布长宽
            int drawTableWidth = 0;
            int drawTableHeigth = 0;
            {
                ImageInformation drawTableInfo = galleryInfo.GetDrawTableInformation();     //画布信息
                drawTableWidth = drawTableInfo.Width;
                drawTableHeigth = drawTableInfo.Height;
            }

            //获取背景与表情
            List<ImageInformation> backgroundPictures = galleryInfo.GetBackgroundPictureInformations();       //背景图片信息列表
            List<ImageInformation> emotePictures = galleryInfo.GetEmotePictureInformations();         //表情图片信息列表

            string currentDirectory = galleryInfo.PackageCurrentDirectory;      //根目录
            string packageName = galleryInfo.PackageName;               //立绘表名

            //输出文件夹
            string outputDirectory = Path.Combine(currentDirectory, packageName);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            int picIndex = 0;
            //合成图片
            foreach(ImageInformation background in CollectionsMarshal.AsSpan(backgroundPictures))
            {
                //背景文件名
                string backgroundFilename = galleryInfo.GetPictureFilePath(background);
                //背景文件路径
                string backgroundPath = Path.Combine(currentDirectory, backgroundFilename);
                if (File.Exists(backgroundPath))
                {
                    Bitmap backgroundPicture = null;
                    {
                        Bitmap fileBitmap = new(backgroundPath);     //背景图

                        //设置透明度
                        backgroundPicture = ImageProcess.ChangeOpacity(fileBitmap, (byte)background.Opacity);

                        fileBitmap.Dispose();
                    }

                    foreach (ImageInformation emote in CollectionsMarshal.AsSpan(emotePictures))
                    {
                        //表情文件名
                        string emoteFilename = galleryInfo.GetPictureFilePath(emote);
                        //表情文件路径
                        string emotePath = Path.Combine(currentDirectory, emoteFilename);
                        if (File.Exists(emotePath))
                        {
                            Bitmap emotePicture = null;
                            {
                                Bitmap bitmapFile = new(emotePath);       //表情图

                                //设置透明度
                                emotePicture = ImageProcess.ChangeOpacity(bitmapFile, (byte)emote.Opacity);

                                bitmapFile.Dispose();
                            }

                            {
                                Bitmap drawTable = new(drawTableWidth, drawTableHeigth, PixelFormat.Format32bppArgb);
                                //绘制背景
                                ImageProcess.Merge(drawTable, backgroundPicture, background.OffsetX, background.OffsetY, ImageProcess.MergeMode.Override);
                                //合并表情
                                ImageProcess.Merge(drawTable, emotePicture, emote.OffsetX, emote.OffsetY, ImageProcess.MergeMode.Overlay);

                                //输出
                                drawTable.Save(Path.Combine(outputDirectory, picIndex.ToString() + GalleryInformation.SPictureFileExtension), ImageFormat.Png);
                                drawTable.Dispose();
                            }

                            emotePicture.Dispose();
                            ++picIndex;
                        }
                        else
                        {
                            //文件不存在
                        }
                    }

                    backgroundPicture.Dispose();
                }
                else
                {
                    //文件不存在
                }
            }
            return true;
        }
    }
}
