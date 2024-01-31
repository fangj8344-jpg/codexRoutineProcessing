using NLog;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.IonPump.Model;
using UtilityTools.Modules.IonPump.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;


namespace UtilityTools.Modules.IonPump.ViewModels
{
    public class AgilentIonPumpViewModel : RegionViewModelBase
    {
        public AgilentIonPumpViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider)
        {
            _dialogHostService = dialogHostService;

            InitCommand();
            InitProperty();
        }

        ~AgilentIonPumpViewModel()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (_service != null)
            {
                _service.Close();
            }
        }


        #region ------------Field------------
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly IDialogHostService _dialogHostService;

        private RespParser _respParser;

        private System.Timers.Timer _timer;


        #endregion

        #region ------------Property------------
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }


        private AgilentIonPumpVo _ionPumpVo;

        public AgilentIonPumpVo IonPumpVo
        {
            get { return _ionPumpVo; }
            set { _ionPumpVo = value; }
        }


        private ObservableCollection<string> _logs;
        /// <summary>
        /// 通讯日志
        /// </summary>
        public ObservableCollection<string> Logs
        {
            get { return _logs; }
            set { _logs = value; RaisePropertyChanged(); }
        }


        private IAsynRWService _service;
        /// <summary>
        /// 异步通信服务
        /// </summary>
        public IAsynRWService Service

        {
            get { return _service; }
            set { _service = value; RaisePropertyChanged(); }
        }

        private string _monitorState;
        /// <summary>
        /// 监控状态
        /// </summary>
        public string MonitorState
        {
            get { return _monitorState; }
            set { _monitorState = value; RaisePropertyChanged(); }
        }

        private int _monitorInterval;
        /// <summary>
        /// 采样间隔，单位ms
        /// </summary>
        public int MonitorInterval
        {
            get { return _monitorInterval; }
            set
            {
                _monitorInterval = value;
                RaisePropertyChanged();
                if (_timer != null && _timer.Enabled)
                {
                    _timer.Interval = value;
                }
            }
        }


        private int _targetVoltage;
        public int TargetVoltage
        {
            get { return _targetVoltage; }
            set
            {
                if (value < 0) value = 0;
                if (7000 < value) value = 7000;

                _targetVoltage = value;
                RaisePropertyChanged();
            }
        }


        private PlotModel _ironPumpPlotModel;
        public PlotModel IronPumpPlotModel
        {
            get { return _ironPumpPlotModel; }
            set { _ironPumpPlotModel = value; RaisePropertyChanged(); }
        }


        LineSeries _ls_voltage;
        LineSeries _ls_pressure;
        LineSeries _ls_current;
        LineSeries _ls_temperature;

        #endregion


        #region Command
        public DelegateCommand QueryStatusCommand { get; set; }
        public DelegateCommand QueryErrorCodeCommand { get; set; }
        public DelegateCommand QueryPressureCommand { get; set; }
        public DelegateCommand QueryPressureUnitCommand { get; set; }
        public DelegateCommand QueryMaxPowerCommand { get; set; }
        public DelegateCommand QueryTempCommand { get; set; }
        public DelegateCommand QueryVoltageCommand { get; set; }
        public DelegateCommand QueryCurrentCommand { get; set; }

        public DelegateCommand SetHvOnCommand { get; set; }
        private void SetHvOn()
        {
            _service.SendMsg(AgilentIonPumpProtocol.IncreaseVoltage());
        }


        public DelegateCommand SetHvOffCommand { get; set; }
        private void SetHvOff()
        {
            _service.SendMsg(AgilentIonPumpProtocol.DecreaseVoltage());
        }

        public DelegateCommand SetTargetVolCommand { get; set; }
        private void SetTargetVol()
        {
            _service.SendMsg(AgilentIonPumpProtocol.SetTargetVoltage(TargetVoltage));
        }

        public DelegateCommand ChangeMonitorStateCommand { get; set; }
        private void ChangeMonitorState()
        {
            if (_timer == null)
            {
                _timer = new Timer();
                _timer.AutoReset = true;
                _timer.Elapsed += Timer_Elapsed;
            }

            _timer.Interval = MonitorInterval;
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Start();
            }


            MonitorState = _timer.Enabled ? "停止监控" : "开始监控";
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Service.IsOpen) return;


            QueryStatusCommand.Execute();
            QueryPressureCommand.Execute();
            QueryCurrentCommand.Execute();
            QueryVoltageCommand.Execute();
            QueryTempCommand.Execute();
        }

        public DelegateCommand ClearMonitorCommand { get; set; }
        private void ClearMonitor()
        {
            _ls_current.Points.Clear();
            _ls_pressure.Points.Clear();
            _ls_voltage.Points.Clear();

            IronPumpPlotModel.InvalidatePlot(true);
        }


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

        #endregion


        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);

            QueryStatusCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryStatusCmd()); });
            QueryErrorCodeCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryErrorCodeCmd()); });
            QueryPressureCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryPressureCmd()); });
            QueryPressureUnitCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryPressureUnitCmd()); });
            QueryMaxPowerCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryMaxPowerCmd()); });
            QueryTempCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryTempCmd()); });
            QueryVoltageCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryVoltageCmd()); });
            QueryCurrentCommand = new DelegateCommand(() => { Service.SendMsg(AgilentIonPumpProtocol.QueryCurrentCmd()); });


            ChangeMonitorStateCommand = new DelegateCommand(ChangeMonitorState);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            SetTargetVolCommand = new DelegateCommand(SetTargetVol);
            SetHvOffCommand = new DelegateCommand(SetHvOff);
            SetHvOnCommand = new DelegateCommand(SetHvOn);
        }

        private void InitProperty()
        {
            TargetVoltage = 6000;

            IsConnected = false;
            MonitorState = "开始监控";
            MonitorInterval = 1000;

            IronPumpPlotModel = new PlotModel();
            IronPumpPlotModel.Legends.Add(new Legend());

            IronPumpPlotModel.Axes.Add(new LinearAxis() { Title = "电流", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.None, PositionTier = 1, Key = "axisY_Current", IsAxisVisible = true });
            IronPumpPlotModel.Axes.Add(new LinearAxis() { Title = "压强", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.None, PositionTier = 2, Key = "axisY_Pressure", IsAxisVisible = true });
            IronPumpPlotModel.Axes.Add(new LinearAxis() { Title = "电压", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.None, PositionTier = 3, Key = "axisY_Voltage", IsAxisVisible = true });
            IronPumpPlotModel.Axes.Add(new LinearAxis() { Title = "温度", Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.None, PositionTier = 4, Key = "axisY_Temperature", IsAxisVisible = true });

            IronPumpPlotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, FontSize = 13 });

            _ls_current = new LineSeries() { Title = "电流", RenderInLegend = true, YAxisKey = "axisY_Current" };
            _ls_pressure = new LineSeries() { Title = "压强", RenderInLegend = true, YAxisKey = "axisY_Pressure" };
            _ls_voltage = new LineSeries() { Title = "电压", RenderInLegend = true, YAxisKey = "axisY_Voltage" };
            _ls_temperature = new LineSeries() { Title = "温度", RenderInLegend = true, YAxisKey = "axisY_Temperature" };

            IronPumpPlotModel.Series.Add(_ls_current);
            IronPumpPlotModel.Series.Add(_ls_pressure);
            IronPumpPlotModel.Series.Add(_ls_voltage);
            IronPumpPlotModel.Series.Add(_ls_temperature);


            Service = containerProvider.Resolve<IServiceFactory>().GetAsynRWService("GSP", "安捷伦离子泵");
            Service.IsBinary = false;
            Service.MinWriteInterval = 100;
            var Model = Service.GetHandle() as SerialPortModel;
            Model.BaudRate = 9600;
            Service.UpdateResponse += (object sender, byte[] data) => _respParser.ReceiveBytes(data);

            _respParser = new RespParser();
            //_respParser.RespReceivedEvent += AsyncRespReceived;
            _respParser.RespReceivedEvent += RespReceived;

            Logs = new ObservableCollection<string>();

            IonPumpVo = new AgilentIonPumpVo();
        }

        private void AsyncRespReceived(object sender, Resp resp)
        {
            Task.Run(() =>
            {
                RespReceived(sender, resp);
            });
        }

        private void RespReceived(object sender, Resp resp)
        {
            var now = DateTime.Now;
            if (resp.CmdType == EnumCmdType.READ)
            {
                var packet = resp.GetReadResp();
                switch (packet.GetWinType())
                {
                    case EnumWinType.R_ERROR_CODE:
                        {
                            IonPumpVo.ErrorCode = ((EnumErrorCode)int.Parse(packet.GetDataStr())).ToString();
                            break;
                        }
                    case EnumWinType.RW_PRESSURE_UNIT:
                        {
                            IonPumpVo.PressureUnit = ((EnumPressureUnit)int.Parse(packet.GetDataStr())).ToString();
                            break;
                        }
                    case EnumWinType.RW_MAX_POWER:
                        {
                            IonPumpVo.MaxPower = $"{packet.GetDataStr()}W";
                            break;
                        }
                    case EnumWinType.R_STATUS: IonPumpVo.Status = packet.GetDataStr(); break;
                    case EnumWinType.R_TEMP_POWER_SECTION:
                        {
                            IonPumpVo.Temp = packet.GetDataStr();
                            _ls_temperature.Points.Add(DateTimeAxis.CreateDataPoint(now, float.Parse(IonPumpVo.Temp)));
                            IronPumpPlotModel.InvalidatePlot(true);
                            break;
                        }
                    case EnumWinType.R_PRESSURE:
                        {
                            IonPumpVo.Pressure = packet.GetDataStr();
                            _ls_pressure.Points.Add(DateTimeAxis.CreateDataPoint(now, float.Parse(IonPumpVo.Pressure)));
                            IronPumpPlotModel.InvalidatePlot(true);
                            break;
                        }
                    case EnumWinType.R_VOLTAGE:
                        {
                            IonPumpVo.Voltage = packet.GetDataStr();
                            _ls_voltage.Points.Add(DateTimeAxis.CreateDataPoint(now, float.Parse(IonPumpVo.Voltage)));
                            IronPumpPlotModel.InvalidatePlot(true);
                            break;
                        }
                    case EnumWinType.R_CURRENT:
                        {
                            IonPumpVo.Current = packet.GetDataStr();
                            _ls_current.Points.Add(DateTimeAxis.CreateDataPoint(now, float.Parse(IonPumpVo.Current)));
                            IronPumpPlotModel.InvalidatePlot(true);
                            break;
                        }
                }
            }
            else if (resp.CmdType == EnumCmdType.WRITE)
            {
                if (resp.GetWriteRespType() != EnumWriteRespType.SUCCESS)
                {
                    LOGGER.Error($"写命令异常：{resp.GetWriteRespType()}");
                }
            }
        }
    }
}
