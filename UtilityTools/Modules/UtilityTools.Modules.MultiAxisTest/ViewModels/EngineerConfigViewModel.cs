using CoreTools.Global;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using UtilityTools.Core.Helper;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MultiAxisTest.ViewModels
{
    public class EngineerConfigViewModel:BindableBase
    {

        private readonly IThingboardService _thingboardService;
        // ==========================================
        // 1. 绑定的配置属性
        // ==========================================
        private string _deviceId;
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }


        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        private string _serverUrl;
        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        // ==========================================
        // 2. Token 状态显示专用属性 ( UI 联动)
        // ==========================================
        private bool _isTokenValid;
        public bool IsTokenValid
        {
            get => _isTokenValid;
            set
            {
                if (SetProperty(ref _isTokenValid, value))
                {
                    // 只要状态改变，自动刷新 UI 上的颜色、图标和文字
                    RaisePropertyChanged(nameof(TokenStatusIcon));
                    RaisePropertyChanged(nameof(TokenStatusColor));
                    RaisePropertyChanged(nameof(TokenStatusText));
                }
            }
        }

        private string _expireTimeString = "暂无授权记录";
        public string ExpireTimeString
        {
            get => _expireTimeString;
            set => SetProperty(ref _expireTimeString, value);
        }

        // 动态图标：绿盾牌 vs 报错红盾牌
        public string TokenStatusIcon => IsTokenValid ? "ShieldCheck" : "ShieldAlertOutline";

        // 动态颜色：绿色 vs 红色
        public SolidColorBrush TokenStatusColor => IsTokenValid
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))  // 护眼绿
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")); // 警示红

        public string TokenStatusText => IsTokenValid ? "授权令牌 (Token) 有效" : "令牌未获取或已过期";

        // ==========================================
        // 3. 绑定的命令
        // ==========================================
        public DelegateCommand GetTokenCommand { get; }
        public DelegateCommand SaveConfigCommand { get; }
        public DelegateCommand ResetCommand { get; }


        public EngineerConfigViewModel(IContainerProvider containerProvider)
        {
            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            // 初始化命令
            GetTokenCommand = new DelegateCommand(ExecuteGetToken);
            SaveConfigCommand = new DelegateCommand(ExecuteSaveConfig);
            ResetCommand = new DelegateCommand(ExecuteReset);

            loadConfigToUI();
        }

        private void loadConfigToUI()
        {
            // 从硬盘读取最新 JSON
            ThingsBoardAuthManager.LoadConfig();

            //赋值给绑定的UI属性
            DeviceId = ThingsBoardAuthManager.Current.DeviceId;
            Username = ThingsBoardAuthManager.Current.Username;
            ServerUrl = ThingsBoardAuthManager.Current.ServerUrl;
            // 密码要解密后再显示到界面上 (如果你不希望密码反显，这里可以不赋值)
            Password = ThingsBoardAuthManager.DecryptPassword(ThingsBoardAuthManager.Current.EncryptedPassword);

            // 恢复 Token 状态
            if (!string.IsNullOrEmpty(ThingsBoardAuthManager.Current.JwtToken) &&
                ThingsBoardAuthManager.Current.TokenExpireTime > DateTime.Now)
            {
                IsTokenValid = true;
                ExpireTimeString = $"过期时间: {ThingsBoardAuthManager.Current.TokenExpireTime:yyyy-MM-dd HH:mm:ss}";
            }
            else
            {
                IsTokenValid = false;
                ExpireTimeString = "令牌未获取或已过期";
            }

        }

        private void ExecuteSaveConfig()
        {
            //1.把界面上的值，塞给全局配置对象
            ThingsBoardAuthManager.Current.DeviceId = DeviceId;
            ThingsBoardAuthManager.Current.Username = Username;
            ThingsBoardAuthManager.Current.ServerUrl = ServerUrl;
            //2.密码必须加密后再存到对象里面
            ThingsBoardAuthManager.Current.EncryptedPassword = ThingsBoardAuthManager.EncryptPassword(Password);
            ThingsBoardAuthManager.SaveConfig();
        }

     

      

        private void ExecuteReset()
        {
            DeviceId = "";
            Username = "";
            Password = "";
        }

        private async void ExecuteGetToken() 
        {
            //1.先验证工程师有没有把基础信息填全
            if (string.IsNullOrWhiteSpace(DeviceId) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                //提示一下
                return;
            }
            //2.先将当前的值保存到配置文件里面
            ExecuteSaveConfig();

            //3.调用底层服务的登录方法
            
            var config = ThingsBoardAuthManager.Current;
            _thingboardService.ServerUrl = config.ServerUrl;
            var  result = await _thingboardService.LoginAsync(config.Username, Password);

            if (result.IsSuccess)
            {
                // 登录成功！更新管家的票据并保存
                config.JwtToken = result.Token;
                config.TokenExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn);
                ThingsBoardAuthManager.SaveConfig();

                // 更新 UI，让盾牌变绿
                IsTokenValid = true;
                ExpireTimeString = $"过期时间: {config.TokenExpireTime:yyyy-MM-dd HH:mm:ss}";
            }

            else
            {
                // 登录失败！盾牌变红
                IsTokenValid = false;
                ExpireTimeString = $"授权失败: {result.ErrorMsg}";

                // 可选：用 DialogHost 弹出一个漂亮的错误提示框
            }

        }
    }
}
