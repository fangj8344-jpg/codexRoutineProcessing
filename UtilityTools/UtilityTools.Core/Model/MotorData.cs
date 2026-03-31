using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{

    /// <summary>
    /// 第一层:最底层的定位误差表
    /// </summary>
    public class PositionError
    {
        [JsonPropertyName("目标位置")]
        public double TargetPosition { get; set; }

        [JsonPropertyName("实际位置")]
        public double ActualPosition { get; set; }

    }

    public class MotorData
    {
        [JsonPropertyName("轴类型")]
        public string AxisType { get; set; }

        [JsonPropertyName("正向速度标准差")]
        public double ForwardSpeedStdDev { get; set; }

        [JsonPropertyName("反向速度标准差")]
        public double BackwardSpeedStdDev { get; set; }

        [JsonPropertyName("最小量程")]
        public float MinRange { get; set; }

        [JsonPropertyName("最大量程")]
        public float MaxRange { get; set; }

        [JsonPropertyName("负向限位]")]
        public bool NegativeLimit { get; set; }

        [JsonPropertyName("正向限位")]
        public bool PositiveLimit { get; set; }

        [JsonPropertyName("定位精度标准差")]
        public double PositioningStdDev { get; set; }
        [JsonPropertyName("定位精度误差表")]
        public List<PositionError> PositionErrors { get; set; }

    }
    /// <summary>
    /// 第三层，最外层的样品台总报告
    /// </summary>
    public class SampleStageReport
    {

        [JsonPropertyName("样品台ID")]
        public string StageId { get; set; }

        /// <summary>
        /// ISO 8601格式
        /// </summary>
        [JsonPropertyName("开始测试时间")]
        public string StartTime { get; set; }

        [JsonPropertyName("结束测试时间")]
        public string EndTime { get; set; }

        [JsonPropertyName("电机列表")]
        public List<MotorData> Motors { get; set; }
    }

    public class UploadInformation
    {
        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }
        [JsonPropertyName("sample_stage_id")]
        public string SampleStageId { get; set; }
        [JsonPropertyName("content")]
        public SampleStageReport Content { get; set; }

    }
    
}
