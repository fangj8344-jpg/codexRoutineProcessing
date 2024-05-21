#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.Motor5Controller.ViewModels
 * 唯一标识：fc6911fc-8414-4ad4-b571-1e5140f1645b
 * 文件名：Motor5ControllerViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/14 16:16:38
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

using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.Motor5Controller.Model;
using System.Windows.Media.Media3D;
using UtilityTools.Core.Mvvm;
using UtilityTools.Services.Interfaces;
using System.Security.Policy;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Core.Dialog;
using System.IO.Ports;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Documents;
using Prism.Events;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Model;
using UtilityTools.Core.Helper;
using System.Collections;
using System.Windows.Markup;
using NLog.Fluent;
using System.Xml.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UtilityTools.Modules.Motor5Controller.Protocol;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using OpenCvSharp.Flann;
using System.Windows.Forms;
using OxyPlot.Wpf;
using System.IO;

namespace UtilityTools.Modules.Motor5Controller.ViewModels
{
    internal class Motor5ControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public Motor5ControllerViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;
            MotorList = CretetModels();
            InitCommand();
            InitProperty();
            CacheCallBackDate = new List<byte>();
            //消息提示
            aggregator = containerProvider.Resolve<IEventAggregator>();
            //下位机数据回调
            Service.UpdateResponse += Service_UpdateResponse;
            Logs = new ObservableCollection<string>();

