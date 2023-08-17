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

            Service.UpdateResponse += Service_UpdateResponse;
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
        private Timer _timer;

        LineSeries _vacuum1;
        LineSeries _vacuum2;
        LineSeries _vacuum3;
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

        /// <summary>
        /// 下位机服务端接口
        /// </summary>
        public IAsynRWService Service { get; set; }

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
        public DelegateCommand ChangeMonitorStateCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
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
            ChangeMonitorStateCommand = new DelegateCommand(ChangeMonitorState); 
            ClearMonitorCommand = new DelegateCommand(ClearMonitor); 
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            Service = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPVM");
            MonitorState = "开始监控";
            MonitorInterval = 1000;

            // 初始化图表信息
            VacuumPlotModel = new PlotModel();
            VacuumPlotModel.Legends.Add(new Legend());
            _vacuum1 = new LineSeries() { Title = "Vac1", RenderInLegend = true };
            _vacuum2 = new LineSeries() { Title = "Vac2", RenderInLegend = true };
            _vacuum3 = new LineSeries() { Title = "Vac3", RenderInLegend = true };
            VacuumPlotModel.Series.Add(_vacuum1);
            VacuumPlotModel.Series.Add(_vacuum2);
            VacuumPlotModel.Series.Add(_vacuum3);

            LogSource = new ObservableCollection<VacuumLogModel>();
        }

        /// <summary>
        /// 显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service.GetHandle());
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.VacMonitorRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                Service.SetHandle(diaglogResult.Parameters.GetValue<object>("Value"));
                IsConnected = Service.IsOpen;
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
                    _timer = new Timer();
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
            VacuumPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 定时器超时处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Work
            SendMsg(VacMonitorProtocol.GetVacuumValue(1));
            SendMsg(VacMonitorProtocol.GetVacuumValue(2));
            SendMsg(VacMonitorProtocol.GetVacuumValue(3));
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

                    Application.Current.Dispatcher.Invoke(new Action(() =>
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
                                    _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vac));
                                    break;
                                }
                            case 3:
                                {
                                    _vacuum1.Points.Add(new DataPoint(_vacuum3.Points.Count(), vac));
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Service_UpdateResponse(object sender, byte[] e)
        {
            var msg = Encoding.Default.GetString(e);

            Application.Current.Dispatcher.Invoke(new Action(() =>
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
                            _vacuum1.Points.Add(new DataPoint(_vacuum2.Points.Count(), vac));
                            break;
                        }
                    case 3:
                        {
                            _vacuum1.Points.Add(new DataPoint(_vacuum3.Points.Count(), vac));
                            break;
                        }
                }
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
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                LogSource.Insert(0, new VacuumLogModel()
                {
                    Time = DateTime.Now,
                    Message = Encoding.Default.GetString(msg),
                    Direct = "W"
                });

                int length = LogSource.Count;
                if (length > 1000)
                {
                    LogSource.RemoveAt(length - 1);
                }
            }));

            Service.SendMsg(msg);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
