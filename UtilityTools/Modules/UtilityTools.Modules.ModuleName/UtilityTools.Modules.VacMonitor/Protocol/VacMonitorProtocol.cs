#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.VacMonitor.Protocol
 * 唯一标识：c5b6f6e0-2a5a-49b8-ba25-71731e46f0a5
 * 文件名：VacMonitorProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:48:27
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

using NLog;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.VacMonitor.Protocol
{
    public enum EnumBackVacChannel
    {
        [Description("通道1")]
        VACCHANNEL_CH1 = 0x0101,

        [Description("通道2")]
        VACCHANNEL_CH2 = 0x0102,

        [Description("通道3")]
        VACCHANNEL_CH3 = 0x0103,

        [Description("通道4")]
        VACCHANNEL_CH4 = 0x0104,

        [Description("所有通道")]
        VACCHANNEL_CH0 = 0x0400
    }
    public enum VacFunctionCode
    {
        [Description("读规数值")]
        CMD_GETVAC = 0x0800,

        [Description("读规得类型")]
        CMD_GETALLVAC = 0x0900,
    }
    public class VacMonitorProtocol
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0101);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x01 };
        #endregion

        #region ------------StaticMethod------------
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(VacFunctionCode command, byte[] data)
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
        public class VacDataPacket
        {
            public VacDataPacket(DataPacket packet)
            {
                this.packet = packet;
            }
            public UInt16 cmd { get { return BitConverter.ToUInt16(this.packet.command); } }
            public byte[] DataSource { get => packet.data; }
            private DataPacket packet;
        }

        public class VacMonitorProtocolParser
        {
            public VacMonitorProtocolParser()
            {
                _parser = new ZepGenericProtocolParser();
                _parser.PacketReceivedEvent += GeneriaPackReceived;
            }
            private void GeneriaPackReceived(object sender, DataPacket packet)
            {
                PacketReceivedEvent(this, new VacDataPacket(packet));
            }
            public event EventHandler<VacDataPacket> PacketReceivedEvent;  
            public void ReceiveBytes(byte[] data)
            {
                _parser.ReceiveBytes(data);
            }

            private ZepGenericProtocolParser _parser;
            public IAsynRWService Service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static byte[] GetVacuumValue(int index)
        {
            byte key = (byte)'U';
            switch (index)
            {
                case 1: key = (byte)'V'; break;
                case 2: key = (byte)'W'; break;
                case 3: key = (byte)'X'; break;
            }
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = key;
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }
        /// 读取所有通道真空
        /// </summary>
        /// <returns></returns>
        public static byte[] GetAllVacuumValueNew()
        {
            ByteWriter writer = new ByteWriter(36);
           
            return GetCmd(VacFunctionCode.CMD_GETVAC, writer.EndWrite());
           
        }
        /// <summary>
        /// 获取枪头真空度
        /// </summary>
        /// <returns></returns>
        public static byte[] GetGunVac()
        {
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = (byte)'V';
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }

        /// <summary>
        /// 获取样品仓真空度
        /// </summary>
        /// <returns></returns>
        public static byte[] GetSampleVac()
        {
            byte[] bytes = new byte[6];
            bytes[0] = (byte)'?';
            bytes[1] = (byte)'W';
            bytes[2] = (byte)'7';
            bytes[3] = (byte)'5';
            bytes[4] = (byte)'2';
            bytes[5] = 0x0D;
            return bytes;
        }

        /// <summary>
        /// 设置校验信息
        /// </summary>
        /// <param name="buf"></param>
        public static void SetCheckout(ref byte[] buf)
        {
            byte checkout = 0;
            int i = 0;
            for (; i < buf.Length - 1; i++)
            {
                checkout += buf[i];
            }

            buf[i] = checkout;
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
        /// 找出回包格式字节数组
        /// </summary>
        /// <param name="response">回包字节数组</param>
        /// <param name="length">回包数据长度</param>
        /// <returns>符合协议的回包格式字节数组，没找到则返回null</returns>
        public static byte[] FindResponse(ref byte[] response, ref int length)
        {
            if (length == 0)
                return null;
            string ret = Encoding.Default.GetString(response, 0, length);
            length = 0;
            return response;
        }

        /// <summary>
        /// 计算校验位
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool CheckResponse(byte[] response)
        {
            return true;
        }

        /// <summary>
        /// 试图解析数据回包中的真空读数
        /// </summary>
        /// <param name="response">回包报文</param>
        /// <param name="pos">回包报文</param>
        /// <param name="ret">真空读数</param>
        /// <returns></returns>
        public static bool TryParseResponse(string response, out int index, out double ret)
        {
            index = 1;
            ret = 0.0;
            var list = response.Split(' ', ';');
            if (list.Length != 3)
            {
                return false;
            }

            if (list[0].Contains('V'))
            {
                index = 1;
            }
            else if (list[0].Contains('W'))
            {
                index = 2;
            }
            else if (list[0].Contains('X'))
            {
                index = 3;
            }
            else
            {
                return false;
            }

            return double.TryParse(list[1], out ret);
        }
        #endregion
    }
}
