using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.TestItems
{

    /// <summary>
    /// 分段线性测试
    /// </summary>
    public class LinearStepPrecisionTestItem: IMotorTestItem
    {
        private readonly int _posMin;
        private readonly int _posMax;
        private readonly double _stdDevThresholdUm;
        private readonly Action<string> _logAction;
        public string TestName => "分段定位线性测试";

        public LinearStepPrecisionTestItem(int posMin, int posMax, double stdDevThresholdUm = 1.0, Action<string> logAction = null)
        {
            _posMin = posMin;
            _posMax = posMax;
            _stdDevThresholdUm = stdDevThresholdUm;
            _logAction = logAction;
        }

        public async Task<MotorTestResult> ExecuteAsync(EnumMotorId motorId, MotorModel motorModel, IMotorEntity motorEntity, CancellationToken ct)
        {
         
            var result = new LinearTestResult { IsPassed = false };
            if (motorModel.MotorParams.SubRatio == 0)
            {
                result.ErrorDescription = $"严重异常：[{TestName}] 检测到电机转换系数(SubRatio)为 0！配置丢失，测试强制终止！";
                return result;
            }
            double ratio = motorModel.MotorParams.SubRatio;
            // 用于记录：目标位置、实际位置、偏差

            // 1. 切换到位置模式并使能
            motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
            motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
            await Task.Delay(500, ct);

            // 2. 循环 98 个点 (从第 1 份到第 98 份，避开 0 和 100)
            for (int i = 1; i <= 98; i++)
            {
                if (ct.IsCancellationRequested) break;
               
                // --- 直接计算目标位置，不使用外部 stepSize ---
                // 公式：起点 + (当前份数 * 总行程 / 100)
                int targetPos = _posMin + (int)(i * (double)(_posMax - _posMin) / 100.0);
                _logAction?.Invoke($"正在前往第 {i}/98 个点 (目标位置: {targetPos})...");
                // 3. 发送移动指令
                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, targetPos);

                // 4. 【关键】先等电机“动起来”，再等电机“停下来”
                bool isOk = await WaitForMoveAndStop(motorModel, ct);
                if (!isOk)
                {
                    result.ErrorDescription = $"点 {i} 响应超时或电机未启动";
                    return result;
                }

                // 5. 记录数据
                int actualPos = motorModel.MotorParams.Pos;
                double actualUm = motorModel.MotorParams.PosUm;
                double targetUm = targetPos / ratio;
                // 【核心动作】直接存进咱们 JSON 报表需要的 PositionError 格式
                result.PositionErrors.Add(new PositionError
                {
                    TargetPosition = targetPos,
                    ActualPosition = actualPos,
                    TargetPositionUm = Math.Round(targetUm, 3),
                    ActualPositionUm = Math.Round(actualUm, 3),
                });


                // 稍微停顿，让机械彻底稳定
                await Task.Delay(100, ct);
            }

            // 6. 计算标准差
            // 计算标准差 ...
            if (result.PositionErrors.Count > 1)
            {
                // 这里计算 stdDev 的逻辑只需稍微改下数据源
                var errors = result.PositionErrors.Select(p => p.ActualPosition - p.TargetPosition).ToList();
                double stdDev = CalculateStdDev(errors); // 你原有的计算逻辑
                double stdDevUm = Math.Round(stdDev / ratio, 3);
                result.FinalStdDev = Math.Round(stdDev, 3);
                result.FinalStdDevUm = stdDevUm;
                result.IsPassed = stdDevUm < _stdDevThresholdUm;
                
                result.MeasuredValue = $"标准差: {stdDevUm:F3}um (脉冲标准差: {stdDev:F1})";
            }

            return result;
        }

        /// <summary>
        /// 利用你提供的 MoveState 进行双重判定
        /// </summary>
        private async Task<bool> WaitForMoveAndStop(MotorModel model, CancellationToken ct)
        {
            // 第一步：等待电机离开 Stop 状态（确认启动）
            DateTime startWait = DateTime.Now;
            while (model.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
            {
                if (ct.IsCancellationRequested) return false;
                // 如果 2 秒都没动，说明指令没执行
                if ((DateTime.Now - startWait).TotalSeconds > 2) return false;
                await Task.Delay(50, ct);
            }

            // 第二步：等待电机回到 Stop 状态（确认停止）
            startWait = DateTime.Now;
            while (model.MotorParams.MoveState != EnumMotorMoveState.MotorStop)
            {
                if (ct.IsCancellationRequested) return false;
                // 给一个 15 秒的最长运行时间
                if ((DateTime.Now - startWait).TotalSeconds > 15) return false;
                await Task.Delay(50, ct);
            }

            return true;
        }
        /// <summary>
        /// 计算一组数据的标准差
        /// </summary>
        private double CalculateStdDev(IEnumerable<double> values)
        {
            // 如果点数太少，没法算波动，直接返回 0
            if (values == null || !values.Any() || values.Count() < 2)
                return 0;

            // 1. 算平均值
            double avg = values.Average();

            // 2. 算“差值的平方和”
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));

            // 3. 算方差再开根号
            double variance = sumOfSquares / values.Count();

            // 保留 3 位小数返回
            return Math.Round(Math.Sqrt(variance), 3);
        }
    }
}
