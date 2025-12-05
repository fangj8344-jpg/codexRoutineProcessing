using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Protocol
{
    public class FDC12CHVBoxProtocol
    {
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0205);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public enum FDC12CHVBoxFunctionCode
        {
            [Description("读取高压信息")]
            GET_HV_READ = 0x0100,
            [Description("设置升压间隔")]
            SET_HV_STEP = 0x0101,
            [Description("查询设置的升压间隔")]
            GET_HV_STEP = 0x0102,
            [Description("设置高压值")]
            SET_HV = 0x0201,
            [Description("查询设置的高压值")]
            GET_HV = 0x0202,
            [Description("高压控制板初始化")]
            SET_HV_INIT = 0x0400,
            [Description("高压初始化状态查询")]
            GET_HV_INIT = 0x0401,
            [Description("高压箱初始化曲线，关闭输出")]
            SET_HV_DEINIT = 0x0403,
            [Description("固件版本获取")]
            FV = 0x0005,
        }
        public enum FDC12CHVBoxInitState
        {
            [Description("空闲状态")]
            IDLE = 0x01,
            [Description("初始化中")]
            INITALIZING = 0x02,
            [Description("运行中")]
            RUNNUNG = 0x03,
            [Description("高压错误")]
            ERROR = 0x07,
            [Description("未连接")]
            DISCONNECTED = 0xFF,
        }
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(FDC12CHVBoxFunctionCode command, byte[] data)
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
        /// 读取高压信息
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHvRead()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.GET_HV_READ, writer.EndWrite());
        }
        /// <summary>
        /// 设置升压间隔
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public static byte[] SetHvStep(ushort step)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(step);
            return GetCmd(FDC12CHVBoxFunctionCode.SET_HV_STEP, writer.EndWrite());
        }
        /// <summary>
        /// 查询升压间隔
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHvStep()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.GET_HV_STEP, writer.EndWrite());
        }

        /// <summary>
        /// 设置高压值
        /// </summary>
        /// <param name="hv">设置的高压值</param>
        /// <returns></returns>
        public static byte[] SetHv(ushort hv,UInt32 inter, ushort step)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(hv);
            writer.Write(inter);
            writer.Write(step);
            return GetCmd(FDC12CHVBoxFunctionCode.SET_HV, writer.EndWrite());
        }

        /// <summary>
        /// 查询设置的高压值
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHv( )
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.GET_HV, writer.EndWrite());
        }

        /// <summary>
        /// 高压控制板初始化
        /// </summary>
        /// <returns></returns>
        public static byte[] SetHvInit()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.SET_HV_INIT, writer.EndWrite());
        }

        /// <summary>
        /// 高压初始化状态查询
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHvInit()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.GET_HV_INIT, writer.EndWrite());
        }
        /// <summary>
        /// 高压箱关闭输出
        /// </summary>
        /// <returns></returns>
        public static byte[] SetHvDeInit()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(FDC12CHVBoxFunctionCode.SET_HV_DEINIT, writer.EndWrite());
        }

    }
    public class FDC12CHVBoxPacket
    {
        public FDC12CHVBoxPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public FDC12CHVBoxFunctionCode CmdType { get { return (FDC12CHVBoxFunctionCode)BitConverter.ToUInt16(this.packet.command); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class FDC12CHVBoxParser
    {
        public FDC12CHVBoxParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }

        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new FDC12CHVBoxPacket(packet));
        }

        public event EventHandler<FDC12CHVBoxPacket> PacketReceivedEvent;


        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }


        private ZepGenericProtocolParser _parser;
    }
}
