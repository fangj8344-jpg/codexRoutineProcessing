using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.OtaTool.Protocol
{
    public enum ENUM_ETH_CMD : short
    {
        /// <summary>
        /// 设置以太网ip
        /// </summary>
        CMD_SetIP = 0x0630,
        /// <summary>
        /// 以太网握手指令
        /// </summary>
        CMD_HandShake = 0x0631,
        /// <summary>
        /// 获取以太网的ip地址
        /// </summary>
        CMD_GetIP = 0X0632,
        ///以安全方式修改MAC地址
        CMD_Set_MAC_Safely = 0x0633,
        /// <summary>
        /// 设置子网掩码
        /// </summary>
        CMD_Set_NetMask = 0x0636,
        /// <summary>
        /// 获取默认网关
        /// </summary>
        CMD_Get_NetMask = 0x0637,
        /// <summary>
        /// 设置默认网关
        /// </summary>
        CMD_Set_Gateway = 0x0638,
        /// <summary>
        /// 获取默认网关
        /// </summary>
        CMD_Get_Gateway = 0x0639,

    }
    internal class EthProtocol
    {

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        public static byte[] GetCmd(ENUM_ETH_CMD command, int? deviceID, byte[] data)
        {
            // data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                data = tmp;
            }

            var cmd = BitConverter.GetBytes((ushort)command);
            var id = BitConverter.GetBytes((ushort)deviceID);
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }
        /// <summary>
        /// 获取对方ip地址
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public static byte[] GetIP(int? deviceID = 0)
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(ENUM_ETH_CMD.CMD_GetIP, deviceID, writer.EndWrite());
        }
        /// <summary>
        /// 设置以太网IP
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SetIP(byte[] data,int? deviceID = 0) 
        {
            return GetCmd(ENUM_ETH_CMD.CMD_SetIP,deviceID,data);
        }
    }
}
