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
    }



    /// <summary>
    /// 多轴测试流程的共享状态（配置 + 扫码结果）
    /// </summary>
    public class MultiAxisWorkflowState : BindableBase, ITestReportService
    {

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


        public MultiAxisWorkflowState()
        {
            ResetNewTestCycle();
        }

        public void ResetNewTestCycle()
        {
            CurrentScanDisplay = new ScanDisplayModel();

            UploadInformation = new UploadInformation()
            {
                Content = new SampleStageReport() 
                {
                    Motors = new List<MotorData>(),

                },
               
            };
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

            UploadInformation.Content = new SampleStageReport()
            {
                Motors = new List<MotorData>(),
            };
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
    }
}
