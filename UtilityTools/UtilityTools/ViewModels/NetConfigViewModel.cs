#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.ViewModels
 * 唯一标识：56aab063-c9fb-4e9f-aedb-1d81b712e1c3
 * 文件名：NetConfigViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/14 16:59:36
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
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.UserControls.ViewModels;

namespace UtilityTools.ViewModels
{
    public class NetConfigViewModel : BindableBase, IDialogHostAware
    {
        #region ------------Constructor------------
        public NetConfigViewModel()
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
        /// 基础服务接口
        /// </summary>
        public IBaseService BaseService { get; set; }

        /// <summary>
        /// 网络配置模型
        /// </summary>
        public NetConfigModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 
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
        /// 
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
            BaseService = parameters.GetValue<IBaseService>("Value");
            if (BaseService != null)
            {
                Model = BaseService.GetHandle() as NetConfigModel;
                if (Model != null)
                {
                    TargetIP.AddressText = Model.TargetIp;
                    HostIP.AddressText = Model.HostIp;
                    if (Model.IsOpen())
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
                case "ConnectTest":
                    ConnnectTest(); break;
                default:
                    break;
            }
        }

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

        private async void ConnnectTest()
        {
            if (!BaseService.IsOpen)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        if (BaseService.Open())
                        {
                            BaseService.ConnectTest?.Invoke(BaseService);
                            TargetIP.AddressText = Model.TargetIp;
                            Status = "断开";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{BaseService.Name} Open failed: {ex.Message}");
                    }
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    try
                    {
                        BaseService.Close();
                        if(!BaseService.IsOpen) 
                        {
                            Status = "连接";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{BaseService.Name} Open failed: {ex.Message}");
                    }
                });
            }

        }

        private void HostIP_AddressChanged(object sender, EventArgs e)
        {
            Model.HostIp = HostIP.AddressText;
        }

        private void TargetIP_AddressChanged(object sender, EventArgs e)
        {
            Model.TargetIp = TargetIP.AddressText;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
