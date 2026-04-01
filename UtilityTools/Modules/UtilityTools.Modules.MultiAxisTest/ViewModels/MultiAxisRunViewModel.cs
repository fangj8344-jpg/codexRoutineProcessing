using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TouchSocket.Core;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.MultiAxisTest.Model;
using UtilityTools.Modules.MultiAxisTest.Views;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Services.Interfaces.IServices.IThingboardService;

namespace UtilityTools.Modules.MultiAxisTest.ViewModels
{
    public class MultiAxisRunViewModel:BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly MultiAxisWorkflowState _state;
        private IThingboardService _thingboardService;

        private bool _httpTestResult;
        /// <summary>
        /// 使用HTTP上传的结果
        /// </summary>
        public bool HttpTestResult
        {
            get { return _httpTestResult; }
            set { _httpTestResult = value; RaisePropertyChanged(); }
        }
        public DelegateCommand ExecuteGoBackCommand { get; set; }
        public DelegateCommand UploadDataCommand { get; set; }
        public DelegateCommand OpenEngineerConfigCommand { get; set; }

        //构造函数注入
        public MultiAxisRunViewModel(IRegionManager region, MultiAxisWorkflowState workflowState, Prism.Ioc.IContainerProvider containerProvider)
        {
            _regionManager = region;
            _state = workflowState;
            ExecuteGoBackCommand = new DelegateCommand(ExecuteGoBack);
            UploadDataCommand = new DelegateCommand(async () => await UploadData());
            OpenEngineerConfigCommand = new DelegateCommand(ExecuteOpenEngineerConfig);
            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            _thingboardService.DataUploaded += _thingboardService_DataUploaded;
            _thingboardService.UploadFailed += _thingboardService_UploadFailed;

        }

        private async void ExecuteOpenEngineerConfig()
        {
            // 防止重复弹窗
            if (DialogHost.IsDialogOpen("MultiAxisRunViewHost")) return;

            // 实例化咱们刚才写的漂亮页面
            var view = new EngineerConfigView();

            // 🚨 直接用你的 DialogHost 弹出来，自带黑色半透明遮罩，高级感拉满！
            // 注意这里的 Identifier 要和 XAML 里定义的一致："MultiAxisRunViewHost"
            await DialogHost.Show(view, "MultiAxisRunViewHost");
        }

        private void _thingboardService_UploadFailed(object? sender, UploadFailedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(async () =>
            {
                var errorContent = new StackPanel
                {
                    Margin = new System.Windows.Thickness(16)
                };
                errorContent.Children.Add(new TextBlock
                {
                    Text = "上传失败",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red
                });
                errorContent.Children.Add(new TextBlock
                {
                    Text = $"详情：{e.ErrorMessage}",
                    Margin = new Thickness(0, 10, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
                errorContent.Children.Add(new Button
                {
                    Content = "确定",
                    Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand
                });
                await MaterialDesignThemes.Wpf.DialogHost.Show(errorContent, "MultiAxisRunViewHost");
            });
        }

        private void _thingboardService_DataUploaded(object? sender, string e)
        {
            App.Current.Dispatcher.Invoke(async() => 
            {
                var errorContent = new StackPanel
                {
                    Margin = new System.Windows.Thickness(16)
                };
                errorContent.Children.Add(new TextBlock
                {
                    Text = "上传成功",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red
                });
                errorContent.Children.Add(new TextBlock
                {
                    Text = $"详情：{e}",
                    Margin = new Thickness(0, 10, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
                errorContent.Children.Add(new Button
                {
                    Content = "确定",
                    Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand
                });
                await MaterialDesignThemes.Wpf.DialogHost.Show(errorContent, "MultiAxisRunViewHost");
            });
        }

        private void ExecuteGoBack()
        {

            _state.ResetNewTestCycle();
            _regionManager.RequestNavigate("ContentRegion", "MultiAxisScanView");

        }
        public async Task UploadData()
        {
            try
            {
                // 1. 打上测试结束时间
                // 因为你的 _state 就是档案柜，直接调它！
                //取出最终要上传的数据原件
                _state.CompleteReport();

                var motorTestData = _state.GetFinalReport();
              
                // 2. 拿到管家，准备拼装数据
                // 因为确保配置存在这件事交给了底层 Service，我们这里只要拿 DeviceId 就行
                ThingsBoardAuthManager.LoadConfig();
                var config = ThingsBoardAuthManager.Current;
                // 🚨 3. 严格按照 API 文档的要求，拼装匿名外壳对象
                _state.UploadInformation.DeviceId = config.DeviceId;


                // 4. 序列化成 JSON 字符串
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(_state.UploadInformation, options);

                // 5. 🚀 一键发射！底层保镖会负责查票、买票、带票过安检
                HttpTestResult = await _thingboardService.UploadTelemetryAsync(jsonPayload);

                // 6. 后续处理：如果成功（盾牌变绿弹窗），准备下一台设备的测试 
                if (HttpTestResult)
                {
                    NLog.LogManager.GetCurrentClassLogger().Debug($"标定数据上传成功:\n{jsonPayload}");
                    _state.ResetNewTestCycle();
                    // 注意：你现有的代码里通过 _thingboardService_DataUploaded 事件已经处理了成功弹窗，这里就不需要再弹了
                }


            }
            catch (Exception ex) 
            {
                NLog.LogManager.GetCurrentClassLogger().Debug($"上传失败:{ex}");
            }
        }

      

    }
}
