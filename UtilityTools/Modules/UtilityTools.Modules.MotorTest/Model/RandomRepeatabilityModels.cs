using Prism.Mvvm;
using System;

namespace UtilityTools.Modules.MotorTest.Model
{
    /// <summary>
    /// 随机重复精度测试：生成点位列表。
    /// </summary>
    public class RandomTargetPointRecord : BindableBase
    {
        public int Index { get; set; }
        public double TargetXUm { get; set; }
        public double TargetYUm { get; set; }
        public int TargetXPulse { get; set; }
        public int TargetYPulse { get; set; }
    }

    /// <summary>
    /// 随机重复精度测试：单轴单次行程记录（X、Y 各一行，同一时间片并行到位）。
    /// </summary>
    public class RandomMoveTripAxisRecord : BindableBase
    {
        public int SeqNo { get; set; }
        public int PointIndex { get; set; }
        public double PlannedStartUm { get; set; }
        public double ActualStartUm { get; set; }
        public double TargetUm { get; set; }
        public double ActualTargetUm { get; set; }
        public double MoveTimeMs { get; set; }
        public double DistanceUm { get; set; }
        public double AvgSpeedUmPerSec { get; set; }
    }

    /// <summary>
    /// 单轴重复点统计（到位坐标分布）。
    /// </summary>
    public class RandomPointStatAxisRecord : BindableBase
    {
        public int PointIndex { get; set; }
        public int SampleCount { get; set; }
        public double MeanUm { get; set; }
        public double StdUm { get; set; }
        /// <summary>
        /// 目标位置（微米）
        /// </summary>
        public double TargetUm { get; set; }
    }

    /// <summary>
    /// 随机重复精度测试：距离/速度的直方图区间记录。
    /// </summary>
    public class HistogramBinRecord : BindableBase
    {
        public int BinIndex { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int Count { get; set; }
    }
}
