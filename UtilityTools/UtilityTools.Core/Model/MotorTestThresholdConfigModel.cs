using System.Collections.Generic;

namespace UtilityTools.Core.Model
{
    public class AxisPulseRange
    {
        public int MinPulse { get; set; }
        public int MaxPulse { get; set; }
    }

    /// <summary>
    /// 电机基础测试标准阈值配置（可通过 JSON 读写）/// </summary>
    public class MotorTestThresholdConfigModel
    {
        /// <summary>移动测试最小位移阈值（脉冲）。</summary>
        public int MovementMinDistancePulse { get; set; } = 50;

        /// <summary>编码器测试最小变化阈值（脉冲）。</summary>
        public int EncoderMinDeltaPulse { get; set; } = 100;

        /// <summary>限位精度测试最大允许偏差（脉冲）。</summary>
        public int LimitAccuracyMaxDiffPulse { get; set; } = 200;

        /// <summary>分段线性测试标准差阈值（um）。</summary>
        public double LinearStdDevMaxUm { get; set; } = 1.0;

        /// <summary>丝杆顺滑度标准差阈值（um）。</summary>
        public double SmoothnessStdDevMaxUm { get; set; } = 150.0;

        /// <summary>满行程测试最小脉冲阈值（统一用于该型号下各轴）。</summary>
        public int FullTravelMinPulse { get; set; } = 215000;

        /// <summary>满行程测试最大脉冲阈值（统一用于该型号下各轴）。</summary>
        public int FullTravelMaxPulse { get; set; } = 255000;

        /// <summary>
        /// 按轴配置的满行程阈值（Key: X/Y/Z/T/R）。
        /// 两轴机默认至少应配置 X、Y。
        /// </summary>
        public Dictionary<string, AxisPulseRange> FullTravelAxisRanges { get; set; } = new()
        {
            ["X"] = new AxisPulseRange { MinPulse = 235000, MaxPulse = 255000 },
            ["Y"] = new AxisPulseRange { MinPulse = 215000, MaxPulse = 225000 }
        };
    }
}
