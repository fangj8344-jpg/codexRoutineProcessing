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
    public class MovementTestItem : IMotorTestItem
    {
        public string TestName => "电机控制测试";

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var result = new MotorTestResult();

            // 1. 记录初始位置
            var posStart = motorModel.MotorParams.Pos;

            // 2. 发送正向移动指令 (这里逻辑参照你原代码)
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, posStart + 100000);

            // 3. 等待一段时间 (模仿你原来的 Task.Delay)
            await Task.Delay(3000, ct);

            // 4. 停止电机并等待稳定
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
            await Task.Delay(3000, ct);

            // 5. 获取结束位置并计算
            var posEnd = motorModel.MotorParams.Pos;
            var distance = Math.Abs(posEnd - posStart);

            // 6. 【关键点】这里写这个测试项“独特的”合格判定逻辑
            if (distance > 50)
            {
                result.IsPassed = true;
                result.MeasuredValue = $"移动距离: {distance}";
            }
            else
            {
                result.IsPassed = false;
                result.MeasuredValue = $"移动距离: {distance} (过小)";
                result.ErrorDescription = "电机响应不正常,请查询电机手册排查故障";
            }

            return result;
        }
    }
}
