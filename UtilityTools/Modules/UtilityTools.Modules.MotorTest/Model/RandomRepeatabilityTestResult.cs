using System; using System.Collections.Generic; using System.Collections.ObjectModel; using System.Linq; using System.Text; using System.Threading.Tasks; using UtilityTools.Modules.MotorTest.Interface;

namespace UtilityTools.Modules.MotorTest.Model
{
    /// <summary>
    /// 随机坐标重复精度测试结果
    /// 
    /// 该类封装了随机坐标重复精度测试的所有结果数据，包括：
    /// 1. 目标点信息（位置坐标、脉冲值）
    /// 2. 每次跳转的详细记录（起点、终点、耗时、距离、速度）
    /// 3. 每个目标点的重复精度统计（平均值、标准差、样本数）
    /// 4. 距离和速度的分布直方图
    /// 5. 测试摘要数据（平均标准差、平均移动时间、行程范围）
    /// 
    /// 所有集合属性使用 ObservableCollection 以支持 WPF UI 的数据绑定
    /// </summary>
    public class RandomRepeatabilityTestResult : MotorTestResult
    {
        /// <summary>
        /// 目标点列表
        /// 
        /// 包含所有生成的随机目标点，每个点包含：
        /// - Index: 点的序号
        /// - TargetXUm/YUm: 目标位置（微米）
        /// - TargetXPulse/YPulse: 目标脉冲值
        /// </summary>
        public ObservableCollection<RandomTargetPointRecord> TargetPoints { get; } = new ObservableCollection<RandomTargetPointRecord>();

        /// <summary>
        /// X轴行程记录
        /// 
        /// 记录每次X轴跳转的详细信息，包括：
        /// - SeqNo: 跳转序号
        /// - PointIndex: 目标点序号
        /// - PlannedStartUm: 计划起点位置（微米）
        /// - ActualStartUm: 实际起点位置（微米）
        /// - TargetUm: 目标位置（微米）
        /// - ActualTargetUm: 实际到达位置（微米）
        /// - MoveTimeMs: 移动耗时（毫秒）
        /// - DistanceUm: 移动距离（微米）
        /// - AvgSpeedUmPerSec: 平均速度（微米/秒）
        /// </summary>
        public ObservableCollection<RandomMoveTripAxisRecord> MoveTripsX { get; } = new ObservableCollection<RandomMoveTripAxisRecord>();

        /// <summary>
        /// Y轴行程记录
        /// 
        /// 记录每次Y轴跳转的详细信息，结构同X轴
        /// </summary>
        public ObservableCollection<RandomMoveTripAxisRecord> MoveTripsY { get; } = new ObservableCollection<RandomMoveTripAxisRecord>();

        /// <summary>
        /// X轴统计数据
        /// 
        /// 对每个目标点的多次到达位置进行统计分析，包括：
        /// - PointIndex: 目标点序号
        /// - SampleCount: 样本数量（该点的重复次数）
        /// - MeanUm: 实际位置的平均值（微米）
        /// - StdUm: 实际位置的标准差（微米），反映重复精度
        /// </summary>
        public ObservableCollection<RandomPointStatAxisRecord> PointStatsX { get; } = new ObservableCollection<RandomPointStatAxisRecord>();

        /// <summary>
        /// Y轴统计数据
        /// 
        /// 对每个目标点的多次到达位置进行统计分析，结构同X轴
        /// </summary>
        public ObservableCollection<RandomPointStatAxisRecord> PointStatsY { get; } = new ObservableCollection<RandomPointStatAxisRecord>();

        /// <summary>
        /// X轴距离直方图
        /// 
        /// 将所有X轴跳转距离按区间分组统计，用于分析距离分布规律
        /// 每个bin包含：
        /// - BinIndex: 区间序号
        /// - MinValue: 区间最小值
        /// - MaxValue: 区间最大值
        /// - Count: 落在该区间的跳转次数
        /// </summary>
        public ObservableCollection<HistogramBinRecord> DistanceHistogramX { get; } = new ObservableCollection<HistogramBinRecord>();

        /// <summary>
        /// Y轴距离直方图
        /// 
        /// 将所有Y轴跳转距离按区间分组统计，结构同X轴
        /// </summary>
        public ObservableCollection<HistogramBinRecord> DistanceHistogramY { get; } = new ObservableCollection<HistogramBinRecord>();

        /// <summary>
        /// X轴速度直方图
        /// 
        /// 将所有X轴跳转的平均速度按区间分组统计，用于分析速度分布规律
        /// </summary>
        public ObservableCollection<HistogramBinRecord> SpeedHistogramX { get; } = new ObservableCollection<HistogramBinRecord>();

        /// <summary>
        /// Y轴速度直方图
        /// 
        /// 将所有Y轴跳转的平均速度按区间分组统计，结构同X轴
        /// </summary>
        public ObservableCollection<HistogramBinRecord> SpeedHistogramY { get; } = new ObservableCollection<HistogramBinRecord>();

        /// <summary>
        /// X轴平均标准差
        /// 
        /// 所有目标点X轴标准差的平均值
        /// 该值越小，表示X轴的重复精度越高
        /// 单位：微米
        /// </summary>
        public double AvgStdX { get; set; }

        /// <summary>
        /// Y轴平均标准差
        /// 
        /// 所有目标点Y轴标准差的平均值
        /// 该值越小，表示Y轴的重复精度越高
        /// 单位：微米
        /// </summary>
        public double AvgStdY { get; set; }

        /// <summary>
        /// 平均移动时间（毫秒）
        /// 
        /// 所有跳转（X轴和Y轴）的平均耗时
        /// 用于评估电机的响应速度和运动性能
        /// 单位：毫秒
        /// </summary>
        public double AvgMoveTimeMs { get; set; }

        /// <summary>
        /// X轴行程范围（微米）
        /// 
        /// X轴实际可移动的有效行程范围
        /// 用于计算随机点的分布密度
        /// 单位：微米
        /// </summary>
        public double XTravelUm { get; set; }

        /// <summary>
        /// Y轴行程范围（微米）
        /// 
        /// Y轴实际可移动的有效行程范围
        /// 用于计算随机点的分布密度
        /// 单位：微米
        /// </summary>
        public double YTravelUm { get; set; }

        /// <summary>
        /// 测试是否完成
        /// 
        /// true: 测试正常完成，所有数据有效
        /// false: 测试未完成或失败，需要查看 ErrorDescription 了解原因
        /// </summary>
        public bool IsTestCompleted { get; set; }
    }
}
