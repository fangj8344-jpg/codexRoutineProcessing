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
        public string TestName => _direction ? "正向限位与堵转检测" : "负向限位与堵转检测";

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

            // 1. 发起移动
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, _direction ? 1000000 : -1000000);

            // 2. 核心监控循环：每隔一段时间检查一次电机状态
            // 这里的逻辑对应你原代码里的 for (int i = 0; i < 60; i++)
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < 60) // 60秒超时
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