            _parser = new Motor5ProtocolParser();
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;
        }

        #endregion

        #region ------------Field------------
        private readonly TaskScheduler _syncContextTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private readonly IAsynRWService _device;
        private List<byte> _cachecallbackdate;
        private bool _isConnected;
        private ObservableCollection<MotorModel> _motorlist;
        private SerialPort comm = new SerialPort();
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator aggregator;
        private ObservableCollection<string> _logs;
        private Motor5ProtocolParser _parser;

        private byte[] _testBuff = new byte[256]; // 测试指令缓存区
        private int _testLength = 0;
        private bool _testFlag = false; // 是否已经收到测试指令头
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 电机列表
        /// </summary>
        public ObservableCollection<MotorModel> MotorList
        {
            get { return _motorlist; }
            set { _motorlist = value; RaisePropertyChanged(); }
        }

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


        /// <summary>
        /// 指定回调信息缓存
        /// </summary>
        public List<byte> CacheCallBackDate
        {
            get { return _cachecallbackdate; }
            set { _cachecallbackdate = value; RaisePropertyChanged(); }
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// 通讯日志
        /// </summary>
        public ObservableCollection<string> Logs
        {
            get { return _logs; }
            set { _logs = value; RaisePropertyChanged(); }
        }

        private PlotModel _monitorPlotModel;
        /// <summary>
        /// 监控图表模型
        /// </summary>
        public PlotModel MonitorPlotModel
        {
            get { return _monitorPlotModel; }
            set { _monitorPlotModel = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand CleanLogCommand { get; set; }
        public DelegateCommand CopyLogCommand { get; set; }
        public DelegateCommand ShowDeviceCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand AutoAdjustComamnd { get; set; }
        public DelegateCommand SaveToFileCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------

        public bool SendMsg(byte[] msg)
        {
            if (msg == null) return false;
            if (Service == null || !Service.IsOpen) return false;

            Service.SendMsg(msg);

            System.Threading.SynchronizationContext.Current.Post(p1 =>
            {
                Logs.Add($"{DateTime.Now.ToString("hh:mm:ss")} 发送: {DataTypeCaster.ByteArrayToString(msg, msg.Length)}");
            }, null);
            return true;
        }

        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化电机信息
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<MotorModel> CretetModels()
        {
            ObservableCollection<MotorModel> List = new ObservableCollection<MotorModel>();
            List.Add(new MotorModel(Protocol.EnumMotorId.MOTOR_1, SendMsg));
            List.Add(new MotorModel(Protocol.EnumMotorId.MOTOR_2, SendMsg));
            List.Add(new MotorModel(Protocol.EnumMotorId.MOTOR_3, SendMsg));
            List.Add(new MotorModel(Protocol.EnumMotorId.MOTOR_4, SendMsg));
            List.Add(new MotorModel(Protocol.EnumMotorId.MOTOR_5, SendMsg));
            return List;
        }

        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            CleanLogCommand = new DelegateCommand(CleanLog);
            CopyLogCommand = new DelegateCommand(CopyLog);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            AutoAdjustComamnd = new DelegateCommand(AutoAdjust);
            SaveToFileCommand = new DelegateCommand(SaveToFile);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            List<byte> CacheCallBackDate = new List<byte>();
            IsConnected = false;
            Service = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPMC");

            // 初始化图表信息
            MonitorPlotModel = new PlotModel();
            MonitorPlotModel.Legends.Add(new Legend());
        }

        /// <summary>
        /// 显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.Motor5ControllerRegionName);
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

        /// <summary>
        /// 清除报文日志信息
        /// </summary>
        private void CleanLog()
        {
            Logs = new ObservableCollection<string>();
        }

        private void CopyLog()
        {
            if (SelectedIndex >= 0 && SelectedIndex < Logs.Count)
            {
                var item = Logs[SelectedIndex];

                var list = item.Split(':');
                if (list.Length > 1)
                {
                    var cmd = list.Last();
                    cmd = cmd.Replace("0x", "");
                    System.Windows.Clipboard.SetText(cmd);
                }
            }
        }

        /// <summary>
        /// 清除监控数据
        /// </summary>
        private void ClearMonitor()
        {
            foreach (var series in MonitorPlotModel.Series)
            {
                var line = series as LineSeries;
                if (line != null)
                { 
                    line.Points.Clear();
                }
            }

            MonitorPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 自动调节监控数据
        /// </summary>
        private void AutoAdjust()
        {
            foreach (var axis in MonitorPlotModel.Axes)
                axis.Reset();
            MonitorPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 保存到数据
        /// </summary>
        private void SaveToFile()
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
                exporter.ExportToFile(MonitorPlotModel, $"{path}\\参数曲线_{timeTip}.png");
                int index = 0;
                foreach(var series in MonitorPlotModel.Series)
                {
                    var line = series as LineSeries;
                    if (line != null)
                    {
                        index++;
                        SaveSeriesToFile(line, $"{path}\\参数{index}_{timeTip}.txt");
                    }
                }
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
        /// 串口通信：接收回调信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Service_UpdateResponse(object sender, byte[] e)
        {
            if (!CheckMsgIsTest(e))
            {
                //报文处理（缓存处理）
                _parser.ReceiveBytes(e);
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                System.Threading.SynchronizationContext.SetSynchronizationContext(new
                System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                System.Threading.SynchronizationContext.Current.Post(p1 =>
                {
                    Logs.Add($"{DateTime.Now.ToString("hh:mm:ss")} 读取: {DataTypeCaster.ByteArrayToString(e, e.Length)}");
                }, null);
            });
        }


        /// <summary>
        /// 检测消息是否是测试消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool CheckMsgIsTest(byte[] e)
        {
            if (_testFlag)
            {
                e.CopyTo(_testBuff, _testLength);
                _testLength += e.Length;
            }
            else
            {
                if (e.Length < 2 || e[0] != '#' || e[1] != '#')
                {
                    return false;
                }

                _testFlag = true;
                e.CopyTo(_testBuff, 0);
                _testLength = e.Length;
            }

            if (_testFlag && _testLength > 4)
            {
                if ((_testBuff[_testLength - 1] == '&') && (_testBuff[_testLength - 2] == '&'))
                {
                    _testFlag = false;

                    var testMsg = new byte[_testLength];
                    Array.Copy(_testBuff, testMsg, _testLength);
                    DealWithTestMsg(testMsg);
                }
            }

            return true;
        }

        private void DealWithTestMsg(byte[] msg)
        {
            if (msg.Length < 4 || msg[0] != '#' || msg[1] != '#' || msg[msg.Length - 2] != '&' || msg[msg.Length - 1] != '&')
                return;

            var str = Encoding.ASCII.GetString(msg, 2, msg.Length - 4);

            var msgList = str.Split("&&##");
            foreach(var item in msgList) 
            {
                var paramList = item.Split(',');

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    for (int i = 1; i <= paramList.Length; i++)
                    {
                        if (MonitorPlotModel.Series.Count < i)
                        {
                            var series = new LineSeries() { Title = $"参数_{i}", RenderInLegend = true };
                            MonitorPlotModel.Series.Add(series);
                        }
                    }

                    for (int i = 0; i < paramList.Length; i++)
                    {
                        var p = paramList[i].Trim();
                        if (double.TryParse(p, out var val))
                        {
                            LineSeries line = MonitorPlotModel.Series[i] as LineSeries;
                            if (line != null)
                            {
                                line.Points.Add(new DataPoint(line.Points.Count(), val));
                            }
                        }
                    }

                    MonitorPlotModel.InvalidatePlot(true);
                }));
            }

        }

        /// <summary>
        /// 报文解析：操作提示，页面数据动态渲染
        /// </summary>
        /// <param name="e"></param>
        private void Parser_PacketReceivedEvent(object sender, Motor5DataPacket e)
        {
            var targetMotor = MotorList.First((item) => item.MotorId == e.MotorId);

            switch (e.CmdType)
            {
                case EnumMotor5CmdType.R_MOTOR_PID:
                    targetMotor.ReadParams.Pid.P = BitConverter.ToSingle(e.DataSource, 0);
                    targetMotor.ReadParams.Pid.I = BitConverter.ToSingle(e.DataSource, 4);
                    targetMotor.ReadParams.Pid.D = BitConverter.ToSingle(e.DataSource, 8);
                    break;
                case EnumMotor5CmdType.R_MOTOR_CTLPARAMS:
                    targetMotor.ReadParams.Times = BitConverter.ToInt32(e.DataSource, 0);
                    targetMotor.ReadParams.Threshold = BitConverter.ToInt32(e.DataSource, 4);
                    targetMotor.ReadParams.InchThreshold = BitConverter.ToInt32(e.DataSource, 8);
                    break;
                case EnumMotor5CmdType.R_MOTOR_SPDPARAMS:
                    targetMotor.ReadParams.StartSpeed = BitConverter.ToInt32(e.DataSource, 0);
                    targetMotor.ReadParams.MaxSpeed = BitConverter.ToInt32(e.DataSource, 4);
                    targetMotor.ReadParams.CurSpeed = BitConverter.ToInt32(e.DataSource, 8);
                    break;
                case EnumMotor5CmdType.R_MOTOR_ACCPARAMS:
                    targetMotor.ReadParams.MaxAcc = BitConverter.ToInt32(e.DataSource, 0);
                    targetMotor.ReadParams.MaxDec = BitConverter.ToInt32(e.DataSource, 4);
                    break;
                case EnumMotor5CmdType.R_MOTOR_TARPARAMS:
                    targetMotor.ReadParams.TargetValue = BitConverter.ToInt32(e.DataSource, 0);
                    targetMotor.ReadParams.CurPos = BitConverter.ToInt32(e.DataSource, 4);
                    break;
                case EnumMotor5CmdType.R_MOTOR_ZEROPOS:
                    targetMotor.ReadParams.OriginPos = BitConverter.ToInt32(e.DataSource, 0);
                    targetMotor.ReadParams.ZeroPos = BitConverter.ToInt32(e.DataSource, 4);
                    break;
                case EnumMotor5CmdType.R_MOTOR_STATE:
                    targetMotor.ReadParams.MotorType = (EnumMotorType)Enum.ToObject(typeof(EnumMotorType), e.DataSource[0]);
                    targetMotor.ReadParams.CtrType = (EnumMotorCtrType)Enum.ToObject(typeof(EnumMotorCtrType), e.DataSource[1]);
                    targetMotor.ReadParams.SubRatio = BitConverter.ToInt32(e.DataSource, 2);
                    targetMotor.ReadParams.RunMode = (EnumMotorRunMode)Enum.ToObject(typeof(EnumMotorRunMode), e.DataSource[6]);
                    targetMotor.ReadParams.MoveState = (EnumMotorMoveState)Enum.ToObject(typeof(EnumMotorMoveState), e.DataSource[7]);
                    targetMotor.ReadParams.Enable = (e.DataSource[8] == 0x00);
                    targetMotor.ReadParams.LimitedState = (EnumMotorLimitedState)Enum.ToObject(typeof(EnumMotorLimitedState), e.DataSource[9]);
                    targetMotor.ReadParams.ZeroState = (int)e.DataSource[10];
                    targetMotor.ReadParams.MoveDirection = (EmumMotorMoveDirection)Enum.ToObject(typeof(EmumMotorMoveDirection), e.DataSource[11]);
                    targetMotor.ReadParams.EncoderDirection = (EnumEncoderDirecton)Enum.ToObject(typeof(EnumEncoderDirecton), e.DataSource[12]);
                    break;
            }
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
