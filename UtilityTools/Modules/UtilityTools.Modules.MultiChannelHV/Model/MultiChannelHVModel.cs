using CsvHelper;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Timers;
using UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol;
using UtilityTools.Modules.MultiChannelHV.Entity;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol.MultiChannelHVProtocol;

namespace UtilityTools.Modules.MultiChannelHV.Model
{
    public class MultiChannelHVModel:BindableBase
    {
        public MultiChannelHVModel() 
        {
            Init();
            InitCommand();
        }
        private System.Timers.Timer _timer;
        private MultiChannelHVParser _parser;
        public MultiChannelHVEntity MultiChannelHVEntity;
        private IAsynRWService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }

        private IAsynRWService _netUdpService;

        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<HVMessageModel>  _hVMessageModels;
        public ObservableCollection<HVMessageModel> HVMessageModels
        {
            get { return _hVMessageModels; }
            set { _hVMessageModels = value; RaisePropertyChanged(); }
        }
        private int _readHV = 0;
        public int ReadHV
        {
            get { return _readHV; }
            set { _readHV = value; RaisePropertyChanged(); }
        }
        private int _writeHV = 0;
        public int WriteHV
        {
            get { return _writeHV; }
            set { _writeHV = value; RaisePropertyChanged(); }
        }
        public DelegateCommand SetHVCommand { get; set; }
        public DelegateCommand GetHVCommand { get; set; }
        public DelegateCommand SetInitCommand { get; set; }
        public DelegateCommand IErrorClearCommand { get; set; }
        public DelegateCommand DisableOutputCommand { get; set; }
        private void InitCommand()
        {
            SetHVCommand = new DelegateCommand(() => MultiChannelHVEntity?.SetHVCommand(WriteHV)); 
            GetHVCommand = new DelegateCommand(() => MultiChannelHVEntity?.GetHVCommand());
            SetInitCommand = new DelegateCommand(() => MultiChannelHVEntity?.SetInitCommand());
            IErrorClearCommand = new DelegateCommand(()=> MultiChannelHVEntity?.IErrorClearCommand());
            DisableOutputCommand = new DelegateCommand(() => MultiChannelHVEntity?.DisableOutputCommand());
        }
        private void Init()
        {
            _parser = new MultiChannelHVParser();
            SerialPortService = new SerialPortService();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            SerialPortService.IsBinary = true;
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "升级网口";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5003;
            netUdp.DeviceInstance.HostIp = "192.168.1.33";
            netUdp.DeviceInstance.HostPort = 6585;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;

            MultiChannelHVEntity = new MultiChannelHVEntity(SerialPortService, NetUdpService);
            HVMessageModels = new ObservableCollection<HVMessageModel>();
            for (byte i = 0; i < 13; i++)
            {
                HVMessageModel hVMessageModel = new HVMessageModel(MultiChannelHVEntity,i);
                HVMessageModels.Add(hVMessageModel);
            }
        }

      

        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        // 定时触发的方法
        public void InitTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
            }
            // 2. 绑定定时触发的事件
            _timer.Elapsed += OnTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _timer.AutoReset = true;

            // 4. 启动定时器
            _timer.Enabled = true;
        }
        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            GetHVCommand.Execute();
            for (byte i = 0; i < HVMessageModels.Count; i++)
            {
                HVMessageModels[i].GetIVCommand.Execute();

            }
        }
        private void Parser_PacketReceivedEvent(object? sender, MultiChannelHVPacket e)
        {

            var data = e.DataSource;
            switch (e.CmdType)
            {
                case MultiChannelHVFunctionCode.SET_HV:break;
                case MultiChannelHVFunctionCode.GET_HV: break;
                case MultiChannelHVFunctionCode.SET_IV: break;
                case MultiChannelHVFunctionCode.GET_IV: break;
                case MultiChannelHVFunctionCode.DISABLE_OUTPUT: break;
                case MultiChannelHVFunctionCode.ERROR_CLEAR: break;
                case MultiChannelHVFunctionCode.SET_INIT: break;
            }
          
        }
    }
}
