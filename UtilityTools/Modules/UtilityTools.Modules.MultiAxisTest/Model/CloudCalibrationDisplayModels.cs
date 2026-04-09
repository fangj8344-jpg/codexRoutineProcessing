using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace UtilityTools.Modules.MultiAxisTest.Model
{
    public class CloudKvDisplayItem : BindableBase
    {
        private string _label = string.Empty;
        private string _value = string.Empty;

        public string Label { get => _label; set => SetProperty(ref _label, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
    }

    public class CloudErrorPointDisplay : BindableBase
    {
        private string _targetUm = string.Empty;
        private string _actualUm = string.Empty;

        public string TargetUm { get => _targetUm; set => SetProperty(ref _targetUm, value); }
        public string ActualUm { get => _actualUm; set => SetProperty(ref _actualUm, value); }
    }

    public class CloudMotorAxisDisplay : BindableBase
    {
        private string _axisType = string.Empty;
        private string _forwardStd = string.Empty;
        private string _reverseStd = string.Empty;
        private string _minRange = string.Empty;
        private string _maxRange = string.Empty;
        private string _negLimit = string.Empty;
        private string _posLimit = string.Empty;
        private string _precisionStd = string.Empty;

        public string AxisType { get => _axisType; set => SetProperty(ref _axisType, value); }
        public string ForwardStd { get => _forwardStd; set => SetProperty(ref _forwardStd, value); }
        public string ReverseStd { get => _reverseStd; set => SetProperty(ref _reverseStd, value); }
        public string MinRange { get => _minRange; set => SetProperty(ref _minRange, value); }
        public string MaxRange { get => _maxRange; set => SetProperty(ref _maxRange, value); }
        public string NegLimit { get => _negLimit; set => SetProperty(ref _negLimit, value); }
        public string PosLimit { get => _posLimit; set => SetProperty(ref _posLimit, value); }
        public string PrecisionStd { get => _precisionStd; set => SetProperty(ref _precisionStd, value); }

        public ObservableCollection<CloudErrorPointDisplay> ErrorPoints { get; } = new();
    }
}