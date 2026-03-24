using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly MultiAxisWorkflowState _state;
        private readonly IContainerProvider _containerProvider;

        private IEventAggregator _eventAggregator;
        private IThingboardService _thingboardService;

        private string _accessToken;
        /// <summary>
        /// 样品台Token
        /// </summary>
        public string AccessToken
        {
            get { return _accessToken; }
            set { _accessToken = value; RaisePropertyChanged(); }
        }

        private bool _httpTestResult = false;
        public bool HttpTestResult
        {
            get { return _httpTestResult; }
            set { _httpTestResult = value; RaisePropertyChanged(); }
        }

        public DelegateCommand UploadDataCommand => new DelegateCommand(async () => await UploadData());

        public MultiAxisScanViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, MultiAxisWorkflowState state, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _regionManager = regionManager;
            _state = state;

            _eventAggregator = eventAggregator;
            _token = _eventAggregator.GetEvent<BarcodeScannedEvent>().Subscribe(OnBarcodeReceived);
            _containerProvider = containerProvider;
            _thingboardService = _containerProvider.Resolve<IServiceFactory>().GetThingboardService();
        }

        public async Task UploadData()
        {
            _thingboardService.ServerUrl = "https://iot.zeptools.cn";
            _thingboardService.EnableMqtt = false;
            _thingboardService.EnableHttp = true;

            var data = GenerateData();

            HttpTestResult = await _thingboardService.UploadTelemetryAsync(data);
        }

        private string GenerateData()
        {
            var report = new SampleStageReport
            {
                StageId = CurrentScanText,
                StartTime = "2026年3月17日8:20:12",
                EndTime = "2026年3月17日8:20:12",
                Motors = new List<MotorData>
                {
                    new MotorData
                    {
                        AxisType = "X",
                        ForwardSpeedStdDev = 0.26,
                        BackwardSpeedStdDev = 0.56,
                        MinRange = "-32000um",
                        MaxRange = "41000um",
                        NegativeLimit = true,
                        PositiveLimit = true,
                        PositioningStdDev = 0.68,
                        PositionErrors = new List<PositionError>
                        {
                            new PositionError { TargetPosition = 2500, ActualPosition = 2631 },
                            new PositionError { TargetPosition = 2500, ActualPosition = 2631 }
                        }
                    }
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(report, options);
        }

        private void OnBarcodeReceived(string[] barcodeParts)
        {
            CurrentScanText = barcodeParts[3];
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

        public DelegateCommand ConfirmCommand => new DelegateCommand(Confirm);

        private void Confirm()
        {
            _regionManager.Regions[RegionNames.ContentRegion].RequestNavigate(nameof(Views.MultiAxisRunView));
        }

        public bool KeepAlive => false;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
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
