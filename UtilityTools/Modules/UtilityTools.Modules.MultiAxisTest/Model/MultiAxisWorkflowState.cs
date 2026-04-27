using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using UtilityTools.Core.Interface;
using UtilityTools.Core.Model;
using UtilityTools.Modules.MotorTest.Model;

namespace UtilityTools.Modules.MultiAxisTest.Model
{
    public enum MultiAxisMotorKind
    {
        TwoAxisZem18,
        TwoAxisZem20,
    }
    public class ScanDisplayModel:BindableBase
    {
        /// <summary>
        /// 样品台类型
        /// </summary>
        public string _stageType;
        public string StageType
        {
            get => _stageType;
            set => SetProperty(ref _stageType, value);
        }
        /// <summary>
        /// 采购信息
        /// </summary>
        private string _purchaseOrder;
        public string PurchaseOrder
        {
            get => _purchaseOrder;
            set => SetProperty(ref _purchaseOrder, value);
        }

        /// <summary>
        /// 生产订单
        /// </summary>
        private string _productionOrder;
        public string ProductionOrder
        {
            get => _productionOrder;
            set => SetProperty(ref _productionOrder, value);
        }

        /// <summary>
        /// 物料编码（新二维码格式第 3 段）
        /// </summary>
        private string _materialCode;
        public string MaterialCode
        {
            get => _materialCode;
            set => SetProperty(ref _materialCode, value);
        }

        /// <summary>
        /// 操作者
        /// </summary>
        private string _operatorId;
        public string OperatorId
        {
            get => _operatorId;
            set => SetProperty(ref _operatorId, value);
        }
        /// <summary>
        /// 生产日期
        /// </summary>
        private string _productionDate;
        public string ProductionDate
        {
            get => _productionDate;
            set => SetProperty(ref _productionDate, value);
        }
        /// <summary>
        /// 序列号
        /// </summary>
        private string _serialNumber;
        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        private string _electronMicroscopeModel;
        /// <summary>
        /// 
        /// </summary>
        public string ElectronMicroscopeModel
        {
            get => _electronMicroscopeModel;
            set => SetProperty(ref _electronMicroscopeModel, value);
        }
    }

   


    /// <summary>
    /// 多轴测试流程的共享状态（配置 + 扫码结果）
    /// </summary>
    public class MultiAxisWorkflowState : BindableBase, ITestReportService, ITestStageTypeProvider, IFirmwareUpgradeWorkflowState
    {
        private const string DefaultFirmwareIndexUrl = @"http://60.173.16.125:9090/安装文件/st工具/电机固件/";

        private MachineProfile _motorKind = MachineProfile.StandardTwoAxis;
        public MachineProfile MotorKind
        {
            get => _motorKind;
            set => SetProperty(ref _motorKind, value);
        }

        private string _currentScanText = string.Empty;
        public string CurrentScanText
        {
            get => _currentScanText;
            set => SetProperty(ref _currentScanText, value);
        }

        private ScanDisplayModel _currentScanDisplay;
        public ScanDisplayModel CurrentScanDisplay
        {
            get => _currentScanDisplay;
            set => SetProperty(ref _currentScanDisplay, value);
        }

        private UploadInformation _uploadInformation;
        /// <summary>
        /// 最后上传的信息
        /// </summary>
        public UploadInformation UploadInformation
        {
            get => _uploadInformation;
            set => SetProperty(ref _uploadInformation, value);
        }
        private object _motorKindObj;
        public object MotorKindObj 
        { 
            get => _motorKindObj; 
            set =>  _motorKindObj = value; }

        /// <inheritdoc />
        public bool DevShortcutXyOnlyAxes { get; set; }

        public MultiAxisWorkflowState()
        {
            ResetNewTestCycle();
        }

        public void ResetNewTestCycle()
        {
            DevShortcutXyOnlyAxes = false;
            CurrentScanDisplay = new ScanDisplayModel();
            ResetFirmwareCheckState();

            UploadInformation = new UploadInformation()
            {
                Content = new SampleStageReport() 
                {
                    Motors = new List<MotorData>(),

                },
               
            };
        }

        private bool _firmwareCheckCompleted;
        public bool FirmwareCheckCompleted
        {
            get => _firmwareCheckCompleted;
            set => SetProperty(ref _firmwareCheckCompleted, value);
        }

        private bool _firmwareUpgradeRequired;
        public bool FirmwareUpgradeRequired
        {
            get => _firmwareUpgradeRequired;
            set => SetProperty(ref _firmwareUpgradeRequired, value);
        }

