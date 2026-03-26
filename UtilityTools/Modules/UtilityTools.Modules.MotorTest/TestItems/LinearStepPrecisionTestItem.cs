using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.TestItems
{
    public class LinearStepPrecisionTestItem: IMotorTestItem
    {
        private readonly int _posMin;
        private readonly int _posMax;
        public string TestName => "分段定位线性测试";

        public LinearStepPrecisionTestItem(int posMin, int posMax)
        {
            _posMin = posMin;
            _posMax = posMax;
        }

        public async Task<MotorTestResult> ExecuteAsync(EnumMotorId motorId, MotorModel motorModel, IMotorEntity motorEntity, CancellationToken ct)
        {
            var result = new MotorTestResult { IsPassed = false };
            // 用于记录：目标位置、实际位置、偏差
            var records = new List<(int Target, int Actual, int Error)>();

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
                records.Add((targetPos, actualPos, actualPos - targetPos));

                // 稍微停顿，让机械彻底稳定
                await Task.Delay(100, ct);
            }

            // 6. 计算标准差
            if (records.Count > 1)
            {
                var errors = records.Select(p => (double)p.Error).ToList();
                double avgError = errors.Average();
                double sumOfSquares = errors.Sum(e => Math.Pow(e - avgError, 2));
                double stdDev = Math.Sqrt(sumOfSquares / errors.Count);

                result.IsPassed = stdDev < 150; // 根据你的精度要求改这个阈值
                result.MeasuredValue = $"StdDev:{stdDev:F2}";
                result.Description = $"测试点:{records.Count}, 均值偏差:{avgError:F1}";
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
    }
}
