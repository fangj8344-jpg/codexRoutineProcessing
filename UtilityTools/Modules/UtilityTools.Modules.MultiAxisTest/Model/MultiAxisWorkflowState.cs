using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace UtilityTools.Modules.MultiAxisTest.Model
{
    public enum MultiAxisMotorKind
    {
        TwoAxisZem18,
        TwoAxisZem20,
    }
    public class ScanDisplayModel
    {
        /// <summary>
        /// 样品台类型
        /// </summary>
        public string StageType { get; set; }
        /// <summary>
        /// 采购信息
        /// </summary>
        public string PurchaseOrder { get; set; }

        /// <summary>
        /// 生产订单
        /// </summary>
        public string ProductionOrder { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        public string OperatorId { get; set; }
        //生产日期
        public string ProductionDate { get; set; }
        //序列号
        public string SerialNumber { get; set; }
    }



    /// <summary>
    /// 多轴测试流程的共享状态（配置 + 扫码结果）
    /// </summary>
    public class MultiAxisWorkflowState : BindableBase
    {
        private MultiAxisMotorKind _motorKind = MultiAxisMotorKind.TwoAxisZem20;
        public MultiAxisMotorKind MotorKind
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
        
    }
}
