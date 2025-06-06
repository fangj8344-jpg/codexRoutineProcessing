#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.AutoFocusTest.Converters
 * 唯一标识：f9547e8e-a3bc-4798-993c-ccf2a9aaf7d2
 * 文件名：FileToRemapImageConverter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/4/28 14:31:56
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

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UtilityTools.Modules.AutoFocusTest.Converters
{
    internal class FileToRemapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imagePath = value?.ToString();
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                Mat mat = new Mat(imagePath, ImreadModes.LoadGdal);
                var remapMat = mat.Normalize(ushort.MinValue, ushort.MaxValue, NormTypes.MinMax);
                remapMat.ConvertTo(mat, MatType.CV_8UC1, 255.0 / 65535.0, 0);
                var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                return GetImageSource(bitmap);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapSource GetImageSource(Bitmap bitmap)
        {
            BitmapSource img;
            IntPtr hBitmap;

            hBitmap = bitmap.GetHbitmap();
            img = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return img;
        }
    }
}
