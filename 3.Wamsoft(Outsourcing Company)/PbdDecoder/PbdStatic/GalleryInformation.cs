using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TJS;

namespace PbdStatic
{
    /// <summary>
    /// 立绘信息
    /// </summary>
    public class GalleryInformation
    {
        /// <summary>
        /// 图片后缀
        /// </summary>
        public static string SPictureFileExtension => ".png";

        /// <summary>
        /// 封包名
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// 当前路径文件夹
        /// </summary>
        public string PackageCurrentDirectory { get; private set; }

        /// <summary>
        /// 图片信息
        /// </summary>
        public List<ImageInformation> ImageInformations { get; private set; }

        /// <summary>
        /// 获取画布信息
        /// </summary>
        public ImageInformation GetDrawTableInformation()
        {
            return this.ImageInformations.Find(info => info.IsDrawTable());
        }

        /// <summary>
        /// 获取图片组信息列表
        /// </summary>
        /// <returns></returns>
        public List<ImageInformation> GetImageGroupInformations()
        {
            return this.ImageInformations.FindAll(info => info.IsImageGroup());
        }

        /// <summary>
        /// 获取图片信息列表
        /// </summary>
        /// <returns></returns>
        public List<ImageInformation> GetImagePictureInformations()
        {
            return this.ImageInformations.FindAll(info => info.IsPicture());
        }

        /// <summary>
        /// 获取背景底图信息列表
        /// </summary>
        /// <returns></returns>
        public List<ImageInformation> GetBackgroundPictureInformations()
        {
            return this.GetImagePictureInformations().FindAll(info => info.PictureType == PictureType.Character);
        }

        /// <summary>
        /// 获取表情信息列表
        /// </summary>
        /// <returns></returns>
        public List<ImageInformation> GetEmotePictureInformations()
        {
            return this.GetImagePictureInformations().FindAll(info => info.PictureType == PictureType.Emote);
        }

        /// <summary>
        /// 获取图片信息列表
        /// </summary>
        /// <param name="groupLayerId">图片组ID</param>
        /// <returns></returns>
        public List<ImageInformation> GetImagePictureInformations(int groupLayerId)
        {
            return this.GetImagePictureInformations().FindAll(info => info.GroupLayerID == groupLayerId);
        }



        /// <summary>
        /// 获取图片文件名
        /// </summary>
        /// <param name="info">图片信息</param>
        /// <returns></returns>
        public string GetPictureFileName(ImageInformation info)
        {
            if (info.IsPicture())
            {
                return string.Format("{0}_{1}{2}", this.PackageName, info.LayerID.ToString(), GalleryInformation.SPictureFileExtension);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取图片全路径
        /// </summary>
        /// <param name="info">图片信息</param>
        /// <returns></returns>
        public string GetPictureFilePath(ImageInformation info)
        {
            string fileName = this.GetPictureFileName(info);
            if (!string.IsNullOrEmpty(fileName))
            {
                return Path.Combine(this.PackageCurrentDirectory, fileName);
            }
            else
            {
                return null;
            }
        }

        private GalleryInformation()
        {
        }

        /// <summary>
        /// 使用pbd全路径构造立绘信息
        /// </summary>
        /// <param name="packageFullPath">pbd文件全路径</param>
        /// <returns></returns>
        public static GalleryInformation Create(string packageFullPath, IKeyInformation keyInfo)
        {
            PbdBinary pbdBinary = PbdBinary.Create(packageFullPath, keyInfo);
            if (pbdBinary != null)
            {
                if (pbdBinary.TryGetTJSVariantObject(out TJSVariant tjsGalleryTable))
                {
                    return GalleryInformation.Create(tjsGalleryTable, packageFullPath);       //创建立绘表信息
                }
                else
                {
                    throw new Exception("TJSVariant反序列化失败");
                }
            }
            else
            {
                throw new Exception("PbdBinary创建失败");
            }
        }

        /// <summary>
        /// 使用tjs立绘表构造立绘信息
        /// </summary>
        /// <param name="tjsGalleryTable">tjs立绘表</param>
        /// <param name="packageFullPath">pbd文件全路径</param>
        /// <returns></returns>
        public static GalleryInformation Create(TJSVariant tjsGalleryTable, string packageFullPath)
        {
            List<TJSVariant> tjsImageInfos = tjsGalleryTable.TJSArray();

            GalleryInformation gallery = new()
            {
                PackageName = Path.GetFileNameWithoutExtension(packageFullPath),
                PackageCurrentDirectory = Path.GetDirectoryName(packageFullPath),
                ImageInformations = new(tjsImageInfos.Count)
            };

            foreach(TJSVariant tjsImageInfo in CollectionsMarshal.AsSpan(tjsImageInfos))
            {
                Dictionary<string, TJSVariant> tjsImageInfoDic = tjsImageInfo.TJSDictionary();

                ImageInformation imageInfo = new();

                checked
                {
                    imageInfo.LayerType = tjsImageInfoDic.TryGetValue("layer_type", out TJSVariant tjsLayerType) ? (int)tjsLayerType.SignInt64() : -1;
                    imageInfo.Name = tjsImageInfoDic.TryGetValue("name", out TJSVariant tjsName) ? tjsName.String() : null;
                    imageInfo.OffsetX = tjsImageInfoDic.TryGetValue("left", out TJSVariant tjsOffsetX) ? (int)tjsOffsetX.SignInt64() : -1;
                    imageInfo.OffsetY = tjsImageInfoDic.TryGetValue("top", out TJSVariant tjsOffsetY) ? (int)tjsOffsetY.SignInt64() : -1;
                    imageInfo.Width = tjsImageInfoDic.TryGetValue("width", out TJSVariant tjsWidth) ? (int)tjsWidth.SignInt64() : -1;
                    imageInfo.Height = tjsImageInfoDic.TryGetValue("height", out TJSVariant tjsHeight) ? (int)tjsHeight.SignInt64() : -1;

                    //不知道干嘛用的
                    imageInfo.Type = tjsImageInfoDic.TryGetValue("type", out TJSVariant tjsType) ? (int)tjsType.SignInt64() : -1;

                    imageInfo.Opacity = tjsImageInfoDic.TryGetValue("opacity", out TJSVariant tjsOpacity) ? (int)tjsOpacity.SignInt64() : -1;
                    imageInfo.Visible = tjsImageInfoDic.TryGetValue("visible", out TJSVariant tjsVisible) ? (int)tjsVisible.SignInt64() : -1;
                    imageInfo.LayerID = tjsImageInfoDic.TryGetValue("layer_id", out TJSVariant tjsLayerID) ? (int)tjsLayerID.SignInt64() : -1;
                    imageInfo.GroupLayerID = tjsImageInfoDic.TryGetValue("group_layer_id", out TJSVariant tjsGroupLayerID) ? (int)tjsGroupLayerID.SignInt64() : -1;

                    //不知道干嘛用的
                    imageInfo.Base = tjsImageInfoDic.TryGetValue("base", out TJSVariant tjsBase) ? (int)tjsBase.SignInt64() : -1;
                    imageInfo.Images = tjsImageInfoDic.TryGetValue("images", out TJSVariant tjsImages) ? (int)tjsImages.SignInt64() : -1;
                }
                gallery.ImageInformations.Add(imageInfo);
            }
            return gallery;
        }
    }

    /// <summary>
    /// 层类型
    /// </summary>
    public enum ImageLayerType : int
    {
        /// <summary>
        /// 图片
        /// </summary>
        Picture = 0,
        /// <summary>
        /// 图片组
        /// </summary>
        Group = 2,
    }

    /// <summary>
    /// 图片类型
    /// </summary>
    public enum PictureType : int
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 角色背景底图
        /// </summary>
        Character = 1,
        /// <summary>
        /// 表情前景图
        /// </summary>
        Emote = 2,
    }

