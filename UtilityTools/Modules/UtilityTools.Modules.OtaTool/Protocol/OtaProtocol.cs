#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.OtaTool.Protocol
 * 唯一标识：3ff2cbbd-25c6-44cf-9da3-9ab1c9cac18c
 * 文件名：OtaProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/12 11:57:08
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.OtaTool.Protocol
{
    /// <summary>
    /// OTA升级指令类型
    /// </summary>
    public enum EnumOtaCommandType
    {
        [Description("升级请求")]
        OTA_REQUEST = 0xFFF0,

        [Description("文件传输")]
        OTA_TRANSFER = 0xFFF1,

        [Description("终止升级")]
        OTA_ABORT = 0xFFF2,

        [Description("重启升级")]
        OTA_RESTART = 0xFFF3,
    }

    public static class OtaProtocol
    {
        #region ------------StaticMethod------------

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(EnumOtaCommandType command, byte[] data)
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
            var id = BitConverter.GetBytes((ushort)0x0000);
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }

        /// <summary>
        /// 获取请求OTA指令
        /// </summary>
        /// <param name="fileLength">文件长度</param>
        /// <param name="frameCount">预期传输帧数</param>
        /// <param name="crc">CRC校验值</param>
        /// <returns></returns>
        public static byte[] GetRequestOtaCmd(int fileLength, int frameCount, ushort crc)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(fileLength);
            writer.Write(frameCount);
            writer.Write(crc);
            return GetCmd(EnumOtaCommandType.OTA_REQUEST, writer.EndWrite());
        }

        /// <summary>
        /// 获取请求OTA指令
        /// </summary>
        /// <param name="frameId">帧ID</param>
        /// <param name="data">传输数据</param>
        /// <returns></returns>
        public static byte[] GetTransferOtaCmd(int frameId, byte[] data)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(frameId);
            writer.Write(data);
            return GetCmd(EnumOtaCommandType.OTA_TRANSFER, writer.EndWrite());
        }

        /// <summary>
        /// 获取终止OTA指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAbortOtaCmd()
        {
            byte[] array = new byte[1];
            return GetCmd(EnumOtaCommandType.OTA_ABORT, array);
        }

        /// <summary>
        /// 获取重启指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRestartCmd()
        {
            byte[] array = new byte[1];
            return GetCmd(EnumOtaCommandType.OTA_RESTART, array);
        }
        #endregion
    }
}
