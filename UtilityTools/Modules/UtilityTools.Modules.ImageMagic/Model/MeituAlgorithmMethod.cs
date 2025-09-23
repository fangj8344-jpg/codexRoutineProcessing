#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：CoreTools.Method
 * 唯一标识：81dbf6b6-7e62-4287-bc1b-ec25ec8ee889
 * 文件名：MeituAlgorithmMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/2/12 19:50:55
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

using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace UtilityTools.Modules.ImageMagic.Model
{
    public class MeituAlgorithmMethod
    {
        #region ------------Class------------
        class MediaProfiles
        {
            public string media_data_type { get; set; }
        }

        class MediaInfo
        {
            public string media_data { get; set; }
            public MediaProfiles media_profiles { get; set; }
        }

        class Parameter
        {
            public string rsp_media_type { get; set; }
            public string version { get; set; }
        }

        class ResponseOk
        {
            public List<MediaInfo> media_info_list { get; set; }
            public Parameter paremeter { get; set; }
        }

        class ResponseBad
        {
            public string ErrorCode { get; set; }
            public string ErrorMsg { get; set; }
            public string Data { get; set; }
        }
        #endregion

        #region ------------Field------------
        // API密钥
        private static string AppID = "352484";
        private static string AppKey = "5e7dfa07d99a4620bb53ca2fa2ab3ddc";
        private static string SecretID = "d7d0ba2a327c4d5f953a66e51ecee0f7";
        private static string url = $"https://openapi.mtlab.meitu.com/v1/AIDenoise?api_key={AppKey}&api_secret={SecretID}";
        #endregion

        #region ------------StaticMethod------------
        /// <summary>
        /// 将图像转换尺Base64字符串
        /// </summary>
        /// <param name="imagePath">图像地址</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ImageToBase64(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new Exception("File Not Exists!");
            }

            byte[] pngBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(pngBytes);
        }

        /// <summary>
        /// 将图像转换成Base64
        /// </summary>
        /// <param name="img">图像本身</param>
        /// <returns></returns>
        public static string ImageToBase64(BitmapImage img)
        {
            string base64String = null;

            // 使用 JpegBitmapEncoder 将 BitmapImage 转换成 byte[]
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                var bytes = stream.ToArray();

                // 将 byte[] 转换为 Base64 字符串
                base64String = Convert.ToBase64String(bytes);
            }

            return base64String;
        }

        /// <summary>
        /// 将Mat图像转换成Base64
        /// </summary>
        /// <param name="src">Mat图像</param>
        /// <returns></returns>
        public static string MatToBase64(Mat src)
        {
            byte[] data = new byte[src.Width * src.Height * src.ElemSize()];
            Cv2.ImEncode(".png", src, out data);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 将图像转换成Base64
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static string WriteableBitmapToBase64(WriteableBitmap bitmap)
        {
            // 创建一个JpegBitmapEncoder对象（也可根据需要选择其他的BitmapEncoder）
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            // 创建一个MemoryStream来存储编码后的图像数据
            MemoryStream memoryStream = new MemoryStream();

            // 将WriteableBitmap对象保存到MemoryStream中
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(memoryStream);

            // 将MemoryStream中的图像数据转换为字节数组
            byte[] bitmapBytes = memoryStream.ToArray();

            // 将字节数组进行Base64编码
            string base64String = Convert.ToBase64String(bitmapBytes);

            return base64String;
        }

        /// <summary>
        /// 将Base64字符串转换图像
        /// </summary>
        /// <param name="imgStr">字符串</param>
        /// <returns></returns>
        public static BitmapImage Base64ToImage(string imgStr)
        {
            BitmapImage bitmap = null;

            // 将 Base64 字符串解码为 byte[]
            var bytes = Convert.FromBase64String(imgStr);

            // 使用 MemoryStream 将 byte[] 转换为 Stream
            using (var stream = new MemoryStream(bytes))
            {
                // 使用 BitmapImage 创建一个新的图片对象并加载到其中
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }

        /// <summary>
        /// 将Base64字符串转换成图像
        /// </summary>
        /// <param name="imgStr"></param>
        /// <returns></returns>
        public static Mat Base64ToMat(string imgStr, int width, int height)
        {
            byte[] imageBytes = Convert.FromBase64String(imgStr);

            // 使用 Cv2.ImDecode 解码字节数组为 Mat 对象
            Mat mat = Cv2.ImDecode(imageBytes, ImreadModes.Color); // 你可以根据需要选择其他模式，如 Grayscale 等
                                                                   // 如果 Mat 对象为空，说明解码失败
            if (mat.Empty())
            {
                return null;
            }

            // 转换为灰度图
            Mat grayMat = new Mat();
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

            // 调整图像大小为固定尺寸
            Mat resizedMat = new Mat();
            Cv2.Resize(grayMat, resizedMat, new OpenCvSharp.Size(width, height));

            return resizedMat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static Bitmap Base64StringToImage(string inputStr)
        {
            try
            {
                byte[] arr = Convert.FromBase64String(inputStr);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                ms.Close();
                return bmp;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal("Base64StringToImage 转换失败/nException：" + ex.Message);
                return null;
            }
        }

        //public static Mat Base64ToMat(string inputStr)
        //{
        //    var bitmap = Base64StringToImage(inputStr);
        //    if (bitmap == null)
        //    {
        //        return null;
        //    }

        //    return BitmapConverter.ToMat(bitmap);
        //}

        /// <summary>
        /// 将base64字符串转换成WriteableBitmap
        /// </summary>
        /// <param name="imgStr"></param>
        /// <returns></returns>
        public static WriteableBitmap Base64ToWriteableBitmap(string imgStr)
        {
            var bitmap = Base64ToImage(imgStr);
            double scale = 1.0;
            if (bitmap.PixelWidth > 2048)
                scale = 2048.0 / (double)bitmap.PixelWidth;
            else if (bitmap.PixelHeight > 1024)
                scale = 1024.0 / (double)bitmap.PixelWidth;
            else
                scale = 512.0 / (double)bitmap.PixelWidth;

            // 创建一个ScaleTransform对象，设置缩放比例
            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);

            // 使用TransformedBitmap对BitmapImage进行缩放
            TransformedBitmap scaledBitmap = new TransformedBitmap(bitmap, scaleTransform);

            return new WriteableBitmap(scaledBitmap);
        }

        //public static WriteableBitmap Base64MedianBlurToWriteableBitmap(string imgStr, int size = 5)
        //{
        //    var src = Base64ToMat(imgStr);
        //    var result = new Mat();
        //    Cv2.MedianBlur(src, result, size);

        //    // 将Mat转换为字节数组
        //    byte[] imageBytes;

        //    // 使用imencode将Mat编码为字节数组
        //    Cv2.ImEncode(".png", result, out imageBytes); // 可根据需要更改格式，例如".png"

        //    // 转换为Base64字符串
        //    var retStr = Convert.ToBase64String(imageBytes);

        //    return Base64ToWriteableBitmap(retStr);
        //}

        public static async Task<string> PostByFile(string filePath)
        {
            string imgStr = ImageToBase64(filePath);
            return await DoPost(imgStr);
        }

        public static async Task<string> PostByMat(Mat mat)
        {
            string imgStr = MatToBase64(mat);
            return await DoPost(imgStr);
        }

        public static async Task<string> PostByImage(BitmapImage img)
        {
            string imgStr = ImageToBase64(img);
            return await DoPost(imgStr);
        }

        public static async Task<string> PostByWriteableBitmap(WriteableBitmap btimap)
        {
            string imgStr = WriteableBitmapToBase64(btimap);
            return await DoPost(imgStr);
        }

        public static async Task<string> DoPost(string imgStr)
        {
            // 发送 POST 请求
            using (var httpClient = new HttpClient())
            {
                String jsonStr = "{\"parameter\":{\"rsp_media_type\":\"base64\", \"outputType\":\"0\", \"nModes\":\"1\"},\"extra\":{},\"media_info_list\":[{\"media_data\":\"" + imgStr + "\",\"media_profiles\":{\"media_data_type\":\"jpg\"}}]}";
                var response = await httpClient.PostAsync(url, new StringContent(jsonStr, Encoding.UTF8, "application/json"));

                string jsonContent = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var resultOk = JsonConvert.DeserializeObject<ResponseOk>(jsonContent);
                        if (resultOk.media_info_list.Count > 0)
                        {
                            return resultOk.media_info_list[0].media_data;
                        }
                        else
                        {
                            throw new Exception($"Result Ok is Error: {jsonStr}");
                        }
                    case HttpStatusCode.BadRequest:
                        var resultBad = JsonConvert.DeserializeObject<ResponseBad>(jsonContent);
                        throw new Exception(resultBad.ErrorMsg);
                    default:
                        break;
                }
            }

            return null;
        }
        #endregion
    }
}
