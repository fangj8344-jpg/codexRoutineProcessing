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

using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        #region 旧版指令
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
        #endregion


        [Description("设置控制模式")]
        CMD_SET_CTL_MODE = 0x0102,

        [Description("获取控制模式")]
        CMD_GET_CTL_MODE = 0x0103,

        [Description("获取当前实时温度")]
        CMD_GET_CURRENT_TEMP = 0x0104,//既有发送参数也有返回参数(发送：查询温度单位；返回：查询到的温度单位 当前温度值)

        [Description("获取当前实时输出电流")]
        CMD_GET_CURRENT_OUT = 0x0105,

        [Description("设置温控启停开关")]
        CMD_SET_RUN_STOP = 0x0106,

        [Description("获取温控启停开关")]
        CMD_GET_RUN_STOP = 0x0107,

        [Description("设置自动模式下的目标温度")]
        CMD_SET_TARGET_TEMP = 0x0108,

        [Description("获取自动模式下的目标温度")]
        CMD_GET_TARGET_TEMP = 0x0109,

        [Description("设置手动模式下的输出电流")]
        CMD_SET_I_OUT = 0x010A,

        [Description("获取手动模式下的输出电流")]
        CMD_GET_I_OUT = 0x010B,

        [Description("设置PID参数")]
        CMD_SET_PID = 0x0201,

        [Description("获取PID参数")]
        CMD_GET_PID = 0x0202,

        [Description("设置最大输出电流")]
        CMD_SET_IMAX = 0x0203,

        [Description("获取最大输出电流")]
        CMD_GET_IMAX = 0x0204,

        [Description("设置工作类型")]
        CMD_SET_WORKER_TYPE = 0x0205,

        [Description("获取工作类型")]
        CMD_GET_WORKER_TYPE = 0x0206,

        [Description("设置传感器类型")]
        CMD_SET_SENSOR_TYPE = 0x0207,

        [Description("获取传感器类型")]
        CMD_GET_SENSOR_TYPE = 0x0208,

        [Description("设置控制定时")]
        CMD_SET_TIMER = 0x0301,

        [Description("获取控制定时")]
        CMD_GET_TIMER = 0x0302,

        [Description("设置控温速率")]
        CMD_SET_RAMP = 0x0303,

        [Description("获取控温速率")]
        CMD_GET_RAMP = 0x0304,

        [Description("设置温度修正值")]
        CMD_SET_TEMP_CORRECTION = 0x0401,

        [Description("获取温度修正值")]
        CMD_GET_TEMP_CORRECTION = 0x0402,

        [Description("获取全部状态数据")]
        CMD_GET_DATA_ALL = 0x0410,

        
        [Description("获取控制器本地温度，也就是冷端温度")]
        CMD_GET_LOCAL_TEMP = 0x0501,
        [Description("获取当前温度变化速率")]
        CMD_GET_CURRENT_RATE = 0x0502,
        [Description("获取当前控制的目标温度变化速率")]
        CMD_GET_TARGET_RATE = 0x0503,
        [Description("设置控制参数")]
        CMD_SET_CTL_PARAMS = 0x0504,
        [Description("获取控制参数")]
        CMD_GET_CTL_PARAMS = 0x0505,
        [Description("设置稳态降温阶段的PID参数")]
        CMD_SET_STA_PID = 0x0506,
        [Description("获取稳态降温阶段的PID参数")]
        CMD_GET_STA_PID = 0x0507,
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

        #region NewMethod
        /// <summary>
        /// 设置控制模式
        /// </summary>
        /// <param name="CtrlMode">0:PID；1：手动控制</param>
        /// <returns></returns>
        public static byte[] SetCtrlModeCommand(byte CtrlMode)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(CtrlMode);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_CTL_MODE, writer.EndWrite());
        }

        /// <summary>
        /// 获取当前实时温度
        /// </summary>
        /// <param name="Tempunit">温度单位：0：℃  1：K</param>
        /// <returns></returns>
        public static byte[] SetRtimeTempCommand(byte Tempunit)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(Tempunit);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_CURRENT_TEMP, writer.EndWrite());
        }


        /// <summary>
        /// 设置温控启停开关
        /// </summary>
        /// <param name="Switch">0：关闭 1:开启</param>
        /// <returns></returns>
        public static byte[] SetCentigradeCommand(byte Switch)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(Switch);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_RUN_STOP, writer.EndWrite());
        }

        /// <summary>
        /// 设置自动模式下的目标温度
        /// </summary>
        /// <param name="tempunit">温度单位：0：℃  1：K</param>
        /// <param name="targetTemp">目标温度值</param>
        /// <returns></returns>
        public static byte[] SetPIDTargetTempCommand(byte tempunit,float targetTemp)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(tempunit);
            writer.Write(targetTemp);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_TARGET_TEMP, writer.EndWrite());
        }

        /// <summary>
        /// 设置手动模式下的输出电流
        /// </summary>
        /// <param name="IoutValue">输出电流值</param>
        /// <returns></returns>
        public static byte[] SetManualIOutCommand(float IoutValue)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(IoutValue);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_I_OUT, writer.EndWrite());
        }

        /// <summary>
        /// 设置PID参数
        /// </summary>
        /// <param name="Kp">比例P</param>
        /// <param name="Ki">积分I</param>
        /// <param name="Kd">微分D</param>
        /// <returns></returns>
        public static byte[] SetPIDparameterCommand(float Kp, float Ki, float Kd)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(Kp);
            writer.Write(Ki);
            writer.Write(Kd);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_PID, writer.EndWrite());
        }

        /// <summary>
        /// 设置最大输出电流
        /// </summary>
        /// <param name="ImaxValue">电流值，单位A</param>
        /// <returns></returns>
        public static byte[] SetIMaxCommand(float ImaxValue)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(ImaxValue);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_IMAX, writer.EndWrite());
        }

        /// <summary>
        /// 设置工作类型
        /// </summary>
        /// <param name="workType">0:冷台；1：热台</param>
        /// <returns></returns>
        public static byte[] SetWorkTypeCommand(byte workType)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(workType);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_WORKER_TYPE, writer.EndWrite());
        }

        /// <summary>
        /// 设置传感器类型
        /// </summary>
        /// <param name="sensorType">0：PT100； 0x11：K型热电偶</param>
        /// <returns></returns>
        public static byte[] SetSensorTypeCommand(byte sensorType)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(sensorType);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_SENSOR_TYPE, writer.EndWrite());
        }

        public static byte[] GetSensorTypeCommand()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_SENSOR_TYPE, writer.EndWrite());
        }

        /// <summary>
        /// 设置控制定时
        /// </summary>
        /// <param name="closeTime">定时关闭时间，单位为秒</param>
        /// <param name="Switch">定时开关：0关；1开</param>
        /// <returns></returns>
        public static byte[] SetControlTimeCommand(float closeTime,byte Switch)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(closeTime);
            writer.Write(Switch);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_TIMER, writer.EndWrite());
        }

        /// <summary>
        /// 设置控温速率
        /// </summary>
        /// <param name="rate">控温速率，单位为℃/Min</param>
        /// <param name="controlSwitch">斜率控制开关：0关；1开</param>
        /// <returns></returns>
        public static byte[] SetTempControlRateCommand(float rate, byte controlSwitch)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(rate);
            writer.Write(controlSwitch);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_RAMP, writer.EndWrite());
        }

        /// <summary>
        /// 设置温度修正值
        /// </summary>
        /// <param name="tempCorrection">温度修正值（矫正硬件偏差）</param>
        /// <returns></returns>
        public static byte[] SetTempCorrectionCommand(float tempCorrection)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(tempCorrection);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_TEMP_CORRECTION, writer.EndWrite());
        }

        public static byte[] GetControlTimeCommand()
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_TEMP_CORRECTION, writer.EndWrite());
        }

        /// <summary>
        /// 获取全部数据
        /// </summary>
        /// <param name="allData">温度单位：0：℃  1：K</param>
        /// <returns></returns>
        public static byte[] SetAllData(byte allData)
        {
            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
            ByteWriter writer = new ByteWriter(36);
            writer.Write(allData);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_DATA_ALL, writer.EndWrite());
        }

        /// <summary>
        /// 设置控制参数
        /// </summary>
        /// <param name="tempRange">触发温度控制的范围（绝对值）也就是温度距离多少时开始精确PID控制，单位℃</param>
        /// <param name="minRate">最小升温速率，单位℃/s</param>
        /// <param name="SC">速度控制的衰减变化系数</param>
        /// <param name="SysC">系统前馈补偿的增益系数</param>
        /// <returns></returns>
        public static byte[] SetControlparms(float tempRange,float minRate,float SC,float SysC)
        {
            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
            ByteWriter writer = new ByteWriter(36);
            writer.Write(tempRange);
            writer.Write(minRate);
            writer.Write(SC);
            writer.Write(SysC);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_CTL_PARAMS, writer.EndWrite());
        }

        /// <summary>
        /// 设置稳态降温阶段的PID参数
        /// </summary>
        /// <param name="SteadyP"></param>
        /// <param name="SteadyI"></param>
        /// <param name="SteadyD"></param>
        /// <returns></returns>
        public static byte[] SetSteadyCoolPIDparms(float SteadyP, float SteadyI, float SteadyD)
        {
            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
            ByteWriter writer = new ByteWriter(36);
            writer.Write(SteadyP);
            writer.Write(SteadyI);
            writer.Write(SteadyD);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_SET_STA_PID, writer.EndWrite());
        }

        /// <summary>
        /// 获取当前PID参数
        /// </summary>
        /// <returns></returns>
        public static byte[] GetPIDparms()
        {
            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_PID, writer.EndWrite());
        }

        public static byte[] GetWorkType()
        {
            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumTemperatureControllerCommandType.CMD_GET_WORKER_TYPE, writer.EndWrite());
        }
        #endregion

        #region OldMethod
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
        #endregion

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
