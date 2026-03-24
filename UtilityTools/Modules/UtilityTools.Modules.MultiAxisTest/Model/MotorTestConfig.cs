using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MultiAxisTest.Model
{
    public enum MotorAxisCount
    {
        TwoAxis,
        ThreeAxis,
        FiveAxis
    }

    public enum TwoAxisSeries
    {
        None,
        Zem18,
        Zem20
    }
    public class MotorTestConfig:BindableBase
    {
        private MotorAxisCount _axisCount;
        public MotorAxisCount AxisCount
        {
            get => _axisCount;
            set => SetProperty(ref _axisCount, value);
        }

        private TwoAxisSeries _twoAxisSeries = TwoAxisSeries.None;
        public TwoAxisSeries TwoAxisSeries
        {
            get => _twoAxisSeries;
            set => SetProperty(ref _twoAxisSeries, value);
        }
        /// <summary>
        /// 行程范围、速度范围等标准，后面可以继续加
        /// </summary>
        public int MinStroke { get; set; }
        public int MaxStroke { get; set; }



    }
}
