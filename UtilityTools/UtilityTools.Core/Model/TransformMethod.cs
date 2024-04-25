#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：a4ad4978-90fd-4f86-8b94-7acdf97045f4
 * 文件名：TransformMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/25 15:30:38
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

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NLog;
using System.IO;

namespace UtilityTools.Core.Model
{
    public static class TransformMethod
    {
        /// <summary>
        /// 将Bitmap转换成WriteableBitmap
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static WriteableBitmap ConvertBitmapToWriteableBitmap(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                // 将 Bitmap 保存到内存流中
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                // 重置流的位置
                memoryStream.Position = 0;

                // 使用 BitmapFrame 创建 BitmapSource
                var bitmapSource = BitmapFrame.Create(
                    memoryStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                // 创建 WriteableBitmap
                var writeableBitmap = new WriteableBitmap(bitmapSource);

                return writeableBitmap;
            }
        }

        /// <summary>
        /// 将WriteableBitmap转换成Bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap WriteableBitmapToBitmap(this WriteableBitmap source)
        {
            try
            {
                var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format24bppRgb);

                unsafe
                {
                    // 导入原始图片
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, source.PixelWidth, source.PixelHeight),
                        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    source.Lock();

                    byte* src = (byte*)source.BackBuffer.ToPointer();
                    byte* dst = (byte*)data.Scan0.ToPointer();
                    int sourceStride = source.BackBufferStride / source.PixelWidth;
                    int dataStride = data.Stride / data.Width;

                    for (int index = 0; index < source.PixelWidth * source.PixelHeight; ++index)
                    {
                        if (sourceStride == 1)
                        {
                            dst[index * dataStride + 0] = src[index * sourceStride];
                            dst[index * dataStride + 1] = src[index * sourceStride];
                            dst[index * dataStride + 2] = src[index * sourceStride];
                        }
                        else if (sourceStride == 2)
                        {
                            dst[index * dataStride + 0] = src[index * sourceStride + 1];
                            dst[index * dataStride + 1] = src[index * sourceStride + 1];
                            dst[index * dataStride + 2] = src[index * sourceStride + 1];
                        }
                        else
                        {
                            dst[index * dataStride + 0] = src[index * sourceStride + 0];
                            dst[index * dataStride + 1] = src[index * sourceStride + 1];
                            dst[index * dataStride + 2] = src[index * sourceStride + 2];
                        }
                    }

                    source.Unlock();
                    bmp.UnlockBits(data);
                }

                return bmp;
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"将WriteableBitmap转成Bitmap失败：{ex.Message}");
                return null;
            }
        }

    }
}
