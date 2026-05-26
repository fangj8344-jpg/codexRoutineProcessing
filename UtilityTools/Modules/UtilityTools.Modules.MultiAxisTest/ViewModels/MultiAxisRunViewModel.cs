using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TouchSocket.Core;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.MotorTest.Event;
using UtilityTools.Modules.MotorTest.Model;
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
        private readonly IEventAggregator _eventAggregator;

        private bool _httpTestResult;
        /// <summary>
        /// 使用HTTP上传的结果
        /// </summary>
        public bool HttpTestResult
        {
            get { return _httpTestResult; }
            set { _httpTestResult = value; RaisePropertyChanged(); }
        }

        private bool _canUpload;
        /// <summary>
        /// 是否允许上传（所有轴基础测试全部通过才允许）
        /// </summary>
        public bool CanUpload
        {
            get { return _canUpload; }
            set
            {
                if (_canUpload == value) return;
                _canUpload = value;
                RaisePropertyChanged();
                UploadDataCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 各轴基础测试完成状态：轴名 -> 是否已通过
        /// </summary>
        private readonly Dictionary<string, bool> _axisTestPassed = new Dictionary<string, bool>();

        /// <summary>
        /// 各轴基础测试是否已完成（不管通过与否）：轴名 -> 是否已完成
        /// </summary>
        private readonly Dictionary<string, bool> _axisTestCompleted = new Dictionary<string, bool>();

        public DelegateCommand ExecuteGoBackCommand { get; set; }
        public DelegateCommand UploadDataCommand { get; set; }
        public DelegateCommand OpenEngineerConfigCommand { get; set; }

        public string FirmwareCurrentVersion => string.IsNullOrWhiteSpace(_state.CurrentFirmwareVersion) ? "--" : _state.CurrentFirmwareVersion;
        public string FirmwareLatestVersion => string.IsNullOrWhiteSpace(_state.LatestFirmwareVersion) ? "--" : _state.LatestFirmwareVersion;
        public string FirmwareStatusText => string.IsNullOrWhiteSpace(_state.FirmwareCheckMessage) ? "未执行固件检查" : _state.FirmwareCheckMessage;

        //构造函数注入
        public MultiAxisRunViewModel(IRegionManager region, MultiAxisWorkflowState workflowState, Prism.Ioc.IContainerProvider containerProvider)
        {
            _regionManager = region;
            _state = workflowState;
            _eventAggregator = containerProvider.Resolve<IEventAggregator>();
            ExecuteGoBackCommand = new DelegateCommand(ExecuteGoBack);
            UploadDataCommand = new DelegateCommand(async () => await UploadData(), () => CanUpload);
            OpenEngineerConfigCommand = new DelegateCommand(ExecuteOpenEngineerConfig);
            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            _thingboardService.DataUploaded += _thingboardService_DataUploaded;
            _thingboardService.UploadFailed += _thingboardService_UploadFailed;
            _state.PropertyChanged += State_PropertyChanged;

            // 订阅测试结果事件，追踪各轴测试状态
            _eventAggregator.GetEvent<MotorTestResultEvent>().Subscribe(OnMotorTestResult, ThreadOption.UIThread);

            // 快捷键进入时直接允许上传
            if (_state.DevShortcutXyOnlyAxes)
            {
                CanUpload = true;
            }
        }

        private void State_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MultiAxisWorkflowState.CurrentFirmwareVersion))
            {
                RaisePropertyChanged(nameof(FirmwareCurrentVersion));
                return;
            }
            if (e.PropertyName == nameof(MultiAxisWorkflowState.LatestFirmwareVersion))
            {
                RaisePropertyChanged(nameof(FirmwareLatestVersion));
                return;
            }
            if (e.PropertyName == nameof(MultiAxisWorkflowState.FirmwareCheckMessage))
            {
                RaisePropertyChanged(nameof(FirmwareStatusText));
            }
        }

        /// <summary>
        /// 监听测试结果事件，判断各轴基础测试是否全部通过
        /// </summary>
        private void OnMotorTestResult(MotorTestMessage msg)
        {
            if (msg == null || string.IsNullOrWhiteSpace(msg.AxisName)) return;

            // 只关注基础测试完成的结果（Passed 或 Failed）
            if (msg.ProgressState != MotorTestProgressState.Passed
                && msg.ProgressState != MotorTestProgressState.Failed)
                return;

            string axisName = msg.AxisName;

            // 如果某个测试项失败了，标记该轴为未通过
            if (msg.ProgressState == MotorTestProgressState.Failed)
            {
                _axisTestPassed[axisName] = false;
                _axisTestCompleted[axisName] = true;
                CanUpload = false;
                return;
            }

            // 测试项通过了：仅当是"丝杆顺滑度测试"（基础测试最后一个项目）时，
            // 才标记该轴基础测试整体完成并通过
            if (msg.TestProject != null && msg.TestProject.Contains("顺滑度"))
            {
                _axisTestPassed[axisName] = true;
                _axisTestCompleted[axisName] = true;
            }

            // 检查是否所有已记录的轴都通过了（至少有一轴完成才算）
            EvaluateCanUpload();
        }

        /// <summary>
        /// 综合评估是否允许上传：所有已完成的轴必须全部通过，且至少有一轴完成；快捷键模式始终允许
        /// </summary>
        private void EvaluateCanUpload()
        {
            if (_state.DevShortcutXyOnlyAxes)
            {
                CanUpload = true;
                return;
            }

            if (_axisTestCompleted.Count == 0)
            {
                CanUpload = false;
                return;
            }

            // 如果有任何轴未完成或未通过，则不允许上传
            foreach (var kvp in _axisTestCompleted)
            {
                if (!_axisTestPassed.TryGetValue(kvp.Key, out bool passed) || !passed)
                {
                    CanUpload = false;
                    return;
                }
            }

            // 所有已完成的轴都通过了，允许上传
            CanUpload = true;
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
            // 重置上传状态追踪
            _axisTestPassed.Clear();
            _axisTestCompleted.Clear();
            CanUpload = false;

            _state.ResetNewTestCycle();
            _regionManager.RequestNavigate("ContentRegion", "MultiAxisScanView");

        }
        public async Task UploadData()
        {
            try
            {
                var motorTestData = _state.GetFinalReport();
                string sampleStageId = motorTestData?.SampleStageId ?? string.Empty;

                // 查询云端是否已有该样品台的标定数据
                bool cloudHasData = false;
                string cloudQueryError = null;
                string cloudRawJson = null;
                if (!string.IsNullOrWhiteSpace(sampleStageId))
                {
                    var queryResult = await _thingboardService.QueryCalibrationExistsAsync(sampleStageId);
                    if (queryResult.Success)
                    {
                        cloudHasData = queryResult.HasData;
                        cloudRawJson = queryResult.RawJson;
                    }
                    else
                    {
                        cloudQueryError = queryResult.ErrorMsg;
                    }
                }

                // 检查本地电机数据状态
                int localMotorCount = motorTestData?.Content?.Motors?.Count ?? 0;
                bool localHasMotorData = localMotorCount > 0;

                // 构建确认弹窗
                bool userConfirmed = await ShowUploadConfirmDialogAsync(
                    sampleStageId, cloudHasData, cloudQueryError, localHasMotorData, localMotorCount);

                if (!userConfirmed)
                    return;

                ThingsBoardAuthManager.LoadConfig();
                var config = ThingsBoardAuthManager.Current;
                _state.UploadInformation.DeviceId = config.DeviceId;

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(_state.UploadInformation, options);

                HttpTestResult = await _thingboardService.UploadTelemetryAsync(jsonPayload);

                if (HttpTestResult)
                {
                    NLog.LogManager.GetCurrentClassLogger().Debug($"标定数据上传成功:\n{jsonPayload}");
                }
            }
            catch (Exception ex) 
            {
                NLog.LogManager.GetCurrentClassLogger().Debug($"上传失败:{ex}");
            }
        }

        private async Task<bool> ShowUploadConfirmDialogAsync(
            string sampleStageId,
            bool cloudHasData,
            string? cloudQueryError,
            bool localHasMotorData,
            int localMotorCount)
        {
            var tcs = new TaskCompletionSource<bool>();

            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var panel = new StackPanel { Margin = new Thickness(20), MinWidth = 360 };

                panel.Children.Add(new TextBlock
                {
                    Text = "上传确认",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 12)
                });

                // 样品台 ID
                panel.Children.Add(new TextBlock
                {
                    Text = $"样品台编号：{(string.IsNullOrWhiteSpace(sampleStageId) ? "未知" : sampleStageId)}",
                    Margin = new Thickness(0, 0, 0, 8)
                });

                // 云端数据状态
                string cloudStatusText;
                Brush cloudStatusColor;
                if (cloudQueryError != null)
                {
                    cloudStatusText = $"⚠ 云端查询失败：{cloudQueryError}";
                    cloudStatusColor = Brushes.Orange;
                }
                else if (cloudHasData)
                {
                    cloudStatusText = "⚠ 云端已有该样品台的电机标定数据，上传将覆盖！";
                    cloudStatusColor = Brushes.Red;
                }
                else
                {
                    cloudStatusText = "✓ 云端暂无该样品台的电机标定数据";
                    cloudStatusColor = Brushes.Green;
                }

                panel.Children.Add(new TextBlock
                {
                    Text = cloudStatusText,
                    Foreground = cloudStatusColor,
                    FontWeight = cloudHasData ? FontWeights.Bold : FontWeights.Normal,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                // 本地数据状态
                string localStatusText;
                Brush localStatusColor;
                if (localHasMotorData)
                {
                    localStatusText = $"✓ 本地有 {localMotorCount} 个轴的电机测试数据";
                    localStatusColor = Brushes.Green;
                }
                else
                {
                    localStatusText = "⚠ 本地无电机测试数据，上传可能覆盖云端已有数据！";
                    localStatusColor = Brushes.Red;
                }

                panel.Children.Add(new TextBlock
                {
                    Text = localStatusText,
                    Foreground = localStatusColor,
                    FontWeight = !localHasMotorData ? FontWeights.Bold : FontWeights.Normal,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 16)
                });

                // 如果云端有数据且本地无数据，加强警告
                if (cloudHasData && !localHasMotorData)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "❌ 严重警告：本地无数据，继续上传将导致云端已有数据被空数据覆盖！",
                        Foreground = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 16)
                    });
                }

                panel.Children.Add(new TextBlock
                {
                    Text = "是否确认上传？",
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 12)
                });

                // 按钮区域
                var btnPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var cancelBtn = new Button
                {
                    Content = "取消",
                    Margin = new Thickness(0, 0, 8, 0),
                    MinWidth = 80
                };
                cancelBtn.Click += (s, e) =>
                {
                    DialogHost.CloseDialogCommand.Execute(false, null);
                };

                var confirmBtn = new Button
                {
                    Content = "确认上传",
                    MinWidth = 80
                };
                confirmBtn.Click += (s, e) =>
                {
                    DialogHost.CloseDialogCommand.Execute(true, null);
                };

                btnPanel.Children.Add(cancelBtn);
                btnPanel.Children.Add(confirmBtn);
                panel.Children.Add(btnPanel);

                var dialogResult = await DialogHost.Show(panel, "MultiAxisRunViewHost");
                tcs.TrySetResult(dialogResult is true);
            });

            return await tcs.Task;
        }

      

    }
}
