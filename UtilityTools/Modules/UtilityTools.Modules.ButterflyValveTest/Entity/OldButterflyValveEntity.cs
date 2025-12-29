using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.ButterflyValveTest.Model;
using UtilityTools.Modules.ButterflyValveTest.Protocol;

namespace UtilityTools.Modules.ButterflyValveTest.Entity
{
    public class OldButterflyValveEntity
    {
        public OldButterflyValveEntity(OldButterflyValveModel butterflyValveModel)
        {
            _butterflyValveModel1 = butterflyValveModel;
        }
        private OldButterflyValveModel _butterflyValveModel1;

        /// <summary>
        /// 设置电机运行停止
        /// </summary>
        /// <param name="runState">运行状态，0:停止，1:运行</param>
        /// <param name="channel"></param>
        public async void SetGateValveCommand(float vol, UInt32 channel = 0x00)
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"设置闸板阀电压{vol}");
            var cmdData = OldButterflyValveTestProtocol.SetGateValve(vol,channel);
            await SentData(cmdData);
        }
        /// <summary>
        /// 获取闸板阀电压
        /// </summary>
        public async void GetLeakValveCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"获取闸板阀电压");
            var cmdData = OldButterflyValveTestProtocol.GetLeakValve();
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
