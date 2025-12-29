using CsvHelper;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UtilityTools.Modules.ButterflyValveTest.Entity;
using UtilityTools.Modules.ButterflyValveTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.ButterflyValveTestProtocol;
using static UtilityTools.Modules.ButterflyValveTest.Protocol.OldButterflyValveTestProtocol;

namespace UtilityTools.Modules.ButterflyValveTest.Model
{
    public class OldButterflyValveModel:BindableBase
    {
        public OldButterflyValveModel(ButterflyValveTestModel butterflyValveTestModel)
        {
            _butterflyValveTestModel = butterflyValveTestModel;
            InitProperty();
            InitCommand();
        }
        private ButterflyValveTestModel _butterflyValveTestModel;
       
    
        private CancellationTokenSource _cts;
        private bool _isTest = false;
        private bool _isStart = false;
        private bool _isEnd = false;
        private OldButterflyValveTestParser _paser;
        private OldButterflyValveEntity _entity;
        /// <summary>
        /// 实例
        /// </summary>
        public OldButterflyValveEntity Entity
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
        private int _eachWaitingInterval = 5;
        public int EachWaitingInterval
        {
            get { return _eachWaitingInterval; }
            set { _eachWaitingInterval = value; RaisePropertyChanged(); }
        }
       
        private int _cycleCount = 15;
        /// <summary>
        /// 一个循环开合次数
        /// </summary>
        public int CycleCount
        {

            get => _cycleCount;
            set { _cycleCount = value; RaisePropertyChanged(); }
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
            set { _testCount = value; RaisePropertyChanged(); }
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
            set { _recordCount = value; RaisePropertyChanged(); }
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
            set { _stopTime = value; RaisePropertyChanged(); }
        }
        private float _vol = 0;
        public float Vol
        {
            get => _vol;
            set { _vol = value; RaisePropertyChanged(); }
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
        private System.Timers.Timer QueryStatusTimer;

        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }

        public DelegateCommand OpenValveCommand { get; set; }
        public DelegateCommand CloseValveCommand { get; set; }

        public DelegateCommand ButterflyValveTestCommand { get; set; }
        public DelegateCommand StopButterflyValveTestCommand { get; set; }
        private void InitProperty()
        {

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
            NetUdpService.UpdateResponse += SerialPortService_UpdateResponse;
            _paser = new OldButterflyValveTestParser();
            _paser.PacketReceivedEvent += Paser_PacketReceivedEvent;
            Entity = new OldButterflyValveEntity(this);
        }

        private void Paser_PacketReceivedEvent(object? sender, OldButterflyValveTestPacket e)
        {
            switch(e.CmdType)
            {
                case OldButterflyValveCmdCode.GET_GATE_VALVE:
                    var order = BitConverter.ToUInt32(e.DataSource, 0);
                    Vol = BitConverter.ToSingle(e.DataSource, 4);
                    break;
            }
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _paser.ReceiveBytes(e);
        }


        private void InitCommand()
        {
          
            OpenValveCommand = new DelegateCommand(OpenValve);
            CloseValveCommand = new DelegateCommand(CloseValve);
            ButterflyValveTestCommand = new DelegateCommand(ButterflyValveTest);
            StopButterflyValveTestCommand = new DelegateCommand(StopButterflyValveTest);


        }

        private void StopButterflyValveTest()
        {
            _cts?.Cancel();
        }


        /// <summary>
        /// 开阀门
        /// </summary>
        private void OpenValve()
        {
            Entity.SetGateValveCommand(3.3f);
        }

        /// <summary>
        /// 关阀门
        /// </summary>
        private void CloseValve()
        {
            Entity.SetGateValveCommand(0);
        }
        public void InitTimer()
        {
            QueryStatusTimer = new System.Timers.Timer(1000);
            QueryStatusTimer.Elapsed += QueryStatusTimer_Elapsed;
            QueryStatusTimer.AutoReset = true;
            QueryStatusTimer.Start();
        }

        private void QueryStatusTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Entity.GetLeakValveCommand();
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
           
            await Task.Run(async () =>
            {
                try
                {
                    for (int i = 1; i <= TestCount * RoundCount; i++)
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            return;
                        }

                        OpenValve();
                        await Task.Delay(EachWaitingInterval * 1000, _cts.Token);
                        CloseValve();
                        await Task.Delay(EachWaitingInterval * 1000, _cts.Token);
                       
                        if (i > 1 && i % CycleCount == 0)
                        {
                            await Task.Delay(StopTime * 1000);
                            RecordCycleCount++;
                        }
                        if (i > 1 && i % RoundCount == 0)
                        {
                           
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                _butterflyValveTestModel.ShowDialog("请开始捡漏，检测完成请确认");
                            }));
                            RecordRoundCount++;
                        }
                        RecordCount++;
                    }
                }
                catch (Exception ex) 
                {
                    _cts?.Dispose();
                    NLog.LogManager.GetCurrentClassLogger().Debug($"蝶阀测试退出，异常{ex}");
                }
               
            }, _cts.Token);
        }
     

        
    }

}
