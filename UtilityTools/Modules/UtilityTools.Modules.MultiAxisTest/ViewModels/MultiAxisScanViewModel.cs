using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UtilityTools.Core;
using UtilityTools.Core.Event;
using UtilityTools.Core.Mvvm;
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
        private MultiAxisWorkflowState _state;
        public MultiAxisWorkflowState State
        {
            get => _state;
            set { SetProperty(ref _state, value); }   
        }
        public string CurrentScanText
        {
            get => _state.CurrentScanText;
            set
            {
                if (_state.CurrentScanText != value)
                {
                    _state.CurrentScanText = value;
                    RaisePropertyChanged();
                }
            }

        }




        public MultiAxisScanViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, MultiAxisWorkflowState state, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _regionManager = regionManager;
            _state = state;

            _eventAggregator = eventAggregator;
           
            _containerProvider = containerProvider;
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");

        }

      
        public DelegateCommand ConfirmCommand => new DelegateCommand(Confirm);
        private void OnBarcodeReceived(string[] barcodeParts)
        {

            CurrentScanText = string.Join("/", barcodeParts);
            _state.CurrentScanDisplay = new ScanDisplayModel
            {

                PurchaseOrder = barcodeParts.Length > 0 ? barcodeParts[0] : string.Empty,

                ProductionOrder = barcodeParts.Length > 1 ? barcodeParts[1] : string.Empty,

                OperatorId = barcodeParts.Length > 2 ? barcodeParts[2] : string.Empty,

                ProductionDate = barcodeParts.Length > 3 ? barcodeParts[3].Substring(0, Math.Min(barcodeParts[3].Length, 6)) : string.Empty,

                SerialNumber = (barcodeParts.Length > 3 && barcodeParts[3].Length > 6) ? barcodeParts[3].Substring(6) : string.Empty
            };
            if (_state.CurrentScanDisplay.ProductionDate.Length != 6 || _state.CurrentScanDisplay.SerialNumber.Length <  3)
            {
                ScanErrorNotice();
            }

            _state.UploadInformation.SampleStageId = CurrentScanText;
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
