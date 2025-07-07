#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.TemperatureController.Protocol
 * 唯一标识：6db7eace-595a-492f-b07a-483c05cc2b3e
 * 文件名：TemperatureControllerProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/23 15:30:41
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
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.TemperatureController.Protocol
{
    /// <summary>
    /// 
    /// </summary>
    public enum EnumTemperatureControllerCommandType
    {
        [Description("设置摄氏度")]
        CMD_SETCENTIGRADE = 0x0001,

        [Description("设置开氏度")]
        CMD_SETKELVIN = 0x0002,

        [Description("设置摄氏度下的PID")]
        CMD_SETCENTIGRADE_PID = 0x0003,

        [Description("设置开氏温度下的PID")]
        CMD_SETKELVIN_PID = 0x0004,

        [Description("设置手动模式下的电流参数")]
        CMD_SETMANUAL_CURRENT = 0x0005,

        [Description("停止控制器的控制参数")]
        CMD_STOPCONTROLLER = 0x0006,

        [Description("释放控制器的控制功能")]
        CMD_RELEASECONTROLLER = 0x0007,

        [Description("获取温控仪的数据")]
        CMD_GETDATA = 0x0010,
    }

    internal static class TemperatureControllerProtocol
    {
        static ushort DeviceID = 0x0105;

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(EnumTemperatureControllerCommandType command, byte[] data)
        {
            // 五轴电机的data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                data = tmp;
            }

            var cmd = BitConverter.GetBytes((ushort)command);
            var id = BitConverter.GetBytes((ushort)DeviceID);
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }

        /// <summary>
        /// 获取设置摄氏度指令码
        /// </summary>
        /// <param name="temp">摄氏度</param>
        /// <returns></returns>
        public static byte[] SetCentigradeCommand(float temp)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(temp);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETCENTIGRADE, writer.EndWrite());
        }

        /// <summary>
        /// 获取设置开氏度指令码
        /// </summary>
        /// <param name="temp">开氏度</param>
        /// <returns></returns>
        public static byte[] SetKelvinCommand(float temp) 
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(temp);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETKELVIN, writer.EndWrite());
        }

        /// <summary>
        /// 设置摄氏温度下的PID参数指令
        /// </summary>
        /// <param name="temp">目标摄氏度</param>
        /// <param name="cur">目标电流</param>
        /// <param name="maxCur">最大电流</param>
        /// <param name="kp">P调节参数</param>
        /// <param name="ki">I调节参数</param>
        /// <param name="kd">D调节参数</param>
        /// <returns></returns>
        public static byte[] SetCentigradePidCommand(float temp, float cur, float maxCur, float kp, float ki, float kd) 
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(temp);
            writer.Write(cur);
            writer.Write(maxCur);
            writer.Write(kp);
            writer.Write(ki);
            writer.Write(kd);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETCENTIGRADE_PID, writer.EndWrite());
        }

        /// <summary>
        /// 设置开氏温度下的PID参数指令
        /// </summary>
        /// <param name="temp">目标开氏度</param>
        /// <param name="cur">目标电流</param>
        /// <param name="maxCur">最大电流</param>
        /// <param name="kp">P调节参数</param>
        /// <param name="ki">I调节参数</param>
        /// <param name="kd">D调节参数</param>
        /// <returns></returns>
        public static byte[] SetKelvinPidCommand(float temp, float cur, float maxCur, float kp, float ki, float kd)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(temp);
            writer.Write(cur);
            writer.Write(maxCur);
            writer.Write(kp);
            writer.Write(ki);
            writer.Write(kd);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETKELVIN_PID, writer.EndWrite());
        }

        /// <summary>
        /// 设置手动模式下电流参数指令
        /// </summary>
        /// <param name="cur">设置电流</param>
        /// <param name="maxCur">最大电流</param>
        /// <returns></returns>
        public static byte[] SetManualCurrentCommand(float cur, float maxCur)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(cur);
            writer.Write(maxCur);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETMANUAL_CURRENT, writer.EndWrite());
        }

        /// <summary>
        /// 设置停止控制指令
        /// </summary>
        /// <returns></returns>
        public static byte[] StopCommand()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETMANUAL_CURRENT, writer.EndWrite());
        }

        /// <summary>
        /// 释放控制指令
        /// </summary>
        /// <returns></returns>
        public static byte[] ReleaseCommand()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SETMANUAL_CURRENT, writer.EndWrite());
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class TemperatureControllerPacket
    {
        public TemperatureControllerPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public EnumTemperatureControllerCommandType CmdType { get { return (EnumTemperatureControllerCommandType)BitConverter.ToUInt16(this.packet.command); } }
        public ushort DeviceID { get { return BitConverter.ToUInt16(this.packet.id); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class TemperatureControllerParser
    {
        public TemperatureControllerParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }

        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new TemperatureControllerPacket(packet));
        }

        public event EventHandler<TemperatureControllerPacket> PacketReceivedEvent;


        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }


        private ZepGenericProtocolParser _parser;
    }
}
