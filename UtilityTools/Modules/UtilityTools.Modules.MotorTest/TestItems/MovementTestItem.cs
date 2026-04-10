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
   //测试成功
    public class MovementTestItem : IMotorTestItem
    {
        public string TestName => "电机控制测试";

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var result = new MotorTestResult { IsPassed = false };

            // 1. 切到位置模式并强制使能
            motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
            motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
            await Task.Delay(200, ct);

            // ==========================================
            // 2. 【防撞避让逻辑】先向反向（左）移动，腾出测试空间
            // ==========================================
            // 模仿你之前的 Goto(motorId, false) 逻辑：给一个极大的负向目标
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, motorModel.MotorParams.Pos - 1000000);

            // 向左跑 4 秒
            await Task.Delay(4000, ct);

            // 停止并等待稳定
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
            await Task.Delay(1000, ct);


            // ==========================================
            // 3. 【正式测试逻辑】向正向（右）移动并检测
            // ==========================================
            // 记录安全的起点位置
            var posStart = motorModel.MotorParams.Pos;

            // 模仿你之前的 Goto(motorId, true) 逻辑：给一个极大的正向目标
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, posStart + 1000000);

            // 向右跑 3 秒
            await Task.Delay(3000, ct);

            // 强行停止电机
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);

            // 你的原逻辑：停止后等待 3 秒，确保完全停稳
            await Task.Delay(3000, ct);

            // ==========================================
            // 4. 数据结算与判定
            // ==========================================
            // 获取结束位置并计算
            var posEnd = motorModel.MotorParams.Pos;
            var distance = Math.Abs(posEnd - posStart);
            double ratio = motorModel.MotorParams.SubRatio;
            double distanceUm = ratio > 0 ? Math.Round(distance / ratio, 3) : 0;

            if (distance > 50)
            {
                result.IsPassed = true;
                result.MeasuredValue = $"移动距离: {distanceUm:F3}um (脉冲: {distance})";
                result.Description = $"起:{posStart} 止:{posEnd}";
            }
            else
            {
                result.IsPassed = false;
                result.MeasuredValue = $"移动距离: {distanceUm:F3}um (脉冲: {distance}, 过小)";
                result.ErrorDescription = "电机响应不正常，请查询电机手册排查故障";
            }

            return result;
        }
    }
}
