using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.ButterflyValveTestProtocol;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.OldButterflyValveTestProtocol;

namespace UtilityTools.Modules.ButterflyValveTest.Protocol
{
    public class OldButterflyValveTestProtocol
    {
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0101);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x01 };
        public enum OldButterflyValveCmdCode
        {
            [Description("闸板阀控制")]
            SET_GATE_VALVE = 0x0803,
            [Description("针阀控制")]
            SET_LEAK_VALVE = 0x0804,
            [Description("读取闸板阀控制值")]
            GET_GATE_VALVE = 0x0823,
            [Description("读取针阀控制值")]
            GET_LEAK_VALVE = 0X0824
        }

       
      
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(OldButterflyValveCmdCode command, byte[] data)
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
            return ZepGenericProtocol.GetCmd(DeviceAddr,id, cmd, data);
        }
        /// <summary>
        /// 闸板阀控制
        /// </summary>
        /// <param name="channel"> 暂定</param>
        /// <param name="vol">设置的电压</param>
        /// <returns></returns>
        public static byte[] SetGateValve(float vol, UInt32 channel = 0)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(channel);
            writer.Write(vol);
            return GetCmd(OldButterflyValveCmdCode.SET_GATE_VALVE, writer.EndWrite());
        }

        /// <summary>
        /// 读取闸板阀控制值
        /// </summary>
        /// <returns></returns>
        public static byte[] GetLeakValve()
        {
            ByteWriter writer = new ByteWriter(36);
      
            return GetCmd(OldButterflyValveCmdCode.GET_GATE_VALVE, writer.EndWrite());
        }


    }
    public class OldButterflyValveTestPacket
    {
        public OldButterflyValveTestPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public OldButterflyValveCmdCode CmdType { get { return (OldButterflyValveCmdCode)BitConverter.ToUInt16(this.packet.command); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class OldButterflyValveTestParser
    {
        public OldButterflyValveTestParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }
        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new OldButterflyValveTestPacket(packet));
        }
        public event EventHandler<OldButterflyValveTestPacket> PacketReceivedEvent;
        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }
        private ZepGenericProtocolParser _parser;
    }
}
