using NLog;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Runtime.CompilerServices;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Event;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.IonPump.Model;
using UtilityTools.Modules.IonPump.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.IonPump.ViewModels
{
    public class GammaIonPumpViewModel : RegionViewModelBase
    {

        public GammaIonPumpViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider)
        {
            _dialogHostService = dialogHostService;

            InitProperty();
            InitCommand();
        }

        ~GammaIonPumpViewModel()
        {
            if (_service != null)
            {
                _service.Close();
            }
        }


        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private PubSub ps;
        private CommandPacketBuilder _builder;
        private ResponsePacketParser _respParser;
        private readonly IDialogHostService _dialogHostService;

        private EnumCommand _selectedCmd;
        public EnumCommand SelectedCmd { get { return _selectedCmd; } set { SetProperty(ref _selectedCmd, value); } }

        private string _respPacketDataStr;
        public string RespPacketDataStr { get { return _respPacketDataStr; } set { SetProperty(ref _respPacketDataStr, value); } }

        private EnumCommand _currSendCmd;

        private bool _isConnected;
        public bool IsConnected { get { return _isConnected; } set { SetProperty(ref _isConnected, value); } }

        private IAsynRWService _service;
        public IAsynRWService Service { get { return _service; } set { SetProperty(ref _service, value); } }


        public DelegateCommand ShowDeviceCommand { get; set; }
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service);

            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.IonPumpRegionName);

            if (diaglogResult == null)
                return;

            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Service = value;
                    IsConnected = Service.IsOpen;
                }
            }
        }

        public DelegateCommand SendCmdCommand { get; set; }
        private void SendCmd()
        {
            var cmd = SelectedCmd;

            switch (cmd)
            {
                case EnumCommand.MODEL_NUMBER:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdSysModel());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.VERSION:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdSysVersion());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.READ_PRESSURE:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdHvReadPressure());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.READ_CURRENT:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdHvReadCurrent());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.READ_VOLTAGE:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdHvReadVoltage());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.SET_PRESS_UNITS:
                    {
                        break;
                    }
                case EnumCommand.START_PUMP:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdStartPump());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                case EnumCommand.STOP_PUMP:
                    {
                        var resp = SendAndWaitResponse(_builder.GetCmdStopPump());
                        RespPacketDataStr = resp.Data;
                        break;
                    }
                default:
                    {
                        LOGGER.Warn($"发送命令暂不支持{cmd}");
                        break;
                    }
            }
        }


        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            SendCmdCommand = new DelegateCommand(SendCmd, () => this.IsConnected).ObservesProperty(() => IsConnected);
        }

        private void InitProperty()
        {
            _builder = new CommandPacketBuilder();
            _respParser = new ResponsePacketParser();
            _respParser.PacketReceivedEvent += ResponsePacketReceived;

            Service = containerProvider.Resolve<IServiceFactory>().GetAsynRWService("GSP", "爱德华离子泵");
            Service.IsBinary = false;
            Service.MinWriteInterval = 100;
            Service.UpdateResponse += (object sender, byte[] data) => _respParser.ReceiveBytes(data);

            var Model = Service.GetHandle() as SerialPortModel;
            Model.BaudRate = 9600;

            SelectedCmd = EnumCommand.MODEL_NUMBER;
            ps = new PubSub();
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private ResponsePacket SendAndWaitResponse(CommandPacket packet, int timeout_ms = 1000)
        {
            _currSendCmd = packet.cmd;
            var result = ps.Sub(_currSendCmd.ToString(), timeout_ms);
            _service.SendMsg(packet.GetBytes());

            try
            {
                result.Wait();
                var resp = result.Result as ResponsePacket;
                return resp;
            }
            catch (Exception ex)
            {
                LOGGER.Warn($"发送请求异常{packet.cmd}", ex);
                return GammaIonPumpProtocol.TIMEOUT;
            }
        }


        private void ResponsePacketReceived(object sender, ResponsePacket packet)
        {
            ps.Pub(_currSendCmd.ToString(), packet);
        }

    }
}
