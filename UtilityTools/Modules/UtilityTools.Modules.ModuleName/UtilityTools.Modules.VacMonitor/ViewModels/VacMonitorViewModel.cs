#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.VacMonitor.ViewModels
 * 唯一标识：63c91e67-bf43-40d1-86e6-61f002099792
 * 文件名：VacMonitorViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:22:37
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

using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Core.Model;
using UtilityTools.Modules.VacMonitor.Model;
using System.IO.Ports;
using UtilityTools.Modules.VacMonitor.Protocol;
using System.Windows;
using NLog;
using UtilityTools.Services.Interfaces.IServices;
using System.Windows.Interop;
using UtilityTools.Services.Interfaces;
using System.Threading;
using Microsoft.Win32;
using OxyPlot.Wpf;
using System.Windows.Forms;
using System.IO;
using OxyPlot.Axes;
using static UtilityTools.Modules.VacMonitor.Protocol.VacMonitorProtocol;
using CsvHelper;
using ImageMagick;
using OpenCvSharp;
using UtilityTools.Services;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.VacMonitor.ViewModels
{
    public class VacMonitorViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public VacMonitorViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider) 
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;
            InitCommand();
            InitProperty();
           
        }

       

        ~VacMonitorViewModel()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            if (Service != null)
            {
                Service.Close();
            }
        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private System.Timers.Timer _timer;

        LineSeries _vacuum1;
        LineSeries _vacuum2;
        LineSeries _vacuum3;
        LineSeries _vacuum4;

        private VacMonitorProtocolParser _parser1;
        private VacMonitorProtocolParser _parser2; 
        #endregion

        #region ------------Property------------
        private bool _isConnected;
        /// <summary>
        /// 是否已经连接设备
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }
        private bool _newAgreement = true;
        /// <summary>
        /// 是否是新协议
        /// </summary>
        public bool NewAgreement
        {
            get { return _newAgreement; }
            set { _newAgreement = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 下位机服务端串接口
        /// </summary>
        public IAsynRWService Service { get; set; }
        private IAsynRWService _netUdpService;
        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }
        private bool _netIsConnected;
        public bool NetIsConnected
        {
            get { return _netIsConnected; }
            set { _netIsConnected = value;RaisePropertyChanged(); } 
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

        private PlotModel _vacuumPlotModel;
        /// <summary>
        /// 真空图表模型
        /// </summary>
        public PlotModel VacuumPlotModel
        {
            get { return _vacuumPlotModel; }
            set { _vacuumPlotModel = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<VacuumLogModel> _logSource;
        /// <summary>
        /// 日志信息列表
        /// </summary>
        public ObservableCollection<VacuumLogModel> LogSource
        {
            get { return _logSource; }
            set { _logSource = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }
        public DelegateCommand ShowNetDeviceCommand { get; set; }
        public DelegateCommand ChangeMonitorStateCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand AutoAdjustComamnd { get; set; }
        public DelegateCommand SaveToFileCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            ShowNetDeviceCommand = new DelegateCommand(ShowNetDevice);
            ChangeMonitorStateCommand = new DelegateCommand(ChangeMonitorState); 
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            AutoAdjustComamnd = new DelegateCommand(AutoAdjust);
            SaveToFileCommand = new DelegateCommand(Save);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            Service = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPVM");
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "真空检测";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5000;
            netUdp.DeviceInstance.HostIp = "192.168.1.33";
            netUdp.DeviceInstance.HostPort = 5001;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            MonitorState = "开始监控";
            MonitorInterval = 1000;

            _parser1 = new VacMonitorProtocolParser();
            _parser2 = new VacMonitorProtocolParser();
            _parser1.Service = Service;
            _parser1.Service = NetUdpService;
            Service.UpdateResponse += Service_VacuumMonitoringResponse;
            NetUdpService.UpdateResponse += NetUdpService_VacuumMonitoringResponse;
            _parser1.PacketReceivedEvent += Parser_VacuumMonitoringResponse;
            _parser2.PacketReceivedEvent += Parser_VacuumMonitoringResponse;

            // 初始化图表信息
            VacuumPlotModel = new PlotModel();
            VacuumPlotModel.Legends.Add(new Legend());

            VacuumPlotModel.Axes.Add(new LinearAxis() { Title = "时间", Position = OxyPlot.Axes.AxisPosition.Bottom });
            VacuumPlotModel.Axes.Add(new LogarithmicAxis() { Title = "真空值", Position = OxyPlot.Axes.AxisPosition.Left });

            _vacuum1 = new LineSeries() { Title = "Vac1", RenderInLegend = true };
            _vacuum2 = new LineSeries() { Title = "Vac2", RenderInLegend = true };
            _vacuum3 = new LineSeries() { Title = "Vac3", RenderInLegend = true };
            _vacuum4 = new LineSeries() { Title = "Vac4", RenderInLegend = true };
            VacuumPlotModel.Series.Add(_vacuum1);
            VacuumPlotModel.Series.Add(_vacuum2);
            VacuumPlotModel.Series.Add(_vacuum3);
            VacuumPlotModel.Series.Add(_vacuum4);

            LogSource = new ObservableCollection<VacuumLogModel>();
        }

       

        /// <summary>
        /// 串口显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.VacMonitorRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if(value != null) 
                {
                    Service = value;
                    IsConnected = Service.IsOpen;
                }
            }
        }
        /// <summary>
        /// 网口显示设备连接弹窗
        /// </summary>
        private async void ShowNetDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", NetUdpService);//传递参数用来读写
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.VacMonitorRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    NetUdpService = value;
                    NetIsConnected = NetUdpService.IsOpen;
                }
            }
        }

        /// <summary>
        /// 修改监控状态
        /// </summary>
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

        /// <summary>
        /// 清除监控数据
        /// </summary>
        private void ClearMonitor()
        {
            _vacuum1.Points.Clear();
            _vacuum2.Points.Clear();
            _vacuum3.Points.Clear();
            _vacuum4.Points.Clear();
            VacuumPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 自适应曲线
        /// </summary>
        private void AutoAdjust()
        {
            foreach (var axis in VacuumPlotModel.Axes)
                axis.Reset();
            VacuumPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="o"></param>
        private void Save()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                var path = dialog.SelectedPath;

                SaveToFile(path);
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
                exporter.ExportToFile(VacuumPlotModel, $"{path}\\真空记录_{timeTip}.png");
                SaveSeriesToFile(_vacuum1, $"{path}\\真空值1_{timeTip}.txt");
                SaveSeriesToFile(_vacuum2, $"{path}\\真空值2_{timeTip}.txt");
                SaveSeriesToFile(_vacuum3, $"{path}\\真空值3_{timeTip}.txt");
                SaveSeriesToFile(_vacuum4, $"{path}\\真空值4_{timeTip}.txt");
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

        /// <summary>
        /// 定时器超时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (NewAgreement == false)
            {
                if (Service.IsOpen)
                {
                    // Work
                    SendMsg(VacMonitorProtocol.GetVacuumValue(1));
                    Thread.Sleep(MonitorInterval / 3);
                    SendMsg(VacMonitorProtocol.GetVacuumValue(2));
                    Thread.Sleep(MonitorInterval / 3);
                    SendMsg(VacMonitorProtocol.GetVacuumValue(3));
                }
                if (NetUdpService.IsOpen)
                {
                    // Work
                    NetUdpSendMsg(VacMonitorProtocol.GetVacuumValue(1));
                    Thread.Sleep(MonitorInterval / 3);
                    NetUdpSendMsg(VacMonitorProtocol.GetVacuumValue(2));
                    Thread.Sleep(MonitorInterval / 3);
                    NetUdpSendMsg(VacMonitorProtocol.GetVacuumValue(3));
                }
                
            }
            else
            {
                if (Service.IsOpen)
                {
                    SendMsg(VacMonitorProtocol.GetVacuumValueNew(0));
                    Thread.Sleep(MonitorInterval);
                }
                if (NetUdpService.IsOpen)
                {
                    NetUdpSendMsg(VacMonitorProtocol.GetVacuumValueNew(0));
                    Thread.Sleep(MonitorInterval);
                }
               
            }
           
        }

        /// <summary>
        /// 串口通讯数据回报接收函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort dev)
            {
                // 解析回包数据
                try
                {
                    var msg = dev.ReadLine();

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        LogSource.Insert(0, new VacuumLogModel()
                        {
                            Time = DateTime.Now,
                            Message = msg,
                            Direct = "R"
                        });

                        int length = LogSource.Count;
                        if (length > 1000)
                        {
                            LogSource.RemoveAt(length - 1);
                        }
                    }));

                    if (VacMonitorProtocol.TryParseResponse(msg, out int index, out double vac))
                    {
                        switch (index)
                        {
                            case 1:
                                {
                                    _vacuum1.Points.Add(new DataPoint(_vacuum1.Points.Count(), vac));
                                    break;
                                }
                            case 2:
                                {
                                    _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vac));
                                    break;
                                }
                            case 3:
                                {
                                    _vacuum3.Points.Add(new DataPoint(_vacuum3.Points.Count(), vac));
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error($"{dev.PortName} ReadLine Error: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 网口回报事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetUdpService_VacuumMonitoringResponse(object sender, byte[] e)
        {
            if (NewAgreement == false)
            {
                var sourceMsg = Encoding.Default.GetString(e);
                var list = sourceMsg.Split((char)0x0D);

                foreach (var msg in list)
                {
                    if (msg.Length != 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            LogSource.Insert(0, new VacuumLogModel()
                            {
                                Time = DateTime.Now,
                                Message = msg,
                                Direct = "R"
                            });

                            int length = LogSource.Count;
                            if (length > 1000)
                            {
                                LogSource.RemoveAt(length - 1);
                            }
                        }));

                        if (VacMonitorProtocol.TryParseResponse(msg, out int index, out double vac))
                        {
                            switch (index)
                            {
                                case 1:
                                    {
                                        _vacuum1.Points.Add(new DataPoint(_vacuum1.Points.Count(), vac));
                                        break;
                                    }
                                case 2:
                                    {
                                        _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vac));
                                        break;
                                    }
                                case 3:
                                    {
                                        _vacuum3.Points.Add(new DataPoint(_vacuum3.Points.Count(), vac));
                                        break;
                                    }
                            }
                            VacuumPlotModel.InvalidatePlot(true);
                        }
                    }
                }
            }
            else
            {
                _parser2.ReceiveBytes(e);
            }
        }


        /// <summary>
        /// 回报事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Service_VacuumMonitoringResponse(object sender, byte[] e)
        {
            if (NewAgreement == false)
            {
                var sourceMsg = Encoding.Default.GetString(e);
                var list = sourceMsg.Split((char)0x0D);

                foreach (var msg in list)
                {
                    if (msg.Length != 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            LogSource.Insert(0, new VacuumLogModel()
                            {
                                Time = DateTime.Now,
                                Message = msg,
                                Direct = "R"
                            });

                            int length = LogSource.Count;
                            if (length > 1000)
                            {
                                LogSource.RemoveAt(length - 1);
                            }
                        }));

                        if (VacMonitorProtocol.TryParseResponse(msg, out int index, out double vac))
                        {
                            switch (index)
                            {
                                case 1:
                                    {
                                        _vacuum1.Points.Add(new DataPoint(_vacuum1.Points.Count(), vac));
                                        break;
                                    }
                                case 2:
                                    {
                                        _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vac));
                                        break;
                                    }
                                case 3:
                                    {
                                        _vacuum3.Points.Add(new DataPoint(_vacuum3.Points.Count(), vac));
                                        break;
                                    }
                            }
                            VacuumPlotModel.InvalidatePlot(true);
                        }
                    }
                }
            }
            else
            {
                _parser1.ReceiveBytes(e);
            }
        }
        /// <summary>
        /// 新协议回报事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Parser_VacuumMonitoringResponse(object sender, VacDataPacket e)
        {
            var parser = sender as VacMonitorProtocolParser;
            IAsynRWService? service = null;
           if (parser != null) 
            {
                service = parser.Service;
            }
            //0x0800是读取真空规数字的命令码
            if (e.cmd == 0x0800)
            {
                ushort channel = BitConverter.ToUInt16(e.DataSource,0);
                ushort floatNumber = BitConverter.ToUInt16(e.DataSource,2);
                
                var msg = BitConverter.ToString (e.DataSource);
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LogSource.Insert(0, new VacuumLogModel()
                    {
                        Time = DateTime.Now,
                        Message = msg,
                        Direct = "R"
                    });

                    int length = LogSource.Count;
                    if (length > 1000)
                    {
                        LogSource.RemoveAt(length - 1);
                    }
                }));
            
                float vacFloat1 = BitConverter.ToSingle(e.DataSource.Skip(4).Take(4).ToArray());
                float vacFloat2 = BitConverter.ToSingle(e.DataSource.Skip(8).Take(4).ToArray());
                float vacFloat3 = BitConverter.ToSingle(e.DataSource.Skip(12).Take(4).ToArray());
                float vacFloat4 = BitConverter.ToSingle(e.DataSource.Skip(16).Take(4).ToArray());
                switch (channel)
                {
                    case 1:
                        _vacuum1.Points.Add(new DataPoint(_vacuum1.Points.Count(), vacFloat1));
                        break;
                    case 2:
                        _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat1));
                        break;
                    case 3:
                        _vacuum1.Points.Add(new DataPoint(_vacuum3.Points.Count(), vacFloat1));
                        break;

                    case 4:
                        _vacuum1.Points.Add(new DataPoint(_vacuum4.Points.Count(), vacFloat1));
                        break;
                    case 0:
                        {
                            switch (floatNumber)
                            {
                                case 4:
                                    _vacuum4.Points.Add(new DataPoint(_vacuum4.Points.Count(), vacFloat4));
                                    _vacuum3.Points.Add(new DataPoint(_vacuum3.Points.Count(), vacFloat3));
                                    _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat2));
                                    _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat1));
                                    break;
                                case 3: 
                                    _vacuum3.Points.Add(new DataPoint(_vacuum3.Points.Count(), vacFloat3));
                                    _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat2));
                                    _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat1));
                                    break;
                                case 2:
                                    _vacuum2.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat2));
                                    _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat1)); 
                                    break;
                                case 1: _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vacFloat1));
                                    break;
                            }

                            break;
                        }
                }
                VacuumPlotModel.InvalidatePlot(true);

            }
        }
        /// <summary>
        /// 串口通讯异常数据回报接收函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (sender is SerialPort dev)
            {
                LogManager.GetCurrentClassLogger().Error($"{dev.PortName} Error: {e.ToString()}");
                IsConnected = dev.IsOpen;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息体</param>
        private void SendMsg(byte[] msg)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if ((NewAgreement == true))
                {
                    if (NewAgreement == true)
                    {
                        LogSource.Insert(0, new VacuumLogModel()
                        {
                            Time = DateTime.Now,
                            Message = BitConverter.ToString(msg.Skip(18).Take(4).ToArray()),
                            Direct = "W"
                        });
                    }
                }
                else
                {
                    LogSource.Insert(0, new VacuumLogModel()
                    {
                        Time = DateTime.Now,
                        Message = Encoding.Default.GetString(msg),
                        Direct = "W"
                    });

                }

                int length = LogSource.Count;
                if (length > 1000)
                {
                    LogSource.RemoveAt(length - 1);
                }
            }));

            Service.SendMsg(msg);
        }
        private void NetUdpSendMsg(byte[] msg)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (NewAgreement == true)
                {
                    LogSource.Insert(0, new VacuumLogModel()
                    {
                        Time = DateTime.Now,
                        Message = BitConverter.ToString(msg.Skip(18).Take(4).ToArray()),
                        Direct = "W"
                    });

                }
                else
                {
                    LogSource.Insert(0, new VacuumLogModel()
                    {
                        Time = DateTime.Now,
                        Message = Encoding.Default.GetString(msg),
                        Direct = "W"
                    });
                }
                int length = LogSource.Count;
                if (length > 1000)
                {
                    LogSource.RemoveAt(length - 1);
                }
            }));

            NetUdpService.SendMsg(msg);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
