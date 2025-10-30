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
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0300);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public enum MultiChannelHVFunctionCode
        {
            [Description("获取高压")]
            GET_HV = 0x01,

            [Description("获取电压")]
            GET_IV = 0x02,
         
            [Description("功能初始化")]
            SET_INIT = 0x03,

            [Description("隔离班错误清楚")]
            ERROR_CLEAR = 0x04,

            [Description("关闭输出，取消初始化")]
            DISABLE_OUTPUT = 0x05,
            [Description("高压设置")]
            SET_HV = 0x06,
            [Description("隔离电压模块输出设置")]
            SET_IV = 0x07,

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
        /// 获取高压
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHV()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.GET_HV, writer.EndWrite());
        }

        /// <summary>
        /// 设置高压
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        public static byte[] SetHV(int hv)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(hv);
            return GetCmd(MultiChannelHVFunctionCode.SET_HV, writer.EndWrite());
        }

        /// <summary>
        /// 获取隔离电压
        /// </summary>
        /// <returns></returns>
        public static byte[] GetIV(byte channel)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)channel);
            return GetCmd(MultiChannelHVFunctionCode.GET_IV, writer.EndWrite());
        }


        /// <summary>
        /// 设置隔离电压
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] SetIV(byte channel ,int iv)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)channel);
            writer.Write(iv);
            return GetCmd(MultiChannelHVFunctionCode.SET_IV, writer.EndWrite());
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static byte[] SetInit()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(MultiChannelHVFunctionCode.SET_INIT, writer.EndWrite());
        }

       /// <summary>
       /// 隔离板错误清楚
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
