#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.ViewModels
 * 唯一标识：27e8855d-d48e-4ff9-974e-451f85dc60f0
 * 文件名：SerialPortViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:54:19
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

using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Core.Extension;
using NLog;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.ViewModels
{
    internal class SerialPortViewModel : BindableBase, IDialogHostAware
    {
        #region ------------Constructor------------
        public SerialPortViewModel(IContainerProvider containerProvider)
        {
            InitBaudRates();
            InitStopBitsTypes();
            InitDataBits();
            InitParityTypes();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            SwitchCommand = new DelegateCommand(SwitchSerialPort);
            DropDownOpenedCommand = new DelegateCommand(DropDownOpened);

            this.containerProvider = containerProvider;
            aggregator = containerProvider.Resolve<IEventAggregator>();
        }
        #endregion

        #region ------------Field------------
        public SerialPortModel _model;
        public static string _status = "连接";

        private List<string> _portNames;
        private List<string> _baudRates;
        private List<string> _stopBitsType;
        private List<string> _dataBits;
        private List<string> _parityTypes;

        private readonly IContainerProvider containerProvider;
        public readonly IEventAggregator aggregator;
        #endregion

        #region ------------Property------------
        public IBaseService BaseService { get; set; }

        public SerialPortModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }
        public List<string> PortNames
        {
            get { return _portNames; }
            set { _portNames = value; RaisePropertyChanged(); }
        }
        public List<string> BaudRates
        {
            get { return _baudRates; }
            set { _baudRates = value; RaisePropertyChanged(); }
        }
        public List<string> StopBitsTypes
        {
            get { return _stopBitsType; }
            set { _stopBitsType = value; RaisePropertyChanged(); }
        }
        public List<string> DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; RaisePropertyChanged(); }
        }
        public List<string> ParityTypes
        {
            get { return _parityTypes; }
            set { _parityTypes = value; RaisePropertyChanged(); }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(); }
        }

        public string DialogHostName { get; set; }
        #endregion

        #region ------------Command------------
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand SwitchCommand { get; set; }
        public DelegateCommand DropDownOpenedCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        public void OnDialogOpend(IDialogParameters parameters)
        {
            BaseService = parameters.GetValue<IBaseService>("Value");
            if(BaseService != null)
            {
                Model = BaseService.GetHandle() as SerialPortModel;
                PortNames = SerialPort.GetPortNames().ToList<string>();
                if (PortNames.Count > 0)
                {
                    if (Model != null)
                    {
                        Model.PortName = PortNames[0];
                        Model.SerialPort.BaudRate = Model.BaudRate;
                        Model.SerialPort.Parity = Model.Parity;
                        Model.SerialPort.StopBits = Model.StopBits;
                        Model.SerialPort.DataBits = Model.DataBits;
                    }
                }
            }
            
        }
        #endregion

        #region ------------PrivateMethod------------
        private void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                parameters.Add("Value", BaseService);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, parameters));
            }
        }

        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                parameters.Add("Value", BaseService);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Cancel, parameters));
            }
        }

        private void InitBaudRates()
        {
            BaudRates = new List<string>();
            BaudRates.Add("1382400");
            BaudRates.Add("921600");
            BaudRates.Add("460800");
            BaudRates.Add("256000");
            BaudRates.Add("230400");
            BaudRates.Add("128000");
            BaudRates.Add("115200");
            BaudRates.Add("76800");
            BaudRates.Add("57600");
            BaudRates.Add("43000");
            BaudRates.Add("38400");
            BaudRates.Add("19200");
            BaudRates.Add("14400");
            BaudRates.Add("9600");
            BaudRates.Add("4800");
            BaudRates.Add("2400");
            BaudRates.Add("1200");
        }
        private void InitStopBitsTypes()
        {
            StopBitsTypes = new List<string>();
            StopBitsTypes.Add("1");
            StopBitsTypes.Add("1.5");
            StopBitsTypes.Add("2");
        }
        private void InitDataBits()
        {
            DataBits = new List<string>();
            DataBits.Add("8");
            DataBits.Add("7");
            DataBits.Add("6");
            DataBits.Add("5");
        }
        private void InitParityTypes()
        {
            ParityTypes = new List<string>();
            ParityTypes.Add("无");
            ParityTypes.Add("奇校验");
            ParityTypes.Add("偶校验");
        }

        private void SwitchSerialPort()
        {
            if (string.IsNullOrEmpty(Model.PortName))
            {
                // Send Message 
                aggregator.SendMessage("请选择正确的端口名称");
                return;
            }

            if (BaseService.IsOpen)
            {
                BaseService.Close();
                Status = "连接";
            }
            else
            {
                Model.SerialPort.BaudRate = Model.BaudRate;
                Model.SerialPort.PortName = Model.PortName;
                Model.SerialPort.DataBits = Model.DataBits;
                Model.SerialPort.StopBits = Model.StopBits;
                Model.SerialPort.Parity = Model.Parity;

                try
                {
                    if(BaseService.Open())
                        Status = "断开";
                }
                catch (Exception ex)
                {
                    //提示错误信息
                    aggregator.SendMessage($"{Model.PortName}串口打开失败: {ex.Message}！");
                    LogManager.GetCurrentClassLogger().Error($"{Model.PortName}串口打开失败: {ex.Message}！");
                }
            }
        }

        /// <summary>
        /// 下拉框打开
        /// </summary>
        private void DropDownOpened()
        {
            PortNames = SerialPort.GetPortNames().ToList<string>();
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
