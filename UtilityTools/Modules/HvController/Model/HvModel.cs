#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：HvController.Model
 * 唯一标识：35082cf2-0af0-4d66-b835-b81c635202bc
 * 文件名：HvModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/22 17:44:08
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

using NLog;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Modules.HvController.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.HvController.Model
{
    public class HvModel : BindableBase
    {
        #region ------------Constructor------------
        public HvModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            SerialPortService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPHV");
            SerialPortService.UpdateResponse += Device_UpdateResponse;
            NetUdpService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("UNHV");
            NetUdpService.UpdateResponse += Device_UpdateResponse;

            if (NetUdpService.GetHandle() is NetConfigModel netConfig)
            {
                netConfig.HostIp = "192.168.1.33";
                netConfig.HostPort = 5005;
                netConfig.TargetIp = "192.168.1.88";
                netConfig.TargetPort = 5010;
            }

            InitProperty();
            InitCommand();

            _response = new byte[4096];
            _responseLength = 0;
        }

        #endregion

        #region ------------Field------------
        private readonly IContainerProvider _containerProvider;

        EventWaitHandle _operateHvWaitHandle = new AutoResetEvent(false);
        EventWaitHandle _setFilaParamWaitHandle = new AutoResetEvent(false);
        bool _operateHvResult = false;
        bool _setFilaParamResult = false;

        byte[] _response;
        int _responseLength;

        private System.Timers.Timer _timer;

        LineSeries _accVolLineSeries;
        LineSeries _emissLineSeries;
        LineSeries _filaRLineSeries;
        LineSeries _gridVolLineSeries;
        #endregion

        #region ------------Property------------
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

        private bool _isNewFila;
        /// <summary>
        /// 是否是新灯丝
        /// </summary>
        public bool IsNewFila
        {
            get { return _isNewFila; }
            set { _isNewFila = value; RaisePropertyChanged(); }
        }

        private int _setAccVol;
        /// <summary>
        /// 设置灯丝电压
        /// </summary>
        public int SetAccVol
        {
            get { return _setAccVol; }
            set { _setAccVol = value; RaisePropertyChanged(); }
        }

        private int _changeAccVol;
        /// <summary>
        /// 修改灯丝高压
        /// </summary>
        public int ChangeAccVol
        {
            get { return _changeAccVol; }
            set { _changeAccVol = value; RaisePropertyChanged(); }
        }

        private byte _userParam1;
        /// <summary>
        /// 用户参数1
        /// </summary>
        public byte UserParam1
        {
            get { return _userParam1; }
            set { _userParam1 = value; RaisePropertyChanged(); }
        }

        private byte _userParam2;
        /// <summary>
        /// 用户参数2
        /// </summary>
        public byte UserParam2
        {
            get { return _userParam2; }
            set { _userParam2 = value; RaisePropertyChanged(); }
        }

        private byte _userParam3;
        /// <summary>
        /// 用户参数3
        /// </summary>
        public byte UserParam3
        {
            get { return _userParam3; }
            set { _userParam3 = value; RaisePropertyChanged(); }
        }

        private byte _userParam4;
        /// <summary>
        /// 用户参数4
        /// </summary>
        public byte UserParam4
        {
            get { return _userParam4; }
            set { _userParam4 = value; RaisePropertyChanged(); }
        }

        private bool _isOpen;
        /// <summary>
        /// 是否开枪
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; RaisePropertyChanged(); }
        }

        private double _readAccVol;
        /// <summary>
        /// 读取加速电压
        /// </summary>
        public double ReadAccVol
        {
            get { return _readAccVol; }
            set 
            { 
                _readAccVol = value;
                RaisePropertyChanged();
                _accVolLineSeries.Points.Add(new DataPoint(_accVolLineSeries.Points.Count, value));
            }
        }

        private double _readEmissCur;
        /// <summary>
        /// 读取发射电流
        /// </summary>
        public double ReadEmissCur
        {
            get { return _readEmissCur; }
            set 
            { 
                _readEmissCur = value;
                RaisePropertyChanged();
                _emissLineSeries.Points.Add(new DataPoint(_emissLineSeries.Points.Count, value));
            }
        }

        private double _readFilaR;
        /// <summary>
        /// 读取灯丝电阻
        /// </summary>
        public double ReadFilaR
        {
            get { return _readFilaR; }
            set 
            { 
                _readFilaR = value;
                RaisePropertyChanged();
                _filaRLineSeries.Points.Add(new DataPoint(_filaRLineSeries.Points.Count, value));
            }
        }

        private double _readGridVol;
        /// <summary>
        /// 读取栅极电压
        /// </summary>
        public double ReadGridVol
        {
            get { return _readGridVol; }
            set 
            { 
                _readGridVol = value;
                RaisePropertyChanged();
                _gridVolLineSeries.Points.Add(new DataPoint(_gridVolLineSeries.Points.Count, value));
            }
        }

        private string _readRegAddr;
        /// <summary>
        /// 读取寄存器地址
        /// </summary>
        public string ReadRegAddr
        {
            get { return _readRegAddr; }
            set { _readRegAddr = value; RaisePropertyChanged(); }
        }

        private string _readFilaParam;
        /// <summary>
        /// 读取灯丝参数
        /// </summary>
        public string ReadFilaParam
        {
            get { return _readFilaParam; }
            set { _readFilaParam = value; RaisePropertyChanged(); }
        }

        private string _readFilaUsedTime;
        /// <summary>
        /// 读取灯丝使用时间
        /// </summary>
        public string ReadFilaUsedTime
        {
            get { return _readFilaUsedTime; }
            set { _readFilaUsedTime = value; RaisePropertyChanged(); }
        }

        private EnumFilaTypes _readFilaType;
        /// <summary>
        /// 读取灯丝类型
        /// </summary>
        public EnumFilaTypes ReadFilaType
        {
            get { return _readFilaType; }
            set { _readFilaType = value; RaisePropertyChanged(); }
        }

        private EnumFilaStates _readFilaState;
        /// <summary>
        /// 灯丝状态
        /// </summary>
        public EnumFilaStates ReadFilaState
        {
            get { return _readFilaState; }
            set { _readFilaState = value; RaisePropertyChanged(); }
        }

        private string _lastError;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string LastError
        {
            get { return _lastError; }
            set { _lastError = value; RaisePropertyChanged(); }
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

        private ObservableCollection<CustomCmdModel> _customCmds;
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CustomCmdModel> CustomCmds
        {
            get { return _customCmds; }
            set { _customCmds = value; RaisePropertyChanged(); }
        }


        private int _monitorInterval;
        /// <summary>
        /// 时间间隔
        /// </summary>
        public int MonitorInterval
        {
            get { return _monitorInterval; }
            set { _monitorInterval = value; RaisePropertyChanged(); }
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
        #endregion

        #region ------------Command------------
        public DelegateCommand SetAccVolCommand { get; set; }
        public DelegateCommand ClearErrorCommand { get; set; }
        public DelegateCommand ReDefaultCommand { get; set; }
        public DelegateCommand ReadRegisterCommand { get; set; }
        public DelegateCommand SetUserParamCommand { get; set; }
        public DelegateCommand CloseHvCommand { get; set; }
        public DelegateCommand OpenHvCommand { get; set; }
        public DelegateCommand ChangeHvCommand { get; set; }
        public DelegateCommand ChangeMonitorStateCommand { get; set; }
        public DelegateCommand<object> SendCmdStringCommand { get; set; }
        public DelegateCommand AutoAdjustCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand SaveMonitorInfoCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 添加日志信息
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="isRead">是否是读取</param>
        public void AddLog(string log, bool isRead = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Logs == null)
                {
                    Logs = new ObservableCollection<string>();
                }

                if (isRead)
                {
                    Logs.Add($"{DateTime.Now.ToString("t")} 读取: {log}");
                }
                else
                {
                    Logs.Add($"{DateTime.Now.ToString("t")} 发送: {log}");
                }
            });
            
        }

        /// <summary>
        /// 发送消息接口
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMsg(byte[] bytes)
        {
            SerialPortService.SendMsg(bytes);
            AddLog(SerialPortService.GetCmdString(bytes, bytes.Length), false);
        }

        /// <summary>
        /// 设置高压数值
        /// </summary>
        public void SetAccVolValue()
        {
            byte[] bytes = HvControllerProtocol.GetSetStartHVCmd((byte)SetAccVol);
            SendMsg(bytes);
        }

        /// <summary>
        /// 清除错误
        /// </summary>
        public void ClearError()
        {
            byte[] bytes = HvControllerProtocol.GetClearErrCmd();
            SendMsg(bytes);
        }

        /// <summary>
        /// 恢复默认
        /// </summary>
        public void ReDefault()
        {
            byte[] bytes = HvControllerProtocol.GetClearErrCmd();
            SendMsg(bytes);
        }

        /// <summary>
        /// 读寄存器
        /// </summary>
        public void ReadRegister()
        {
            byte[] bytes = HvControllerProtocol.GetReadRegisterCmd();
            SendMsg(bytes);
        }

        /// <summary>
        /// 设置用户参数
        /// </summary>
        public void SetUserParam()
        {
            byte[] bytes = HvControllerProtocol.GetSetFilaParamCmd(UserParam1, UserParam2, UserParam3, UserParam4);
            SendMsg(bytes);
        }

        /// <summary>
        /// 关闭高压
        /// </summary>
        public void CloseHv()
        {
            byte[] bytes = HvControllerProtocol.GetGunCloseCmd();
            SendMsg(bytes);
        }

        /// <summary>
        /// 开启高压
        /// </summary>
        public void OpenHv()
        {
            byte[] bytes = HvControllerProtocol.GetGunOpenCmd(IsNewFila);
            SendMsg(bytes);
        }

        /// <summary>
        /// 修改高压
        /// </summary>
        public void ChangeHv()
        {
            byte[] bytes = HvControllerProtocol.GetSetChangedHVCmd((byte)ChangeAccVol);
            SendMsg(bytes);
        }

        /// <summary>
        /// 清除监控数据
        /// </summary>
        public void ClearMonitor() 
        {
            _accVolLineSeries.Points.Clear();
            _emissLineSeries.Points.Clear();
            _filaRLineSeries.Points.Clear();
            _gridVolLineSeries.Points.Clear();
        }

        /// <summary>
        /// 图表自适应显示
        /// </summary>
        public void AutoAdjust()
        {
            foreach (var axis in HvPlotModel.Axes)
            { 
                axis.Reset();
            }

            HvPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 改变监控状态
        /// </summary>
        public async void ChangeMonitorState()
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
        /// 发送消息字符串，十六进制发送
        /// </summary>
        public void SendCmdString(object o)
        { 
            CustomCmdModel cmdModel = o as CustomCmdModel;
            if(cmdModel != null) 
            {
                var cmd = DataTypeCaster.StringToByteArray(cmdModel.CmdString);
                SendMsg(cmd);
            }
        }

        /// <summary>
        /// 保存图表数据
        /// </summary>
        public void SaveMonitorInfo()
        { 
            // 借鉴已经存在表格数据
        }
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsNewFila = false;
            SetAccVol = 5;
            IsOpen = false;
            MonitorState = "开始监控";
            MonitorInterval = 1000;
            UserParam1 = 0x00;
            UserParam2 = 0x00;
            UserParam3 = 0x00;
            UserParam4 = 0x00;

            // 初始化图表信息
            HvPlotModel = new PlotModel();
            HvPlotModel.Legends.Add(new Legend());
            HvPlotModel.Axes.Add(new LinearAxis() { Title = "时间", Position = OxyPlot.Axes.AxisPosition.Bottom });
            HvPlotModel.Axes.Add(new LogarithmicAxis() { Title = "数值", Position = OxyPlot.Axes.AxisPosition.Left });
            _accVolLineSeries = new LineSeries() { Title = "加速电压", RenderInLegend = true };
            _emissLineSeries = new LineSeries() { Title = "发射电流", RenderInLegend = true };
            _filaRLineSeries = new LineSeries() { Title = "灯丝电阻", RenderInLegend = true };
            _gridVolLineSeries = new LineSeries() { Title = "栅极电压", RenderInLegend = true };
            HvPlotModel.Series.Add(_accVolLineSeries);
            HvPlotModel.Series.Add(_emissLineSeries);
            HvPlotModel.Series.Add(_filaRLineSeries);
            HvPlotModel.Series.Add(_gridVolLineSeries);

            Logs = new ObservableCollection<string>();

            CustomCmds = new ObservableCollection<CustomCmdModel>();
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
            CustomCmds.Add(new CustomCmdModel(SendMsg));
        }

        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            SetAccVolCommand = new DelegateCommand(SetAccVolValue);
            ClearErrorCommand = new DelegateCommand(ClearError);
            ReDefaultCommand = new DelegateCommand(ReDefault);
            ReadRegisterCommand = new DelegateCommand(ReadRegister);
            SetUserParamCommand = new DelegateCommand(SetUserParam);
            CloseHvCommand = new DelegateCommand(CloseHv);
            OpenHvCommand = new DelegateCommand(OpenHv);
            ChangeHvCommand = new DelegateCommand(ChangeHv);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            AutoAdjustCommand = new DelegateCommand(AutoAdjust);
            ChangeMonitorStateCommand = new DelegateCommand(ChangeMonitorState);
            SendCmdStringCommand = new DelegateCommand<object>(SendCmdString);
            SaveMonitorInfoCommand = new DelegateCommand(SaveMonitorInfo);
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
            var cmd = HvControllerProtocol.GetReadHvParamCmd();
            SendMsg(cmd);

            Thread.Sleep(100);
            cmd = HvControllerProtocol.GetCheckHvStatusCmd();
            SendMsg(cmd);

            foreach (var custom in CustomCmds) 
            {
                if (custom != null && custom.IsAutoSend)
                {
                    Thread.Sleep(100);
                    custom.Send();
                }
            }
        }

        /// <summary>
        /// 设备数据回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_UpdateResponse(object sender, byte[] e)
        {
            if (e == null)
                return;

            Array.Copy(e, 0, _response, _responseLength, e.Length);
            _responseLength += e.Length;

            var response = HvControllerProtocol.FindResponse(ref _response, ref _responseLength);

            if (response != null)
            {
                int size = response.Length;

                // 检查数据头尾
                for (int i = 0; i < 4; i++)
                {
                    if (response[i] != 0xFF && response[size - 1 - i] != 0xEE)
                    {
                        return;
                    }
                }

                // 判断回复内容
                if (response.Length <= 13)
                {
                    AddLog(SerialPortService.GetCmdString(response, size));
                    switch (response[4])
                    {
                        case 0x70:
                            // 电压设置指令
                            {
                                if (response[8] == 0x01)
                                {
                                    _operateHvResult = true;
                                    _operateHvWaitHandle.Set();
                                }
                                else
                                {
                                    LastError = "电压设置失败";
                                    _operateHvResult = false;
                                    _operateHvWaitHandle.Set();
                                }
                            }
                            break;
                        case 0x80:
                            // 开枪指令
                            {
                                if (response[8] != 0x01)
                                {
                                    LastError = "开枪指令发送失败";
                                    _operateHvResult = false;
                                    _operateHvWaitHandle.Set();
                                }
                            }
                            break;
                        case 0xF1:
                        case 0xF2:
                        case 0xF3:
                        case 0xF4:
                        case 0xF5:
                        case 0xF6:
                            break;
                        case 0xF7:
                            // 开枪成功指令
                            {
                                _operateHvResult = true;
                                _operateHvWaitHandle.Set();
                                IsOpen = true;
                            }
                            break;
                        case 0xE0:
                            //开枪错误：系统错误
                            LastError = "由于系统错误开枪失败";
                            _operateHvResult = false;
                            _operateHvWaitHandle.Set();
                            IsOpen = false;
                            break;
                        case 0xE1:
                            //开枪错误：高压错误，打火
                            LastError = "开枪可能打火导致高压错误";
                            _operateHvResult = false;
                            _operateHvWaitHandle.Set();
                            IsOpen = false;
                            break;
                        case 0xE2:
                            //开枪错误：灯丝耗尽
                            LastError = "灯丝耗尽，开枪失败！";
                            _operateHvResult = false;
                            _operateHvWaitHandle.Set();
                            IsOpen = false;
                            break;
                        case 0xE3:
                            //开枪错误：高压启动失败
                            LastError = $"高压启动失败，失败环节发生在{response[8]}环节！";
                            _operateHvResult = false;
                            _operateHvWaitHandle.Set();
                            IsOpen = false;
                            break;
                        case 0x90:
                            //关枪指令
                            {
                                if (response[8] == 0x01)
                                {
                                    _operateHvResult = true;
                                    _operateHvWaitHandle.Set();
                                    IsOpen = false;
                                }
                                else
                                {
                                    LastError = "关枪失败";
                                    _operateHvResult = false;
                                    _operateHvWaitHandle.Set();
                                }
                            }
                            break;
                        case 0x71:
                            // 高压切换
                            {
                                if (response[7] == 0x01)
                                {
                                    LastError = "数值错误导致高压切换失败";
                                    _operateHvResult = false;
                                    _operateHvWaitHandle.Set();
                                }
                                else
                                {
                                    if (response[8] == 0x01)
                                    {
                                        _operateHvResult = true;
                                        _operateHvWaitHandle.Set();
                                    }
                                    else
                                    {
                                        LastError = "当前高压未开启，高压切换失败";
                                        _operateHvResult = false;
                                        _operateHvWaitHandle.Set();
                                        IsOpen = false;
                                    }
                                }
                            }
                            break;
                        case 0x73:
                            //灯丝类型设置
                            {
                                if (response[8] == 0x01)
                                {
                                    _operateHvResult = true;
                                    _operateHvWaitHandle.Set();
                                }
                                else
                                {
                                    LastError = "灯丝类型设置失败";
                                    _operateHvResult = false;
                                    _operateHvWaitHandle.Set();
                                }
                            }
                            break;
                        case 0x7F:
                            //灯丝参数手动设置
                            {
                                if (response[8] == 0x01)
                                {
                                    _setFilaParamResult = true;
                                    _setFilaParamWaitHandle.Set();
                                }
                                else
                                {
                                    LastError = "灯丝参数手动设置失败";
                                    _setFilaParamResult = false;
                                    _setFilaParamWaitHandle.Set();
                                }
                            }
                            break;
                        case 0x72:
                            //高压错误清除指令
                            {
                                if (response[8] == 0x01)
                                {
                                    _operateHvResult = true;
                                    _operateHvWaitHandle.Set();
                                }
                                else
                                {
                                    if (response[7] == 0x01)
                                    {
                                        LastError = "开枪中不可清除错误";
                                        _operateHvResult = false;
                                        _operateHvWaitHandle.Set();
                                    }
                                    else
                                    {
                                        LastError = "当前没有错误";
                                        _operateHvResult = false;
                                        _operateHvWaitHandle.Set();
                                    }
                                }
                            }
                            break;
                        case 0xA0:
                            //高压状态查询指令
                            {
                                if (response[8] == 0x01)
                                {
                                    switch (response[7])
                                    {
                                        case 0x00:
                                            ReadFilaState = EnumFilaStates.Idle;
                                            break;
                                        case 0x10:
                                            ReadFilaState = EnumFilaStates.HvErr;
                                            break;
                                        case 0x20:
                                            ReadFilaState = EnumFilaStates.SysErr;
                                            break;
                                        case 0x30:
                                            ReadFilaState = EnumFilaStates.OpenFailed;
                                            break;
                                        case 0x40:
                                            ReadFilaState = EnumFilaStates.FilaOut;
                                            break;
                                        case 0x50:
                                            ReadFilaState = EnumFilaStates.ChangFailed;
                                            break;
                                        case 0x01:
                                            ReadFilaState = EnumFilaStates.Opening;
                                            break;
                                        case 0x02:
                                            ReadFilaState = EnumFilaStates.Opened;
                                            break;
                                        case 0x03:
                                            ReadFilaState = EnumFilaStates.Changing;
                                            break;
                                        case 0x04:
                                            ReadFilaState = EnumFilaStates.Closing;
                                            break;
                                    }
                                }
                            }
                            break;
                        default:
                            //ParseResponse(Encoding.UTF8.GetString(_response));
                            break;
                    }
                }
                else
                {
                    //非规则查询指令
                    string str = Encoding.UTF8.GetString(response);
                    AddLog(str);
                    ParseResponse(str);
                }

            }
        }

        /// <summary>
        /// 非规则指令解析
        /// </summary>
        /// <param name="response"></param>
        private void ParseResponse(string response)
        {
            try
            {
                if (response.Contains("readhHv_"))
                {
                    int pos1 = response.IndexOf("readhHv_") + 8;
                    int pos2 = response.IndexOf("---readhiv_");
                    if (double.TryParse(response.Substring(pos1, pos2 - pos1), out var accVol))
                    {
                        ReadAccVol = accVol;
                    }
                    pos1 = pos2 + 11;
                    pos2 = response.IndexOf("---readhHr_");
                    if (double.TryParse(response.Substring(pos1, pos2 - pos1), out var emissCur))
                    {
                        ReadEmissCur = emissCur;
                    }
                    pos1 = pos2 + 11;
                    pos2 = response.IndexOf("---readhBv_");
                    if (double.TryParse(response.Substring(pos1, pos2 - pos1), out var filaR))
                    {
                        ReadFilaR = filaR;
                    }
                    pos1 = pos2 + 11;
                    pos2 = response.IndexOf("\r\n");
                    if (double.TryParse(response.Substring(pos1, pos2 - pos1), out var gridVol))
                    {
                        ReadGridVol = gridVol;
                    }

                    HvPlotModel.InvalidatePlot(true);
                }
                else if (response.Contains("NUM_"))
                {
                    int pos1 = response.IndexOf("NUM_") + 4;
                    int pos2 = response.IndexOf("--pn_");
                    ReadRegAddr = response.Substring(pos1, pos2 - pos1);
                    pos1 = pos2 + 5;
                    pos2 = response.IndexOf("--P_");
                    ReadFilaParam = response.Substring(pos1, pos2 - pos1);
                    pos1 = pos2 + 4;
                    pos2 = response.IndexOf("--T_");
                    ReadFilaParam += (" " + response.Substring(pos1, pos2 - pos1));
                    pos1 = pos2 + 4;
                    pos2 = response.IndexOf("--ft_");
                    ReadFilaUsedTime = response.Substring(pos1, pos2 - pos1);
                    pos1 = pos2 + 5;
                    pos2 = response.Length - 4;
                    int filaType = int.Parse(response.Substring(pos1, pos2 - pos1));
                    switch (filaType)
                    {
                        case 0:
                            ReadFilaType = EnumFilaTypes.SelfProduct;// ("自焊接灯丝");
                            break;
                        case 1:
                            ReadFilaType = EnumFilaTypes.CFProduct; //("曹峰提供");
                            break;
                        case 2:
                            ReadFilaType = EnumFilaTypes.LabFila; //("六硼化镧");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                LogManager.GetCurrentClassLogger().Error($"HVBoardEntity ParseResponse Exception : " + ex.Message);
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
