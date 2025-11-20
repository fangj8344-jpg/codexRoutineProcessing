using CsvHelper;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Common;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UtilityTools.Modules.FDC12CHVBox.Entity;
using UtilityTools.Modules.FDC12CHVBox.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Model
{
    public class HVBoxModel:BindableBase
    {
        public HVBoxModel(byte channel, string ip,int port,string remortIp, int remortPort, FDC12CHVBoxModel fDC12CHVBoxModel)
        {
            _channel = channel;
            _port = port;
            _ip = ip;
            _remortPort = remortPort;
            _remortIp = remortIp;
            _fDC12CHVBoxModel = fDC12CHVBoxModel;
            InitCommand();
            Init();
        }
        private FDC12CHVBoxModel _fDC12CHVBoxModel;
        private FDC12CHVBoxParser _parser;
        private int _port ;
        private string _ip;
        private string _remortIp;
        private int _remortPort;
       
        private bool _direction = true;
        private ushort _maxHV = 30000;
        private System.Timers.Timer _timer;
        private System.Timers.Timer _setHvTimer;
        private ushort _setHvCount = 0;
        private int _setHvTotalCount = 0;
        private ushort _endBufferHv = 0;
        private ushort _startBufferHv = 0;
        private IAsynRWService _netUdpService;
        private ObservableCollection<HvMessage> _hvMessage;
        public ObservableCollection<HvMessage> HvMessages
        {
            get {return _hvMessage;}
            set { _hvMessage = value;RaisePropertyChanged(); }
        }
       

      
        private LineSeries _hVlineSeries;
        public LineSeries HVlineSeries
        {
            get { return _hVlineSeries; }
            set { _hVlineSeries = value; RaisePropertyChanged(); }
        }
        private LineSeries _ilineSeries;
        public LineSeries IlineSeries
        {
            get { return _ilineSeries; }
            set { _ilineSeries = value; RaisePropertyChanged(); }
        }
        private FDC12CHVBoxEntity _entity;
        public FDC12CHVBoxEntity Entity
        {
            get { return _entity; }
            set { _entity = value; RaisePropertyChanged(); }
        }
        private UdpNetAsyncDevice _udpNetAsyncDevice;
        public UdpNetAsyncDevice UdpNetAsyncDevice
        {
            get { return _udpNetAsyncDevice; }
            set { _udpNetAsyncDevice = value; RaisePropertyChanged(); }
        }
        private readonly byte _channel;
        public byte Channel
        {
            get { return _channel; }
        }
        private float _readHV = 0;
        [JsonIgnore]
        public float ReadHV
        {
            get { return _readHV; }
            set { _readHV = value; RaisePropertyChanged(); }
        }
        private ushort _writeHV = 0;
        public ushort WriteHV
        {
            get { return _writeHV; }
            set
            {
                if (value > _maxHV)
                {
                    _writeHV = _maxHV;
                }
                else
                {
                    _writeHV = value;
                }

                RaisePropertyChanged();
            }
        }
        private float _readI;

        [JsonIgnore]
        public float ReadI
        {
            get { return _readI; }
            set {_readI = value; RaisePropertyChanged(); }
        }
        private ushort _setHV = 0;
        /// <summary>
        /// 设置的高压值
        /// </summary>
        public ushort SetHV
        {
            get { return _setHV; }
            set {_setHV = value; RaisePropertyChanged(); }
        }
        private ushort _wirteStep = 5;
        public ushort WriteStep
        {
            get { return _wirteStep; }
            set { _wirteStep = value; RaisePropertyChanged(); }
        }
        private ushort _setStep = 5;
        public ushort SetStep
        {
            get { return _setStep; }
            set { _setStep = value; RaisePropertyChanged(); }
        }
       
        private ushort _readStep;
        [JsonIgnore]
        public ushort ReadStep
        {
            get { return _readStep; }
            set {_readStep = value; RaisePropertyChanged(); }
        }
        private bool _isEnable = false;
       
        [JsonIgnore]
        public bool IsEnable
        {
            get { return _isEnable; }
            set { _isEnable = value; RaisePropertyChanged(); }
        }
        private FDC12CHVBoxInitState? _hVBoxInitState  = FDC12CHVBoxInitState.DISCONNECTED;
        public FDC12CHVBoxInitState? HVBoxInitState
        {
            get { return _hVBoxInitState; }
            set {_hVBoxInitState = value; RaisePropertyChanged(); }
        }

        
        [JsonIgnore]
        public DelegateCommand SetHvCommand { get; set; }
   
        [JsonIgnore]
        public DelegateCommand SetHvStepCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SetHvInitCommand { get; set; }

        private void InitCommand()
        {
            SetHvCommand = new DelegateCommand(SetHv);
            SetHvStepCommand = new DelegateCommand(SetHvStep);
            SetHvInitCommand = new DelegateCommand(SetHvInit);

        }
        private void Init() 
        {
            HvMessages = new ObservableCollection<HvMessage>();
            HVlineSeries = new LineSeries()
            {
                Title = $"{Channel}电压",
                RenderInLegend = true,
                ItemsSource = HvMessages,
                DataFieldX = "DateTime",
                DataFieldY = "HV",
            };
            IlineSeries = new LineSeries()
            {
                Title = $"{Channel}电流",
                RenderInLegend = true,
                ItemsSource = HvMessages,
                DataFieldX = "DateTime",
                DataFieldY = "I",
            };
        }
        public void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        private void SetHvStep()
        {
            SetStep = WriteStep;
        }


        private void SetHv()
        {
            _setHvCount = 0;
            _startBufferHv = (ushort)ReadHV;
            _endBufferHv = WriteHV;
            if (_endBufferHv > _startBufferHv)
            {
                _setHvTotalCount = (_endBufferHv - _startBufferHv) / SetStep + 1;
                _direction = true;
            }
            else
            {
                _setHvTotalCount = (_startBufferHv - _endBufferHv) / SetStep + 1;
                _direction = false;
            }
            StopSetHvTimer();
            InitSetHVTimer();
        }
        private void SetHvInit()
        {
            Entity.SetHvInitCommand();
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
            _timer = null;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (UdpNetAsyncDevice.IsOpen)
            {
                _entity.GetHvReadCommand();
                _entity.GetHvCommand();
                _entity.GetHvInitCommand();
            }
        }
        public void InitSetHVTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_setHvTimer == null)
            {
                _setHvTimer = new System.Timers.Timer(1000);
            }
            if (_setHvTimer.Enabled)
            {
                _setHvTimer.Stop();
            }
            // 2. 绑定定时触发的事件
            _setHvTimer.Elapsed += SetHvTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _setHvTimer.AutoReset = true;

            // 4. 启动定时器
            _setHvTimer.Enabled = true;
        }
        public void StopSetHvTimer()
        {
            _setHvTimer?.Stop();
            _setHvTimer?.Dispose();
            _setHvTimer = null;
        }
        private void SetHvTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _setHvCount++;
            if (_direction)
            {
                if (_setHvCount >= _setHvTotalCount)
                {
                    _startBufferHv = _endBufferHv;
                    _entity.SetHvCommand((ushort)(_endBufferHv));
                }
                else
                {
                    _startBufferHv = (ushort)(_startBufferHv +   SetStep);
                    _entity.SetHvCommand((ushort)(_startBufferHv));
                }    
            }
            else
            {
                if (_setHvCount >= _setHvTotalCount)
                {
                    _startBufferHv = _endBufferHv;
                    _entity.SetHvCommand((ushort)(_endBufferHv));
                }
                else
                {
                    _startBufferHv = (ushort)(_startBufferHv - SetStep);
                    _entity.SetHvCommand((ushort)(_startBufferHv));
                }
            }
            if (_setHvCount >= _setHvTotalCount)
            {
                StopSetHvTimer();
            }
        }
        private void Parser_PacketReceivedEvent(object? sender, FDC12CHVBoxPacket e)
        {
            var cmd = e.CmdType;
            var data = e.DataSource;
            switch (cmd)
            {
                //读取高压信息
                case FDC12CHVBoxFunctionCode.GET_HV_READ: GetHvReadReturnProcessing(data); break;
                //设置升压间隔
                case FDC12CHVBoxFunctionCode.SET_HV_STEP: SetHvStepReturnProcessing(data); break;
                //查询设置的升压间隔
                case FDC12CHVBoxFunctionCode.GET_HV_STEP: GetHvStepReturnProcessing(data);  break;
                //设置高压值
                case FDC12CHVBoxFunctionCode.SET_HV: SetHVReturnProcessing(data); break;
                //查询设置的高压值
                case FDC12CHVBoxFunctionCode.GET_HV: GetHvReturnProcessing(data); break;
                //高压控制板初始化
                case FDC12CHVBoxFunctionCode.SET_HV_INIT: SetHvInitReturnProcessing(data); break;
                //高压初始化状态查询
                case FDC12CHVBoxFunctionCode.GET_HV_INIT: GetHvinitReturnProcessing(data); break;
                //固件版本获取
                case FDC12CHVBoxFunctionCode.FV:break;
            }
        }

        private void GetHvReadReturnProcessing(byte[] data)
        {
            var hv = BitConverter.ToSingle(data, 0);
            var i = BitConverter.ToSingle(data, 4);
            var hr = BitConverter.ToSingle(data, 8);
            var bv = BitConverter.ToSingle(data, 12);
            ReadHV = hv;
            ReadI  = i;
            var dateTime = DateTime.Now;
            var hvMessage = new HvMessage();
            hvMessage.DateTime = dateTime;
            hvMessage.HV = hv;
            hvMessage.I = i;
            HvMessages.Add(hvMessage);
            _fDC12CHVBoxModel.PlotModel.InvalidatePlot(true);
        }

        private void SetHvStepReturnProcessing(byte[] data)
        {
            
        }
        private void GetHvStepReturnProcessing(byte[] data)
        {
            var step = BitConverter.ToUInt16(data,0);
            SetStep = step;
        }
        private void SetHVReturnProcessing(byte[] data)
        {
            
        }
        private void GetHvReturnProcessing(byte[] data)
        {
            var sethv =  BitConverter.ToUInt16(data,0);
            SetHV = sethv;
        }

        private void SetHvInitReturnProcessing(byte[] data)
        {
            var result = data[0];
        }
        private void GetHvinitReturnProcessing(byte[] data)
        {
            var initState = (FDC12CHVBoxInitState)data[0];
            HVBoxInitState = initState;
        }
        public void InitConnect(int port,string hostIp,int targetPort,string targetIp)
        {
            _port = port;
            _remortIp = targetIp;
            _remortPort = targetPort;
            _parser = new FDC12CHVBoxParser();
            UdpNetAsyncDevice = new UdpNetAsyncDevice();
            UdpNetAsyncDevice.UpdateResponse += NetUdpService_UpdateResponse;
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;
            UdpNetAsyncDevice.DeviceInstance.HostPort = port;
            UdpNetAsyncDevice.DeviceInstance.HostIp = hostIp;
            UdpNetAsyncDevice.DeviceInstance.TargetPort = targetPort;
            UdpNetAsyncDevice.DeviceInstance.TargetIp = targetIp;
            UdpNetAsyncDevice.Open();
            _entity = new FDC12CHVBoxEntity(_udpNetAsyncDevice, _channel);
            SetHvInit();
            StopTimer();
            InitTimer();
        }
    }
    public class HvMessage 
    {
        public DateTime  DateTime { get; set; }
        public float HV { get; set; }
        public float I { get; set; }
    }
  
}
