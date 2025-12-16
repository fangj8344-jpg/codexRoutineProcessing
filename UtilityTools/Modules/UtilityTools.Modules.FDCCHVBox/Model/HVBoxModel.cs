using CsvHelper;
using OxyPlot;
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
using System.Windows.Media.Effects;
using UtilityTools.Modules.FDC12CHVBox.Entity;
using UtilityTools.Modules.FDC12CHVBox.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Model
{
    public class HVBoxModel : BindableBase
    {
        public HVBoxModel(byte channel, string ip, int port, string remortIp, int remortPort, FDC12CHVBoxModel fDC12CHVBoxModel)
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
        private TaskCompletionSource<string> _waitingReply;
        private int _port;
        private string _ip;
        private string _remortIp;
        private int _remortPort;

        private bool _isConnectTest = false;
        private bool _direction = true;
        private ushort _maxHV = 30000;
        private System.Timers.Timer _timer;
        private System.Timers.Timer _setHvTimer;

        
        private IAsynRWService _netUdpService;
      
        
        public bool IsSetHvDone = false;
        private bool _isSetHvShow = false;
        public bool IsSetHvShow
        {
            get { return _isSetHvShow; }
            set { _isSetHvShow = value; RaisePropertyChanged(); }
        }
        private int _endBufferHv = 0;
        public int EndBufferHv
        {
            get { return _endBufferHv; }
            private set
            {
                if (value < 0)
                {
                    _endBufferHv = 0;
                }
                else if (value > _maxHV)
                {
                    _endBufferHv = _maxHV;
                }
                else
                {
                    _endBufferHv = value;
                }

            }
        }
        private int _startBufferHv = 0;
        public int StartBufferHv
        {
            get { return _startBufferHv; }
            private set
            {
                if (value < 0)
                {
                    _startBufferHv = 0;
                }
                else if (value > _maxHV)
                {
                    _startBufferHv = _maxHV;
                }
                else
                {
                    _startBufferHv = value;
                }

            }
        }
        private List<HvMessage> _hvMessage;
        public List<HvMessage> HvMessages
        {
            get { return _hvMessage; }
            set { _hvMessage = value; RaisePropertyChanged(); }
        }
        private LineSeries _hVlineSeries;
        public LineSeries HVlineSeries
        {
            get { return _hVlineSeries; }
            set { _hVlineSeries = value; RaisePropertyChanged(); }
        }
        private LineSeries _setHVSeries;
        public LineSeries SetHVSeries
        {
            get { return _setHVSeries; }
            set { _setHVSeries = value; RaisePropertyChanged(); }
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
        private int _writeHV = 0;
        public int WriteHV
        {
            get { return _writeHV; }
            set
            {
                if (value > _maxHV)
                {
                    _writeHV = _maxHV;
                }
                else if (value < 0)
                {
                    _writeHV = 0;
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
            set { _readI = value; RaisePropertyChanged(); }
        }
        private ushort _setHV = 0;
        /// <summary>
        /// 设置的高压值
        /// </summary>
        public ushort SetHV
        {
            get { return _setHV; }
            set { _setHV = value; RaisePropertyChanged(); }
        }
        private ushort _showSetHV ;
        public ushort ShowSetHV
        {
            get { return _showSetHV; }
            set { _showSetHV = value;RaisePropertyChanged(); }
        }
        private ushort _wirteStep = 5;

        private UInt32 _timerInterval = 1000;
        public UInt32 TimerInterval
        {
            get { return _timerInterval; }
            set { _timerInterval = value; RaisePropertyChanged(); }
        }

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
        private bool _isCheck = false;
        public bool IsCheck
        {
            get { return _isCheck; }
            set
            {
                _isCheck = value; RaisePropertyChanged();
                if (value == true && HVBoxInitState == FDC12CHVBoxInitState.RUNNUNG)
                {
                    IsEnable = true;
                }
                else
                {
                    IsEnable = false;
                }
                if (value)
                {
                    if (_fDC12CHVBoxModel.IsSetHvLineShow &&!_fDC12CHVBoxModel.PlotModel.Series.Contains(SetHVSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Add(SetHVSeries);
                    }
                    if (_fDC12CHVBoxModel.IsILineShow && !_fDC12CHVBoxModel.PlotModel.Series.Contains(IlineSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Add(IlineSeries);
                    }
                    if (_fDC12CHVBoxModel.IsSetHvLineShow && !_fDC12CHVBoxModel.PlotModel.Series.Contains(HVlineSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Add(HVlineSeries);
                    }

                    _fDC12CHVBoxModel.PlotModel.InvalidatePlot(true);
                }
                else
                {
                    if (_fDC12CHVBoxModel.PlotModel.Series.Contains(SetHVSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Remove(SetHVSeries);
                    }
                    if (_fDC12CHVBoxModel.PlotModel.Series.Contains(IlineSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Remove(IlineSeries);
                    }
                    if (_fDC12CHVBoxModel.PlotModel.Series.Contains(HVlineSeries))
                    {
                        _fDC12CHVBoxModel.PlotModel.Series.Remove(HVlineSeries);
                    }
                    _fDC12CHVBoxModel.PlotModel.InvalidatePlot(true);
                    _fDC12CHVBoxModel.IsAllCheck = false;
                }
            }
        }
        private ushort _readStep;
        [JsonIgnore]
        public ushort ReadStep
        {
            get { return _readStep; }
            set { _readStep = value; RaisePropertyChanged(); }
        }

        private FDC12CHVBoxInitState? _hVBoxInitState = FDC12CHVBoxInitState.DISCONNECTED;
        public FDC12CHVBoxInitState? HVBoxInitState
        {
            get { return _hVBoxInitState; }
            set
            {
                _hVBoxInitState = value;
                if (value == FDC12CHVBoxInitState.RUNNUNG && IsCheck == true)
                {
                    IsEnable = true;
                }
                else
                {
                    IsEnable = false;
                }

                RaisePropertyChanged();
            }
        }
        private bool _isEnable = false;
        public bool IsEnable
        {
            get
            {

                return _isEnable;
            }
            set { _isEnable = value; RaisePropertyChanged(); }
        }


        [JsonIgnore]
        public DelegateCommand SetHvCommand { get; set; }

        [JsonIgnore]
        public DelegateCommand SetHvStepCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SetHvInitCommand { get; set; }
        public DelegateCommand CloseOutputCommand { get; set; }

        private void InitCommand()
        {
            SetHvCommand = new DelegateCommand(SetHv);
            SetHvStepCommand = new DelegateCommand(SetHvStep);
            SetHvInitCommand = new DelegateCommand(SetHvInit);
            CloseOutputCommand = new DelegateCommand(CloseOutput);
        }
        private void Init()
        {
            HvMessages = new List<HvMessage>();
            HVlineSeries = new LineSeries()
            {
                Title = $"{Channel}电压",
                RenderInLegend = true,
                ItemsSource = HvMessages,
                DataFieldX = "DateTime",
                DataFieldY = "HV",
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "曲线: {0}\n时间: {2:yyyy-MM-dd HH:mm:ss.fff}\n数值: {4:0.000000}", // 毫秒级精度
            };
            IlineSeries = new LineSeries()
            {
                Title = $"{Channel}电流",
                RenderInLegend = true,
                ItemsSource = HvMessages,
                DataFieldX = "DateTime",
                DataFieldY = "I",
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "曲线: {0}\n时间: {2:yyyy-MM-dd HH:mm:ss.fff}\n数值: {4:0.000000}", // 毫秒级精度
            };
            SetHVSeries = new LineSeries()
            {
                Title = $"{Channel}目标电压",
                RenderInLegend = true,
                ItemsSource = HvMessages,
                DataFieldX = "DateTime",
                DataFieldY = "setHv",
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                TrackerFormatString = "{曲线: {0}\n时间: {2:yyyy-MM-dd HH:mm:ss.fff}\n数值: {4:0.000000}", // 毫秒级精度
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
            if (InitSetHv())
            {
                InitSetHVTimer();
            }
           
        }
        private void SetHvInit()
        {
            Entity.SetHvInitCommand();
        }
        //单路设置定时器
        public void InitSetHVTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_setHvTimer == null)
            {
                _setHvTimer = new System.Timers.Timer(TimerInterval);
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
            SetHvBySetp();
        }
       
        public bool InitSetHv()
        {
            IsSetHvDone = false;
            StopSetHvTimer();
            if (!IsEnable)
            {
                return false;
            }

            StartBufferHv = SetHV;
            EndBufferHv = WriteHV;
            if (EndBufferHv > StartBufferHv)
            {
                _direction = true;
            }
            else
            {
                _direction = false;
            }
            ShowSetHV = (ushort)WriteHV;
            return true;
        }
        public void SetHvBySetp()
        {
            if (!IsEnable)
            {
                return;
            }
            if (_direction)
            {
                StartBufferHv  = StartBufferHv + SetStep;
                if (StartBufferHv >= EndBufferHv)
                {
                    Entity.SetHvCommand((ushort)EndBufferHv, TimerInterval, SetStep);
                    IsSetHvDone = true;
                    return ;
                }
                else
                {
                    Entity.SetHvCommand((ushort)StartBufferHv, TimerInterval, SetStep);
                    IsSetHvDone = false;
                    return ;
                }
            }
            else 
            {
                StartBufferHv = StartBufferHv - SetStep;
                if (StartBufferHv <= EndBufferHv)
                {
                    Entity.SetHvCommand((ushort)EndBufferHv, TimerInterval, SetStep);
                    IsSetHvDone = true;
                    return;
                }
                else
                {
                    Entity.SetHvCommand((ushort)StartBufferHv, TimerInterval, SetStep);
                    IsSetHvDone = false;
                    return;
                }
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
            if (_isConnectTest)
            {
                if (_waitingReply != null)
                {
                    _isConnectTest = false;
                    _waitingReply?.TrySetResult("reply");
                }
               
            }
           
        }

        private void GetHvReadReturnProcessing(byte[] data)
        {
            var hv = BitConverter.ToSingle(data, 0);
            var i = BitConverter.ToSingle(data, 4);
            var hr = BitConverter.ToSingle(data, 8);
            var bv = BitConverter.ToSingle(data, 12);
            var stepHv = BitConverter.ToUInt16(data,16);
            ReadHV = hv;
            ReadI  = i;
            SetHV = stepHv;
            var dateTime = DateTime.Now;
            var hvMessage = new HvMessage();
            hvMessage.DateTime = dateTime;
            hvMessage.HV = hv;
            hvMessage.I = i;
            hvMessage.setHv = SetHV;
            HvMessages.Add(hvMessage);
            for (int j = _fDC12CHVBoxModel.HVBoxModels.Count - 1; j >= 0; j--)
            {
                if (_fDC12CHVBoxModel.IsRealtimeRefresh && _fDC12CHVBoxModel.HVBoxModels[j].Channel == Channel && _fDC12CHVBoxModel.HVBoxModels[j].IsEnable == true)
                {
                    _fDC12CHVBoxModel.PlotModel.InvalidatePlot(true);
                }
            } 
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
        }
        public async void CheckConnect()
        {
            try
            {
              
                _isConnectTest = true;
                _waitingReply = new TaskCompletionSource<string>();
                var result = await _waitingReply.Task.WaitAsync(TimeSpan.FromSeconds(10));
                if (result == "reply")
                {

                }
                else
                {
                    HVBoxInitState = FDC12CHVBoxInitState.DISCONNECTED;
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"通道号:{_channel}连接测试失败，连接断开{ex}");
            }
           
        }
        public void CloseOutput()
        {
            StopSetHvTimer();
            Entity?.SetHvDeInitCommand();
        }
    }
    public class HvMessage 
    {
        public DateTime  DateTime { get; set; }
        public float HV { get; set; }
        public float I { get; set; }
        public ushort setHv { get; set; }
    }
  
}
