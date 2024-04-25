#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：a9518714-e2dd-48e3-bcbd-e251a99b93d2
 * 文件名：XmpMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/25 14:29:28
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using ImageMagick;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// 依赖第三方的ImageMagick库
    /// 向文件中写入标准的XMP协议，补充文件的相关信息
    /// JPEG图像写入XMP后再次加载出现无法加载的情况
    /// </summary>
    public static class XmpMethod
    {
        #region ------------StaticMethod------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ObservableCollection<PropertyInfoModel> GetMetadatas2(string fileName)
        {
            if (!File.Exists(fileName))
                return null;
            try
            {
                using (Bitmap bitmap = new Bitmap(fileName))
                {
                    var items = bitmap.PropertyItems;
                    var list = items.Where(p => p.Id == 40092);
                    if (list.Count() > 0)
                    {
                        var property = list.First();
                        var str = System.Text.Encoding.ASCII.GetString(property.Value);
                        var xdoc = XDocument.Parse(str);
                        if (xdoc != null)
                        {
                            ObservableCollection<PropertyInfoModel> result = new ObservableCollection<PropertyInfoModel>();
                            var root = xdoc.Root;
                            foreach (var item in root.Elements())
                            {
                                var keyStr = item.Attribute("ID").Value;
                                if (int.TryParse(keyStr, out var key))
                                {
                                    PropertyInfoModel info = new PropertyInfoModel(key);
                                    info.Value = item.Value.ToString();
                                    result.Add(info);
                                }

                            }
                            return result;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"加载文件EXIF元素异常 ：{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="metadatas"></param>
        public static void SetMetadatas2(string fileName, ICollection<PropertyInfoModel> metadatas)
        {
            if (!File.Exists(fileName))
                return;
            var ext = Path.GetExtension(fileName);
            try
            {
                bool isSave = false;
                string destFilename = "tmp" + ext;
                // 读取图像
                using (Bitmap bitmap = new Bitmap(fileName))
                {
                    XDocument document = new XDocument();

                    var root = new XElement("Zeptools");
                    foreach (PropertyInfoModel info in metadatas)
                    {
                        var child = new XElement(info.Name);
                        child.Value = info.Value.ToString();
                        child.SetAttributeValue("ID", info.Key);
                        root.Add(child);
                    }
                    document.Add(root);

                    var comment = document.ToString();

                    // 将备注信息写入图像的属性
                    var property = bitmap.PropertyItems[0];
                    property.Id = 40092; // 注释的标识符（必须是40092）
                    property.Type = 2; // ASCII字符串
                    property.Len = comment.Length + 1; // 注释字符串长度（包括终止符）
                    property.Value = System.Text.Encoding.ASCII.GetBytes(comment + "\0"); // 注释字符串的字节数组
                    bitmap.SetPropertyItem(property);

                    // 保存修改后的图像文件
                    bitmap.Save(destFilename);
                }

                if (isSave)
                {
                    File.Delete(fileName);
                    File.Move(destFilename, fileName);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"为文件添加EXIF元素异常 ：{ex.Message}");
                return;
            }
        }

        /// <summary>
        /// 解析文件中的PropertyInfo相关的信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ObservableCollection<PropertyInfoModel> GetMetadatas(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            try
            {
                ObservableCollection<PropertyInfoModel> result = new ObservableCollection<PropertyInfoModel>();
                using (var image = new MagickImage(fileName))
                {
                    var comment = image.Comment;
                    var xmpFile = image.GetXmpProfile();
                    //var xdoc = xmpFile.ToXDocument();
                    var xdoc = XDocument.Parse(comment);
                    var root = xdoc.Root;
                    foreach (var item in root.Elements())
                    {
                        var keyStr = item.Attribute("ID").Value;
                        if (int.TryParse(keyStr, out var key))
                        {
                            PropertyInfoModel info = new PropertyInfoModel(key);
                            info.Value = item.Value.ToString();
                            result.Add(info);
                        }

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"加载文件EXIF元素异常 ：{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 向文件中写入PropertyInfo相关信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="metadatas"></param>
        public static void SetMetadatas(string fileName, ICollection<PropertyInfoModel> metadatas)
        {
            if (!File.Exists(fileName))
                return;

            var ext = Path.GetExtension(fileName);
            var format = MagickFormat.APng;
            switch (ext)
            {
                case ".png":
                    format = MagickFormat.Png24;
                    break;
                case ".jpeg":
                    format = MagickFormat.Jpeg;
                    break;
                case ".bmp":
                    format = MagickFormat.Bmp2;
                    break;
                case ".tiff":
                    format = MagickFormat.Tiff;
                    break;
            }

            try
            {
                bool isSave = false;
                string destFilename = "tmp" + ext;
                using (var image = new MagickImage(fileName))
                {
                    XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

                    var root = new XElement("Zeptools");
                    foreach (PropertyInfoModel info in metadatas)
                    {
                        var child = new XElement(info.Name);
                        child.Value = info.Value.ToString();
                        child.SetAttributeValue("ID", info.Key);
                        root.Add(child);
                    }
                    document.Add(root);

                    image.Comment = document.ToString();

                    //XmpProfile xmpProfile = new XmpProfile(document);
                    //image.SetProfile(xmpProfile);
                    image.Write(destFilename, format);
                    isSave = true;
                }

                if (isSave)
                {
                    File.Delete(fileName);
                    File.Move(destFilename, fileName);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"为文件添加EXIF元素异常 ：{ex.Message}");
                return;
            }
        }
        #endregion
    }
}
