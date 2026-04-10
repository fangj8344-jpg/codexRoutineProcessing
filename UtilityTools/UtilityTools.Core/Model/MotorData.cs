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
        //[JsonPropertyName("目标位置(脉冲)")]
        [JsonIgnore]
        public double TargetPosition { get; set; }

        //[JsonPropertyName("实际位置(脉冲)")]
        [JsonIgnore]
        public double ActualPosition { get; set; }

        [JsonPropertyName("目标位置(um)")]
        public double TargetPositionUm { get; set; }

        [JsonPropertyName("实际位置(um)")]
        public double ActualPositionUm { get; set; }


    }

    public class MotorData
    {
        [JsonPropertyName("轴类型")]
        public string AxisType { get; set; }

        [JsonPropertyName("正向速度标准差(um)")]
        public double ForwardSpeedStdDev { get; set; }

        [JsonPropertyName("反向速度标准差(um)")]
        public double BackwardSpeedStdDev { get; set; }

        [JsonPropertyName("最小量程(um)")]
        public float MinRange { get; set; }

        [JsonPropertyName("最大量程(um)")]
        public float MaxRange { get; set; }

        [JsonPropertyName("负向限位")]
        public bool NegativeLimit { get; set; }

        [JsonPropertyName("正向限位")]
        public bool PositiveLimit { get; set; }

        [JsonPropertyName("定位精度标准差(um)")]
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

        /// <summary>
        /// 样品台类型
        /// </summary>
        [JsonPropertyName("样品台类型")]
        public string StageType { get; set; }

        /// <summary>
        /// 采购信息
        /// </summary>
        [JsonPropertyName("采购订单")]
        public string PurchaseOrder { get; set; }


        /// <summary>
        /// 生产订单
        /// </summary>
        [JsonPropertyName("生产订单")]
        public string ProductionOrder { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        [JsonPropertyName("操作检验")]
        public string OperatorId { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        [JsonPropertyName("生产日期")]
        public string ProductionDate { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [JsonPropertyName("序列号")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 电镜型号
        /// </summary>
        [JsonPropertyName("电镜型号")]
        public string ElectronMicroscopeModel { get; set; }
    

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
