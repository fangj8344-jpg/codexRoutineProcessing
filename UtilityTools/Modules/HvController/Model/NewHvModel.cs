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

using OxyPlot;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;
using UtilityTools.Modules.HvController.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.HvController.Model
{
    internal class NewHvModel : BindableBase
    {
        #region ------------Constructor------------
        public NewHvModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            SerialPortService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPHV");
            SerialPortService.UpdateResponse += Device_UpdateResponse;
            NetUdpService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("UNHV");
            NetUdpService.UpdateResponse += Device_UpdateResponse;


            PrepareWorkCommand = new DelegateCommand(PrepareWorkMethod);
            SetAccVolCommand = new DelegateCommand(SetAccVolMethod);
            CloseHvCommand = new DelegateCommand(CloseHvMethod);
        }
        #endregion

        #region ------------Field------------
        private readonly IContainerProvider _containerProvider;
        private System.Timers.Timer _timer;

        private string _response = string.Empty;

        private BackgroundWorker _backgroundWorker;
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

        private bool _isPrepared = false;
        /// <summary>
        /// 高压箱是否已经准备完毕
        /// </summary>
        public bool IsPrepared
        {
            get { return _isPrepared; }
            set { _isPrepared = value; RaisePropertyChanged(); }
        }

        private float _setAccVol;
        /// <summary>
        /// 设置加速电压
        /// </summary>
        public float SetAccVol
        {
            get { return _setAccVol; }
            set { _setAccVol = value; RaisePropertyChanged(); }
        }

        private float _setHeatCur;
        /// <summary>
        /// 设置加热电流
        /// </summary>
        public float SetHeatCur
        {
            get { return _setHeatCur; }
            set { _setHeatCur = value; RaisePropertyChanged(); }
        }

        private float _setEmissionVol;
        /// <summary>
        /// 设置吸取极电压
        /// </summary>
        public float SetEmissionVol
        {
            get { return _setEmissionVol; }
            set { _setEmissionVol = value; RaisePropertyChanged(); }
        }

        private float _setGridVol;
        /// <summary>
        /// 设置栅极电压
        /// </summary>
        public float SetGridVol
        {
            get { return _setGridVol; }
            set { _setGridVol = value; RaisePropertyChanged(); }
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

        private float _filaCur = float.NaN;
        /// <summary>
        /// 灯丝电流
        /// </summary>
        public float FilaCur
        {
            get { return _filaCur; }
            set { _filaCur = value; RaisePropertyChanged(); }
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

        private float _emissionCur = float.NaN;
        /// <summary>
        /// 吸取极电流
        /// </summary>
        public float EmissionCur
        {
            get { return _emissionCur; }
            set { _emissionCur = value; RaisePropertyChanged(); }
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
        public DelegateCommand PrepareWorkCommand { get; set; }

        private void PrepareWorkMethod()
        { 
            
        }

        public DelegateCommand SetAccVolCommand { get; set; }

        private void SetAccVolMethod()
        { 
            
        }

        public DelegateCommand CloseHvCommand { get; set; }

        private void CloseHvMethod() 
        {
            
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
        #endregion

        #region ------------PublicMethod------------
        public void StartBackgroundWorker()
        { 
        
        }

        public void StopBackgroundWorker() 
        {
            
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
            else
            {
                AddLog("无设备连接", false);
                return;
            }
            AddLog(SerialPortService.GetCmdString(bytes, bytes.Length), false);
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
            AddLog("请求高压状态");
            var cmd = NewHvControllerProtocol.GetRequestCommand();
            SendMsg(cmd);
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

                for(int i = 0; i < count; i++)
                {
                    ParseResponse(list[i]);
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
            }
            else if (msg.StartsWith("HV_init_done"))
            {
                AddLog("完成初始化高压设备");
            }
            else if (msg.StartsWith("BV"))
            {
                AddLog($"设置栅极电压:{msg}");
            }
            else if (msg.StartsWith("PI"))
            {
                AddLog($"设置加热电流:{msg}");
            }
            else if (msg.StartsWith("EV"))
            {
                AddLog($"设置吸取电压:{msg}");
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
                    switch(tmpList[1]) 
                    {
                        case "HV":
                            if (float.TryParse(tmpList[2], out float hv))
                            {
                                AccVol = hv;
                            }
                            break;
                        case "HI":
                            if (float.TryParse(tmpList[2], out float hi))
                            {
                                
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
                            }
                            break;
                        case "R":
                            if (float.TryParse(tmpList[2], out float r))
                            {
                                FilaR = r;
                            }
                            break;
                        case "EV":
                            if (float.TryParse(tmpList[2], out float ev))
                            {
                                EmissionVol = ev;
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
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
