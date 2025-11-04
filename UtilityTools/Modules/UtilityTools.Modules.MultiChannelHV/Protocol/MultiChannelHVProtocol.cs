using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol.MultiChannelHVProtocol;

namespace UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol
{
    public class MultiChannelHVProtocol
    {
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0204);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public enum MultiChannelHVFunctionCode
        {
            [Description("获取单路隔离电压")]
            GET_IV = 0x0100,
            [Description("获取1-6路隔离电压")]
            GET_P1_6_IV = 0x0101,
            [Description("获取7-13路隔离电压")]
            GET_P7_13_IV = 0x0102,
            [Description("设置单路隔离电压")]
            SET_IV = 0x0103,
            [Description("获取整机悬浮高压")]
            GET_HV = 0x0200,
            [Description("设置整机悬浮高压")]
            SET_HV = 0x0201,
            [Description("设置高压控制板初始化")]
            SET_INIT = 0x0400,
            [Description("高压初始化状态查询")]
            GET_INIT_STATE = 0x0401,
            [Description("隔离板错误清除")]
            ERROR_CLEAR = 0x0402,          
            [Description("高压箱初始化取消，关闭输出")]
            DISABLE_OUTPUT = 0x0403,
            [Description("固件版本获取")]
            FV = 0x0005,
        }
        public enum MultiChannelHVInitState
        {
            [Description("空闲状态")]
            IDLE = 0x00,
            [Description("初始化中")]
            INITALIZING = 0x01,
            [Description("初始化完成")]
            RUNNUNG = 0x02,
            [Description("高压错误")]
            ERROR = 0x06,
        }
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(MultiChannelHVFunctionCode command, byte[] data)
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
            var id = DeviceID;
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }
        /// <summary>
        /// 获取单路隔离电压
        /// </summary>
        /// <returns></returns>
        public static byte[] GetIV(byte channel)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)channel);
            return GetCmd(MultiChannelHVFunctionCode.GET_IV, writer.EndWrite());
        }
        /// <summary>
        /// 获取1-6路隔离电压
        /// </summary>
        /// <returns></returns>
        public static byte[] Get1To6IV()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.GET_P1_6_IV, writer.EndWrite());
        }
        /// <summary>
        /// 获取7-13路隔离电压
        /// </summary>
        /// <returns></returns>
        public static byte[] Get7To13IV()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.GET_P7_13_IV, writer.EndWrite());
        }
        /// <summary>
        /// 设置隔离电压
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] SetIV(byte channel, ushort iv)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)channel);
            writer.Write(iv);
            return GetCmd(MultiChannelHVFunctionCode.SET_IV, writer.EndWrite());
        }
        /// <summary>
        /// 获取整机悬浮高压
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHV()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.GET_HV, writer.EndWrite());
        }

        /// <summary>
        /// 设置整机悬浮高压
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        public static byte[] SetHV(UInt16 hv)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(hv);
            return GetCmd(MultiChannelHVFunctionCode.SET_HV, writer.EndWrite());
        }
        /// <summary>
        /// 高压控制板初始化
        /// </summary>
        /// <returns></returns>
        public static byte[] SetInit()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.SET_INIT, writer.EndWrite());
        }
        /// <summary>
        /// 高压初始化状态查询
        /// </summary>
        /// <returns></returns>
        public static byte[] GetInitState()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.GET_INIT_STATE, writer.EndWrite());
        }

        /// <summary>
        /// 隔离板错误清除
        /// </summary>
        /// <returns></returns>
        public static byte[] IErrorClear()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.ERROR_CLEAR, writer.EndWrite());
        }

        /// <summary>
        /// 关闭输出，取消初始化
        /// </summary>
        /// <returns></returns>
        public static byte[] DisableOutput()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.DISABLE_OUTPUT, writer.EndWrite());
        }
        public static byte[] GetFV()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.FV, writer.EndWrite());
        }


    }
    public class MultiChannelHVPacket
    {
        public MultiChannelHVPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public MultiChannelHVFunctionCode CmdType { get { return (MultiChannelHVFunctionCode)BitConverter.ToUInt16(this.packet.command); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class MultiChannelHVParser
    {
        public MultiChannelHVParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }

        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new MultiChannelHVPacket(packet));
        }

        public event EventHandler<MultiChannelHVPacket> PacketReceivedEvent;


        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }


        private ZepGenericProtocolParser _parser;
    }
}
