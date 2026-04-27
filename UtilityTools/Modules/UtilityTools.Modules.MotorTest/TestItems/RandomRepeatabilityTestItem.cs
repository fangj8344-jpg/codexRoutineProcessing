using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.TestItems
{
    /// <summary>
    /// 随机坐标重复精度测试（单轴版本）
    /// 
    /// 该测试用于评估电机在随机位置跳转时的重复精度，主要步骤包括：
    /// 1. 满行程检测：检测轴的有效行程范围
    /// 2. 随机点生成：以轴中心为原点，按高斯分布生成随机目标点
    /// 3. 随机跳转测试：在随机点之间进行跳转，每点重复指定次数
    /// 4. 数据统计：计算每个目标点的重复精度（标准差）
    /// 5. 直方图生成：生成距离和速度的分布直方图
    /// 
    /// 测试特点：
    /// - 单轴测试（<see cref="ExecuteAsync"/>，符合 IMotorTestItem）；X/Y 并行由上层通过两次 <see cref="ExecuteAsync"/> 调度
    /// - 支持失败降级处理
    /// - 使用两段判停机制（离开停止状态 → 回到停止状态）
    /// - 超时后自动跳过故障点
    /// </summary>
    public class RandomRepeatabilityTestItem : IMotorTestItem
    {
        private const double FixedPointSpacingUm = 5000.0; // 5mm
        private const int FixedRepeatsPerPoint = 40;
        private static readonly object FlowLogFileLock = new();
        /// <summary>
        /// 测试名称
        /// </summary>
        public string TestName => "随机坐标重复精度测试";

        /// <summary>
        /// 点间距（微米）
        /// 
        /// 用于计算随机点的分布密度和数量
        /// 默认值：5000 微米（5mm）
        /// </summary>
        private readonly double _spacingUm;

        /// <summary>
        /// 每点重复次数
        /// 
        /// 每个随机目标点需要跳转的次数
        /// 用于计算重复精度
        /// 默认值：10 次
        /// </summary>
        private readonly int _repeatsPerPoint;

        /// <summary>
        /// 直方图 bin 数
        /// 
        /// 将数据分组的区间数量
        /// 用于生成距离和速度的分布直方图
        /// 默认值：10 个
        /// </summary>
        private readonly int _histogramBins;
        private readonly Action<int, int, TimeSpan, TimeSpan>? _progressReporter;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="spacingUm">点间距（微米）</param>
        /// <param name="repeatsPerPoint">每点重复次数</param>
        /// <param name="histogramBins">直方图 bin 数</param>
        public RandomRepeatabilityTestItem(
            double spacingUm = 5000,  // 5mm（兼容参数，执行时固定按 5mm 计算点数）
            int repeatsPerPoint = 40, // 默认每点40次
            int histogramBins = 10,
            Action<int, int, TimeSpan, TimeSpan>? progressReporter = null)
        {
            _spacingUm = spacingUm;
            _repeatsPerPoint = repeatsPerPoint;
            _histogramBins = histogramBins;
            _progressReporter = progressReporter;
        }

        /// <summary>
        /// 执行测试（单轴）
        /// 
        /// 测试流程：
        /// 1. 参数验证：检查电机换算系数是否有效
        /// 2. 满行程检测：检测轴的有效行程范围，支持失败降级
        /// 3. 随机点生成：根据行程范围生成高斯分布的随机目标点
        /// 4. 电机准备：设置轴为闭环位置控制、使能、运行状态
        /// 5. 随机跳转测试：在随机点之间进行跳转，记录详细数据
        /// 6. 数据统计：计算每个点的重复精度统计
        /// 7. 直方图生成：生成距离和速度的分布直方图
        /// 8. 结果汇总：计算平均标准差、平均移动时间等摘要数据
        /// 
        /// 异常处理：
        /// - OperationCanceledException：向上传播，表示测试被取消
        /// - 其他异常：捕获并设置错误描述，返回未完成的结果
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="motorModel">电机模型</param>
        /// <param name="motorEntity">电机控制实体</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>测试结果</returns>
        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var result = new RandomRepeatabilityTestResult { IsPassed = false };
            string flowLogPath = CreateFlowLogPath(motorId);
            WriteFlowLog(flowLogPath, $"[{motorId}] 流程开始：designSpacingUm={FixedPointSpacingUm:F1}, repeatsPerPoint={FixedRepeatsPerPoint}, histogramBins={_histogramBins}, 当前PosUm={motorModel.MotorParams.PosUm:F3}");

            // 验证参数
            if (motorModel.MotorParams.SubRatio <= 0)
            {
                result.ErrorDescription = "电机换算系数异常（SubRatio<=0）";
                WriteFlowLog(flowLogPath, $"[{motorId}] 参数异常：SubRatio={motorModel.MotorParams.SubRatio}");
                return result;
            }

            try
            {
                var overallSw = Stopwatch.StartNew();
                // 1. 满行程检测
                double minUm, maxUm;
                bool measureOk = false;

                try
                {
                    (minUm, maxUm) = await MeasureAxisFullTravelUmAsync(motorId, motorModel, motorEntity, ct);
                    measureOk = true;
                    WriteFlowLog(flowLogPath, $"[{motorId}] 满行程检测成功：minUm={minUm:F3}, maxUm={maxUm:F3}, travelUm={maxUm - minUm:F3}");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    measureOk = false;
                    var p = motorModel.MotorParams.PosUm;
                    minUm = maxUm = p;
                    WriteFlowLog(flowLogPath, $"[{motorId}] 满行程检测失败，降级为当前位置单点：posUm={p:F3}，异常={ex.Message}");
                }

                if (!measureOk && maxUm - minUm <= 0)
                {
                    result.ErrorDescription = "满行程检测失败，无法生成随机点";
                    WriteFlowLog(flowLogPath, $"[{motorId}] 满行程降级后范围无效，流程终止。");
                    return result;
                }

                double travelUm = maxUm - minUm;
                if (travelUm < FixedPointSpacingUm)
                {
                    result.ErrorDescription = $"行程范围过小（{travelUm:F1} μm < {FixedPointSpacingUm:F1} μm），无法生成有效随机点";
                    WriteFlowLog(flowLogPath, $"[{motorId}] 行程过小：travelUm={travelUm:F3}, spacingUm={FixedPointSpacingUm:F3}，流程终止。");
                    return result;
                }

                result.XTravelUm = travelUm;
                result.YTravelUm = 0; // 单轴测试，Y轴设为0

                // 2. 计算中心点和生成随机点
                double centerUm = (minUm + maxUm) / 2;
                // 新方案：按总行程每 5mm 设计一个目标点，取上整确保“总点数”覆盖全行程。
                int totalPointCount = Math.Max(1, (int)Math.Ceiling(travelUm / FixedPointSpacingUm));
                int minPulse = (int)Math.Round(minUm * motorModel.MotorParams.SubRatio);
                int maxPulse = (int)Math.Round(maxUm * motorModel.MotorParams.SubRatio);

                var points = GenerateGaussianPoints(
                    totalPointCount,
                    FixedPointSpacingUm,
                    centerUm,
                    0, // 单轴测试，Y轴设为0
                    motorModel.MotorParams.SubRatio,
                    1, // 单轴测试，Y轴比例设为1
                    minPulse,
                    maxPulse,
                    0,
                    0,
                    new Random());

                WriteFlowLog(flowLogPath, $"[{motorId}] 随机点生成完成：totalPointCount={points.Count}, centerUm={centerUm:F3}, pulseRange=[{minPulse},{maxPulse}]");
                foreach (var point in points.Take(10))
                {
                    WriteFlowLog(flowLogPath, $"[{motorId}] 点明细：index={point.Index}, targetUm={point.TargetXUm:F3}, targetPulse={point.TargetXPulse}");
                }
                if (points.Count > 10)
                {
                    WriteFlowLog(flowLogPath, $"[{motorId}] 点明细已截断：仅打印前10个点，剩余={points.Count - 10}");
                }

                foreach (var point in points)
                    result.TargetPoints.Add(point);

                // 3. 准备电机
                WriteFlowLog(flowLogPath, $"[{motorId}] 准备电机：设置闭环位置控制 + 使能 + 运行。");
                await PrepareAxisForRandomJumpsAsync(motorId, motorEntity, ct);
                WriteFlowLog(flowLogPath, $"[{motorId}] 电机准备完成。");

                int totalPlannedMoves = FixedRepeatsPerPoint * points.Count;
                int completedMoves = 0;
                ReportProgress(completedMoves, totalPlannedMoves, overallSw.Elapsed);
                WriteFlowLog(flowLogPath, $"[{motorId}] 跳转计划：points={points.Count}, repeatsPerPoint={FixedRepeatsPerPoint}, totalPlannedMoves={totalPlannedMoves}");

                // 4. 随机跳转测试
                var moveTrips = new ObservableCollection<RandomMoveTripAxisRecord>();
                int seqNo = 1;
                var plannedCurrent = points[0];

                // 随机点跳转参数
                const int randomJumpLeaveStopTimeoutSec = 10;
                const int randomJumpBothStopTimeoutSec = 30;
                const int randomJumpMoveStatePollMs = 200;

                bool axisOk = true;

                for (int repeat = 0; repeat < FixedRepeatsPerPoint; repeat++)
                {
                    foreach (var target in points.OrderBy(_ => Guid.NewGuid()))
                    {
                        ct.ThrowIfCancellationRequested();

                        if (!axisOk) break;

                        // 记录起点位置
                        double actualStartUm = motorModel.MotorParams.PosUm;
                        double plannedStartUm = plannedCurrent.TargetXUm;

                        // 下发 GOTO 指令
                        bool commanded = false;
                        try
                        {
                            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, target.TargetXPulse);
                            commanded = true;
                        }
                        catch (Exception ex)
                        {
                            result.ErrorDescription = $"发送GOTO指令失败: {ex.Message}";
                            axisOk = false;
                            break;
                        }

                        if (!commanded) continue;

                        // 等待电机停止
                        var sw = Stopwatch.StartNew();
                        bool ok = await TryRandomAxisLeaveThenStopForTargetPulseAsync(
                            motorModel,
                            target.TargetXPulse,
                            ct,
                            randomJumpLeaveStopTimeoutSec,
                            randomJumpBothStopTimeoutSec,
                            randomJumpMoveStatePollMs);
                        sw.Stop();

                        if (!ok)
                        {
                            axisOk = false;
                            result.ErrorDescription =
                                $"电机超时未停止，目标脉冲={target.TargetXPulse}，当前位置={motorModel.MotorParams.Pos}，状态={motorModel.MotorParams.MoveState}";
                            WriteFlowLog(flowLogPath, $"[{motorId}] 跳转失败：seqNo={seqNo}, pointIndex={target.Index}, targetPulse={target.TargetXPulse}, currentPulse={motorModel.MotorParams.Pos}, state={motorModel.MotorParams.MoveState}");
                            break;
                        }

                        // 记录终点位置和时间
                        double actualTargetUm = motorModel.MotorParams.PosUm;
                        double moveMs = sw.Elapsed.TotalMilliseconds;
                        double distanceUm = Math.Abs(actualTargetUm - actualStartUm);
                        double speed = moveMs > 1e-6
                            ? distanceUm / (moveMs / 1000.0)
                            : 0; // 微米/秒；避免计时过短导致除零

                        // 添加行程记录
                        moveTrips.Add(new RandomMoveTripAxisRecord
                        {
                            SeqNo = seqNo++,
                            PointIndex = target.Index,
                            PlannedStartUm = plannedStartUm,
                            ActualStartUm = actualStartUm,
                            TargetUm = target.TargetXUm,
                            ActualTargetUm = actualTargetUm,
                            MoveTimeMs = moveMs,
                            DistanceUm = distanceUm,
                            AvgSpeedUmPerSec = speed
                        });

                        completedMoves++;
                        ReportProgress(completedMoves, totalPlannedMoves, overallSw.Elapsed);
                        if (completedMoves <= 5 || completedMoves % 20 == 0 || completedMoves == totalPlannedMoves)
                        {
                            WriteFlowLog(flowLogPath, $"[{motorId}] 跳转进度：{completedMoves}/{totalPlannedMoves}, seqNo={seqNo - 1}, pointIndex={target.Index}, actualStartUm={actualStartUm:F3}, actualTargetUm={actualTargetUm:F3}, moveMs={moveMs:F1}, distanceUm={distanceUm:F3}, speedUmPerSec={speed:F3}");
                        }

                        plannedCurrent = target;
                    }
                }

                // 5. 计算统计数据
                BuildRepeatabilityAxisStats(points, moveTrips, result.PointStatsX, t => t.ActualTargetUm, p => p.TargetXUm);
                WriteFlowLog(flowLogPath, $"[{motorId}] 统计完成：pointStatsCount={result.PointStatsX.Count}, tripCount={moveTrips.Count}");

                foreach (var trip in moveTrips)
                    result.MoveTripsX.Add(trip);

                // 6. 生成直方图
                BuildHistogram(moveTrips.Select(t => t.DistanceUm).ToList(), _histogramBins, result.DistanceHistogramX);
                BuildHistogram(moveTrips.Select(t => t.AvgSpeedUmPerSec).ToList(), _histogramBins, result.SpeedHistogramX);
                WriteFlowLog(flowLogPath, $"[{motorId}] 直方图完成：distanceBins={result.DistanceHistogramX.Count}, speedBins={result.SpeedHistogramX.Count}");

                // 7. 计算结果
                result.AvgStdX = result.PointStatsX.Count == 0 ? 0 : result.PointStatsX.Average(s => s.StdUm);
                result.AvgStdY = 0; // 单轴测试，Y轴设为0
                var tripCount = result.MoveTripsX.Count;
                result.AvgMoveTimeMs = tripCount == 0
                    ? 0
                    : result.MoveTripsX.Sum(t => t.MoveTimeMs) / tripCount;

                result.MeasuredValue = $"标准差: {result.AvgStdX:F3} μm";
                if (!axisOk)
                {
                    result.IsPassed = false;
                    result.IsTestCompleted = false;
                    result.Description = "随机坐标重复精度测试未正常完成（存在超时或指令失败）";
                    WriteFlowLog(flowLogPath, $"[{motorId}] 流程结束（失败）：isCompleted={result.IsTestCompleted}, avgStdX={result.AvgStdX:F3}, avgMoveMs={result.AvgMoveTimeMs:F2}, error={result.ErrorDescription ?? "无"}");
                }
                else
                {
                    result.IsPassed = true;
                    result.IsTestCompleted = true;
                    result.Description = "随机坐标重复精度测试完成";
                    WriteFlowLog(flowLogPath, $"[{motorId}] 流程结束（成功）：isCompleted={result.IsTestCompleted}, avgStdX={result.AvgStdX:F3}, avgMoveMs={result.AvgMoveTimeMs:F2}, points={result.TargetPoints.Count}, trips={result.MoveTripsX.Count}");
                }

            }
            catch (OperationCanceledException)
            {
                result.ErrorDescription = "测试已取消";
                WriteFlowLog(flowLogPath, $"[{motorId}] 流程取消。");
                throw;
            }
            catch (Exception ex)
            {
                result.ErrorDescription = $"测试异常: {ex.Message}";
                WriteFlowLog(flowLogPath, $"[{motorId}] 流程异常终止：{ex.Message}");
            }

            WriteFlowLog(flowLogPath, $"[{motorId}] 详细日志文件：{flowLogPath}");
            return result;
        }

        private static string CreateFlowLogPath(EnumMotorId motorId)
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "报告", "随机重复精度", "流程日志");
            Directory.CreateDirectory(dir);
            string file = $"随机重复精度_{motorId}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.log";
            return Path.Combine(dir, file);
        }

        private static void WriteFlowLog(string filePath, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            lock (FlowLogFileLock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }

        private void ReportProgress(int completed, int total, TimeSpan elapsed)
        {
            if (_progressReporter == null)
                return;

            TimeSpan eta = TimeSpan.Zero;
            if (completed > 0 && total > completed)
            {
                double avgMsPerPoint = elapsed.TotalMilliseconds / completed;
                double remainingMs = avgMsPerPoint * (total - completed);
                eta = TimeSpan.FromMilliseconds(Math.Max(0, remainingMs));
            }

            _progressReporter(completed, total, elapsed, eta);
        }

        /// <summary>
        /// 测量轴满行程
        /// 
        /// 通过调用 FullTravelTestItem 检测指定轴的满行程范围
        /// 该方法会执行正向和反向限位检测，获取轴的最大和最小位置
        /// 
        /// 返回值：
        /// - minUm: 轴的最小位置（微米）
        /// - maxUm: 轴的最大位置（微米）
        /// 
        /// 异常：
        /// - 如果满行程检测返回结果类型错误，抛出 InvalidOperationException
        /// - 如果最大值和最小值差值 ≤ 0，抛出 InvalidOperationException
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="motorModel">电机模型</param>
        /// <param name="motorEntity">电机控制实体</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>轴的最小和最大位置（微米）</returns>
        private async Task<(double minUm, double maxUm)> MeasureAxisFullTravelUmAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var testItem = new FullTravelTestItem((0, int.MaxValue));
            var result = await testItem.ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (result is not TravelTestResult travel)
                throw new InvalidOperationException($"满行程检测返回结果异常");

            double minUm = Math.Min(travel.RealMinPosUm, travel.RealMaxPosUm);
            double maxUm = Math.Max(travel.RealMinPosUm, travel.RealMaxPosUm);
            if (maxUm - minUm <= 0)
                throw new InvalidOperationException("满行程检测结果无效");

            return (minUm, maxUm);
        }

        /// <summary>
        /// 准备轴进行随机跳转
        /// 
        /// 在开始随机跳转测试前，需要将轴设置为正确的状态：
        /// 1. 设置为闭环位置控制模式（CloseLoopPosCtr）
        /// 2. 使能电机（Enable）
        /// 3. 发送运行命令（Run）
        /// 
        /// 延时说明：
        /// - 第一次延时 120ms：等待控制模式切换完成
        /// - 第二次延时 80ms：等待运行命令生效
        /// 
        /// 注意：必须先发送运行命令，否则 GOTO 指令可能无效
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="motorEntity">电机控制实体</param>
        /// <param name="ct">取消令牌</param>
        private async Task PrepareAxisForRandomJumpsAsync(
            EnumMotorId motorId,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
            motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);

            await Task.Delay(120, ct);
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Run);
            await Task.Delay(80, ct);
        }

        /// <summary>
        /// 生成高斯分布的点
        /// 
        /// 算法说明：
        /// 1. 使用 Box-Muller 变换生成标准正态分布随机数
        /// 2. 以轴中心为原点，按高斯分布生成随机点
        /// 3. 将微米值转换为脉冲值，并限制在有效行程范围内
        /// 
        /// 标准差计算：
        /// σ = max(spacing, spacing * √N / 3)
        /// 其中 N 为总点数，spacing 为点间距
        /// 
        /// 边界处理：
        /// - 生成的脉冲值会被限制在 [minPulse, maxPulse] 范围内
        /// - 确保所有目标点都在电机的有效行程范围内
        /// </summary>
        /// <param name="totalPointCount">总点数</param>
        /// <param name="spacingUm">点间距（微米）</param>
        /// <param name="centerXUm">X轴中心位置（微米）</param>
        /// <param name="centerYUm">Y轴中心位置（微米）</param>
        /// <param name="xRatio">X轴换算系数（脉冲/微米）</param>
        /// <param name="yRatio">Y轴换算系数（脉冲/微米）</param>
        /// <param name="minXPulse">X轴最小脉冲值</param>
        /// <param name="maxXPulse">X轴最大脉冲值</param>
        /// <param name="minYPulse">Y轴最小脉冲值</param>
        /// <param name="maxYPulse">Y轴最大脉冲值</param>
        /// <param name="random">随机数生成器</param>
        /// <returns>随机目标点列表</returns>
        private static List<RandomTargetPointRecord> GenerateGaussianPoints(
            int totalPointCount,
            double spacingUm,
            double centerXUm,
            double centerYUm,
            double xRatio,
            double yRatio,
            int minXPulse,
            int maxXPulse,
            int minYPulse,
            int maxYPulse,
            Random random)
        {
            var list = new List<RandomTargetPointRecord>();
            int idx = 1;
            double sigma = Math.Max(spacingUm, spacingUm * Math.Sqrt(totalPointCount) / 3.0);
            for (int i = 0; i < totalPointCount; i++)
            {
                double targetXUm = centerXUm + NextGaussian(random, 0, sigma);
                double targetYUm = centerYUm + NextGaussian(random, 0, sigma);
                int xPulse = (int)Math.Round(targetXUm * xRatio);
                int yPulse = (int)Math.Round(targetYUm * yRatio);
                xPulse = Math.Max(minXPulse, Math.Min(maxXPulse, xPulse));
                yPulse = Math.Max(minYPulse, Math.Min(maxYPulse, yPulse));

                list.Add(new RandomTargetPointRecord
                {
                    Index = idx++,
                    TargetXUm = xPulse / xRatio,
                    TargetYUm = yPulse / yRatio,
                    TargetXPulse = xPulse,
                    TargetYPulse = yPulse
                });
            }
            return list;
        }

        /// <summary>
        /// 生成高斯随机数
        /// 
        /// 使用 Box-Muller 变换生成标准正态分布随机数
        /// 
        /// 算法说明：
        /// 1. 生成两个独立的均匀分布随机数 u1 和 u2 (0, 1]
        /// 2. 应用 Box-Muller 变换：
        ///    z = sqrt(-2 * ln(u1)) * sin(2π * u2)
        /// 3. 结果 z 服从标准正态分布 N(0, 1)
        /// 4. 转换为指定均值和标准差的正态分布：
        ///    result = mean + stdDev * z
        /// 
        /// 特点：
        /// - 生成的随机数服从正态分布
        /// - 可以生成任意均值和标准差的正态分布随机数
        /// - 一次调用生成一个随机数
        /// </summary>
        /// <param name="random">随机数生成器</param>
        /// <param name="mean">均值</param>
        /// <param name="stdDev">标准差</param>
        /// <returns>服从 N(mean, stdDev²) 分布的随机数</returns>
        private static double NextGaussian(Random random, double mean, double stdDev)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        /// <summary>
        /// 等待电机离开停止状态并再次停止
        /// 
        /// 使用两段判停机制确保电机真正完成运动：
        /// 1. 第一段：等待电机离开 MotorStop 状态（开始运动）
        ///    - 超时时间：leaveStopTimeoutSeconds
        ///    - 如果超时，说明电机没有开始运动，返回 false
        /// 
        /// 2. 第二段：等待电机回到 MotorStop 状态（停止运动）
        ///    - 超时时间：stopTimeoutSeconds
        ///    - 如果超时，说明电机没有在规定时间内停止，返回 false
        /// 
        /// 轮询机制：
        /// - 每隔 pollDelayMs 毫秒检查一次电机状态
        /// - 使用 ConfigureAwait(false) 避免死锁
        /// 
        /// 异常处理：
        /// - OperationCanceledException：向上传播，表示测试被取消
        /// - 其他异常：捕获并返回 false，不中断测试
        /// </summary>
        /// <param name="motorModel">电机模型</param>
        /// <param name="targetPulse">目标脉冲；若已在目标（±1 脉冲内）则视为完成，避免零位移一直判停超时。</param>
        /// <param name="ct">取消令牌</param>
        /// <param name="leaveStopTimeoutSeconds">离开停止状态的超时时间（秒）</param>
        /// <param name="stopTimeoutSeconds">停止状态的超时时间（秒）</param>
        /// <param name="pollDelayMs">状态轮询间隔（毫秒）</param>
        /// <returns>true: 电机正常完成运动；false: 超时或异常</returns>
        private static async Task<bool> TryRandomAxisLeaveThenStopForTargetPulseAsync(
            MotorModel motorModel,
            int targetPulse,
            CancellationToken ct,
            int leaveStopTimeoutSeconds,
            int stopTimeoutSeconds,
            int pollDelayMs)
        {
            static bool IsMotorStop(MotorModel m) =>
                m.MotorParams.MoveState == EnumMotorMoveState.MotorStop;

            try
            {
                int ratioBasedTolerance = (int)Math.Ceiling(Math.Abs(motorModel.MotorParams.SubRatio) * 0.5); // 约 0.5um
                int targetTolerancePulse = Math.Max(2, ratioBasedTolerance);
                int leaveDetectDeltaPulse = Math.Max(1, targetTolerancePulse / 2);
                int startPulse = motorModel.MotorParams.Pos;
                bool IsAtTarget() => Math.Abs(motorModel.MotorParams.Pos - targetPulse) <= targetTolerancePulse;

                if (IsAtTarget())
                    return true;

                // 第一段：等待“离开停止”或“位置已明显变化/直接到位”。
                var startWait = DateTime.UtcNow;
                while (IsMotorStop(motorModel) && Math.Abs(motorModel.MotorParams.Pos - startPulse) < leaveDetectDeltaPulse)
                {
                    ct.ThrowIfCancellationRequested();
                    if (IsAtTarget())
                        return true;
                    if ((DateTime.UtcNow - startWait).TotalSeconds > leaveStopTimeoutSeconds)
                        return IsAtTarget(); // 某些驱动状态位不变化，但位置已经到位
                    await Task.Delay(pollDelayMs, ct).ConfigureAwait(false);
                }

                // 第二段：等待“回到停止”或“位置连续收敛到目标”。
                startWait = DateTime.UtcNow;
                int stableAtTargetCount = 0;
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    if (IsAtTarget())
                    {
                        if (IsMotorStop(motorModel))
                            return true;

                        stableAtTargetCount++;
                        if (stableAtTargetCount >= 3)
                            return true; // 状态位可能延迟，位置已连续稳定到位则判完成
                    }
                    else
                    {
                        stableAtTargetCount = 0;
                    }

                    if ((DateTime.UtcNow - startWait).TotalSeconds > stopTimeoutSeconds)
                        return IsAtTarget();

                    await Task.Delay(pollDelayMs, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 构建重复精度统计数据
        /// 
        /// 对每个目标点的多次到达位置进行统计分析：
        /// 1. 筛选出该点的所有行程记录
        /// 2. 提取实际到达位置值
        /// 3. 计算平均值和标准差
        /// 4. 生成统计记录并添加到目标集合
        /// 
        /// 统计指标：
        /// - MeanUm: 实际位置的平均值，反映系统性误差
        /// - StdUm: 实际位置的标准差，反映重复精度
        /// - SampleCount: 样本数量，即该点的重复次数
        /// </summary>
        /// <param name="points">目标点列表</param>
        /// <param name="trips">行程记录列表</param>
        /// <param name="stats">统计记录集合（输出）</param>
        /// <param name="valueSelector">值选择器，用于提取实际位置值</param>
        /// <param name="targetDisplayUmSelector">目标坐标（用于统计表展示，单轴 X 用 <see cref="RandomTargetPointRecord.TargetXUm"/>）</param>
        private static void BuildRepeatabilityAxisStats(
            List<RandomTargetPointRecord> points,
            ObservableCollection<RandomMoveTripAxisRecord> trips,
            ObservableCollection<RandomPointStatAxisRecord> stats,
            Func<RandomMoveTripAxisRecord, double> valueSelector,
            Func<RandomTargetPointRecord, double> targetDisplayUmSelector)
        {
            foreach (var point in points)
            
            {
                var pointTrips = trips.Where(t => t.PointIndex == point.Index).ToList();
                if (pointTrips.Count == 0) continue;

                var values = pointTrips.Select(valueSelector).ToList();
                double mean = values.Average();
                double variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
                double std = Math.Sqrt(variance);

                stats.Add(new RandomPointStatAxisRecord
                {
                    PointIndex = point.Index,
                    TargetUm = targetDisplayUmSelector(point),
                    MeanUm = mean,
                    StdUm = std,
                    SampleCount = pointTrips.Count
                });
            }
        }

        /// <summary>
        /// 构建直方图
        /// 
        /// 将数据分组到指定的 bin 数中，统计每个 bin 的数据数量
        /// 
        /// 算法说明：
        /// 1. 计算数据的最小值和最大值
        /// 2. 将数据范围划分为 binCount 个等宽区间
        /// 3. 统计每个区间内的数据数量
        /// 4. 生成直方图记录
        /// 
        /// 输出：
        /// - BinMin: 区间最小值
        /// - BinMax: 区间最大值
        /// - Count: 该区间内的数据数量
        /// </summary>
        /// <param name="values">数据值列表</param>
        /// <param name="binCount">bin 数量</param>
        /// <param name="histogram">直方图集合（输出）</param>
        private static void BuildHistogram(
            List<double> values,
            int binCount,
            ObservableCollection<HistogramBinRecord> histogram)
        {
            if (values.Count == 0) return;

            double min = values.Min();
            double max = values.Max();
            double binWidth = (max - min) / binCount;

            for (int i = 0; i < binCount; i++)
            {
                double binMin = min + i * binWidth;
                double binMax = binMin + binWidth;
                int count = values.Count(v => v >= binMin && (i == binCount - 1 ? v <= binMax : v < binMax));

                histogram.Add(new HistogramBinRecord
                {
                    MinValue = binMin,
                    MaxValue = binMax,
                    Count = count
                });
            }
        }
    }
}
