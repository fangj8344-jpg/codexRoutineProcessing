#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：Zeptools.VacuumLibTest.ViewModels
 * 唯一标识：b7a67832-2c0c-4b78-ac17-dae30000143e
 * 文件名：VacuumLibTestViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/26 14:01:12
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

using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Core.Extension;
using Zeptools.VacuumLib.Entity;
using Zeptools.CommonLib.ComDevice;
using Zeptools.CommonLib.Help;
using UtilityTools.VacuumLibTest.Model;
using Zeptools.CommonLib.Model;
using Prism.Events;

namespace UtilityTools.VacuumLibTest.ViewModels
{
    internal class VacuumLibTestViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public VacuumLibTestViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService; 
            _aggregator = containerProvider.Resolve<IEventAggregator>();

            InitDeviceMethod();
            InitCommand();
            InitProperty();
        }

        ~VacuumLibTestViewModel()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator _aggregator;
        private System.Timers.Timer _timer;

        private IAsynRWDevice _device;
        private VacuumEntity _entity;
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

        private VacuumModel _model;
        /// <summary>
        /// 真空模型
        /// </summary>
        public VacuumModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }
        public DelegateCommand<object> UpdateVacVauleCommand { get; set; }
        public DelegateCommand<object> OpLowVacuumCommand { get; set; }
        public DelegateCommand<object> ValveControlCommand { get; set; }
        public DelegateCommand<object> PidControlCommand { get; set; }
        public DelegateCommand<object> CalibrateCommand { get; set; }
        public DelegateCommand<object> SetIOCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化设备
        /// </summary>
        private void InitDeviceMethod()
        {
            IBaseDevice baseDevice= new SerialPortDevice("真空控制板");
            baseDevice.IsBinary = true;
            _device = new AsynRWDevice(baseDevice);
            _device.Name = "真空控制板";
            _entity = new VacuumEntity(_device);
        }

        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            UpdateVacVauleCommand = new DelegateCommand<object>(UpdateVacValue);
            OpLowVacuumCommand = new DelegateCommand<object>(OpLowVacuum);
            ValveControlCommand = new DelegateCommand<object>(ValveControl);
            PidControlCommand = new DelegateCommand<object>(PidControl);
            CalibrateCommand = new DelegateCommand<object>(Calibrate);
            SetIOCommand = new DelegateCommand<object>(SetIO);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            Model = new VacuumModel();
        }

        /// <summary>
        /// 显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", _device);
            var diaglogResult = await this._dialogHostService.ShowDialog("CommonSerialPortView", parameter, CommonModel.VacuumLibTestRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWDevice>("Value");
                if (value != null)
                {
                    _device = value;
                    IsConnected = _device.IsOpen;
                }
            }
        }

        /// <summary>
        /// 更新真空值
        /// </summary>
        /// <param name="obj">参数</param>
        private void UpdateVacValue(object obj)
        { 
            if (obj != null) 
            {
                byte channel = 0;
                switch(obj.ToString())
                {
                    case "1":
                        channel = 1;
                        break;
                    case "2":
                        channel = 2;
                        break;
                    case "3":
                        channel = 3;
                        break;
                    case "4":
                        channel = 4;
                        break;
                    default:
                        channel = 0;
                        break;
                }

                var result = _entity.GetVAC(channel);
                if (result != null && result.Result)
                {
                    var list = result.Params.Split(',');
                    if (list.Length > 2 && list[0] == "0")
                    {
                        for (int i = 1; i < list.Length; i++)
                        {
                            if (float.TryParse(list[i], out float v))
                            {
                                switch (i)
                                {
                                    case 1:
                                        Model.VacValue1 = v;
                                        break;
                                    case 2:
                                        Model.VacValue2 = v;
                                        break;
                                    case 3:
                                        Model.VacValue3 = v;
                                        break;
                                    case 4:
                                        Model.VacValue4 = v;
                                        break;
                                }
                            }
                        }
                    }
                    else if (list.Length == 2)
                    {
                        if (int.TryParse(list[0], out int ch) && float.TryParse(list[1], out float value))
                        {
                            switch (ch)
                            {
                                case 1:
                                    Model.VacValue1 = value;
                                    break;
                                case 2:
                                    Model.VacValue2 = value;
                                    break;
                                case 3:
                                    Model.VacValue3 = value;
                                    break;
                                case 4:
                                    Model.VacValue4 = value;
                                    break;
                            }
                        }
                    }
                }
                else 
                {
                    // 失败
                    ShowResponseResult(result);
                }
            }
        }

        /// <summary>
        /// 操作低真空
        /// </summary>
        /// <param name="obj">参数</param>
        private void OpLowVacuum(object obj)
        {
            if (obj != null)
            {
                switch (obj.ToString())
                {
                    case "Open":
                        {
                            var result = _entity.SetLowVac(Model.LowVacValue);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    case "Close":
                        {
                            var result = _entity.StopLowVac();
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 阀门控制
        /// </summary>
        /// <param name="obj">参数</param>
        private void ValveControl(object obj)
        {
            if (obj != null)
            {
                switch (obj.ToString())
                {
                    case "Gate":
                        {
                            var result = _entity.SetGateValve(Model.GateValveValue);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    case "Leak":
                        {
                            var result = _entity.SetLeakValve(Model.LeakValveValue);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// PID控制
        /// </summary>
        /// <param name="obj">参数</param>
        private void PidControl(object obj)
        {
            if (obj != null)
            {
                switch (obj.ToString())
                {
                    case "Get":
                        {
                            var result = _entity.GetPid();
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                            else
                            {
                                // 显示
                                var list = result.Params.Split(',');
                                if(list.Length == 3) 
                                {
                                    if (float.TryParse(list[0], out var p))
                                    {
                                        Model.P = p;
                                    }

                                    if (float.TryParse(list[1], out var i))
                                    {
                                        Model.I = i;
                                    }

                                    if (float.TryParse(list[2], out var d))
                                    {
                                        Model.D = d;
                                    }
                                }
                            }
                        }
                        break;
                    case "Set":
                        {
                            var result = _entity.SetPid(Model.P, Model.I, Model.D);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 标定功能
        /// </summary>
        /// <param name="obj"></param>
        private void Calibrate(object obj) 
        { 
            if (obj != null)
            {
                switch (obj.ToString())
                {
                    case "Start":
                        {
                            var result = _entity.StartCalibration();
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                            else
                            {
                                // 成功提醒
                                _aggregator.SendMessage("标定成功");
                            }
                        }
                        break;
                    case "CheckVol":
                        {
                            var result = _entity.GetCaliVol();
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                            else
                            {
                                // 显示参数
                                var list = result.Params.Split(',');
                                if (list.Length == 4)
                                {
                                    if (float.TryParse(list[0], out var v1))
                                    {
                                        Model.CaliVol1 = v1;
                                    }
                                    if (float.TryParse(list[1], out var v2))
                                    {
                                        Model.CaliVol2 = v2;
                                    }
                                    if (float.TryParse(list[2], out var v3))
                                    {
                                        Model.CaliVol3 = v3;
                                    }
                                    if (float.TryParse(list[3], out var v4))
                                    {
                                        Model.CaliVol4 = v4;
                                    }
                                }
                            }
                        }
                        break;
                    case "CheckParam":
                        {
                            var result = _entity.SetGateValve(Model.GateValveValue);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                            else 
                            {
                                var list = result.Params.Split(',');
                                if (list.Length == 4)
                                {
                                    if (float.TryParse(list[0], out var v1))
                                    {
                                        Model.CaliParam1 = v1;
                                    }
                                    if (float.TryParse(list[1], out var v2))
                                    {
                                        Model.CaliParam2 = v2;
                                    }
                                    if (float.TryParse(list[2], out var v3))
                                    {
                                        Model.CaliParam3 = v3;
                                    }
                                    if (float.TryParse(list[3], out var v4))
                                    {
                                        Model.CaliParam4 = v4;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 设置端口IO状态
        /// </summary>
        /// <param name="obj"></param>
        private void SetIO(object obj)
        {
            if (obj != null)
            {
                switch (obj.ToString())
                {
                    case "0":
                        {
                            var result = _entity.SetIOState(0, Model.IOState0);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    case "1":
                        {
                            var result = _entity.SetIOState(0, Model.IOState1);
                            if (result == null || result.Result == false)
                            {
                                // 失败
                                ShowResponseResult(result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 显示结果
        /// </summary>
        /// <param name="response"></param>
        private void ShowResponseResult(ResponseProto response)
        {
            if (response != null && response.Result == false)
            {
                _aggregator.SendMessage(response.Message);
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