        private bool _firmwareUpgradeSkipped;
        public bool FirmwareUpgradeSkipped
        {
            get => _firmwareUpgradeSkipped;
            set => SetProperty(ref _firmwareUpgradeSkipped, value);
        }

        private bool _firmwareUpgradeInProgress;
        public bool FirmwareUpgradeInProgress
        {
            get => _firmwareUpgradeInProgress;
            set => SetProperty(ref _firmwareUpgradeInProgress, value);
        }

        private string _firmwareCheckMessage = "未执行固件检查";
        public string FirmwareCheckMessage
        {
            get => _firmwareCheckMessage;
            set => SetProperty(ref _firmwareCheckMessage, value);
        }

        private string _currentFirmwareVersion = string.Empty;
        public string CurrentFirmwareVersion
        {
            get => _currentFirmwareVersion;
            set => SetProperty(ref _currentFirmwareVersion, value);
        }

        private string _latestFirmwareVersion = string.Empty;
        public string LatestFirmwareVersion
        {
            get => _latestFirmwareVersion;
            set => SetProperty(ref _latestFirmwareVersion, value);
        }

        private string _latestFirmwareUrl = string.Empty;
        public string LatestFirmwareUrl
        {
            get => _latestFirmwareUrl;
            set => SetProperty(ref _latestFirmwareUrl, value);
        }

        private string _firmwareIndexUrl = DefaultFirmwareIndexUrl;
        public string FirmwareIndexUrl
        {
            get => _firmwareIndexUrl;
            set => SetProperty(ref _firmwareIndexUrl, value);
        }

        public void ResetFirmwareCheckState()
        {
            FirmwareCheckCompleted = false;
            FirmwareUpgradeRequired = false;
            FirmwareUpgradeSkipped = false;
            FirmwareUpgradeInProgress = false;
            FirmwareCheckMessage = "未执行固件检查";
            CurrentFirmwareVersion = string.Empty;
            LatestFirmwareVersion = string.Empty;
            LatestFirmwareUrl = string.Empty;
            FirmwareIndexUrl = string.IsNullOrWhiteSpace(FirmwareIndexUrl) ? DefaultFirmwareIndexUrl : FirmwareIndexUrl;
        }

        public void AddOrUpdateMotorData(MotorData data)
        {
            // 加上 lock 防止多线程跑电机时把列表挤崩溃
            lock (UploadInformation.Content.Motors)
            {
                var existing = UploadInformation.Content.Motors.FirstOrDefault(m => m.AxisType == data.AxisType);
                if (existing != null)
                {
                    UploadInformation.Content.Motors.Remove(existing);
                }
                UploadInformation.Content.Motors.Add(data);
            }
        }

        public void InitReport()
        {
            // 如果 Content 为空才去 new（防呆设计）
            if (UploadInformation.Content == null)
            {
                UploadInformation.Content = new SampleStageReport();
            }

            // 💡 正确做法：只清空电机列表，保留扫码得到的基础信息
            if (UploadInformation.Content.Motors != null)
            {
                UploadInformation.Content.Motors.Clear();
            }
            else
            {
                UploadInformation.Content.Motors = new List<MotorData>();
            }

            // 这一步依然保留，确保 ID 一致
            UploadInformation.Content.StageId = UploadInformation.SampleStageId;
        }

        public void CompleteReport()
        {
            UploadInformation.Content.EndTime = DateTime.Now.ToString("o");
        }
        /// <summary>
        /// 2. 拿表
        /// </summary>
        public MotorData GetOrCreateMotorData(string axisType)
        {
            lock (UploadInformation.Content.Motors)
            {
                // 找找看有没有
                var existing = UploadInformation.Content.Motors.FirstOrDefault(m => m.AxisType == axisType);
                if (existing != null) return existing;

                // 没有就新建一张
                var newMotor = new MotorData
                {
                    AxisType = axisType,
                    PositionErrors = new List<PositionError>()
                };
                UploadInformation.Content.Motors.Add(newMotor);

                return newMotor;
            }
        }
        /// <summary>
        /// 4. 拿最终数据：给 Thingsboard 上传用
        /// </summary>
        public UploadInformation GetFinalReport()
        {
            return UploadInformation;
        }

        public void StartReport()
        {
            UploadInformation.Content.StartTime = DateTime.Now.ToString("o"); // ISO 8601格式
        }

        public string GetCurrentStageType()
        {
            return CurrentScanDisplay?.StageType ?? string.Empty;
        }
    }
}
