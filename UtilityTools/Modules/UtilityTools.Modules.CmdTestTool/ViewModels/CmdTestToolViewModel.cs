#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.CmdTestTool.ViewModels
 * 唯一标识：4592491f-6b17-4cc2-8c74-0dd076a27d88
 * 文件名：CmdTestToolViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/1/9 14:39:16
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

using CsvHelper;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.CmdTestTool.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.CmdTestTool.ViewModels
{
    public class CmdTestToolViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public CmdTestToolViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;

            InitCommand();
            InitProperty();

            //消息提示
            _aggregator = containerProvider.Resolve<IEventAggregator>();

            _service = _containerProvider.Resolve<IServiceFactory>().GetSyncRWService("UNCB");
            SetServiceInfo();
        }

        #endregion

        #region ------------Field------------
        private ISyncRWService _service;

        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator _aggregator;

        private string configPath = "./conf";
        private string defaultConfigName = "cmdTestTool.json";
        #endregion

        #region ------------Property------------
        private ObservableCollection<CmdModel> _cmdModelList;

        public ObservableCollection<CmdModel> CmdModelList
        {
            get { return _cmdModelList; }
            set { _cmdModelList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> _logs;

        public ObservableCollection<string> Logs
        {
            get { return _logs; }
            set { _logs = value; RaisePropertyChanged(); }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand ShowDeviceCommand { get; set; }

        public DelegateCommand ClearLogCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------

        private void Model_RequestEvent(object sender, byte[] e)
        {
            SaveConfig();

            if(_service == null || !_service.IsOpen) 
            {
                _aggregator.SendMessage("设备未连接");
                return;
            }

            Logs.Add($"{DateTime.Now.ToString("t")} 发送: {DataTypeCaster.ByteArrayToString(e, e.Length)}");

            byte[] response;
            int resLen = 0;
            _service.Request("测试指令", e, out response, out resLen, 2000);
            if (resLen <= 0)
            {
                _aggregator.SendMessage("通信失败");
                return;
            }

            Logs.Add($"{DateTime.Now.ToString("t")} 接收: {DataTypeCaster.ByteArrayToString(response, resLen)}");
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", _service);
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.CmdTestToolRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<ISyncRWService>("Value");
                if (value != null)
                {
                    _service = value;
                    IsConnected = _service.IsOpen;
                }
            }
        }

        private void ClearLog()
        { 
            Logs.Clear();
        }

        private void InitCommand() 
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            ClearLogCommand = new DelegateCommand(ClearLog);
        }

        private void InitProperty() 
        {
            CmdModelList = new ObservableCollection<CmdModel>();
            Logs = new ObservableCollection<string>();

            for (int i = 0; i < 10; i++)
            {
                CmdModel model = new CmdModel();
                model.RequestEvent += Model_RequestEvent;
                CmdModelList.Add(model);
            }

            LoadConfig();
        }


        /// <summary>
        /// 设置服务信息
        /// </summary>
        /// <param name="service"></param>
        private void SetServiceInfo()
        {
            if (_service == null)
            {
                LogManager.GetCurrentClassLogger().Error($"NetController has No Service!");
                return;
            }

            if (_service.GetHandle() is NetConfigModel netConfig)
            {
                netConfig.HostIp = "192.168.1.33";
                netConfig.HostPort = 5005;
                netConfig.TargetIp = "192.168.1.88";
                netConfig.TargetPort = 5000;
            }
        }

        private void LoadConfig()
        {
            ObservableCollection<CmdModel> list = null;
            try
            {
                string path = Path.Combine(configPath, defaultConfigName);
                if (File.Exists(path))
                {
                    var jsonData = File.ReadAllText(path);
                    JsonSerializerSettings jsetting = new JsonSerializerSettings();
                    jsetting.NullValueHandling = NullValueHandling.Ignore;
                    list = JsonConvert.DeserializeObject<ObservableCollection<CmdModel>>(jsonData, jsetting);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"反序列化仪器模型异常，{ex.Message}");
            }

            if (list != null && list.Count != 0)
            { 
                for(int i = 0; i < list.Count || i < CmdModelList.Count; i++) 
                {
                    CmdModelList[i].CmdType = list[i].CmdType;
                    CmdModelList[i].CmdData = list[i].CmdData;
                }
            }
        }

        private void SaveConfig()
        {
            try
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;

                var jsonString = JsonConvert.SerializeObject(CmdModelList, settings);

                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }

                string path = Path.Combine(configPath, defaultConfigName);

                using (var file = File.Create(path))
                {
                    var fileData = Encoding.UTF8.GetBytes(jsonString);
                    file.Write(fileData);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"序列化仪器模型异常，{ex.Message}");
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
