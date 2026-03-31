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
using UtilityTools.Modules.MultiAxisTest.Model;
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

        //构造函数注入
        public MultiAxisRunViewModel(IRegionManager region, MultiAxisWorkflowState workflowState, Prism.Ioc.IContainerProvider containerProvider)
        {
            _regionManager = region;
            _state = workflowState;
            ExecuteGoBackCommand = new DelegateCommand(ExecuteGoBack);
            UploadDataCommand = new DelegateCommand(async () => await UploadData());
            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            _thingboardService.DataUploaded += _thingboardService_DataUploaded;
            _thingboardService.UploadFailed += _thingboardService_UploadFailed;

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
                _state.CompleteReport();
                //取出最终要上传的数据原件
                var finalData = _state.GetFinalReport();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonPayload = JsonSerializer.Serialize(finalData, options);


                _thingboardService.ServerUrl = "http://192.168.111.207:8989";
                _thingboardService.EnableMqtt = false;
                _thingboardService.EnableHttp = true;
                _thingboardService.AccessToken = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJ0ZW5hbnRAdGhpbmdzYm9hcmQub3JnIiwidXNlcklkIjoiYTY0Zjg1NDAtZDRhZi0xMWYwLTgxNDMtZGQ4YTYxYTA4Y2EyIiwic2NvcGVzIjpbIlRFTkFOVF9BRE1JTiJdLCJzZXNzaW9uSWQiOiIwYWQxOGI4ZC00ZWIzLTRjYjItODBjNC04NzY0NDc5OGRlYjUiLCJleHAiOjE3NzQ0MzYwNjksImlzcyI6InRoaW5nc2JvYXJkLmlvIiwiaWF0IjoxNzc0NDI3MDY5LCJlbmFibGVkIjp0cnVlLCJpc1B1YmxpYyI6ZmFsc2UsInRlbmFudElkIjoiYTYxYmNiMTAtZDRhZi0xMWYwLTgxNDMtZGQ4YTYxYTA4Y2EyIiwiY3VzdG9tZXJJZCI6IjEzODE0MDAwLTFkZDItMTFiMi04MDgwLTgwODA4MDgwODA4MCJ9.gOzhq6RXL0loAcQwrY8kUxZFetLDhG0jhwaJkK0of6ts32vXwqEIVuFwstEtcwVgEjFBIjvpFfwBbd5854kE4A";


                HttpTestResult = await _thingboardService.UploadTelemetryAsync(jsonPayload);
                // 发送完毕后，如果成功，系统会自动清理测试循环，准备测下一台设备
                if (HttpTestResult)
                {
                    NLog.LogManager.GetCurrentClassLogger().Debug($"上传成功:{jsonPayload}");
                    _state.ResetNewTestCycle();
                }

            }
            catch (Exception ex) 
            {
                NLog.LogManager.GetCurrentClassLogger().Debug($"上传失败:{ex}");
            }
        }

      

    }
}