    /// <summary>
    /// 图片信息
    /// </summary>
    public class ImageInformation
    {
        /// <summary>
        /// 层类型
        /// </summary>
        public int LayerType { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 绘制X偏移
        /// </summary>
        public int OffsetX { get; set; }
        /// <summary>
        /// 绘制Y偏移
        /// </summary>
        public int OffsetY { get; set; }
        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }
        public int Type { get; set; }
        /// <summary>
        /// 不透明度
        /// </summary>
        public int Opacity { get; set; }
        /// <summary>
        /// 可见性
        /// </summary>
        public int Visible { get; set; }
        /// <summary>
        /// 图像ID
        /// <para>文件名后缀</para>
        /// <para>在图像组为组ID</para>
        /// </summary>
        public int LayerID { get; set; }
        /// <summary>
        /// 图片组ID
        /// </summary>
        public int GroupLayerID { get; set; }
        public int Base { get; set; }
        public int Images { get; set; }


        /// <summary>
        /// 图片类型
        /// </summary>
        public PictureType PictureType { get; set; } = PictureType.None;
        /// <summary>
        /// 图片类型  (字符串信息)
        /// </summary>
        public string PictureStringType
        { 
            get
            {
                return this.PictureType switch
                {
                    PictureType.None => "未设置",
                    PictureType.Character => "角色底图",
                    PictureType.Emote => "表情",
                    _ => "未知类型",
                };
            }
        }

        /// <summary>
        /// 图片类型是否已设置
        /// </summary>
        public bool IsPictureTypeSet
        {
            get
            {
                return this.PictureType switch
                {
                    PictureType.None => false,
                    PictureType.Character or PictureType.Emote => true,
                    _ => false,
                };
            }
        }

        /// <summary>
        /// 是否为画布
        /// </summary>
        /// <returns></returns>
        public bool IsDrawTable()
        {
            return this.LayerType == -1 && string.IsNullOrEmpty(this.Name);
        }

        /// <summary>
        /// 是否为图像组
        /// </summary>
        /// <returns></returns>
        public bool IsImageGroup()
        {
            return this.LayerType != -1 && this.LayerType == (int)ImageLayerType.Group;
        }

        /// <summary>
        /// 是否为单个图片
        /// </summary>
        /// <returns></returns>
        public bool IsPicture()
        {
            return this.LayerType != -1 && this.LayerType == (int)ImageLayerType.Picture;
        }

    }
}
