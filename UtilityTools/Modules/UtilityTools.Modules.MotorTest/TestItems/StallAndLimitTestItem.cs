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
            var result = new MotorTestResult { IsPassed = false };
            var posHistory = new List<int>();
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

                motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
                motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
                await Task.Delay(100, ct);

                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, escapeTarget);

                DateTime escapeStartTime = DateTime.Now;
                while (motorModel.MotorParams.LimitedState != EnumMotorLimitedState.None || motorModel.MotorParams.MoveState != EnumMotorMoveState.MotorStop)
                {
                    if ((DateTime.Now - escapeStartTime).TotalSeconds > 20) break; // 
                    await Task.Delay(200, ct);
                }
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
            }
            else
            {
                // 【位置挡】
                motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr); // 切换为位置模式
                motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
                await Task.Delay(100, ct);

                motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, _direction ? motorModel.MotorParams.Pos  +  1000000 : motorModel.MotorParams.Pos - 1000000);
            }

            // 2. 核心监控循环：每隔一段时间检查一次电机状态
            // 这里的逻辑对应你原代码里的 for (int i = 0; i < 60; i++)
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < 120) // 60秒超时
            {
                if (ct.IsCancellationRequested) return new MotorTestResult { ErrorDescription = "测试被用户取消" };

                int currentPos = motorModel.MotorParams.Pos;
                posHistory.Add(currentPos);

                // --- 判定逻辑 A：堵转判定 ---
                if (posHistory.Count > 5)
                {
                    // 如果最近几次位置几乎没动 (变化 < 50)，判定为堵转
                    if (Math.Abs(currentPos - posHistory[posHistory.Count - 4]) < 50)
                    {
                        motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
                        result.MeasuredValue = "stall";
                        result.ErrorDescription = $"位置 {currentPos} 发生堵转";
                        return result;
                    }
                    // --- 判定逻辑 B：物理限位判定 ---
                    var limitState = motorModel.MotorParams.LimitedState;
                    if (_direction && limitState == EnumMotorLimitedState.PhyForwardLimited)
                    {
                        result.IsPassed = true;
                        result.MeasuredValue = "PhyForwardLimited";
                        return result;
                    }
                    if (!_direction && limitState == EnumMotorLimitedState.PhyBackwardLimited)
                    {
                        result.IsPassed = true;
                        result.MeasuredValue = "PhyBackwardLimited";
                        return result;
                    }

                    // --- 判定逻辑 C：软件限位判定 ---
                    if (motorModel.MotorParams.SNLimted || motorModel.MotorParams.SPLimted)
                    {
                        result.MeasuredValue = motorModel.MotorParams.SNLimted ? "SNLimted" : "SPLimted";
                        result.ErrorDescription = "触发软件限位";
                        return result;
                    }
                }

              

                await Task.Delay(1000, ct); // 每秒监测一次
            }

            result.ErrorDescription = "检测超时";
            return result;
        }
    }
}
