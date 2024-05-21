#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.CCSTool.ViewModels
 * 唯一标识：5db0839c-b61f-4ede-88aa-33d7beed66a0
 * 文件名：CCSToolViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/5/21 10:10:51
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
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.CCSTool.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;
using NLog;
using UtilityTools.Core.Model;
using System.ComponentModel;
using System.Windows.Threading;

namespace UtilityTools.Modules.CCSTool.ViewModels
{
    public class CCSToolViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public CCSToolViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;
            InitCommand();
            InitProperty();

            Service.UpdateResponse += Service_UpdateResponse;
        }

        ~CCSToolViewModel()
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

        private CCSModel _modelA;

        public CCSModel ModelA
        {
            get { return _modelA; }
            set { _modelA = value; RaisePropertyChanged(); }
        }

        private CCSModel _modelB;

        public CCSModel ModelB
        {
            get { return _modelB; }
            set { _modelB = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }
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
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            Service = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPCCS");

            ModelA = new CCSModel();
            ModelA.Title = "A板";
            ModelA.ControlItems = new ObservableCollection<IntSliderInfoModel>();
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH0", Channel = 0xA0, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH1", Channel = 0xA1, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH2", Channel = 0xA2, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH3", Channel = 0xA3, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH4", Channel = 0xA4, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH5", Channel = 0xA5, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH7", Channel = 0xA7, MinValue = 0, MaxValue = 65535, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CH8", Channel = 0xA8, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CA0", Channel = 0xAA, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CA1", Channel = 0xAB, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelA.ControlItems.Add(new IntSliderInfoModel() { Title = "CA2", Channel = 0xAC, MinValue = -4095, MaxValue = 4095, Value = 0 });

            ModelB = new CCSModel();
            ModelB.Title = "B板";
            ModelB.ControlItems = new ObservableCollection<IntSliderInfoModel>();
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH0", Channel = 0xB0, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH1", Channel = 0xB1, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH2", Channel = 0xB2, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH3", Channel = 0xB3, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH4", Channel = 0xB4, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH5", Channel = 0xB5, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH7", Channel = 0xB7, MinValue = 0, MaxValue = 65535, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CH8", Channel = 0xB8, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CA0", Channel = 0xBA, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CA1", Channel = 0xBB, MinValue = -4095, MaxValue = 4095, Value = 0 });
            ModelB.ControlItems.Add(new IntSliderInfoModel() { Title = "CA2", Channel = 0xBC, MinValue = -4095, MaxValue = 4095, Value = 0 });

            foreach (var item in ModelA.ControlItems)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }

            foreach (var item in ModelB.ControlItems)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        /// <summary>
        /// 显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.CCSToolRegionName);
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

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var item = sender as IntSliderInfoModel;
            if (item != null)
            {
                byte[] data = new byte[12];
                data[0] = 0x57;
                data[1] = 0x61;
                data[2] = 0x6E;
                data[3] = 0x67;
                data[4] = (byte)item.Channel;
                data[5] = (byte)((Math.Abs(item.Value) & 0xFF00) >> 8);
                data[6] = (byte)(Math.Abs(item.Value) & 0xFF);
                data[7] = item.Value > 0 ? (byte)0x53 : (byte)0x52;
                data[8] = 0x46;
                data[9] = 0x65;
                data[10] = 0x69;
                data[11] = 0x20;

                SendMsg(data);
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
            var sourceMsg = Encoding.Default.GetString(e);
            var list = sourceMsg.Split((char)0x0D);

            foreach (var msg in list)
            {
                if (msg.Length != 0)
                {
                    
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
            Service.SendMsg(msg);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
