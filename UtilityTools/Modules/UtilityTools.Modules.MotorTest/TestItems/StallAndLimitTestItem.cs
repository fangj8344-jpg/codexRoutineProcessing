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
    public class StallAndLimitTestItem: IMotorTestItem
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool _direction; // true 为正向，false 为负向
        private readonly bool _useSpeedMode;
        public string TestName => _direction ? "正向限位与堵转检测" : "负向限位与堵转检测";

        //加了一个默认参数，默认用位置模式
        public StallAndLimitTestItem(bool direction, bool useSpeedMode = false)
        {
            _direction = direction;
            _useSpeedMode = useSpeedMode;
        }
        public StallAndLimitTestItem(bool direction)
        {
            _direction = direction;
        }

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId, 
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            void LogFlow(string msg) => _logger.Info($"【FLOW】[{TestName}][{motorId}] {msg}");
            void LogKey(string msg) => _logger.Info($"【KEY】[{TestName}][{motorId}] {msg}");
            void LogWarn(string msg) => _logger.Warn($"【WARN】[{TestName}][{motorId}] {msg}");
            void LogFail(string msg) => _logger.Error($"【FAIL】[{TestName}][{motorId}] {msg}");

            var result = new MotorTestResult { IsPassed = false };
            var posHistory = new List<int>();
            LogFlow($"开始执行。direction={(_direction ? "Forward" : "Backward")}, useSpeedMode={_useSpeedMode}, initialPos={motorModel.MotorParams.Pos}, initialLimit={motorModel.MotorParams.LimitedState}");
            // ==========================================
            // 🚨 1. 【新增：限位预检查与脱离】
            // ==========================================
            var initialLimit = motorModel.MotorParams.LimitedState;
            // 如果我们要往正向跑，但现在已经在正向限位上了
            if ((_direction && initialLimit == EnumMotorLimitedState.PhyForwardLimited) ||
                (!_direction && initialLimit == EnumMotorLimitedState.PhyBackwardLimited))
            {
                // 往反方向挪一点点（比如挪 50000 脉冲），把限位开关释放掉
                int escapeTarget = _direction ? motorModel.MotorParams.Pos - 50000 : motorModel.MotorParams.Pos + 50000;
                LogWarn($"起始即在目标方向物理限位上，先脱离限位。escapeTarget={escapeTarget}");

                motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
                motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
                await Task.Delay(100, ct);

                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, escapeTarget);

                DateTime escapeStartTime = DateTime.Now;
                while (motorModel.MotorParams.LimitedState != EnumMotorLimitedState.None || motorModel.MotorParams.MoveState != EnumMotorMoveState.MotorStop)
                {
                    if ((DateTime.Now - escapeStartTime).TotalSeconds > 20)
                    {
                        LogWarn("脱离限位等待超过20s，继续后续检测。");
                        break;
                    }
                    await Task.Delay(200, ct);
                }
                LogKey($"脱离限位完成。currentPos={motorModel.MotorParams.Pos}, currentLimit={motorModel.MotorParams.LimitedState}");
            }
            if (_useSpeedMode)
            {
                // 【速度挡】
                motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.OpenLoopSpeedCtr); // 切换为速度模式
                motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
                await Task.Delay(100, ct);

                // 发送连续运动指令 (这里用你代码里实际的速度/Jog指令，我暂写一个示例)
                // 假设正向是正速度，负向是负速度
                int testSpeed = _direction ? 10000 : -10000;
                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, testSpeed); // 👈 替换成你底层的速度驱动方法
                LogFlow($"已下发速度模式运动指令。testSpeed={testSpeed}");
            }
            else
            {
                // 【位置挡】
                motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr); // 切换为位置模式
                motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
                await Task.Delay(100, ct);

                int targetPos = _direction ? motorModel.MotorParams.Pos + 1000000 : motorModel.MotorParams.Pos - 1000000;
                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, targetPos);
                LogFlow($"已下发位置模式运动指令。targetPos={targetPos}");
            }

            // 2. 核心监控循环：每隔一段时间检查一次电机状态
            DateTime startTime = DateTime.Now;
            int loopCount = 0;
            const int stallCompareSamples = 4;       // 与约 3 秒前的采样比位移
            const int stallPulseThreshold = 50;
            const int stallWhileMoveThreshold = 3;   // MotorMove 下连续 3 次几乎不动 → 堵转
            const int initialStopConfirmSeconds = 10; // 初始即 Stop 时，给下位机一个稳定确认窗口
            int stallWhileMoveCount = 0;
            bool hasObservedMove = false;
            DateTime? initialStopSince = null;

            while ((DateTime.Now - startTime).TotalSeconds < 180)
            {
                if (ct.IsCancellationRequested)
                {
                    LogWarn("测试被用户取消。");
                    return new MotorTestResult { ErrorDescription = "测试被用户取消" };
                }

                int currentPos = motorModel.MotorParams.Pos;
                var moveState = motorModel.MotorParams.MoveState;
                var limitState = motorModel.MotorParams.LimitedState;
                posHistory.Add(currentPos);
                loopCount++;
                if (loopCount % 5 == 0)
                {
                    LogFlow($"状态快照: pos={currentPos}, moveState={moveState}, limit={limitState}, SN={motorModel.MotorParams.SNLimted}, SP={motorModel.MotorParams.SPLimted}");
                }

                // --- 优先判定限位（停稳后也能命中，避免被堵转逻辑抢先 return）---
                if (_direction && limitState == EnumMotorLimitedState.PhyForwardLimited)
                {
                    result.IsPassed = true;
                    result.MeasuredValue = "PhyForwardLimited";
                    LogKey($"命中物理正限位，测试通过。pos={currentPos}, limit={limitState}");
                    return result;
                }
                if (!_direction && limitState == EnumMotorLimitedState.PhyBackwardLimited)
                {
                    result.IsPassed = true;
                    result.MeasuredValue = "PhyBackwardLimited";
                    LogKey($"命中物理负限位，测试通过。pos={currentPos}, limit={limitState}");
                    return result;
                }
                if (motorModel.MotorParams.SNLimted || motorModel.MotorParams.SPLimted)
                {
                    result.MeasuredValue = motorModel.MotorParams.SNLimted ? "SNLimted" : "SPLimted";
                    result.ErrorDescription = "触发软件限位";
                    result.IsLimitAbnormal = true;
                    LogFail($"触发软件限位。SN={motorModel.MotorParams.SNLimted}, SP={motorModel.MotorParams.SPLimted}, pos={currentPos}");
                    return result;
                }

                if (!hasObservedMove && moveState == EnumMotorMoveState.MotorStop)
                {
                    initialStopSince ??= DateTime.Now;
                    if (loopCount == 1)
                    {
                        LogFlow($"下发运动后初始状态即为 MotorStop，进入 {initialStopConfirmSeconds}s 到位确认窗口。");
                    }

                    var stopSec = (DateTime.Now - initialStopSince.Value).TotalSeconds;
                    if (loopCount % 5 == 0)
                    {
                        LogFlow($"初始阶段仍为 MotorStop，继续等待启动/到位确认（已等待 {stopSec:F0}s / {initialStopConfirmSeconds}s）...");
                    }

                    if (stopSec >= initialStopConfirmSeconds)
                    {
                        result.IsPassed = true;
                        result.MeasuredValue = limitState == EnumMotorLimitedState.None ? "MotorStop" : limitState.ToString();
                        result.Description = "电机保持停止状态，按到位完成";
                        LogKey($"下发运动后 {initialStopConfirmSeconds}s 内持续 MotorStop，按到位完成。pos={currentPos}, limit={limitState}");
                        return result;
                    }

                    await Task.Delay(1000, ct);
                    continue;
                }

                if (!hasObservedMove && moveState != EnumMotorMoveState.MotorStop)
                {
                    hasObservedMove = true;
                    initialStopSince = null;
                    LogKey($"检测到电机已启动，转入运动监控。pos={currentPos}, moveState={moveState}");
                }

                if (hasObservedMove && moveState == EnumMotorMoveState.MotorStop)
                {
                    result.IsPassed = true;
                    result.MeasuredValue = limitState == EnumMotorLimitedState.None ? "MotorStop" : limitState.ToString();
                    result.Description = "电机停止，按到位完成";
                    LogKey($"检测到电机已停止，按到位完成。pos={currentPos}, limit={limitState}");
                    return result;
                }

                // --- 堵转判定（仅在已启动且仍处于 MotorMove 时生效）---
                if (posHistory.Count > stallCompareSamples)
                {
                    int comparePos = posHistory[posHistory.Count - 1 - stallCompareSamples];
                    int deltaPulse = Math.Abs(currentPos - comparePos);
                    bool barelyMoved = deltaPulse < stallPulseThreshold;

                    if (barelyMoved && moveState == EnumMotorMoveState.MotorMove)
                    {
                        // 仍处在运动状态但长时间不动 → 真堵转
                        stallWhileMoveCount++;
                        if (loopCount % 5 == 0)
                        {
                            LogWarn($"电机处于 MotorMove 但位移很小，疑似堵转累计中。deltaPulse={deltaPulse}, count={stallWhileMoveCount}/{stallWhileMoveThreshold}");
                        }
                        if (stallWhileMoveCount >= stallWhileMoveThreshold)
                        {
                            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
                            result.MeasuredValue = "stall";
                            result.ErrorDescription = $"位置 {currentPos} 发生堵转（运动中 {stallWhileMoveThreshold} 秒无有效位移）";
                            LogFail($"判定堵转。currentPos={currentPos}, comparePos={comparePos}, moveState=MotorMove");
                            return result;
                        }
                    }
                    else
                    {
                        stallWhileMoveCount = 0;
                    }
                }

                await Task.Delay(1000, ct);
            }

            result.ErrorDescription = "检测超时";
            result.IsLimitAbnormal = true;
            LogFail($"检测超时(180s)。finalPos={motorModel.MotorParams.Pos}, finalLimit={motorModel.MotorParams.LimitedState}, SN={motorModel.MotorParams.SNLimted}, SP={motorModel.MotorParams.SPLimted}");
            return result;
        }
    }
}
