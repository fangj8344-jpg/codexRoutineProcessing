using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.ButterflyValveTest.Model;
using UtilityTools.Modules.ButterflyValveTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.ButterflyValveTestProtocol;

namespace UtilityTools.Modules.ButterflyValveTest.Entity
{
    public class ButterflyValveEntity
    {
        public ButterflyValveEntity(ButterflyValveModel butterflyValveModel)
        {
            _butterflyValveModel1 = butterflyValveModel;
        }
        private ButterflyValveModel _butterflyValveModel1;

        /// <summary>
        /// 设置电机运行停止
        /// </summary>
        /// <param name="runState">运行状态，0:停止，1:运行</param>
        /// <param name="channel"></param>
        public async void SetRsCommand(byte runState, byte channel = 0x00)
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"设置蝶阀状态{runState}");
            var cmdData = ButterflyValveTestProtocol.SetRs(runState, channel);
            await SentData(cmdData);
        }
        /// <summary>
        /// 设置阀门开度
        /// </summary>
        /// <param name="pos">阀门开度0-100(全关-全开)</param>
        /// <param name="reservedByte1">保留字段</param>
        /// <param name="reservedByte2">保留字段</param>
        public async void SetPosCommand(float pos, byte reservedByte1 = 0x00, byte reservedByte2 = 0x00)
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"设置阀门开度{pos}");
            var cmdData = ButterflyValveTestProtocol.SetPos(pos, reservedByte1, reservedByte2);
            await SentData(cmdData);
        }
        /// <summary>
        /// 获取控制板状态
        /// </summary>
        public async void GetStatusCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"获取控制板状态");
            var cmdData = ButterflyValveTestProtocol.GetStatus();
            await SentData(cmdData);
        }

        public async void GetAbsPosCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"获取编码器的绝对数值");
            var cmdData = ButterflyValveTestProtocol.GetAbsPos();
            await SentData(cmdData);
        }
       
        private async Task SentData(byte[] bytes)
        {
            if (_butterflyValveModel1.NetUdpService.IsOpen)
                _butterflyValveModel1.NetUdpService.SendMsg(bytes);
            if (_butterflyValveModel1.SerialPortService.IsOpen)
                _butterflyValveModel1.SerialPortService.SendMsg(bytes);
        }
    }
}
