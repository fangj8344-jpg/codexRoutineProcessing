#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：HvController.Protocol
 * 唯一标识：5b17b758-878f-4d75-bc4c-1727b845b04a
 * 文件名：HvControllerProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/23 9:50:30
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

namespace UtilityTools.Modules.HvController.Protocol
{
    public static class HvControllerProtocol
    {
        public const int MsgMaxLength = 256;

        #region ------------StaticMethod------------
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
        /// 获取开枪指令
        /// </summary>
        /// <param name="IsNewFila"></param>
        /// <returns></returns>
        public static byte[] GetGunOpenCmd(bool IsNewFila)
        {
            byte cmdCode = 0x80;
            byte[] cmd = new byte[4] { 0x01, 0x00, 0x00, 0x00 };
            cmd[0] = IsNewFila ? (byte)0x01 : (byte)0x00;
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取关枪指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetGunCloseCmd()
        {
            byte cmdCode = 0x90;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取恢复默认指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRedefaultCmd()
        {
            byte cmdCode = 0x6A;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取清除错误指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetClearErrCmd()
        {
            byte cmdCode = 0x72;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取设置灯丝参数指令
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public static byte[] GetSetFilaParamCmd(byte p1, byte p2, byte p3, byte p4)
        {
            byte cmdCode = 0x7F;
            byte[] cmd = new byte[4] { p1, p2, p3, p4 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取变更高压数值指令
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        public static byte[] GetSetChangedHVCmd(byte hv)
        {
            byte cmdCode = 0x71;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            cmd[0] = hv;
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取启动高压指令
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        public static byte[] GetSetStartHVCmd(byte hv)
        {
            byte cmdCode = 0x70;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            cmd[0] = hv;
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取设置灯丝类型指令
        /// </summary>
        /// <param name="FilaType"></param>
        /// <returns></returns>
        public static byte[] GetSetFilaTypeCmd(byte FilaType)
        {
            byte cmdCode = 0x73;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            cmd[0] = (byte)FilaType;
            return GetCommand(cmdCode, cmd);

        }

        /// <summary>
        /// 获取检查高压状态指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetCheckHvStatusCmd()
        {
            byte cmdCode = 0xA0;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 获取高压寄存器数据指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetReadRegisterCmd()
        {
            byte cmdCode = 0x6F;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 读取高压参数
        /// </summary>
        public static byte[] GetReadHvParamCmd()
        {
            byte cmdCode = 0x34;
            byte[] cmd = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            return GetCommand(cmdCode, cmd);
        }

        /// <summary>
        /// 找出回包格式字节数组
        /// </summary>
        /// <param name="response">回包字节数组</param>
        /// <param name="length">回包数据长度</param>
        /// <returns>符合协议的回包格式字节数组，没找到则返回null</returns>
        public static byte[] FindResponse(ref byte[] response, ref int length)
        {
            string test = Encoding.Default.GetString(response, 0, length);
            int sPos = 0;
            int ePos = 0;
            bool isStart = false;
            bool isEnd = false;

            while (!isStart)
            {
                sPos = Array.IndexOf(response, (byte)0xFF, sPos, length - sPos);
                if (sPos < 0 || sPos > length - 8)
                {
                    return null;
                }
                if (response[sPos + 1] == 0xFF && response[sPos + 2] == 0xFF
                    && response[sPos + 3] == 0xFF)
                {
                    isStart = true;
                    ePos = sPos;
                }
                else
                {
                    sPos++;
                }
            }

            while (!isEnd)
            {
                ePos = Array.IndexOf(response, (byte)0xEE, ePos, length - ePos);
                if (ePos < 0 || ePos > length - 4)
                {
                    return null;
                }
                if (response[ePos + 1] == 0xEE && response[ePos + 2] == 0xEE
                    && response[ePos + 3] == 0xEE)
                {
                    isEnd = true;
                }
                else
                {
                    ePos++;
                }
            }

            var res = response.Skip(sPos).Take(ePos - sPos + 4).ToArray();
            Array.Copy(response, ePos + 4, response, 0, response.Length - ePos - 4);
            length = length - ePos - 4;

            return res;
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
        /// 匹配发送和收到回包的结果
        /// </summary>
        /// <param name="send">发送指令</param>
        /// <param name="response">回复指令</param>
        /// <returns>true:匹配成功；false：匹配失败</returns>
        public static bool MatchDetection(byte[] send, byte[] response)
        {
            switch (send[4])
            {
                case 0x70:
                    return response[4] == 0x70;
                case 0x80:
                    return (response[4] == 0xF7 ||
                        response[4] == 0xE0 ||
                        response[4] == 0xE1 ||
                        response[4] == 0xE2 ||
                        response[4] == 0xE3);
                case 0x90:
                    return response[4] == 0x90;
                case 0x71:
                    return response[4] == 0x71;
                case 0x73:
                    return response[4] == 0x73;
                case 0x7F:
                    return response[4] == 0x7F;
                case 0x72:
                    return response[4] == 0x72;
                case 0xA0:
                    return response[4] == 0xA0;
                case 0x34:
                    return Encoding.UTF8.GetString(response).Contains("readhHv_");
                case 0x6F:
                    return Encoding.UTF8.GetString(response).Contains("NUM_");
                default:
                    break;
            }
            return false;
        }
        #endregion
    }
}
