using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UtilityTools.Core;
using UtilityTools.Core.Event;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MultiAxisTest.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MultiAxisTest.ViewModels
{
    public class MultiAxisScanViewModel : RegionViewModelBase, INavigationAware, IRegionMemberLifetime
    {
        private SubscriptionToken _token;

        private readonly IRegionManager _regionManager;
        
        private readonly IContainerProvider _containerProvider;

        private IEventAggregator _eventAggregator;
        public MultiAxisWorkflowState State { get; }
       
        public MultiAxisScanViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, MultiAxisWorkflowState state, IContainerProvider containerProvider )
            : base(containerProvider)
        {
            _regionManager = regionManager;
            State = state;
            _eventAggregator = eventAggregator;
            ConfirmCommand = new DelegateCommand(Confirm, CanConfirm);
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
            

        }

      
        public DelegateCommand ConfirmCommand { get; private set; }

        private bool CanConfirm()
        {
            // 必须有扫码文本，且生产日期和序号都没问题
            return !string.IsNullOrWhiteSpace(State.CurrentScanText)
                   && State.CurrentScanDisplay != null
                   && !string.IsNullOrWhiteSpace(State.CurrentScanDisplay.SerialNumber);
        }
        /*
        private void OnBarcodeReceived(string[] barcodeParts)
        {

           
            for (int i = 0; i < barcodeParts.Length; i++)
            {
                barcodeParts[i] = barcodeParts[i]?.Trim() ?? string.Empty;
            }
            State.CurrentScanText = string.Join("/", barcodeParts);
            // 3. 拿到最后一段最关键的“日期序列号”
            string lastPart = barcodeParts[3];

            State.CurrentScanDisplay = new ScanDisplayModel
            {
                PurchaseOrder = barcodeParts[0],
                ProductionOrder = barcodeParts[1],
                OperatorId = barcodeParts[2],

                // 日期固定取前 6 位，取不到 6 位有多少取多少
                ProductionDate = lastPart.Length >= 6 ? lastPart.Substring(0, 6) : lastPart,

                // 序列号取 6 位之后的所有内容
                SerialNumber = lastPart.Length > 6 ? lastPart.Substring(6) : string.Empty
            };

            // 4. 严谨校验
            bool isDateOk = State.CurrentScanDisplay.ProductionDate.Length == 6;
            bool isSnOk = State.CurrentScanDisplay.SerialNumber.Length >= 3;

            if (!isDateOk || !isSnOk)
            {
                ScanErrorNotice();
                // 建议在这里打个日志，看看扫出来到底是什么鬼
                // _logger.Warn($"扫码格式不对：日期长度{isDateOk}, 序号长度{isSnOk}, 原始文本: {State.CurrentScanText}");
            }

            // 5. 赋值上传信息
            State.UploadInformation.SampleStageId = State.CurrentScanText;
            State.UploadInformation.Content.StageId = State.CurrentScanText;

            ConfirmCommand.RaiseCanExecuteChanged();
        }
        */

        /// <summary>
        /// 扫码枪扫码触发事件
        /// </summary>
        /// <param name="barcodeParts"></param>
        private void OnBarcodeReceived(string[] barcodeParts)
        {
            for (int i = 0; i < barcodeParts.Length; i++)
            {
                barcodeParts[i] = barcodeParts[i]?.Trim() ?? string.Empty;
            }
            State.CurrentScanText = string.Join("/", barcodeParts);
            string lastPart = barcodeParts[barcodeParts.Length - 1];
            State.CurrentScanDisplay = new ScanDisplayModel
            {
                // 前三个固定不变
                PurchaseOrder = barcodeParts[0],
                ProductionOrder = barcodeParts[1],
                OperatorId = barcodeParts[2],

                // 日期固定取前 6 位，取不到 6 位有多少取多少
                ProductionDate = lastPart.Length >= 6 ? lastPart.Substring(0, 6) : lastPart,

                // 序列号取 6 位之后的所有内容
                SerialNumber = lastPart.Length > 6 ? lastPart.Substring(6) : string.Empty
            };
            if (barcodeParts.Length >= 6)
            {
                State.CurrentScanDisplay.ElectronMicroscopeModel = barcodeParts[3]; // 比如 ZEM20
                string stageType = barcodeParts[4];                                 // 比如 SampleMini
                State.CurrentScanDisplay.StageType = stageType;

                // 如果你的 State.UploadInformation 里也有这两个字段，也可以在这里一并赋值：
                // State.UploadInformation.ElectronMicroscopeModel = barcodeParts[3];
                // State.UploadInformation.StageType = barcodeParts[4];
                // 🌟 核心映射：识别样品台，装箱存入全局状态
                string normalizedStage = stageType.ToUpper();
                switch (normalizedStage)
                {
                    case "SAMPLEMINI":
                        State.MotorKindObj = MachineProfile.CompactTwoAxis;
                        break;
                    case "SAMPLESTANDARD":
                        State.MotorKindObj = MachineProfile.StandardTwoAxis;
                        break;
                    case "SAMPLEPRO":
                        State.MotorKindObj = MachineProfile.HeavyDutyThreeAxis;
                        break;
                    case "SAMPLEULTRA":
                        State.MotorKindObj = MachineProfile.UniversalFiveAxis;
                        break;
                    default:
                        // 碰到不认识的新型号兜底
                        State.MotorKindObj = MachineProfile.StandardTwoAxis;
                        break;
                }
            }
            else
            {
                // 如果是旧码（只有4段），赋个空值或者默认值，防止空引用
                State.CurrentScanDisplay.ElectronMicroscopeModel = string.Empty;
                State.CurrentScanDisplay.StageType = string.Empty;
            }
            bool isDateOk = State.CurrentScanDisplay.ProductionDate.Length == 6;
            bool isSnOk = State.CurrentScanDisplay.SerialNumber.Length >= 3;

            if (!isDateOk || !isSnOk)
            {
                ScanErrorNotice();
                return; // 校验失败直接中断，保护后续逻辑
            }
            if (State.UploadInformation != null)
            {
                State.UploadInformation.SampleStageId = State.CurrentScanText;
                if (State.UploadInformation.Content != null)
                {
                    State.UploadInformation.Content.StageId = State.CurrentScanText;
                }
            }
            ConfirmCommand.RaiseCanExecuteChanged();
        }
        private void ScanErrorNotice() 
        {
            App.Current.Dispatcher.Invoke(async () =>
            {
                var errorContent = new StackPanel
                {
                    Margin = new System.Windows.Thickness(16)
                };
                errorContent.Children.Add(new TextBlock
                {
                    Text = "扫码解析失败",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red
                });
                errorContent.Children.Add(new TextBlock
                {
                    Text = $"请检查输入是否是英文",
                    Margin = new Thickness(0, 10, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
                errorContent.Children.Add(new Button
                {
                    Content = "确定",
                    Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand
                });
                await MaterialDesignThemes.Wpf.DialogHost.Show(errorContent, "MultiAxisScanViewHost");
            });
        }

        private void Confirm()
        {
            State.InitReport();
            _regionManager.Regions[RegionNames.ContentRegion].RequestNavigate(nameof(Views.MultiAxisRunView));

        }

         
        public bool KeepAlive => true;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 将当前输入法的语言强制切换为美式英文
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
            _token = _eventAggregator.GetEvent<BarcodeScannedEvent>().Subscribe(OnBarcodeReceived);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (_token != null)
            {
                _eventAggregator.GetEvent<BarcodeScannedEvent>().Unsubscribe(_token);

                _token = null;
            }
           
        }
    }
}
