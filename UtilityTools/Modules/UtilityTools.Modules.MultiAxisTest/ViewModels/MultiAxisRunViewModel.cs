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
            _thingboardService.ServerUrl = "http://192.168.111.206:8000";  
            //_thingboardService.ServerUrl = "http://192.168.111.207:8989";
            _thingboardService.EnableMqtt = false;
            _thingboardService.EnableHttp = true;

            var data = GenerateData();

            HttpTestResult = await _thingboardService.UploadTelemetryAsync(data);
        }

        private string GenerateData()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            _state.UploadInformation.DeviceId = "905e2760-d4a4-11f0-802d-47c72d04dc3f";
            _state.UploadInformation.SampleStageId = "1234";
            _state.UploadInformation.Content = new SampleStageReport
            {
                StageId = "1234",
                StartTime = DateTime.Now.AddMinutes(-5).ToString("o"),
                EndTime = DateTime.Now.ToString("o"),
                Motors = new List<MotorData>
                {
                    new MotorData
                    {
                        AxisType = "X",
                        ForwardSpeedStdDev = 0.01,
                        BackwardSpeedStdDev = 0.02,
                        MinRange = -10,
                        MaxRange = 10,
                        NegativeLimit = true,
                        PositiveLimit = true,
                        PositioningStdDev = 0.005,
                        PositionErrors = new List<PositionError>
                        {
                            new PositionError { TargetPosition = 0, ActualPosition = 0.01 },
                            new PositionError { TargetPosition = 5, ActualPosition = 5.02 },
                            new PositionError { TargetPosition = -5, ActualPosition = -4.98 }
                        }
                    },
                    new MotorData
                    {
                        AxisType = "Y",
                        ForwardSpeedStdDev = 0.015,
                        BackwardSpeedStdDev = 0.025,
                        MinRange = -20,
                        MaxRange = 20,
                        NegativeLimit = true,
                        PositiveLimit = true,
                        PositioningStdDev = 0.007,
                        PositionErrors = new List<PositionError>
                        {
                            new PositionError { TargetPosition = 0, ActualPosition = -0.02 },
                            new PositionError { TargetPosition = 10, ActualPosition = 9.95 },
                            new PositionError { TargetPosition = -10, ActualPosition = -10.05 }
                        }
                    }
                }
            };
            return JsonSerializer.Serialize(_state.UploadInformation, options);
        }

    }
}
