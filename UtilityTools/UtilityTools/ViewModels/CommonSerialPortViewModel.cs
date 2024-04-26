#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.ViewModels
 * 唯一标识：83798a5e-5f9c-438d-876b-ff6e641e9eb2
 * 文件名：CommonSerialPortViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/25 18:08:23
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
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Core.Extension;
using UtilityTools.Services.Interfaces.IServices;
using NLog;
using Zeptools.CommonLib.ComDevice;
using Zeptools.CommonLib.Model;

namespace UtilityTools.ViewModels
{
    public class CommonSerialPortViewModel : BindableBase, IDialogHostAware
    {
        #region ------------Constructor------------
        public CommonSerialPortViewModel(IContainerProvider containerProvider)
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
        public SerialPortInfoModel _serialPortInfo;
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
        public IComDevice ComDevice { get; set; }

        public ISerialPortDevice SerialPortDevice { get; set; }

        public SerialPortInfoModel SerialPortInfo
        {
            get { return _serialPortInfo; }
            set { _serialPortInfo = value; RaisePropertyChanged(); }
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
            ComDevice = parameters.GetValue<IComDevice>("Value");
            if (ComDevice != null)
            {
                SerialPortDevice = ComDevice.BaseDevice as ISerialPortDevice;
                if (SerialPortDevice != null)
                {
                    SerialPortInfo = SerialPortDevice.SerialPortInfo;
                    PortNames = SerialPort.GetPortNames().ToList<string>();
                    if (PortNames.Count > 0)
                    {
                        if (SerialPortInfo != null)
                        {
                            if (string.IsNullOrEmpty(SerialPortInfo.PortName))
                                SerialPortInfo.PortName = PortNames[0];
                        }
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
                parameters.Add("Value", SerialPortDevice);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, parameters));
            }
        }

        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                parameters.Add("Value", SerialPortDevice);
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
            if (string.IsNullOrEmpty(SerialPortInfo.PortName))
            {
                // Send Message 
                aggregator.SendMessage("请选择正确的端口名称");
                return;
            }

            if (SerialPortDevice.IsConnect)
            {
                SerialPortDevice.Close();
                Status = "连接";
            }
            else
            {
                try
                {
                    SerialPortDevice.Connect();
                    if (SerialPortDevice.IsConnect)
                        Status = "断开";
                }
                catch (Exception ex)
                {
                    //提示错误信息
                    aggregator.SendMessage($"{SerialPortInfo.PortName}串口打开失败: {ex.Message}！");
                    LogManager.GetCurrentClassLogger().Error($"{SerialPortInfo.PortName}串口打开失败: {ex.Message}！");
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
