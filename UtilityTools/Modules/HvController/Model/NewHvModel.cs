#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.HvController.Model
 * 唯一标识：dd91c703-d3c3-4d22-a7e2-efdd42ecf344
 * 文件名：NewHvModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/12/5 9:54:55
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Linq;
using UtilityTools.Modules.HvController.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.HvController.Model
{
    public class NewHvModel : BindableBase
    {
        #region ------------Constructor------------
        public NewHvModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _paser1 = new HVProtocolParser();
            _paser2 = new HVProtocolParser();
            _paser1.Service = SerialPortService;
            _paser2.Service = NetUdpService;
            SerialPortService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPHV");
            SerialPortService.UpdateResponse += Device_UpdateResponse;
            NetUdpService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("UNHV");
            NetUdpService.UpdateResponse += Device_UpdateResponse;
            _paser1.PacketReceivedEvent += Paser_PacketReceivedEvent;
            _paser2.PacketReceivedEvent += Paser_PacketReceivedEvent;

            PrepareWorkCommand = new DelegateCommand(PrepareWorkMethod);
            SetAccVolCommand = new DelegateCommand<object>(SetAccVolMethod);
            ChangeMonitorStateCommand = new DelegateCommand(ChangeMonitorState);
            AutoAdjustCommand = new DelegateCommand(AutoAdjust);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            SaveMonitorInfoCommand = new DelegateCommand(SaveMonitorInfo);
            SetCommand = new DelegateCommand<string>(Set);

            HvPlotModel = new PlotModel();
            HvPlotModel.Legends.Add(new Legend());
            HvPlotModel.Axes.Add(new LinearAxis() { Title = "时间", Position = OxyPlot.Axes.AxisPosition.Bottom });
            HvPlotModel.Axes.Add(new LogarithmicAxis() { Title = "数值", Position = OxyPlot.Axes.AxisPosition.Left });
            _accVolLineSeries = new LineSeries() { Title = "加速电压", RenderInLegend = true };
            _filaCurLineSeries = new LineSeries() { Title = "灯丝电流", RenderInLegend = true };
            _filaRLineSeries = new LineSeries() { Title = "灯丝电阻", RenderInLegend = true };
            _emissionVolLineSeries = new LineSeries() { Title = "吸取极电压", RenderInLegend = true };
            _gridVolLineSeries = new LineSeries() { Title = "栅极电压", RenderInLegend = true };
            HvPlotModel.Series.Add(_accVolLineSeries);
            HvPlotModel.Series.Add(_filaCurLineSeries);
            HvPlotModel.Series.Add(_filaRLineSeries);
            HvPlotModel.Series.Add(_emissionVolLineSeries);
            HvPlotModel.Series.Add(_gridVolLineSeries);

            if (_timer == null)
            {
                _timer = new System.Timers.Timer();
                _timer.AutoReset = true;
                _timer.Elapsed += Timer_Elapsed;
            }

            _timer.Interval = MonitorInterval;

            if (_timer.Enabled)
            {
                _timer.Stop();
                MonitorState = "开始监控";
            }
            else
            {
                _timer.Start();
                MonitorState = "停止监控";
            }
        }

       


        #endregion

        #region ------------Field------------
        private readonly IContainerProvider _containerProvider;
        private System.Timers.Timer _timer;
        private System.Timers.Timer _timerNewProtocol;
        private bool _isPiPrepareToComplete = false;
        private bool _isInquireHvInitDone = false;
        private bool _isHvInitDone =false;
        public bool _isNewProtocolConnect = false;
        

        private HVProtocolParser _paser1;
        private HVProtocolParser _paser2;

        private string _response = string.Empty;

        private BackgroundWorker _backgroundWorker;
        private bool _initResult = false;
        private AutoResetEvent _initEvent;
        private AutoResetEvent _initResultEvent;
        private AutoResetEvent _gridVolEvent;
        private AutoResetEvent _heatCurEvent;
        private AutoResetEvent _emissionVolEvent;

        LineSeries _accVolLineSeries;
        LineSeries _filaCurLineSeries;
        LineSeries _filaRLineSeries;
        LineSeries _emissionVolLineSeries;
        LineSeries _gridVolLineSeries;
        #endregion

        #region ------------Property------------
        private bool _isNewProtocol = true;
        public bool IsNewProtocol
        {
            get { return _isNewProtocol; }
            set { _isNewProtocol = value; RaisePropertyChanged(); }
        }

        private bool _isBvAdjust = true;
        public bool IsBvAdjust
        {
            get { return _isBvAdjust; }
            set { _isBvAdjust = value; RaisePropertyChanged(); }
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
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }

        private bool _isPrepared = false;
        /// <summary>
        /// 高压箱是否已经准备完毕
        /// </summary>
        public bool IsPrepared
        {
            get { return _isPrepared; }
            set 
            {
                _isPrepared = value;
                RaisePropertyChanged();

                if (_isPrepared && IsNewProtocol == false)
                {
                    PrepareState = "关闭高压";
                }
                else if(!_isPrepared && IsNewProtocol == false)
                {
                    PrepareState = "高压准备";
                }
            }
        }
        private bool _isPrepareAllComplete = false;
        public bool IsPrepareAllComplete
        { 
            get { return _isPrepareAllComplete; }
            set { _isPrepareAllComplete = value; RaisePropertyChanged();
                if (_isPrepareAllComplete && IsNewProtocol == true)
                {
                    PrepareState = "取消高压准备";
                }
                else if (!_isPrepareAllComplete && IsNewProtocol == true)
                {
                    PrepareState = "高压准备";
                }

            }
        }

        private float _setAccVol = 0;
        /// <summary>
        /// 设置加速电压，0-16kV
        /// </summary>
        public float SetAccVol
        {
            get { return _setAccVol; }
            set 
            {
                if (value < 0)
                    value = 0;
                else if (value > 16.0f)
                    value = 16.0f;
                _setAccVol = value; 
                RaisePropertyChanged();

                if (IsPrepared && IsNewProtocol == false)
                {
                    SendMsg(NewHvControllerProtocol.GetAccVolCommand(value * 1000));
                }
                else if (IsPrepareAllComplete && IsNewProtocol == true)
                {
                    SendMsg(NewHvControllerProtocol.SetHVCmd(value));
                }
            }
        }

        private float _setHeatCur = 2.0f;
        /// <summary>
        /// 设置加热电流, 0-3.2A
        /// </summary>
        public float SetHeatCur
        {
            get { return _setHeatCur; }
            set 
            {
                if (value < 0)
                    value = 0;
                else if (value > 3.2f)
                    value = 3.2f;
                _setHeatCur = value; 
                RaisePropertyChanged();

                if (IsPrepared && IsNewProtocol == false)
                {
                    SendMsg(NewHvControllerProtocol.GetHeatCurCommand(value, 0x02));
                }
                else if (IsPrepareAllComplete == true && IsNewProtocol == true)
                {
                    SendMsg(NewHvControllerProtocol.SetPICmd(value, 0x02));
                }
            }
        }

        private float _setEmissionVol = 3.3f;
        /// <summary>
        /// 设置吸取极电压, 0-6.0kV
        /// </summary>
        public float SetEmissionVol
        {
            get { return _setEmissionVol; }
            set 
            {
                if (value < 0)
                    value = 0;
                else if (value > 6.0f)
                    value = 6.0f;

                _setEmissionVol = value;
                RaisePropertyChanged();

                if (IsPrepared && IsNewProtocol == false)
                {
                    SendMsg(NewHvControllerProtocol.GetEmissionVolCommand(value * 1000));
                }
                else if (IsPrepareAllComplete && IsNewProtocol == true)
                {
                    SendMsg(NewHvControllerProtocol.SetEVCmd(value));
                }
            }
        }

        private float _setGridVol = 0.3f;
        /// <summary>
        /// 设置栅极电压，0-2.0kV
        /// </summary>
        public float SetGridVol
        {
            get { return _setGridVol; }
            set 
            {
                if (value < 0)
                    value = 0;
                else if (value > 2.0f)
                    value = 2.0f;

                _setGridVol = value; 
                RaisePropertyChanged();

                if (IsPrepared && IsNewProtocol == false)
                {
                    SendMsg(NewHvControllerProtocol.GetGridVolCommand(value * 1000));
                }
                else if (IsPrepareAllComplete && IsNewProtocol == true)
                {
                    SendMsg(NewHvControllerProtocol.SetBVCmd(value, IsBvAdjust));
                }
            }
        }

        private float _accVol = float.NaN;
        /// <summary>
        /// 加速电压
        /// </summary>
        public float AccVol
        {
            get { return _accVol; }
            set { _accVol = value; RaisePropertyChanged(); }
        }

        private float _hi;
        /// <summary>
        /// 钨灯丝电流
        /// </summary>
        public float Hi
        {
            get { return _hi; }
            set { _hi = value; RaisePropertyChanged(); }
        }


        private float _filaCur = float.NaN;
        /// <summary>
        /// 灯丝电流
        /// </summary>
        public float FilaCur
        {
            get { return _filaCur; }
            set { _filaCur = value; RaisePropertyChanged(); }
        }
        private float _currentValue = float.NaN;
        /// <summary>
        /// 步进增长的当前数值
        /// </summary>
        public float CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = value; RaisePropertyChanged(); }
        }
        private float _targetValue = float.NaN;
        /// <summary>
        /// 步进增长的目标数值
        /// </summary>
        public float TargetValue
        {
            get { return _targetValue; }
            set { _targetValue = value; RaisePropertyChanged(); }
        }

        private float _filaVol = float.NaN;
        /// <summary>
        /// 灯丝电压
        /// </summary>
        public float FilaVol
        {
            get { return _filaVol; }
            set { _filaVol = value; RaisePropertyChanged(); }
        }

        private float _filaR = float.NaN;
        /// <summary>
        /// 灯丝电阻
        /// </summary>
        public float FilaR
        {
            get { return _filaR; }
            set { _filaR = value; RaisePropertyChanged(); }
        }

        private float _gridVol = float.NaN;
        /// <summary>
        /// 栅极电压
        /// </summary>
        public float GridVol
        {
            get { return _gridVol; }
            set { _gridVol = value; RaisePropertyChanged(); }
        }

        private float _emissionVol = float.NaN;
        /// <summary>
        /// 吸取极电压
        /// </summary>
        public float EmissionVol
        {
            get { return _emissionVol; }
            set { _emissionVol = value; RaisePropertyChanged(); }
        }
        private ushort _step = 2;
        public ushort Step
        {
            get { return _step; }
            set { _step = value; RaisePropertyChanged(); }
        }

        private float _emissionCur = float.NaN;
        /// <summary>
        /// 吸取极电流
        /// </summary>
        public float EmissionCur
        {
            get { return _emissionCur; }
            set { _emissionCur = value; RaisePropertyChanged(); }
        }

   

        private int _monitorInterval = 2000;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public int MonitorInterval
        {
            get { return _monitorInterval; }
            set { _monitorInterval = value; RaisePropertyChanged(); }
        }

        private string _prepareState = "高压准备";
        /// <summary>
        /// 准备状态
        /// </summary>
        public string PrepareState
        {
            get { return _prepareState; }
            set { _prepareState = value; RaisePropertyChanged(); }
        }

        private string _CurState = "高压未准备";
        /// <summary>
        /// 当前状态
        /// </summary>
        public string CurState
        {
            get { return _CurState; }
            set { _CurState = value; RaisePropertyChanged(); }
        }

        private double _progressValue;
        /// <summary>
        /// 进度条状态
        /// </summary>
        public double ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; RaisePropertyChanged(); }
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

        private PlotModel _hvPlotModel;
        /// <summary>
        /// 高压参数图表模型
        /// </summary>
        public PlotModel HvPlotModel
        {
            get { return _hvPlotModel; }
            set { _hvPlotModel = value; RaisePropertyChanged(); }
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
        #endregion

        #region ------------Command------------
        public DelegateCommand<string> SetCommand { get; set; }
      
        private void Set(string obj)
        {
            var message1 = NewHvControllerProtocol.SetHVCmd(SetGridVol);
            var message2 = NewHvControllerProtocol.SetPICmd(SetHeatCur, 0x02);
            var message3 = NewHvControllerProtocol.SetEVCmd(SetEmissionVol);
            var message4 = NewHvControllerProtocol.SetHVCmd(SetAccVol);
            switch (obj)
            {
                case "0":SendMsg(NewHvControllerProtocol.SetBVCmd(SetGridVol)); break;
                case "1":SendMsg(NewHvControllerProtocol.SetPICmd(SetHeatCur,0x02));break;
                case "2": SendMsg(NewHvControllerProtocol.SetEVCmd(SetEmissionVol)); break;
                case "3": SendMsg(NewHvControllerProtocol.SetHVCmd(SetAccVol)); break;
                case "4": SendMsg(NewHvControllerProtocol.SetInitHVCmd()); break;
            }
        }

        public DelegateCommand PrepareWorkCommand { get; set; }

        private void PrepareWorkMethod()
        {
            if (IsPrepared && IsNewProtocol == false)
            {
                IsPrepared = false;
                // 卸载高压准备工作
                StopBackgroundWorker();


                SendMsg(NewHvControllerProtocol.GetHeatCurCommand(0.0f, 0x02));
                SendMsg(NewHvControllerProtocol.GetAccVolCommand(0.0f));
                SendMsg(NewHvControllerProtocol.GetGridVolCommand(0.0f));
                SendMsg(NewHvControllerProtocol.GetEmissionVolCommand(0.0f));
                SendMsg(NewHvControllerProtocol.GetCloseHvCommand());
            }
            else if (_isPrepareAllComplete && IsNewProtocol == true)
            {
                IsPrepared = false;
                StopBackgroundWorker();
                //卸载高压工作后在将状态归零
                SendMsg(NewHvControllerProtocol.SetHVCmd(0.0f));
                Thread.Sleep(1000);
                SendMsg(NewHvControllerProtocol.SetEVCmd(0.0f));
                Thread.Sleep(1000);
                SendMsg(NewHvControllerProtocol.SetPICmd(0.0f, 0x02));
                Thread.Sleep(2000);
                SendMsg(NewHvControllerProtocol.SetBVCmd(0.0f, IsBvAdjust));
                Thread.Sleep(2000);
                SendMsg(NewHvControllerProtocol.CloseAllCmd());
                _isHvInitDone = false;
                IsPrepareAllComplete = false;
                

            }
            else
            {
                // 开启高压准备工作
                StartBackgroundWorker();
            }
        }

        public DelegateCommand<object> SetAccVolCommand { get; set; }

        private void SetAccVolMethod(object obj)
        {
            if (int.TryParse(obj.ToString(), out int value))
            {
                SetAccVol = value;
            }
        }

        public DelegateCommand ChangeMonitorStateCommand { get; set; }

        private async void ChangeMonitorState()
        {
            await Task.Run(() =>
            {
                if (_timer == null)
                {
                    _timer = new System.Timers.Timer();
                    _timer.AutoReset = true;
                    _timer.Elapsed += Timer_Elapsed;
                }

                _timer.Interval = MonitorInterval;

                if (_timer.Enabled)
                {
                    _timer.Stop();
                    MonitorState = "开始监控";
                }
                else
                {
                    _timer.Start();
                    MonitorState = "停止监控";
                }
            });
        }
       


        public DelegateCommand AutoAdjustCommand { get; set; }

        private void AutoAdjust()
        {
            foreach (var axis in HvPlotModel.Axes)
            {
                axis.Reset();
            }

            HvPlotModel.InvalidatePlot(true);
        }

        public DelegateCommand ClearMonitorCommand { get; set; }

        private void ClearMonitor()
        {
            _accVolLineSeries.Points.Clear();
            _filaCurLineSeries.Points.Clear();
            _filaRLineSeries.Points.Clear();
            _emissionVolLineSeries.Points.Clear();
            _gridVolLineSeries.Points.Clear();
        }

        public DelegateCommand SaveMonitorInfoCommand { get; set; }

        private void SaveMonitorInfo()
        {
            // 借鉴已经存在表格数据
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                var path = dialog.SelectedPath;

                SaveToFile(path);
            }
        }
        public async void GetHVParam()
        {
            if (IsNewProtocol)
            {
                await Task.Run(() =>
                {
                    if (_timerNewProtocol == null)
                    {
                        _timerNewProtocol = new System.Timers.Timer();
                        _timerNewProtocol.AutoReset = true;
                        _timerNewProtocol.Elapsed += TimerNewProtocol_Elapsed;
                    }

                    _timerNewProtocol.Interval = 1000;

                    if (!_timerNewProtocol.Enabled)
                    {
                        _timerNewProtocol.Start();
                    }
                });
            }
        }

        private void TimerNewProtocol_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendMsg(NewHvControllerProtocol.GetParamCmd());
            if (_isInquireHvInitDone)
            {
                SendMsg(NewHvControllerProtocol.GetHVInitState());
            }
      
        }

        private void SaveToFile(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var timeTip = DateTime.Now.ToString("HHmmss");
            PngExporter exporter = new PngExporter();

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                exporter.ExportToFile(HvPlotModel, $"{path}\\高压监控_{timeTip}.png");
                SaveSeriesToFile(_accVolLineSeries, $"{path}\\加速电压_{timeTip}.txt");
                SaveSeriesToFile(_filaCurLineSeries, $"{path}\\灯丝电流_{timeTip}.txt");
                SaveSeriesToFile(_filaRLineSeries, $"{path}\\灯丝电阻_{timeTip}.txt");
                SaveSeriesToFile(_emissionVolLineSeries, $"{path}\\吸取极电压_{timeTip}.txt");
                SaveSeriesToFile(_gridVolLineSeries, $"{path}\\栅极电压_{timeTip}.txt");
            }));
        }

        private void SaveSeriesToFile(DataPointSeries series, string filePath)
        {
            string gunReport = string.Empty;
            foreach (var report in series.Points)
            {
                gunReport += $"{report.X}\t{report.Y}\n";
            }
            try
            {
                using (var gunStream = File.OpenWrite(filePath))
                {
                    var gunData = Encoding.UTF8.GetBytes(gunReport);
                    gunStream.Write(gunData, 0, gunData.Length);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal($"保存图表数据异常：目标路径【{filePath}】，异常原因【{ex.Message}】");
            }
        }
        #endregion

        #region ------------PublicMethod------------
        public void StartBackgroundWorker()
        {
            StopBackgroundWorker();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopBackgroundWorker()
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();
            }
        }

        public void SetProgressInfo(string state, double value)
        {
            CurState = state;
            ProgressValue = value;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = sender as BackgroundWorker;
            if (IsNewProtocol == false)
            {

                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    SetProgressInfo("取消高压准备", 100);
                    return;
                }

                // 判断当前的灯丝电流
                if (float.IsNaN(FilaCur))
                {
                    e.Cancel = true;
                    SetProgressInfo("未连接设备", 100);
                    return;
                }

                if (FilaCur > 3.5)      // 高压箱处于未准备状态
                {
                    // 发送初始化指令
                    SetProgressInfo("初始化高压箱", 0);
                    _initEvent = new AutoResetEvent(false);
                    _initResultEvent = new AutoResetEvent(false);
                    SendMsg(NewHvControllerProtocol.GetInitCommand());
                    if (!_initEvent.WaitOne(3000))
                    {
                        SetProgressInfo("初始化高压箱超时", 100);
                        _initEvent = null;
                        e.Cancel = true;
                        return;
                    }
                    _initEvent = null;
                    SetProgressInfo("初始化高压箱", 10);

                    if (!_initResult)
                    {
                        SetProgressInfo("初始化高压箱失败，请稍后重试", 100);
                        _initResultEvent = null;
                        e.Cancel = true;
                        return;
                    }

                    if (!_initResultEvent.WaitOne(10000))
                    {
                        SetProgressInfo("初始化高压箱等待结果超时", 100);
                        e.Cancel = true;
                        _initResultEvent = null;
                        return;
                    }
                    _initResultEvent = null;
                }

                SetProgressInfo("初始化高压箱成功", 20);

                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    SetProgressInfo("取消高压准备", 100);
                    return;
                }

                // 初始化栅极电压
                _gridVolEvent = new AutoResetEvent(false);
                SendMsg(NewHvControllerProtocol.GetGridVolCommand(SetGridVol * 1000));
                if (!_gridVolEvent.WaitOne(3000))
                {
                    SetProgressInfo("设置栅极电压超时", 100);
                    _gridVolEvent = null;
                    e.Cancel = true;
                    return;
                }
                _gridVolEvent = null;
                SetProgressInfo("设置栅极电压成功", 30);

                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    SetProgressInfo("取消高压准备", 100);
                    return;
                }

                // 初始化加热电流
                _heatCurEvent = new AutoResetEvent(false);
                SendMsg(NewHvControllerProtocol.GetHeatCurCommand(SetHeatCur, 0x02));
                if (!_heatCurEvent.WaitOne(3000))
                {
                    _heatCurEvent = null;
                    e.Cancel = true;
                    SetProgressInfo("设置加热电流超时", 100);
                    return;
                }
                _heatCurEvent = null;
                SetProgressInfo("设置加热电流成功", 40);

                while (true)
                {
                    if (bw.CancellationPending == true)
                    {
                        e.Cancel = true;
                        SetProgressInfo("取消高压准备", 100);
                        return;
                    }
                    Thread.Sleep(500);

                    if (Math.Abs(SetHeatCur - FilaCur) < 0.1)
                    {
                        SetProgressInfo("完成加载加热电流", 90);
                        break;
                    }
                    else
                    {
                        SetProgressInfo("正在加载加热电流", 40 + FilaCur / SetHeatCur * 50);
                    }
                }

                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    SetProgressInfo("取消高压准备", 100);
                    return;
                }

                // 初始化吸取极电压
                _emissionVolEvent = new AutoResetEvent(false);
                SendMsg(NewHvControllerProtocol.GetEmissionVolCommand(SetEmissionVol * 1000));
                if (!_emissionVolEvent.WaitOne(3000))
                {
                    SetProgressInfo("设置吸取极电压超时", 100);
                    _emissionVolEvent = null;
                    e.Cancel = true;
                    return;
                }
                _emissionVolEvent = null;
                SetProgressInfo("设置吸取极电压成功", 100);
            }
            else if (IsNewProtocol)
            {
                NewProtocolWorkPrepare(sender,e);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void NewProtocolWorkPrepare(object sender, DoWorkEventArgs e)
        {
            if (_isNewProtocolConnect == true)
            {
                SendMsg(NewHvControllerProtocol.SetInitHVCmd());
                SetProgressInfo("初始化高压箱", 100);
            }
           
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == false && IsNewProtocol == false)
            {
                IsPrepared = true;
                SetProgressInfo("完成高压箱初始化", 100);
            }

            _backgroundWorker.DoWork -= BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted -= BackgroundWorker_RunWorkerCompleted;
            _backgroundWorker = null;
        }

        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 添加日志信息
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="isRead">是否是读取</param>
        private void AddLog(string log, bool isRead = true)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (Logs == null)
                {
                    Logs = new ObservableCollection<string>();
                }

                if (isRead)
                {
                    Logs.Add($"{DateTime.Now.ToString("hh:mm:ss")} 读取: {log}");
                }
                else
                {
                    Logs.Add($"{DateTime.Now.ToString("hh:mm:ss")} 发送: {log}");
                }
            });
        }

        /// <summary>
        /// 发送消息接口
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMsg(byte[] bytes)
        {
            if (SerialPortService.IsOpen)
                SerialPortService.SendMsg(bytes);
            else if (NetUdpService.IsOpen)
                NetUdpService.SendMsg(bytes);
        }


        /// <summary>
        /// 定时器超时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsNewProtocol == false)
            {
                // Work
                AddLog("请求高压状态");
                var cmd = NewHvControllerProtocol.GetRequestCommand();
                SendMsg(cmd);
            }
        }


        /// <summary>
        /// 设备数据回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_UpdateResponse(object sender, byte[] e)
        {
            if (IsNewProtocol == false)
            {
                if (e == null)
                    return;

                _response += Encoding.UTF8.GetString(e);

                while (_response.Contains("\r\n"))
                {
                    bool isEndWith = _response.EndsWith("\r\n");
                    var list = _response.Split('\n');
                    var count = list.Count();
                    if (!isEndWith)
                    {
                        _response = list.Last();
                        count--;
                    }
                    else
                    {
                        _response = string.Empty;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        ParseResponse(list[i]);
                    }

                }
            }
            else if (IsNewProtocol == true) 
            {
                if (SerialPortService.IsOpen)
                {
                    _paser1.ReceiveBytes(e);
                }
                else if (NetUdpService.IsOpen)
                {
                    _paser2.ReceiveBytes(e);
                }
                
            }
            
        }

        private void ParseResponse(string msg)
        {
            if (msg.StartsWith("read"))
            {
                // 状态信息
                ParseReadParams(msg);
            }
            else if (msg.StartsWith("HV_INIT"))
            {
                AddLog("开始初始化高压设备");
                if (_initEvent != null)
                {
                    _initResult = true;
                    _initEvent.Set();
                }
            }
            else if (msg.StartsWith("HV_init_done"))
            {
                AddLog("完成初始化高压设备");
                if (_initResultEvent != null)
                    _initResultEvent.Set();
            }
            else if (msg.StartsWith("BV"))
            {
                AddLog($"设置栅极电压:{msg}");
                if(_gridVolEvent != null)
                    _gridVolEvent.Set();
            }
            else if (msg.StartsWith("PI"))
            {
                AddLog($"设置加热电流:{msg}");
                if (_heatCurEvent != null)
                    _heatCurEvent.Set();
            }
            else if (msg.StartsWith("EV"))
            {
                AddLog($"设置吸取电压:{msg}");
                if (_emissionVolEvent != null)
                    _emissionVolEvent.Set();
            }
            else if (msg.StartsWith("HV"))
            {
                AddLog($"设置加速电压:{msg}");
            }
            else if (msg.StartsWith("CLOSE"))
            {
                AddLog($"关闭高压设备");
            }
            else if (msg.StartsWith("DEFAULT"))
            {
                AddLog("初始化高压设备失败");
                if (_initEvent != null)
                {
                    _initResult = false;
                    _initEvent.Set();
                }
            }
        }

        private void ParseReadParams(string readStr)
        {
            var list = readStr.Split("---");
            foreach (var item in list)
            {
                var tmpList = item.Split("_");
                if (tmpList.Length == 3)
                {
                    switch (tmpList[1])
                    {
                        case "HV":
                            if (float.TryParse(tmpList[2], out float hv))
                            {
                                AccVol = hv;
                                _accVolLineSeries.Points.Add(new DataPoint(_accVolLineSeries.Points.Count, hv));
                            }
                            break;
                        case "HI":
                            if (float.TryParse(tmpList[2], out float hi))
                            {
                                Hi = hi;
                            }
                            break;
                        case "PV":
                            if (float.TryParse(tmpList[2], out float pv))
                            {
                                FilaVol = pv;
                            }
                            break;
                        case "PI":
                            if (float.TryParse(tmpList[2], out float pi))
                            {
                                FilaCur = pi;
                                _filaCurLineSeries.Points.Add(new DataPoint(_filaCurLineSeries.Points.Count, pi));
                            }
                            break;
                        case "R":
                            if (float.TryParse(tmpList[2], out float r))
                            {
                                FilaR = r;
                                _filaRLineSeries.Points.Add(new DataPoint(_filaRLineSeries.Points.Count, r));
                            }
                            break;
                        case "EV":
                            if (float.TryParse(tmpList[2], out float ev))
                            {
                                EmissionVol = ev;
                                _emissionVolLineSeries.Points.Add(new DataPoint(_emissionVolLineSeries.Points.Count, ev));
                            }
                            break;
                        case "EI":
                            if (float.TryParse(tmpList[2], out float ei))
                            {
                                EmissionCur = ei;
                            }
                            break;
                        case "BV":
                            if (float.TryParse(tmpList[2], out float bv))
                            {
                                GridVol = bv;
                                _gridVolLineSeries.Points.Add(new DataPoint(_gridVolLineSeries.Points.Count, bv));
                            }
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 新协议回报接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Paser_PacketReceivedEvent(object sender, HVDataPacket e)
        {
            var paser = sender as HVProtocolParser;
            IAsynRWService? service = null;
            if (paser != null)
            {
                service = paser.Service;
            }
            var cmd = e.CmdType;
            if (!Enum.IsDefined(typeof(EnumHvCommandType), cmd))
            {
                return;
            }
            switch (e.CmdType)
            {
                case  EnumHvCommandType.CMD_GET_PARAM:
                    _isNewProtocolConnect = true;
                    float hv = BitConverter.ToSingle(e.DataSource,0);
                    float hi = BitConverter.ToSingle(e.DataSource, 4);
                    float pv = BitConverter.ToSingle(e.DataSource, 8);
                    float pi = BitConverter.ToSingle(e.DataSource, 12);
                    float r = BitConverter.ToSingle(e.DataSource, 16);
                    float ev = BitConverter.ToSingle(e.DataSource, 20);
                    float ei = BitConverter.ToSingle(e.DataSource, 24);
                    float bv = BitConverter.ToSingle(e.DataSource, 28);
                    AccVol = hv;
                    Hi = hi;
                    FilaCur = pi;
                    FilaVol = pv;
                    FilaR = r;
                    GridVol = ev;
                    EmissionVol = bv;
                    EmissionCur = ei;
                    break;
                case EnumHvCommandType.CMD_HV_INIT:
                    byte initResult = e.DataSource[0];
                    if (initResult == 0)//0代表行为执行1代表不执行
                    {
                        //初始化成功后开始查询进度，查询初始化完成后才能够进行发送的工作
                        _isInquireHvInitDone = true;
                       
                    }
                    else if (initResult == 1)
                    {

                        //初始化失败
                    }
                    break;
                case EnumHvCommandType.CMD_SET_BV:
                    if (_isPrepareAllComplete == false && _isHvInitDone == true)
                    {
                        SetProgressInfo("设置栅极电压", 100);
                        //栅极电压接收成功，开始设置加热电流
                        SendMsg(NewHvControllerProtocol.SetPICmd(FilaCur, Step));
                    }
                    break;  
                case EnumHvCommandType.CMD_SET_PI:
                    if (_isPrepareAllComplete == false && _isHvInitDone == true)
                    {
                        SetProgressInfo("设置加热电流中", 100);
                        //加热电流命令接收 需要等待加热电流完成 此时启用等待 等待完成后设置吸取极电压
                        WaitPIPrepareToComplete();
                    }
                        
                    break;
                case EnumHvCommandType.CMD_SET_EV:
                    if (_isHvInitDone == true && IsPrepareAllComplete == false)
                    {
                        IsPrepareAllComplete = true;
                        IsPrepared = true;
                    }
                    //吸取电压设置完成后 可以表示准备成功


                    break;
                case EnumHvCommandType.CMD_SET_HV:
                    break;
                case EnumHvCommandType.CMD_CLOSE_ALL:
                    break;
                case EnumHvCommandType.CMD_GET_HV_INIT_STATE:
                    var value = e.DataSource[0];
                    if (value == 1 && _isHvInitDone == false && IsPrepareAllComplete == false)
                    {
                        //代表高压箱初始化完成
                        SetProgressInfo("初始化高压箱完成", 100);
                        _isHvInitDone = true;
                        _isInquireHvInitDone = false;
                        SendMsg(NewHvControllerProtocol.SetBVCmd(GridVol, IsBvAdjust));
                    }
                    else if (value == 0)
                    {
                        SetProgressInfo("正在初始化高压箱", 100);
                        //代表还在初始化高压箱中。
                    }
                    break;
                case EnumHvCommandType.CMD_GET_PI_STEP:
                    UInt16 StepByStepCurrentValue = BitConverter.ToUInt16(e.DataSource,0);//当前步进增长数值
                    UInt16 StepByStepTargetValue = BitConverter.ToUInt16(e.DataSource, 2);//目标步进增长数值
                    TargetValue = StepByStepTargetValue;
                    CurrentValue = StepByStepCurrentValue;
                    break;
            }
        }
        private async void WaitPIPrepareToComplete()
        {
            await Task.Run(() =>
            {
                while (true) 
                {
                    if (SetHeatCur <= FilaCur && FilaCur <3.5)
                    {
                        SendMsg(NewHvControllerProtocol.SetEVCmd(SetEmissionVol));
                        SetProgressInfo("加热电流设置完成中", 100);
                        return;
                    }
                    else { Thread.Sleep(500); }
                }
            });
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
