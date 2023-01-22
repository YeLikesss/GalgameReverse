using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;


namespace PbdStatic
{
    /// <summary>
    /// 图像处理  32bppARGB格式
    /// </summary>
    internal class ImageProcess
    {
        /// <summary>
        /// 混合模式
        /// </summary>
        public enum MergeMode : int
        {
            /// <summary>
            /// 覆盖
            /// </summary>
            Override = 0,
            /// <summary>
            /// 叠加
            /// </summary>
            Overlay = 1,
        }

        /// <summary>
        /// 修改透明度
        /// </summary>
        /// <param name="src">位图</param>
        /// <param name="opacity">透明度</param>
        public unsafe static Bitmap ChangeOpacity(Bitmap src, byte opacity)
        {
            int w = src.Width;
            int h = src.Height;

            Bitmap dest = new(w, h);
            {
                BitmapData srcData = src.LockBits(new(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData destData = dest.LockBits(new(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                {
                    int pixelCount = w * h;
                    Span<uint> srcMemory = new(srcData.Scan0.ToPointer(), pixelCount);
                    Span<uint> destMemory = new(destData.Scan0.ToPointer(), pixelCount);

                    if (opacity == 0xFF)
                    {
                        //完全不透明
                        srcMemory.CopyTo(destMemory);
                    }
                    else
                    {
                        //有透明度
                        for (int i = 0; i < pixelCount; ++i)
                        {
                            uint pixel = srcMemory[i];

                            uint alpha = pixel >> 0x18;
                            alpha = alpha * opacity / 0xFF;

                            destMemory[i] = (pixel & 0x00FFFFFF) | (alpha << 0x18);
                        }
                    }
                }
                src.UnlockBits(srcData);
                dest.UnlockBits(destData);
            }
            return dest;
        }

        /// <summary>
        /// 像素混合
        /// <para>多余部分自动裁切</para>
        /// </summary>
        /// <param name="drawTable">待绘制区域</param>
        /// <param name="src">绘制源</param>
        /// <param name="posX">X方向偏移</param>
        /// <param name="posY">Y方向偏移</param>
        public unsafe static bool Merge(Bitmap drawTable, Bitmap src, int posX, int posY, MergeMode mode)
        {
            int tableW = drawTable.Width;
            int tableH = drawTable.Height;

            int srcW = src.Width;
            int srcH = src.Height;

            int drawW = Math.Min(srcW - Math.Abs(Math.Min(posX, 0)), tableW - Math.Abs(Math.Max(posX, 0)));     //要绘制的宽度
            int drawH = Math.Min(srcH - Math.Abs(Math.Min(posY, 0)), tableH - Math.Abs(Math.Max(posY, 0)));     //要绘制的高度

            if (drawW <= 0 || drawH <= 0)
            {
                return false;
            }

            {
                BitmapData drawTableData = drawTable.LockBits(new(0, 0, tableW, tableH), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                BitmapData srcData = src.LockBits(new(0, 0, srcW, srcH), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                {
                    Span<uint> drawTableMemory = new(drawTableData.Scan0.ToPointer(), tableW * tableH);
                    Span<uint> srcMemory = new(srcData.Scan0.ToPointer(), srcW * srcH);

                    //起点
                    int startX = Math.Max(posX, 0);
                    int startY = Math.Max(posY, 0);
                    //终点
                    int endX = startX + drawW;
                    int endY = startY + drawH;

                    {
                        int drawY = startY;
                        int srcY = Math.Abs(Math.Min(posY, 0));

                        for ( ; drawY < endY; ++drawY, ++srcY)
                        {
                            {
                                int drawX = startX;
                                int srcX = Math.Abs(Math.Min(posX, 0));

                                for (; drawX < endX; ++drawX, ++srcX)
                                {
                                    int drawPos = tableW * drawY + drawX;    //当前画布位置
                                    int srcPos = srcW * srcY + srcX;        //原图像位置

                                    if (mode == MergeMode.Override)
                                    {
                                        //覆盖模式
                                        drawTableMemory[drawPos] = srcMemory[srcPos];
                                    }
                                    else if (mode == MergeMode.Overlay)
                                    {
                                        //混合模式
                                        uint drawPixel = drawTableMemory[drawPos];
                                        uint srcPixel = srcMemory[srcPos];
                                        uint blendPixel = 0;

                                        {
                                            Span<byte> drawArgb = new(&drawPixel, 4);
                                            Span<byte> srcArgb = new(&srcPixel, 4);
                                            Span<byte> blendArgb = new(&blendPixel, 4);

                                            blendArgb[3] = ImageProcess.AlphaChannelBlend(drawArgb[3], srcArgb[3]);
                                            blendArgb[2] = ImageProcess.RGBChannelBlend(drawArgb[2], srcArgb[2], drawArgb[3], srcArgb[3]);
                                            blendArgb[1] = ImageProcess.RGBChannelBlend(drawArgb[1], srcArgb[1], drawArgb[3], srcArgb[3]);
                                            blendArgb[0] = ImageProcess.RGBChannelBlend(drawArgb[0], srcArgb[0], drawArgb[3], srcArgb[3]);
                                        }

                                        drawTableMemory[drawPos] = blendPixel;
                                    }
                                }
                            }
                        }
                    }

                }
                drawTable.UnlockBits(drawTableData);
                src.UnlockBits(srcData);
            }
            return true;
        }

        /// <summary>
        /// 混合Alpha通道
        /// </summary>
        /// <param name="back">背景</param>
        /// <param name="fore">前景</param>
        /// <returns></returns>
        private static byte AlphaChannelBlend(byte back, byte fore)
        {
            return (byte)(back + fore - (back * fore / 0xFF));
        }

        /// <summary>
        /// 混合RGB通道
        /// </summary>
        /// <param name="backColor">背景色</param>
        /// <param name="foreColor">前景色</param>
        /// <param name="backAlpha">背景Alpha</param>
        /// <param name="foreAlpha">前景Alpha</param>
        /// <returns></returns>
        private static byte RGBChannelBlend(byte backColor, byte foreColor, byte backAlpha, byte foreAlpha)
        {
            byte blendAlpha = ImageProcess.AlphaChannelBlend(backAlpha, foreAlpha);
            if (blendAlpha == 0)
            {
                return 0;
            }
            else
            {
                return (byte)((backColor * backAlpha + foreColor * foreAlpha - backColor * backAlpha * foreAlpha / 0xFF) / blendAlpha);
            }
        }

    }
}
