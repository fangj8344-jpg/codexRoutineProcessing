#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：fbc3205f-170c-4bf5-ad9f-74217e7fe9e9
 * 文件名：DataTypeCaster
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 15:34:27
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public static class DataTypeCaster
    {
        /// <summary>
        /// 类型转换，将byte[]转换成明文字符串显示
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] arr, int length)
        {
            if (length > arr.Length)
                length = arr.Length;

            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += String.Format(" 0x{0:X2}", arr[i]);
            }

            return result;
        }

      
        /// <summary>
        /// 讲16进制的消息字符串转换成byte数组，以空格为分割符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string str)
        {
            var list = str.Split(' ');
            byte[] arr = new byte[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                if (byte.TryParse(list[i], out var value))
                {
                    arr[i] = value;
                }
            }
            return arr;
        }

        /// <summary>
        /// 将struct数据内容copy到byte数组中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] StructToByteArray<T>(T data) where T : struct
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(data);
            byte[] byteArray = new byte[size];
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            System.Runtime.InteropServices.Marshal.StructureToPtr(data, ptr, true);
            System.Runtime.InteropServices.Marshal.Copy(ptr, byteArray, 0, size);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            return byteArray;
        }

        /// <summary>
        /// Int转二进制字符串
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string IntToBinary(int x)
        {
            char[] buff = new char[32];

            for (int i = 31; i >= 0; i--)
            {
                int mask = 1 << i;
                buff[31 - i] = (x & mask) != 0 ? '1' : '0';
            }

            return new string(buff);
        }


        /// <summary>
        /// 类型转换，将byte[]转二进制字符串
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ByteArrayToBinaryStr(byte[] bytes)
        {
            string strResult = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                string strTemp = System.Convert.ToString(bytes[i], 2);
                strTemp = strTemp.Insert(0, new string('0', 8 - strTemp.Length));

                strResult += strTemp;
            }
            return strResult;
        }


    }
}
