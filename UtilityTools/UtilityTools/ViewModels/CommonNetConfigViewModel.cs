#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.ViewModels
 * 唯一标识：d5f524a4-858c-42c5-a86f-3b2b50a8abfa
 * 文件名：CommonNetConfigViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/25 18:08:05
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
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.UserControls.ViewModels;
using Zeptools.CommonLib.ComDevice;
using Zeptools.CommonLib.Model;

namespace UtilityTools.ViewModels
{
    public class CommonNetConfigViewModel : BindableBase, IDialogHostAware
    {
        #region ------------Constructor------------
        public CommonNetConfigViewModel()
        {
            ExecuteCommand = new DelegateCommand<string>(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

            TargetIP = new IPTextBoxViewModel();
            HostIP = new IPTextBoxViewModel();
            Status = "连接";

            TargetIP.AddressChanged += TargetIP_AddressChanged;
            HostIP.AddressChanged += HostIP_AddressChanged;
        }

        #endregion

        #region ------------Field------------
        private NetConfigModel _model;
        private IPTextBoxViewModel _targetIP;
        private IPTextBoxViewModel _hostIP;
        private string _status;
        #endregion

        #region ------------Property------------
        public string DialogHostName { get; set; }
        public DelegateCommand<string> ExecuteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 通信设备接口
        /// </summary>
        public IComDevice ComDevice { get; set; }

        /// <summary>
        /// 基础通信接口
        /// </summary>
        public INetDevice NetDevice { get; set; }

        /// <summary>
        /// 目标IP地址
        /// </summary>
        public IPTextBoxViewModel TargetIP
        {
            get { return _targetIP; }
            set
            {
                _targetIP = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 本地IP地址
        /// </summary>
        public IPTextBoxViewModel HostIP
        {
            get { return _hostIP; }
            set
            {
                _hostIP = value;
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------

        public void OnDialogOpend(IDialogParameters parameters)
        {
            ComDevice = parameters.GetValue<IComDevice>("Value");
            if (ComDevice != null)
            {
                NetDevice = ComDevice.BaseDevice as INetDevice;
                if (NetDevice != null && NetDevice.NetInfo != null)
                {
                    NetDevice.NetInfo.PropertyChanged += NetInfo_PropertyChanged;

                    TargetIP.AddressText = NetDevice.NetInfo.TargetIP;
                    HostIP.AddressText = NetDevice.NetInfo.HostIP;
                    if (NetDevice.IsConnect)
                    {
                        Status = "断开";
                    }
                }
            }
        }

        #endregion

        #region ------------PrivateMethod------------

        private void Execute(string arg)
        {
            switch (arg)
            {
                case "ConnectOrNot":
                    ConnnectOrNot(); break;
                default:
                    break;
            }
        }

        private void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                parameters.Add("Value", NetDevice);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, parameters));
            }
        }

        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                parameters.Add("Value", NetDevice);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Cancel, parameters));
            }
        }

        private async void ConnnectOrNot()
        {
            if (!NetDevice.IsConnect)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        NetDevice.Connect();

                        if(NetDevice.IsConnect) 
                        {
                            Status = "断开";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{NetDevice.Name} Open failed: {ex.Message}");
                    }
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    try
                    {
                        NetDevice.Close();
                        if (!NetDevice.IsConnect)
                        {
                            Status = "连接";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{NetDevice.Name} Open failed: {ex.Message}");
                    }
                });
            }

        }

        private void NetInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var netInfo = sender as NetInfoModel;
            if (netInfo != null)
            {
                switch (e.PropertyName)
                {
                    case "TargetIP":
                        TargetIP.AddressText = netInfo.TargetIP;
                        break;
                    case "HostIP":
                        HostIP.AddressText = netInfo.HostIP;
                        break;
                }
            }
        }

        private void HostIP_AddressChanged(object sender, EventArgs e)
        {
            var model = sender as IPTextBoxViewModel;

            if (model != null && NetDevice != null && NetDevice.NetInfo != null) 
            {
                NetDevice.NetInfo.HostIP = model.AddressText;
            }
        }

        private void TargetIP_AddressChanged(object sender, EventArgs e)
        {
            var model = sender as IPTextBoxViewModel;

            if (model != null && NetDevice != null && NetDevice.NetInfo != null)
            {
                NetDevice.NetInfo.TargetIP = model.AddressText;
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
