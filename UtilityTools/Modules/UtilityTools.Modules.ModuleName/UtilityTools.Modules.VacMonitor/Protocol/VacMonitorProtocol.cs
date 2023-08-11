#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.VacMonitor.Protocol
 * 唯一标识：c5b6f6e0-2a5a-49b8-ba25-71731e46f0a5
 * 文件名：VacMonitorProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:48:27
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

namespace UtilityTools.Modules.VacMonitor.Protocol
{
    public class VacMonitorProtocol
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static byte[] GetVacuumValue(int index)
        {
            byte key = (byte)'U';
            switch (index)
            {
                case 1: key = (byte)'U'; break;
                case 2: key = (byte)'V'; break;
                case 3: key = (byte)'W'; break;
            }
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = key;
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }

        /// <summary>
        /// 获取枪头真空度
        /// </summary>
        /// <returns></returns>
        public static byte[] GetGunVac()
        {
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = (byte)'V';
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }

        /// <summary>
        /// 获取样品仓真空度
        /// </summary>
        /// <returns></returns>
        public static byte[] GetSampleVac()
        {
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = (byte)'W';
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }

        /// <summary>
        /// 设置校验信息
        /// </summary>
        /// <param name="buf"></param>
        public static void SetCheckout(ref byte[] buf)
        {
            byte checkout = 0;
            int i = 0;
            for (; i < buf.Length - 1; i++)
            {
                checkout += buf[i];
            }

            buf[i] = checkout;
        }

        /// <summary>
        /// 检查回包是否是来自虚拟设备的回复报文
        /// </summary>
        /// <param name="response">回包数据</param>
        /// <param name="length">数据长度</param>
        /// <returns>是否是虚拟设备的回包响应</returns>
        public static bool CheckIsVirtualDevice(byte[] response, int length)
        {
            string ret = Encoding.Default.GetString(response, 0, length);
            return string.Equals(ret, "VirtualDevice");
        }

        /// <summary>
        /// 找出回包格式字节数组
        /// </summary>
        /// <param name="response">回包字节数组</param>
        /// <param name="length">回包数据长度</param>
        /// <returns>符合协议的回包格式字节数组，没找到则返回null</returns>
        public static byte[] FindResponse(ref byte[] response, ref int length)
        {
            if (length == 0)
                return null;
            string ret = Encoding.Default.GetString(response, 0, length);
            length = 0;
            return response;
        }

        /// <summary>
        /// 计算校验位
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool CheckResponse(byte[] response)
        {
            return true;
        }

        /// <summary>
        /// 试图解析数据回包中的真空读数
        /// </summary>
        /// <param name="response">回包报文</param>
        /// <param name="pos">回包报文</param>
        /// <param name="ret">真空读数</param>
        /// <returns></returns>
        public static bool TryParseResponse(string response, out int index, out double ret)
        {
            index = 1;
            ret = 0.0;
            var list = response.Split(' ', ';');
            if (list.Length != 3)
            {
                return false;
            }

            if (list[0].Contains('U'))
            {
                index = 1;
            }
            else if (list[0].Contains('V'))
            {
                index = 2;
            }
            else if (list[0].Contains('W'))
            {
                index = 3;
            }
            else
            {
                return false;
            }

            return double.TryParse(list[1], out ret);
        }
        #endregion
    }
}
