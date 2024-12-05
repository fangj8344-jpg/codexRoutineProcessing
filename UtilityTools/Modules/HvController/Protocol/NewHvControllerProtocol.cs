#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.HvController.Protocol
 * 唯一标识：9d80e101-b602-49bd-b51d-a642ce04890b
 * 文件名：NewHvControllerProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/12/5 14:23:51
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
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.HvController.Protocol
{
    internal static class NewHvControllerProtocol
    {
        /// <summary>
        /// 打包指令头和指令尾
        /// </summary>
        /// <param name="cmdCode"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static byte[] GetCommand(byte cmdCode, byte[] cmd)
        {
            byte[] msgBuf = new byte[13];
            for (int i = 0; i < 4; ++i)
            {
                msgBuf[i] = 0xFF;
                msgBuf[12 - i] = 0xFE;
            }

            msgBuf[4] = cmdCode;
            cmd.CopyTo(msgBuf, 5);

            return msgBuf;
        }

        /// <summary>
        /// 请求高压状态指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRequestCommand() 
        {
            return GetCommand(0xF0, new byte[4]);
        }

        /// <summary>
        /// 请求初始化指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetInitCommand() 
        {
            return GetCommand(0xF1, new byte[4]);
        }

        /// <summary>
        /// 请求栅极电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-2000，单位V</param>
        /// <returns></returns>
        public static byte[] GetGridVolCommand(float vol) 
        {
            ushort value = (ushort)(vol * 0xFFFF / 2000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            Array.Copy(arr, msgBuf, arr.Length);

            return GetCommand(0xF2, msgBuf);
        }

        /// <summary>
        /// 请求加热电流指令
        /// </summary>
        /// <param name="cur">电流值，取值范围为0-3.2，单位A</param>
        /// <param name="step">电流变换步长</param>
        /// <returns></returns>
        public static byte[] GetHeatCurCommand(float cur, byte step)
        {
            ushort value = (ushort)(cur * 0xFFFF / 3.2);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            Array.Copy(arr, msgBuf, arr.Length);
            msgBuf[3] = step;

            return GetCommand(0xF3, msgBuf);
        }

        /// <summary>
        /// 请求吸取极电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-6000，单位V</param>
        /// <returns></returns>
        public static byte[] GetEmissionVolCommand(float vol)
        {
            ushort value = (ushort)(vol * 0xFFFF / 6000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            Array.Copy(arr, msgBuf, arr.Length);

            return GetCommand(0xF4, msgBuf);
        }

        /// <summary>
        /// 请求加速电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-16000，单位V</param>
        /// <returns></returns>
        public static byte[] GetAccVolCommand(float vol) 
        {
            ushort value = (ushort)(vol * 0xFFFF / 16000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            Array.Copy(arr, msgBuf, arr.Length);

            return GetCommand(0xF4, msgBuf);
        }

        /// <summary>
        /// 请求关枪指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetCloseHvCommand() 
        {
            return GetCommand(0xF6, new byte[4]);
        }
    }

    internal class NewHvControllerProtocolParser
    { 
        
    }
}
