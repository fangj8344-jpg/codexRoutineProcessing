using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.FDC12CHVBox.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Entity
{
    public class FDC12CHVBoxEntity
    {
        public FDC12CHVBoxEntity(UdpNetAsyncDevice netUdpService, byte channel)
        {
          
            _netUdpService = netUdpService;
            _channed = channel;
        }
        private UdpNetAsyncDevice _netUdpService;
        private byte _channed;
        private TaskCompletionSource<string> _waitingReply;
        /// <summary>
        /// 读取高压信息
        /// </summary>
        public async void GetHvReadCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道，读取高压信息");
            var cmdData = FDC12CHVBoxProtocol.GetHvRead();
            await SentData(cmdData);
        }
        /// <summary>
        /// 设置升压间隔
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async void SetHvStepCommand(ushort step)
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,设置升压间隔{step}");
            var cmdData = FDC12CHVBoxProtocol.SetHvStep(step);
            await SentData(cmdData);
        }
        /// <summary>
        /// 查询升压间隔
        /// </summary>
        /// <returns></returns>
        public async void GetHvStepCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,查询升压间隔");
            var cmdData = FDC12CHVBoxProtocol.GetHvStep();
            await SentData(cmdData);
        }

        /// <summary>
        /// 设置高压值
        /// </summary>
        /// <param name="hv">设置的高压值</param>
        /// <returns></returns>
        public async void SetHvCommand(ushort hv)
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,设置高压值为{hv}");
            var cmdData = FDC12CHVBoxProtocol.SetHv(hv);
            await SentData(cmdData);
        }

        /// <summary>
        /// 查询设置的高压值
        /// </summary>
        /// <returns></returns>
        public async void GetHvCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,查询设置的高压值");
            var cmdData = FDC12CHVBoxProtocol.GetHv();
            await SentData(cmdData);
        }

        /// <summary>
        /// 高压控制板初始化
        /// </summary>
        /// <returns></returns>
        public async void SetHvInitCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,高压控制板初始化");
            var cmdData = FDC12CHVBoxProtocol.SetHvInit();
            await SentData(cmdData);
        }

        /// <summary>
        /// 高压初始化状态查询
        /// </summary>
        /// <returns></returns>
        public async void GetHvInitCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道,高压初始化状态查询");
            var cmdData = FDC12CHVBoxProtocol.GetHvInit();
            await SentData(cmdData);
        }

        /// <summary>
        /// 高压初关闭输出
        /// </summary>
        /// <returns></returns>
        public async void SetHvDeInitCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"{_channed}通道关闭输出");
            var cmdData = FDC12CHVBoxProtocol.SetHvDeInit();
            await SentData(cmdData);
        }
        private async Task SentData(byte[] bytes)
        {
            if (_netUdpService.IsOpen)
                _netUdpService.SendMsg(bytes);
        }
    }
}
