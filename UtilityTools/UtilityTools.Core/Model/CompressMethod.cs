#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：8580525f-3720-46df-9d44-65b08a25be33
 * 文件名：CompressMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/6/6 14:29:58
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
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public class CompressMethod
    {
        // 压缩数据
        public static byte[] CompressData(string data)
        {
            byte[] inputData = Encoding.UTF8.GetBytes(data);

            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(inputData, 0, inputData.Length);
                    return ms.ToArray();  // 返回压缩后的数据
                }
            }
        }

        // 解压数据
        public static string DecompressData(byte[] compressedData)
        {
            using (MemoryStream ms = new MemoryStream(compressedData))
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();  // 返回解压后的数据
                    }
                }
            }
        }


        // Deflate 压缩（更小的头部开销）
        public static byte[] CompressDeflate(string data)
        {
            byte[] inputData = Encoding.UTF8.GetBytes(data);
            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.SmallestSize))
            {
                deflateStream.Write(inputData, 0, inputData.Length);
            }
            return outputStream.ToArray();
        }

        // Deflate 解压缩
        public static byte[] DecompressDeflate(byte[] compressedData)
        {
            using var inputStream = new MemoryStream(compressedData);
            using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            deflateStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}
