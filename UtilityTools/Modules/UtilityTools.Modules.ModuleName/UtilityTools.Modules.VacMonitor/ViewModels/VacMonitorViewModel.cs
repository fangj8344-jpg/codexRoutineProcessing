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

using CsvHelper;
using ImageMagick;
using Microsoft.Win32;
using NLog;
using OpenCvSharp;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using TouchSocket.Core;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.VacMonitor.Model;
using UtilityTools.Modules.VacMonitor.Protocol;
using UtilityTools.Services;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using Microsoft.Win32;
using static UtilityTools.Modules.VacMonitor.Protocol.VacMonitorProtocol;

namespace UtilityTools.Modules.VacMonitor.ViewModels
{
    public class VacMonitorViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public VacMonitorViewModel(IDialogHostService dialogHostService, Prism.Ioc.IContainerProvider containerProvider) 
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

            if (SericalService != null)
            {
                SericalService.Close();
            }
        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly Prism.Ioc.IContainerProvider _containerProvider;
        private System.Timers.Timer _timer;

        LineSeries _vacuum1;
        LineSeries _vacuum2;
        LineSeries _vacuum3;
        LineSeries _vacuum4;

        private VacMonitorProtocolParser _parser1;

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
        public IAsynRWService _sericalService;
        /// <summary>
        /// 串口通信
        /// </summary>
        public IAsynRWService SericalService
        { 
            get { return _sericalService; }
            set{ _sericalService = value;RaisePropertyChanged(); }
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
        private ObservableCollection<VacMessageModel> _vac1MessageModels;
        public ObservableCollection<VacMessageModel> Vac1MessageModels
        {
            get { return _vac1MessageModels; }
            set { _vac1MessageModels = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<VacMessageModel> _vac2MessageModels;
        public ObservableCollection<VacMessageModel> Vac2MessageModels
        {
            get { return _vac2MessageModels; }
            set { _vac2MessageModels = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<VacMessageModel> _vac3MessageModels;
        public ObservableCollection<VacMessageModel> Vac3MessageModels
        {
            get { return _vac3MessageModels; }
            set { _vac3MessageModels = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<VacMessageModel> _vac4MessageModels;
        public ObservableCollection<VacMessageModel> Vac4MessageModels
        {
            get { return _vac4MessageModels; }
            set { _vac4MessageModels = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }
        public DelegateCommand ShowNetDeviceCommand { get; set; }
        public DelegateCommand ChangeMonitorStateCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand AutoAdjustComamnd { get; set; }
        public DelegateCommand SaveToFileCommand { get; set; }
        public DelegateCommand VacImportFromCsvCommand { get; set; }
        public DelegateCommand VacExportToCsvCommand { get; set; }
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
            VacImportFromCsvCommand = new DelegateCommand(VacImportFromCsv);
            VacExportToCsvCommand = new DelegateCommand(VacExportToCsv);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            SericalService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPVM");
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
            Vac1MessageModels = new ObservableCollection<VacMessageModel>();
            Vac2MessageModels = new ObservableCollection<VacMessageModel>();
            Vac3MessageModels = new ObservableCollection<VacMessageModel>();
            Vac4MessageModels = new ObservableCollection<VacMessageModel>();
            _parser1 = new VacMonitorProtocolParser();
            _parser1.Service = SericalService;
            _parser1.Service = NetUdpService;
            SericalService.UpdateResponse += Service_VacuumMonitoringResponse;
            NetUdpService.UpdateResponse += NetUdpService_VacuumMonitoringResponse;
            _parser1.PacketReceivedEvent += Parser_VacuumMonitoringResponse;

            // 初始化图表信息
            VacuumPlotModel = new PlotModel() { Title="真空曲线"};
            VacuumPlotModel.Legends.Add(new Legend());

            VacuumPlotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = OxyPlot.Axes.AxisPosition.Bottom });
            VacuumPlotModel.Axes.Add(new LogarithmicAxis() { Title = "真空值", Position = OxyPlot.Axes.AxisPosition.Left });

            _vacuum1 = new LineSeries() { Title = "Vac1", RenderInLegend = true,ItemsSource = Vac1MessageModels, DataFieldX= "DateTime", DataFieldY= "VacValue" };
            _vacuum2 = new LineSeries() { Title = "Vac2", RenderInLegend = true,ItemsSource = Vac2MessageModels, DataFieldX = "DateTime", DataFieldY = "VacValue" };
            _vacuum3 = new LineSeries() { Title = "Vac3", RenderInLegend = true,ItemsSource = Vac3MessageModels, DataFieldX = "DateTime", DataFieldY = "VacValue" };
            _vacuum4 = new LineSeries() { Title = "Vac4", RenderInLegend = true,ItemsSource = Vac4MessageModels, DataFieldX = "DateTime", DataFieldY = "VacValue" };
            VacuumPlotModel.Series.Add(_vacuum1);
            VacuumPlotModel.Series.Add(_vacuum2);
            VacuumPlotModel.Series.Add(_vacuum3);
            VacuumPlotModel.Series.Add(_vacuum4);

            LogSource = new ObservableCollection<VacuumLogModel>();
        }

        private void Service_VacuumMonitoringResponse(object sender, byte[] e)
        {
            if (NewAgreement == false)
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
                            VacMessageModel vacMessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vac };
                            switch (index)
                            {
                                case 1:
                                    {
                                        Vac1MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                                case 2:
                                    {
                                        Vac2MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                                case 3:
                                    {
                                        Vac3MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                            }

                        }
                        VacuumPlotModel.InvalidatePlot(true);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{dev.PortName} ReadLine Error: {ex.Message}");
                    }
                }
                else
                {
                    _parser1.ReceiveBytes(e);
                }
           
        }



        /// <summary>
        /// 串口显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", SericalService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.VacMonitorRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if(value != null) 
                {
                    SericalService = value;
                    IsConnected = SericalService.IsOpen;
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
            Vac1MessageModels.Clear();
            Vac2MessageModels.Clear();
            Vac3MessageModels.Clear();
            Vac4MessageModels.Clear();
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
                if (SericalService.IsOpen)
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
                if (SericalService.IsOpen)
                {
                    SendMsg(VacMonitorProtocol.GetAllVacuumValueNew());
    
                }
                if (NetUdpService.IsOpen)
                {
                    NetUdpSendMsg(VacMonitorProtocol.GetAllVacuumValueNew());
   
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
                            VacMessageModel vacMessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vac };
                            switch (index)
                            {
                                case 1:
                                    {
                                        Vac1MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                                case 2:
                                    {
                                        Vac2MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                                case 3:
                                    {
                                        Vac3MessageModels.Add(vacMessageModel);
                                        break;
                                    }
                                case 4:
                                    {
                                        Vac4MessageModels.Add(vacMessageModel);
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
                byte channel = e.DataSource[3];
                byte floatNumber = e.DataSource[2];
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
                        LogSource.RemoveAt(0);
                    }
                }));
                float vacFloat1 = BitConverter.ToSingle(e.DataSource.Skip(4).Take(4).ToArray());
                float vacFloat2 = BitConverter.ToSingle(e.DataSource.Skip(8).Take(4).ToArray());
                float vacFloat3 = BitConverter.ToSingle(e.DataSource.Skip(12).Take(4).ToArray());
                float vacFloat4 = BitConverter.ToSingle(e.DataSource.Skip(16).Take(4).ToArray());
                VacMessageModel vac1MessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vacFloat1 };
                VacMessageModel vac2MessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vacFloat2 };
                VacMessageModel vac3MessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vacFloat3 };
                VacMessageModel vac4MessageModel = new VacMessageModel() { DateTime = DateTime.Now, VacValue = vacFloat4 };


                switch (channel)
                {
                    case 1:
                        Vac1MessageModels.Add(vac1MessageModel);
                        break;
                    case 2:
                        Vac2MessageModels.Add(vac2MessageModel);
                        break;
                    case 3:
                        Vac3MessageModels.Add(vac3MessageModel);
                        break;
                    case 4:
                        Vac4MessageModels.Add(vac4MessageModel);
                        break;
                    case 0:
                        
                        Vac1MessageModels.Add(vac1MessageModel);
                        Vac2MessageModels.Add(vac2MessageModel);
                        Vac3MessageModels.Add(vac3MessageModel);
                        Vac4MessageModels.Add(vac4MessageModel);
                        break;
                        
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

            SericalService.SendMsg(msg);
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
            NLog.LogManager.GetCurrentClassLogger().Debug($"msg.ToString():{msg.ToString()}");
            NLog.LogManager.GetCurrentClassLogger().Debug($"BitConverter.ToString(msg): {BitConverter.ToString(msg)}");
           
            NetUdpService.SendMsg(msg);
        }
        
        public void VacExportToCsv()
        {
            var filePath = SelectFolder();
            if (filePath == null)
            {
                return; 
            }
            
            if (Vac1MessageModels?.Count>0 )
            {
                for (int i = 0; i < 100; i++)
                {
                    var path = Path.Combine(filePath, $"Vac1MessageModels_{i}.csv");
                    if (!File.Exists(path))
                    {
                        ExportToCsv(Vac1MessageModels, path);
                        break;
                    }
                }
               
            }
            if (Vac2MessageModels?.Count > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    var path = Path.Combine(filePath, $"Vac2MessageModels_{i}.csv");
                    if (!File.Exists(path))
                    {
                        ExportToCsv(Vac2MessageModels, path);
                        break;
                    }
                }
               
            }
            if (Vac3MessageModels?.Count > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    var path = Path.Combine(filePath, $"Vac3MessageModels_{i}.csv");
                    if (!File.Exists(path))
                    {
                        ExportToCsv(Vac3MessageModels, path);
                        break;
                    }
                }          
            }
            if (Vac4MessageModels?.Count > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    var path = Path.Combine(filePath, $"Vac4MessageModels_{i}.csv");
                    if (!File.Exists(path))
                    {
                        ExportToCsv(Vac4MessageModels, path);
                        break;
                    }
                }
            }
        } 
      
        public void VacImportFromCsv()
        {
            var filePath = SelectFolder();
            if (filePath == null)
            {
                return;
            }
            var files1 = Directory.GetFiles(filePath, "Vac1MessageModels_*.csv");
            if (files1?.Length>0)
            {
                var file =  files1.OrderBy(f=>f).First();
                Vac1MessageModels = ImportFromCsv(file);
                _vacuum1.ItemsSource = Vac1MessageModels;
                _vacuum1.DataFieldX = "DateTime";
                _vacuum1.DataFieldY = "VacValue";
                VacuumPlotModel.InvalidatePlot(true);
            }
            var files2 = Directory.GetFiles(filePath, "Vac2MessageModels_*.csv");
            if (files2?.Length > 0)
            {
                var file = files2.OrderBy(f => f).First();
                Vac2MessageModels = ImportFromCsv(file);
                _vacuum2.ItemsSource = Vac2MessageModels;
                _vacuum2.DataFieldX = "DateTime";
                _vacuum2.DataFieldY = "VacValue";
                VacuumPlotModel.InvalidatePlot(true);
            }
            var files3 = Directory.GetFiles(filePath, "Vac3MessageModels_*.csv");
            if (files3?.Length > 0)
            {
                var file = files3.OrderBy(f => f).First();
                Vac3MessageModels = ImportFromCsv(file);
                _vacuum3.ItemsSource = Vac3MessageModels;
                _vacuum3.DataFieldX = "DateTime";
                _vacuum3.DataFieldY = "VacValue";
                VacuumPlotModel.InvalidatePlot(true);
            }
            var files4 = Directory.GetFiles(filePath, "Vac4MessageModels_*.csv");
            if (files4?.Length > 0)
            {
                var file = files4.OrderBy(f => f).First();
                Vac4MessageModels = ImportFromCsv(file);
                _vacuum4.ItemsSource = Vac4MessageModels;
                _vacuum4.DataFieldX = "DateTime";
                _vacuum4.DataFieldY = "VacValue";
                VacuumPlotModel.InvalidatePlot(true);
            }
        }
        public void ExportToCsv(ObservableCollection<VacMessageModel> data,string filePath)
        {
           
            if (filePath == null)
            {
                return;
            }
            // 使用 StreamWriter 写入文件，并指定 UTF-8 编码以支持中文等特殊字符
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 写入表头
                writer.WriteLine("时间,真空值");

                // 遍历数据并写入每一行
                foreach (var item in data)
                {
                    // 为了避免值本身包含逗号导致格式错乱，可以用引号将每个字段括起来
                    // 时间格式化为标准的 ISO 格式，便于解析
                    string timeStr = item.DateTime.ToString("o"); // "o" 格式会生成如 "2023-10-27T15:30:45.1234567" 的字符串
                    string valueStr = item.VacValue.ToString(System.Globalization.CultureInfo.InvariantCulture); // 使用不变文化，避免逗号和点号的问题

                    writer.WriteLine($"{timeStr},{valueStr}");
                }
            }
            System.Windows.MessageBox.Show($"数据已成功导出到 {filePath}");
        }
        public ObservableCollection<VacMessageModel> ImportFromCsv(string filePath)
        {
            
            var data = new ObservableCollection<VacMessageModel>();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"文件 {filePath} 不存在。");
                return data;
            }

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                // 跳过第一行表头
                string line = reader.ReadLine();

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 分割行数据
                    string[] parts = line.Split(',');

                    // 简单验证数据格式
                    if (parts.Length >= 2)
                    {
                        var model = new VacMessageModel();
                        // 解析时间，使用 TryParse 增加健壮性
                        if (DateTime.TryParse(parts[0], out DateTime time))
                        {
                            model.DateTime = time;
                        }
                        else
                        {
                            Console.WriteLine($"警告：无法解析时间 '{parts[0]}'，该行将被忽略。");
                            continue;
                        }

                        // 解析值
                        if (double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                        {
                            model.VacValue = value;
                        }
                        else
                        {
                            Console.WriteLine($"警告：无法解析值 '{parts[1]}'，该行将被忽略。");
                            continue;
                        }

                        data.Add(model);
                    }
                }
            }
            Console.WriteLine($"数据已成功从 {filePath} 导入。");
            return data;
        }
        public  string SelectFolder()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "选择文件夹";

            if (dialog.ShowDialog() == true)
            {
                return System.IO.Path.GetDirectoryName(dialog.FileName);
            }
            return null;
        }
        public string SelectCsvFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // 设置文件过滤器，只显示 .csv 文件
            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
            openFileDialog.FilterIndex = 1; // 默认选择第一个过滤器
            openFileDialog.Title = "选择 CSV 文件";

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
