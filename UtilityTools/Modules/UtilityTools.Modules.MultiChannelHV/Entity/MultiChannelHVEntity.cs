using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using static UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol.MultiChannelHVProtocol;

namespace UtilityTools.Modules.MultiChannelHV.Entity
{
    public class MultiChannelHVEntity
    {
        public MultiChannelHVEntity(IAsynRWService serialPortService, IAsynRWService netUdpService)
        {
            _serialPortService = serialPortService;
            _netUdpService = netUdpService;
        }
         
        private IAsynRWService _serialPortService;
        private IAsynRWService _netUdpService;

        /// <summary>
        /// 获取单路隔离电压
        /// </summary>
        /// <param name="channel"></param>
        public async void GetIVCommand(byte channel)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"{channel} 通道，获取隔离电压");
            var cmdData = MultiChannelHVProtocol.GetIV(channel);
            await SentData(cmdData);
        }
        /// <summary>
        /// 获取1-6路隔离电压
        /// </summary>
        /// <param name="channel"></param>
        public async void Get1To6IVCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"1-6通道，获取隔离电压");
            var cmdData = MultiChannelHVProtocol.Get1To6IV();
            await SentData(cmdData);
        }

        /// <summary>
        /// 获取7-13路隔离电压
        /// </summary>
        /// <param name="channel"></param>
        public async void Get7To13IVCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"7-13通道，获取隔离电压");
            var cmdData = MultiChannelHVProtocol.Get7To13IV();
            await SentData(cmdData);
        }
        /// <summary>
        /// 设置指定通道的隔离电压
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="iv"></param>
        public async void SetIVCommand(byte channel, ushort iv)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"{channel}通道:设置隔离电压{iv}V");
            var cmdData = MultiChannelHVProtocol.SetIV(channel, iv);
            await SentData(cmdData);
        }


        /// <summary>
        /// 获取整机悬浮高压
        /// </summary>
        /// <returns></returns>
        public async void  GetHVCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug($"获取整机悬浮高压");
            var cmdData = MultiChannelHVProtocol.GetHV();
            await SentData(cmdData);
        }

        /// <summary>
        /// 设置整机悬浮高压
        /// </summary>
        /// <param name="hv"></param>
        /// <returns></returns>
        public  async void SetHVCommand(UInt16 hv)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置整机高压:{hv}V");
            var cmdData = MultiChannelHVProtocol.SetHV(hv);
            await SentData(cmdData);
        }
        /// <summary>
        /// 设置高压控制板初始化
        /// </summary>
        public async void SetInitCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置高压控制板初始化");
            var cmdData = MultiChannelHVProtocol.SetInit();
            await SentData(cmdData);
        }
        /// <summary>
        ///  高压初始化状态查询
        /// </summary>
        public async void GetInitStateCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"高压初始化状态查询");
            var cmdData = MultiChannelHVProtocol.GetInitState();
            await SentData(cmdData);
        }

        /// <summary>
        /// 隔离板错误清除
        /// </summary>
        public async void IErrorClearCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"隔离板错误清除");
            var cmdData = MultiChannelHVProtocol.IErrorClear();
            await SentData(cmdData);
        }
        /// <summary>
        /// 关闭输出，取消初始化
        /// </summary>
        public async void DisableOutputCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"关闭输出，取消初始化");
            var cmdData = MultiChannelHVProtocol.DisableOutput();
            await SentData(cmdData);
        }
        /// <summary>
        /// 获取固件版本
        /// </summary>
        public async void GetFVCommand()
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取固件版本");
            var cmdData = MultiChannelHVProtocol.GetFV();
            await SentData(cmdData);
        }


        private async Task SentData(byte[] bytes)
        {
            if (_serialPortService.IsOpen)
                _serialPortService.SendMsg(bytes);
            if (_netUdpService.IsOpen)
                _netUdpService.SendMsg(bytes);
            await Task.Delay(20);
        }
    }
}
