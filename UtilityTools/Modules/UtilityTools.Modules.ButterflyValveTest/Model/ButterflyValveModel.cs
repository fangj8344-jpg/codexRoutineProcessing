
using Prism.Commands;
using Prism.Mvvm;
using System.Timers;
using System.Windows;

using UtilityTools.Modules.ButterflyValveTest.Entity;
using UtilityTools.Modules.ButterflyValveTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.ButterflyValveTestProtocol;
using Timer = System.Timers.Timer;

namespace UtilityTools.Modules.ButterflyValveTest.Model
{
    public class ButterflyValveModel : BindableBase
    {

        public ButterflyValveModel(ButterflyValveTestModel butterflyValveTestModel) 
        {
            _butterflyValveTestModel = butterflyValveTestModel;
            InitProperty();
            InitCommand();
        }
        private ButterflyValveTestModel _butterflyValveTestModel;
        public Timer QueryStatusTimer { get; set; }
        private TaskCompletionSource<string> _startReply;
        private TaskCompletionSource<string> _endReply;
        private CancellationTokenSource _cts;
        private bool _isTest = false;
        private bool _isStart = false;
        private bool _isEnd = false;
        private ButterflyValveTestParser _parser;
        private ButterflyValveEntity _entity;
        /// <summary>
        /// 实例
        /// </summary>
        public ButterflyValveEntity Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }
        private int _TimeInterval = 1000;
        public int TimeInterval
        {
            get { return _TimeInterval; }
            set { _TimeInterval = value; RaisePropertyChanged(); }
        }
        private float _openingValue = 50;
        /// <summary>
        /// 开度值
        /// </summary>
        public float OpeningValue
        {
            get { return _openingValue; }
            set 
            {
                if (value <= 0)
                {
                    _openingValue = 0;
                }
                else if (value > 100)
                {
                    _openingValue = 100;
                }
                else
                {
                    _openingValue = value;
                }
                RaisePropertyChanged(); }
        }
        private bool _isRunning = false;
        /// <summary>
        /// 电机运行或停止
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; RaisePropertyChanged(); }
        }

        private bool _motorStatus = false;
        /// <summary>
        /// 电机状态（是否异常）
        /// </summary>
        public bool MotorStatus
        {
            get { return _motorStatus; }
            set { _motorStatus = value; RaisePropertyChanged(); }
        }
        private bool _valveFullyClosedHardwareLimit = false;
        /// <summary>
        /// 阀门全关硬件限位
        /// </summary>
        public bool ValveFullyClosedHardwareLimit
        {
            get => _valveFullyClosedHardwareLimit;
            set { _valveFullyClosedHardwareLimit = value;RaisePropertyChanged(); }
        }
        private bool _valveFullyOpenHardwareLimit = false;
        /// <summary>
        /// 阀门全开硬件限位
        /// </summary>
        public bool ValveFullyOpenHardwareLimit
        {
            get => _valveFullyOpenHardwareLimit;
            set { _valveFullyOpenHardwareLimit = value; RaisePropertyChanged(); }
        }
        private bool _valveHardwareLimitStatus;
        /// <summary>
        /// 阀门硬件限位异常
        /// </summary>
        public bool ValveHardwareLimitStatus
        {
            get => _valveHardwareLimitStatus;
            set { _valveHardwareLimitStatus = value;RaisePropertyChanged(); }
        }
        private ButterflyValveRunuingDirection _valveRunuingDirection = ButterflyValveRunuingDirection.NOT_CONNECTION;
        /// <summary>
        /// 电机运行方向
        /// </summary>
        public ButterflyValveRunuingDirection ValveRunuingDirection
        {
            get => _valveRunuingDirection;
            set { _valveRunuingDirection = value;RaisePropertyChanged(); }
        }
        private ButterflyValveHardwareLimit _butterflyValveHardwareLimit = ButterflyValveHardwareLimit.NOT_CONNECTION;
        /// <summary>
        /// 硬件限位触发情况
        /// </summary>
        public ButterflyValveHardwareLimit ButterflyValveHardwareLimit
        {
            get { return _butterflyValveHardwareLimit; }
            set { _butterflyValveHardwareLimit = value;RaisePropertyChanged(); }
        }
        private float _readOpeningValue;
        /// <summary>
        /// 读取的开度值开度值
        /// </summary>
        public float ReadOpeningValue
        {
            get { return _readOpeningValue; }
            set { _readOpeningValue = value; RaisePropertyChanged(); }
        }
        private int _encoderValue = 0;
        /// <summary>
        /// 编码器绝对值
        /// </summary>
        public int EncoderValue
        {
            get => _encoderValue;
            set { _encoderValue = value;RaisePropertyChanged(); }
        }
        private int _waitTime = 60;
        /// <summary>
        /// 等待闸板阀开启或者关闭过程的时间，超出为失败单位s
        /// </summary>
        public int WaitTime
        {
            get => _waitTime;
            set { _waitTime = value; RaisePropertyChanged(); }
        }
      
        private int _cycleCount = 15;
        /// <summary>
        /// 一个循环开合次数
        /// </summary>
        public int CycleCount
        {
            
            get => _cycleCount;
            set { _cycleCount = value;RaisePropertyChanged(); } 
        }
        private int _roundCount = 1000;
        /// <summary>
        /// 一个回合开合次数
        /// </summary>
        public int RoundCount
        {
            get => _roundCount;
            set { _roundCount = value; RaisePropertyChanged(); }
        }

        private int _testCount = 3;
        /// <summary>
        /// 要测试的回合数
        /// </summary>
        public int TestCount
        {
            get => _testCount;
            set { _testCount = value; RaisePropertyChanged() ; }
        }

        private int _recordCycleCount;
        /// <summary>
        /// 记录的循环次数
        /// </summary>
        public int RecordCycleCount
        {
            get => _recordCycleCount;
            set { _recordCycleCount = value; RaisePropertyChanged(); }
        }

        private int _recordCount;
        /// <summary>
        /// 记录的开合次数
        /// </summary>
        public int RecordCount
        {
            get => _recordCount;
            set { _recordCount = value;RaisePropertyChanged() ; }
        }

        private int _recordRoundCount;
        /// <summary>
        /// 记录的回合数
        /// </summary>
        public int RecordRoundCount
        {
            get { return _recordRoundCount; }
            set { _recordRoundCount = value; RaisePropertyChanged(); }
        }

        private int _stopTime = 180;
        /// <summary>
        /// 每个循环后停止的时间
        /// </summary>
        public int StopTime
        {
            get => _stopTime;
            set { _stopTime = value; RaisePropertyChanged() ; }
        }

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
        public DelegateCommand StopMotorCommand { get; set; }
        public DelegateCommand OpenValveCommand { get; set; }
        public DelegateCommand CloseValveCommand { get; set; }
        public DelegateCommand SetPosCommand { get; set; }
        private void InitProperty()
        {
            _parser = new ButterflyValveTestParser();
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;
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
            Entity = new ButterflyValveEntity(this);
        }
        private void InitCommand()
        {
            StopMotorCommand = new DelegateCommand(StopMotor);
            OpenValveCommand = new DelegateCommand(OpenValve);
            CloseValveCommand = new DelegateCommand(CloseValve);
            SetPosCommand = new DelegateCommand(SetPos);
            ButterflyValveTestCommand = new DelegateCommand(ButterflyValveTest);
            ShowDialogCommand = new DelegateCommand(ShowDialog);
        }
       
        /// <summary>
        /// 停止电机
        /// </summary>
        private void StopMotor()
        {
            _cts?.Cancel();
            Entity.SetRsCommand(0);
        }
       /// <summary>
       /// 开阀门
       /// </summary>
        private void OpenValve()
        {
            Entity.SetPosCommand(OpeningValue);
        }
          
        /// <summary>
        /// 关阀门
        /// </summary>
        private void CloseValve()
        {
            Entity.SetPosCommand(0);
        }
        /// <summary>
        /// 设置开比度
        /// </summary>
        private void SetPos()
        {
            Entity.SetPosCommand(OpeningValue);
        }
        public DelegateCommand ButterflyValveTestCommand { get; set; }

        public void InitTimer()
        {
            QueryStatusTimer = new System.Timers.Timer(1000);
            QueryStatusTimer.Elapsed += QueryStatusTimer_Elapsed;
            QueryStatusTimer.AutoReset = true;
            QueryStatusTimer.Start();
        }

        private void QueryStatusTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
           Entity.GetStatusCommand();
        }

        public void StopTimer()
        {
            QueryStatusTimer?.Stop();
            QueryStatusTimer?.Dispose();
            QueryStatusTimer = null;
        }
        /// <summary>
        /// 蝶阀测试
        /// </summary>
        private async void ButterflyValveTest()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            await Task.Run(async() => 
            {
                for (int i = 1; i <= TestCount * RoundCount; i++)
                {
                    if (_cts.IsCancellationRequested)
                    {
                        return;
                    }
                    bool result;
                    if (ReadOpeningValue > 0)
                    {
                        result = await OpeningClosingTask(false);
                    }
                    else
                    {
                        result = await OpeningClosingTask(true);
                    }

                    if (result == false)
                    {
                        return;
                    }
                    RecordCount++;
                    if (i > 0 && i % CycleCount == 0)
                    {
                        await Task.Delay(StopTime * 1000);
                        RecordCycleCount++;
                    }
                    if (i > 0 && i % RoundCount == 0)
                    {
                        ShowDialog();
                        RecordRoundCount++;
                    }
                    
                }
            }, _cts.Token);
                
            
            
        }
        /// <summary>
        /// 一次开合任务
        /// </summary>
        /// <returns></returns>
        private async Task<bool> OpeningClosingTask(bool firstValveSwitch,CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return false;
            }
            var closeResult =  await ResultFeedback(firstValveSwitch);
            if (ct.IsCancellationRequested)
            {
                return false;
            }
            var openResult =  await ResultFeedback(!firstValveSwitch);
            if (closeResult == false || openResult == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 执行单次开或者是关
        /// </summary>
        /// <param name="status"> false 是关 true 是开</param>
        /// <returns></returns>
        private async Task<bool> ResultFeedback(bool status, CancellationToken ct = default)
        {
            if (status)
            {
                OpenValve();
            }
            else
            {
                CloseValve();
            }
            try
            {
                if (ct.IsCancellationRequested)
                {
                    return false;
                }
                _startReply = new TaskCompletionSource<string>();
                var startResult = await _startReply.Task.WaitAsync(TimeSpan.FromSeconds(10),ct);
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"异常,启动阀门超时{ex}");
                StopMotor();
                return false;
            }
            try
            {
                if (ct.IsCancellationRequested)
                {
                    return false;
                }
                _endReply = new TaskCompletionSource<string>();
                var endResult = await _startReply.Task.WaitAsync(TimeSpan.FromSeconds(WaitTime),ct);
                
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"异常,阀门运行超时{ex}");
                StopMotor();
                return false;
            }
          
            return CheckResult(status);
        }

        private bool CheckResult(bool valveSwitch)
        {
            if (ButterflyValveHardwareLimit == ButterflyValveHardwareLimit.ALL_TRIGGERED)
            {
                MessageBox.Show("检测到同时触发硬件全开限位，和硬件全关限位限位，请联系工程师排查故障！");
                StopMotor();
                return false;
            }
            if (valveSwitch)
            {
                if (ButterflyValveHardwareLimit == ButterflyValveHardwareLimit.TRIGGER_CLOSE_LIMIT)
                {
                    MessageBox.Show("检测到触发硬件全关限位，限位可能反向，请联系工程师排查故障后重试！");
                    StopMotor();
                    return false;
                }
            }
            else
            {
                if (ButterflyValveHardwareLimit != ButterflyValveHardwareLimit.TRIGGER_CLOSE_LIMIT)
                {
                    MessageBox.Show("没有检测到触发硬件全关限位，请联系工程师排查故障后重试！");
                    StopMotor();
                    return false;
                }
            }
            return true;
           
        }
        public DelegateCommand ShowDialogCommand { get; set; }
        private void ShowDialog()
        {
            _butterflyValveTestModel.ShowDialog("检漏完成请按下确认按钮");
        }
        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        private void Parser_PacketReceivedEvent(object? sender, ButterflyValveTestPacket e)
        {
            switch (e.CmdType) 
            {
                case ButterflyValveTestProtocol.ButterflyValveCmdCode.GATEVALVE_SET_POS://设置阀门开度
                    break;
                case ButterflyValveTestProtocol.ButterflyValveCmdCode.GATEVALVE_SET_RS://设置电机运行停止
                    break;
                case ButterflyValveTestProtocol.ButterflyValveCmdCode.GATEVALVE_GET_STATUS://获取控制板状态
                    PaeserGetStatue(e);
                    break;
                case ButterflyValveTestProtocol.ButterflyValveCmdCode.GATEVALVE_GET_ABS_POS://获取电位器或编码器的绝对位置
                    PaeserGetAbsPos(e);
                    break;
            }
        }

        private void PaeserGetStatue(ButterflyValveTestPacket e)
        {
            var data = e.DataSource;
            var r0 = data[0];
            var r1 = data[1];
            var r2 = data[2];
            var r3 = data[3];
            var r4 = data[4];
            var runningStatusMask = data[5];
            var r5 = data[6];
            sbyte runDir =(sbyte)data[7];
            int r6 = BitConverter.ToInt32(data, 8);
            float openingValue = BitConverter.ToSingle(data, 12);
            int r7 = BitConverter.ToInt32(data, 16);
            //解析运动状态掩码
            var sz = (runningStatusMask & 0b00100000) >> 5;//电机异常状态显示
            var rs = (runningStatusMask & 0b00001000) >> 3;//运行停止指示
            var n = (runningStatusMask & 0b00000100) >> 2;//阀门全关硬件限位
            var z = (runningStatusMask & 0b00000010) >> 1;//阀门硬件限位异常
            var p = (runningStatusMask & 0b00000001) >> 0;//阀门全开硬件限位
            ValveRunuingDirection = runDir switch
            {
                -1 => ButterflyValveRunuingDirection.REVERSE,
                0 => ButterflyValveRunuingDirection.STOP,
                1 => ButterflyValveRunuingDirection.FORWARD,
                _ => throw new ArgumentOutOfRangeException(nameof(runDir))
            };
            ReadOpeningValue = openingValue;
            MotorStatus = sz == 1 ? true:false;
            IsRunning = rs == 1 ? true:false;
            if (_isTest  && IsRunning && !_isStart)
            {
                _isStart = true;
                _startReply?.TrySetResult("start");
            }
            if (_isTest &&  _isStart && !IsRunning) 
            {
                _isStart = false;
                _endReply?.TrySetResult("end");
            }
            ButterflyValveHardwareLimit = (n, p) switch
            {
                (0, 0) => ButterflyValveTestProtocol.ButterflyValveHardwareLimit.LIMIT_NOT_TRIGGERED,
                (0, 1) => ButterflyValveTestProtocol.ButterflyValveHardwareLimit.TRIGGER_OPEN_LIMIT,
                (1, 0) => ButterflyValveTestProtocol.ButterflyValveHardwareLimit.TRIGGER_CLOSE_LIMIT,
                (1, 1) => ButterflyValveTestProtocol.ButterflyValveHardwareLimit.ALL_TRIGGERED,
            };
            ValveHardwareLimitStatus = z == 1 ? true : false;
        }

        private void PaeserGetAbsPos(ButterflyValveTestPacket e)
        {
            var data = e.DataSource;
            var r0 = data[0];
            int EncoderValue = BitConverter.ToInt32(data, 1);

        }
    }
}
