using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using ZemModel.Entity;

namespace UtilityTools.Modules.MotorTest.Entity
{
    public class MotorEntity
    {
        public MotorEntity(ConcurrentQueue<byte[]> hPQueue, ConcurrentQueue<byte[]> oTSQueue) 
        {
            _hPQueue = hPQueue;
            _oTSQueue = oTSQueue;
        }
        public MotorEntity(IAsynRWService serialPortService, UdpNetAsyncDevice udpService)
        {
            _udpService = udpService;
            _serialPortService = serialPortService;
        }
        private ConcurrentQueue<byte[]> _hPQueue;
        private ConcurrentQueue<byte[]> _oTSQueue;
        private UdpNetAsyncDevice _udpService;
        private IAsynRWService _serialPortService;

        /// <summary>
        /// 设置电机是否使能
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="enable">电机使能</param>
        /// <returns></returns>
        public void SetMotorEnableCommand(EnumMotorId motorId, EnumMotorEnable enable)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置使能状态：{enable}");
            var cmdData = SelfMotorProtocol.SetMotorEnable(motorId,enable);
            SendImportCmd(cmdData);


        }
        /// <summary>
        /// 设置电机运行状态
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="operatingStatus">运行状态</param>
        public void SetMotorOperatingStatusCommand(EnumMotorId motorId, EnumMotorOperatingState operatingStatus)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置运行状态：{operatingStatus}");
            var cmdData = SelfMotorProtocol.SetMotorOperatingStatus(motorId, operatingStatus);

            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 设置电机绝对运动
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="unit">单位</param>
        /// <param name="distance">距离</param>
        public void SetMotorGoToCommand(EnumMotorId motorId, EnumMotorUnit unit, float distance)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置绝对运动距离：{distance}，单位:{distance}");
            var cmdData = SelfMotorProtocol.SetMotorGoTo(motorId, unit, distance);

            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 设置电机零点
        /// </summary>
        /// <param name="motorId">电机通道</param>
        public void SetMotorZeroCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置零点");
            var cmdData = SelfMotorProtocol.SetMotorZero(motorId);

            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 获取电机当前位置，获取的单位和设置的值有关
        /// </summary>
        /// <param name="motorId">点击通道</param>
        public void GetMotorPosCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Trace($"电机：{motorId}，获取当前位置");
            var cmdData = SelfMotorProtocol.GetMotorPos(motorId);

            sendCmd(cmdData);
        }

        /// <summary>
        /// 获取电机的当前状态参数
        /// </summary>
        /// <param name="motorId">电机通道</param>
        public void GetMotorStatusCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Trace($"电机：{motorId}，获取当前状态参数");
            var cmdData = SelfMotorProtocol.GetMotorStatus(motorId);

            SendImportCmd(cmdData);

        }

        /// <summary>
        /// 获取电机的运行速度，单位是脉冲/s
        /// </summary>
        /// <param name="motorId"></param>
        public void GetMotorSpeedCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，获取运行速度(单位是脉冲/s)");
            var cmdData = SelfMotorProtocol.GetMotorSpeed(motorId);
            sendCmd(cmdData);

        }

        /// <summary>
        /// 设置电机的闭环控制模式
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="controlMode"></param>
        public void SetMotorControlModeCommand(EnumMotorId motorId, EnumMotorCtrType controlMode)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置闭环控制模式{controlMode})");
            var cmdData = SelfMotorProtocol.SetMotorControlMode(motorId, controlMode);
            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 设置电机最小闭环速度
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="minSpeed"></param>
        public void SetMotorControlMinClsCommand(EnumMotorId motorId, Int32 minSpeed)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置最小闭环速度{minSpeed})");
            var cmdData = SelfMotorProtocol.SetMotorControlMinCls(motorId, minSpeed);
            SendImportCmd(cmdData);
        }

        /// <summary>
        /// 设置电机最大闭环速度
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="maxSpeed"></param>
        public void SetMotorControlMaxClsCommand(EnumMotorId motorId, Int32 maxSpeed)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置最大闭环速度{maxSpeed})");
            var cmdData = SelfMotorProtocol.SetMotorControlMaxCls(motorId, maxSpeed);
            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 设置软件使能掩码
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="limitEnableMask"></param>
        public void SetMotorLimitEnableCommand(EnumMotorId motorId, byte limitEnableMask)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置软件的限位使能掩码{limitEnableMask})");
            var cmdData = SelfMotorProtocol.SetMotorLimitEnable(motorId, limitEnableMask);
            SendImportCmd(cmdData);
        }
        /// <summary>
        /// 获取当前电机状态使能掩码
        /// </summary>
        /// <param name="motorId"></param>
        public void GetMotorLimitEnableCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，获取当前状态电机使能掩码)");
            var cmdData = SelfMotorProtocol.GetMotorLimitEnable(motorId);
            sendCmd(cmdData);
        }
        /// <summary>
        /// 设置轴类型
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="moveType"></param>
        public void SetAxTypeCommand(EnumMotorId motorId, EnumMotorMoveType moveType)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，设置轴类型{moveType})");
            var cmdData = SelfMotorProtocol.SetAxType(motorId, moveType);
            SendImportCmd(cmdData);
        }

        /// <summary>
        /// 获取轴类型
        /// </summary>
        /// <param name="motorId"></param>
        public void GetAxTypeCommand(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"电机：{motorId}，获取轴类型)");
            var cmdData = SelfMotorProtocol.GetAxType(motorId);
            sendCmd(cmdData);
        }


        private void sendCmd(byte[] cmdData) 
        {
            if (_udpService == null)
            {
                _oTSQueue?.Enqueue(cmdData);
            }
            else
            {
                _udpService?.SendMsg(cmdData);
                _serialPortService?.SendMsg(cmdData);
            }
        }

        private void SendImportCmd(byte[] cmdData) 
        {
            if (_udpService == null)
            {
                _hPQueue?.Enqueue(cmdData);
            }
            else
            {
                _udpService?.SendImportantMsg(cmdData);
                _serialPortService?.SendMsg(cmdData);
            }
        }
      
    }
}
