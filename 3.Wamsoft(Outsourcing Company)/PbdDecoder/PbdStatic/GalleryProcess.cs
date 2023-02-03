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
            //获取所有图层信息
            List<List<ImageInformation>> layers = galleryInfo.GetAllPictureLayerInformations();
            if (layers.Count > 0)
            {
                //画布长宽
                int drawTableWidth = 0;
                int drawTableHeigth = 0;
                {
                    ImageInformation drawTableInfo = galleryInfo.GetDrawTableInformation();     //画布信息
                    drawTableWidth = drawTableInfo.Width;
                    drawTableHeigth = drawTableInfo.Height;
                }

                string currentDirectory = galleryInfo.PackageCurrentDirectory;      //根目录
                string packageName = galleryInfo.PackageName;               //立绘表名

                //输出文件夹
                string outputDirectory = Path.Combine(currentDirectory, packageName);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                //处理数量
                long processCount = 1;
                {
                    int layersCount = layers.Count;
                    for (int i = 0; i < layersCount; ++i)
                    {
                        processCount *= layers[i].Count;
                    }
                }

                for(long index = 0; index < processCount; ++index)
                {
                    int layersCount = layers.Count;     //图层数量
                    long nowIndex = index;

                    List<int> layerIndexes = new(layers.Count);

                    //获取各图层的索引
                    for (int i = layersCount - 1; i >= 0; --i)
                    {
                        int xxxLayerPictures = layers[i].Count;     //每个图层的图片数量

                        layerIndexes.Add((int)(nowIndex % xxxLayerPictures));

                        nowIndex /= xxxLayerPictures;
                    }

                    //反转  此时图层0在索引0处
                    layerIndexes.Reverse();

                    {
                        //创建画布
                        Bitmap drawTable = new(drawTableWidth, drawTableHeigth, PixelFormat.Format32bppArgb);

                        for(int layer = 0; layer< layersCount; ++layer)
                        {
                            List<ImageInformation> xxxLayerPictureInformations = layers[layer];
                            ImageInformation pictureInfo = xxxLayerPictureInformations[layerIndexes[layer]];

                            string pictureFilePath = galleryInfo.GetPictureFilePath(pictureInfo);
                            if (File.Exists(pictureFilePath))
                            {
                                Bitmap layerPicture = null;
                                {
                                    Bitmap bitmapFile = new(pictureFilePath);

                                    //设置透明度
                                    layerPicture = ImageProcess.ChangeOpacity(bitmapFile, (byte)pictureInfo.Opacity);

                                    bitmapFile.Dispose();
                                }

                                //绘制
                                ImageProcess.Merge(drawTable, layerPicture, pictureInfo.OffsetX, pictureInfo.OffsetY, ImageProcess.MergeMode.Overlay);

                                layerPicture.Dispose();
                            }
                            else
                            {
                                //文件不存在
                            }
                        }
                        //输出
                        drawTable.Save(Path.Combine(outputDirectory, index.ToString() + GalleryInformation.SPictureFileExtension), ImageFormat.Png);
                        drawTable.Dispose();
                    }
                }
                return true;
            }
            return false;
        }
    }
}
