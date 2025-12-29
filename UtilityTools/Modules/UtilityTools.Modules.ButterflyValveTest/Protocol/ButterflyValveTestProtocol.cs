using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.ButterflyValveTestProtocol;

namespace UtilityTools.Modules.ButterflyValveTest.Protocol
{
   
    public class ButterflyValveTestProtocol
    {
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0313);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public enum ButterflyValveCmdCode
        {
            [Description("设置电机运行停止")]
            GATEVALVE_SET_RS = 0x0101,
            [Description("设置阀门开度")]
            GATEVALVE_SET_POS = 0x0102,
            [Description("获取控制板状态")]
            GATEVALVE_GET_STATUS = 0x0106,
            [Description("获取电位器或者编码器的绝对数值")]
            GATEVALVE_GET_ABS_POS = 0X0316
        }
    
        public enum ButterflyValveRunuingDirection
        {
            [Description("正向运动")]
            FORWARD = 0x01,
            [Description("反向运动")]
            REVERSE = 0x02,
            [Description("停止")]
            STOP = 0x03,
            [Description("未连接")]
            NOT_CONNECTION,

        }
        public enum ButterflyValveHardwareLimit
        {
            [Description("触发全关限位")]
            TRIGGER_CLOSE_LIMIT = 0x01,
            [Description("触发全开限位")]
            TRIGGER_OPEN_LIMIT = 0x02,
            [Description("未触发限位")]
            LIMIT_NOT_TRIGGERED = 0x03,
            [Description("触发开关限位")]
            ALL_TRIGGERED ,
            [Description("未连接")]
            NOT_CONNECTION,
        }
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(ButterflyValveCmdCode command, byte[] data)
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
        /// 设置电机运行停止
        /// </summary>
        /// <param name="channel"> 暂定</param>
        /// <param name="runState">运行状态，0:停止，1:运行</param>
        /// <returns></returns>
        public static byte[] SetRs( byte runState, byte channel = 0x00)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(channel);
            writer.Write(runState);
            return GetCmd(ButterflyValveCmdCode.GATEVALVE_SET_RS, writer.EndWrite());
        }
        /// <summary>
        /// 设置阀门开度
        /// </summary>
        /// <param name="pos">阀门开度0-100(全关-全开)</param>
        /// <param name="reservedByte1">保留字段</param>
        /// <param name="reservedByte2">保留字段</param>
        /// <returns></returns>
        public static byte[] SetPos(float pos,byte reservedByte1 = 0x00, byte reservedByte2 = 0x00)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(reservedByte1);
            writer.Write(reservedByte2);
            writer.Write(pos);
            return GetCmd(ButterflyValveCmdCode.GATEVALVE_SET_POS, writer.EndWrite());
        }
        /// <summary>
        /// 获取控制板状态
        /// </summary>
        /// <returns></returns>
        public static byte[] GetStatus()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(ButterflyValveCmdCode.GATEVALVE_GET_STATUS, writer.EndWrite());
        }
        /// <summary>
        /// 获取编码器
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAbsPos()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(ButterflyValveCmdCode.GATEVALVE_GET_ABS_POS, writer.EndWrite());
        }
    }

    public class ButterflyValveTestPacket
    {
        public ButterflyValveTestPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public ButterflyValveCmdCode CmdType { get { return (ButterflyValveCmdCode)BitConverter.ToUInt16(this.packet.command); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class ButterflyValveTestParser
    {
        public ButterflyValveTestParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }
        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new ButterflyValveTestPacket(packet));
        }
        public event EventHandler<ButterflyValveTestPacket> PacketReceivedEvent;
        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }
        private ZepGenericProtocolParser _parser;
    }
}
