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

    public sealed class ScanRecord
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Text { get; set; } = string.Empty;
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

        public ObservableCollection<ScanRecord> ScanHistory { get; } = new ObservableCollection<ScanRecord>();
    }
}
