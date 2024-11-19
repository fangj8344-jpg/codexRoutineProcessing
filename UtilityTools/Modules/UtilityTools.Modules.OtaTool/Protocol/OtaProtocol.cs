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
{/// <summary>
 /// OTA升级指令类型
 /// </summary>
    public enum EnumOtaCommandType
    {
        [Description("获取硬件版本信息")]
        OTA_GET_HWV = 0x0004,

        [Description("获取固件版本信息(字符串表达)")]
        OTA_GET_FMV = 0x005,

        [Description("广播通信")]
        OTA_SYS_BRADCAST = 0x0006,

        [Description("获取固件版本信息（字节表达）（用于升级校验）")]
        OTA_GET_UPGRADE_FMV = 0x0010,

        [Description("查询设备运行状态")]
        OTA_GET_STATUS = 0x0011,

        [Description("升级请求")]
        OTA_REQUEST = 0x0012,

        [Description("终止升级")]
        OTA_ABORT = 0x0013,

        [Description("文件传输")]
        OTA_TRANSFER = 0x0014,

        [Description("重启升级")]
        OTA_RESTART = 0X0015,
    }
    public enum EnumDeviceID 
    {
        [Description("主控制板")]
        DeviceID_MAIN_CONTROL_PANEL = 0X0100,

        [Description("真空控制板")]
        DeviceID_VACUUM_CONTROL_PANEL = 0X0101,

        [Description("灯带控制板")]
        DeviceID_LIGHT_BELT_CONTROL_PANEL = 0X0102,

        [Description("20kv高压箱")]
        DeviceID_20KV_HIGH_PRESSURE_BOX = 0X0200,

        [Description("五轴电机控制板")]
        DeviceID_FIVEAXIS_MOTOR_CONTROL_PANEL = 0X0300
    }
    public class OtaToolDataPacket
    {
        public OtaToolDataPacket(DataPacket packet)
        {
            this.packet = packet;
        }
        public EnumOtaCommandType CmdType { get { return (EnumOtaCommandType)BitConverter.ToUInt16(this.packet.command); } }
        public EnumDeviceID DeviceID { get { return (EnumDeviceID)BitConverter.ToUInt16(this.packet.id); } }
        public byte[] DataSource { get => packet.data; }
        private DataPacket packet;
    }

    public class OtaToolProtocolParser
    {
        public OtaToolProtocolParser() 
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GeneriaPackReceived;

        }
        private void GeneriaPackReceived(object sender,DataPacket packet)
        {
            PacketReceivedEvent(this, new OtaToolDataPacket(packet));
        }

        public event EventHandler<OtaToolDataPacket> PacketReceivedEvent;

        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }

        private ZepGenericProtocolParser _parser;
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
        public static byte[] GetCmd(EnumOtaCommandType command, EnumDeviceID? deviceID, byte[] data)
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
            var temp = DeviceIdTransformUInt16(deviceID);
            var id = BitConverter.GetBytes(temp);
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }

        /// <summary>
        /// 获取请求OTA指令
        /// </summary>
        /// <param name="fileLength">文件长度</param>
        /// <param name="frameCount">预期传输帧数</param>
        /// <param name="crc">CRC校验值</param>
        /// <returns></returns>
        public static byte[] GetRequestOtaCmd(int fileLength, int frameCount, EnumDeviceID deviceID, ushort crc)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(fileLength);
            writer.Write(frameCount);
            writer.Write(crc);
            return GetCmd(EnumOtaCommandType.OTA_REQUEST, deviceID, writer.EndWrite());
        }


        /// <summary>
        /// 传输OTA指令
        /// </summary>
        /// <param name="frameId">帧ID</param>
        /// <param name="data">传输数据</param>
        /// <returns></returns>
        public static byte[] GetTransferOtaCmd(int frameId, byte[] data, EnumDeviceID deviceID)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(frameId);
            writer.Write(data);
            return GetCmd(EnumOtaCommandType.OTA_TRANSFER, deviceID, writer.EndWrite());
        }

        /// <summary>
        /// 获取终止OTA指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAbortOtaCmd(EnumDeviceID deviceID)
        {
            byte[] array = new byte[1];
            return GetCmd(EnumOtaCommandType.OTA_ABORT, deviceID, array);
        }

        /// <summary> 
        /// 获取重启指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRestartCmd(EnumDeviceID deviceID)
        {
            byte[] array = new byte[1];
            return GetCmd(EnumOtaCommandType.OTA_RESTART, deviceID, array);
        }


        public static EnumDeviceID? DeviceIdTransformEnum(UInt16 device)
        {
            switch (device)
            {
                case 0X0100: return EnumDeviceID.DeviceID_MAIN_CONTROL_PANEL; break;
                case 0X0101: return EnumDeviceID.DeviceID_VACUUM_CONTROL_PANEL; break;
                case 0X0102: return EnumDeviceID.DeviceID_LIGHT_BELT_CONTROL_PANEL; break;
                case 0X0200: return EnumDeviceID.DeviceID_20KV_HIGH_PRESSURE_BOX; break;
                case 0X0300: return EnumDeviceID.DeviceID_FIVEAXIS_MOTOR_CONTROL_PANEL; break;
                default: return null;
            }
        }
        public static string? DeviceIdTransformString(UInt16 device)
        {
            switch (device)
            {
                case 0X0100: return "主控制板"; break;
                case 0X0101: return "真空控制板"; break;
                case 0X0102: return "灯带控制板"; break;
                case 0X0200: return "20kv高压箱"; break;
                case 0X0300: return "五轴电机控制板"; break;
                default: return null;

            }

        }
        public static UInt16 DeviceIdTransformUInt16(EnumDeviceID? deviceID)
        {
            switch (deviceID)
            {
                case EnumDeviceID.DeviceID_MAIN_CONTROL_PANEL : return 0X0100; break;
                case EnumDeviceID.DeviceID_VACUUM_CONTROL_PANEL : return 0X0101; break;
                case EnumDeviceID.DeviceID_LIGHT_BELT_CONTROL_PANEL : return 0X0102; break;
                case EnumDeviceID.DeviceID_20KV_HIGH_PRESSURE_BOX : return 0X0200; break;
                case EnumDeviceID.DeviceID_FIVEAXIS_MOTOR_CONTROL_PANEL : return 0X0300; break;
                default: return 00;
            }
        }
    }






        #endregion
    
}
