#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.HvController.Protocol
 * 唯一标识：9d80e101-b602-49bd-b51d-a642ce04890b
 * 文件名：NewHvControllerProtocol
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/12/5 14:23:51
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.HvController.Protocol
{
    public enum EnumHvCommandType
    {
      
        [Description("获取当前高压参数")]
        CMD_GET_PARAM = 0x02f0,
        [Description("初始化高压箱")]
        CMD_HV_INIT = 0x02f1,
        [Description("设置栅极电压")]
        CMD_SET_BV = 0x02f2,
        [Description("设置加热电流")]
        CMD_SET_PI= 0x02f3,
        [Description("设置吸取电压")]
        CMD_SET_EV = 0x02f4,
        [Description("设置加速电压")]
        CMD_SET_HV = 0x02f5,
        [Description("断电")]
        CMD_CLOSE_ALL = 0x02f6,
        [Description("查询高压箱初始化进度")]
        CMD_GET_HV_INIT_STATE = 0x02f7,
        [Description("获取当前加热电流设置值")]
        CMD_GET_PI_STEP = 0x02f8,
    }
    public class HVDataPacket
    {
        public HVDataPacket(DataPacket packet)
        {
            this.packet = packet;
        }
        public EnumHvCommandType CmdType { get { return (EnumHvCommandType)BitConverter.ToUInt16(this.packet.command); } }
        public EnumDeviceID DeviceID { get { return (EnumDeviceID)BitConverter.ToUInt16(this.packet.id); } }
        public byte[] DataSource { get => packet.data; }
        private DataPacket packet;
    }

    public class HVProtocolParser
    {
        public HVProtocolParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GeneriaPackReceived;
        }
        private void GeneriaPackReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new HVDataPacket(packet));
        }

        public event EventHandler<HVDataPacket> PacketReceivedEvent;

        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }

        private ZepGenericProtocolParser _parser;
        public IAsynRWService Service;
    }
    internal static class NewHvControllerProtocol
    {
        //流程  初始化高压箱，发栅极电压，发加热电流，发吸取极，每一步都要等待前面一步完成后才能继续进行

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        public static byte[] GetCmd(EnumHvCommandType command, byte[] data , EnumDeviceID? deviceID = EnumDeviceID.DeviceID_20KV_HIGH_PRESSURE_BOX)
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
        /// 获取当前高压参数
        /// </summary>
        /// <returns></returns>
        public static byte[] GetParamCmd()
        {
            byte[] data = new byte[36];
            return GetCmd(EnumHvCommandType.CMD_GET_PARAM,data);
        }
        /// <summary>
        /// 初始化高压箱
        /// </summary>
        /// <returns></returns>
        public static byte[] SetInitHVCmd()
        {
            byte[] data = new byte[36];
            return GetCmd(EnumHvCommandType.CMD_HV_INIT, data);
        }
        /// <summary>
        /// 设置栅极电压
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SetBVCmd(float bv,bool autoAdjust = true)
        {
            ushort value1 = (ushort)(bv * 1000 * 65535/2000.0);
            ushort value2;
            if (autoAdjust)
            {
                value2 = 1;
            }
            else 
            {
                value2 = 0;
            }
            byte[] data = new byte[36];
            Buffer.BlockCopy(BitConverter.GetBytes(value1),0,data,0,2);
            Buffer.BlockCopy(BitConverter.GetBytes(value2), 0, data, 2, 2);
            return GetCmd(EnumHvCommandType.CMD_SET_BV, data);
        }
        /// <summary>
        /// 设置加热电流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SetPICmd(float pi,ushort stepValue) 
        {
            ushort value1 = (ushort)(pi * 65535 / 3.2);
            ushort value2 = stepValue;
            byte[] data = new byte[36];
            Buffer.BlockCopy(BitConverter.GetBytes(value1), 0, data, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(value2), 0, data, 2, 2);
            return GetCmd(EnumHvCommandType.CMD_SET_PI, data);
        }
        /// <summary>
        /// 设置吸取电压
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SetEVCmd(float SetEmissionVol)
        {
            ushort value1 = (ushort)(SetEmissionVol * 1000 * 65535 / 6000);
            byte[] data = new byte[36];
            Buffer.BlockCopy(BitConverter.GetBytes(value1), 0, data, 0, 2);
            return GetCmd(EnumHvCommandType.CMD_SET_EV, data);
        }
        /// <summary>
        /// 设置加速电压
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SetHVCmd(float hv) 
        {
            ushort value1 = (ushort)(hv * 1000 * 65535 / 16000);
            byte[] data = new byte[36];
            Buffer.BlockCopy(BitConverter.GetBytes(value1), 0, data, 0, 2);
            return GetCmd(EnumHvCommandType.CMD_SET_HV, data);
        }
        /// <summary>
        /// 断电
        /// </summary>
        /// <returns></returns>
        public static byte[] CloseAllCmd()
        {
            byte[] data = new byte[36];
            return GetCmd(EnumHvCommandType.CMD_CLOSE_ALL, data);
        }
        /// <summary>
        /// 查询高压箱初始化进度
        /// </summary>
        /// <returns></returns>
        public static byte[] GetHVInitState()
        {
            byte[] data = new byte[36];
            return GetCmd(EnumHvCommandType.CMD_GET_HV_INIT_STATE, data);
        }

        public static byte[] GetPIStep()
        {
            byte[] data = new byte[36];
            return GetCmd(EnumHvCommandType.CMD_GET_PI_STEP, data);
        }
        /// <summary>
        /// 打包指令头和指令尾
        /// </summary>
        /// <param name="cmdCode"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static byte[] GetCommand(byte cmdCode, byte[] cmd)
        {
            byte[] msgBuf = new byte[13];
            for (int i = 0; i < 4; ++i)
            {
                msgBuf[i] = 0xFF;
                msgBuf[12 - i] = 0xFE;
            }

            msgBuf[4] = cmdCode;
            cmd.CopyTo(msgBuf, 5);

            return msgBuf;
        }

        /// <summary>
        /// 请求高压状态指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRequestCommand() 
        {
            return GetCommand(0xF0, new byte[4]);
        }

        /// <summary>
        /// 请求初始化指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetInitCommand() 
        {
            return GetCommand(0xF1, new byte[4]);
        }

        /// <summary>
        /// 请求栅极电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-2000，单位V</param>
        /// <returns></returns>
        public static byte[] GetGridVolCommand(float vol) 
        {
            ushort value = (ushort)(vol * 65536 / 2000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            msgBuf[0] = arr[1];
            msgBuf[1] = arr[0];

            return GetCommand(0xF2, msgBuf);
        }

        /// <summary>
        /// 请求加热电流指令
        /// </summary>
        /// <param name="cur">电流值，取值范围为0-3.2，单位A</param>
        /// <param name="step">电流变换步长</param>
        /// <returns></returns>
        public static byte[] GetHeatCurCommand(float cur, byte step)
        {
            ushort value = (ushort)(cur * 65536 / 3.2);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            msgBuf[0] = arr[1];
            msgBuf[1] = arr[0];
            msgBuf[3] = step;

            return GetCommand(0xF3, msgBuf);
        }

        /// <summary>
        /// 请求吸取极电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-6000，单位V</param>
        /// <returns></returns>
        public static byte[] GetEmissionVolCommand(float vol)
        {
            ushort value = (ushort)(vol * 65536 / 6000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            msgBuf[0] = arr[1];
            msgBuf[1] = arr[0];

            return GetCommand(0xF4, msgBuf);
        }

        /// <summary>
        /// 请求加速电压指令
        /// </summary>
        /// <param name="vol">电压值，取值范围为0-16000，单位V</param>
        /// <returns></returns>
        public static byte[] GetAccVolCommand(float vol) 
        {
            ushort value = (ushort)(vol * 65536 / 16000.0);
            byte[] msgBuf = new byte[4];
            var arr = BitConverter.GetBytes(value);
            msgBuf[0] = arr[1];
            msgBuf[1] = arr[0];

            return GetCommand(0xF5, msgBuf);
        }

        /// <summary>
        /// 请求关枪指令
        /// </summary>
        /// <returns></returns>
        public static byte[] GetCloseHvCommand() 
        {
            return GetCommand(0xF6, new byte[4]);
        }
    }

    internal class NewHvControllerProtocolParser
    { 
        
    }
}
