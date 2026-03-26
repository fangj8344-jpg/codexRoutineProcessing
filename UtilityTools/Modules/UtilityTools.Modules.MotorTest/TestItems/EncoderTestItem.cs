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
    internal class EncoderTestItem : IMotorTestItem
    {
        public string TestName => "编码器测试";

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var result = new MotorTestResult { IsPassed = false };

            // 1. 初始化电机状态 (替代原有的 SetMotorInit)
            // 这里根据原代码逻辑：设置闭环位置模式并使能
            motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
            motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);

            // 给硬件一点响应时间
            await Task.Delay(500, ct);

            // 2. 记录起始位置
            var posStart = motorModel.MotorParams.Pos;

            // 3. 执行移动动作 (Goto)
            // 这里逻辑同原代码：正向移动一个较大的脉冲量
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, posStart + 1000000);
            await Task.Delay(3000, ct);

            // 4. 停止并等待稳定
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
            await Task.Delay(3000, ct);

            // 5. 获取最终位置并判定
            var posEnd = motorModel.MotorParams.Pos;
            var diff = posEnd - posStart;

            result.MeasuredValue = $"编码器变化：{diff}";

            // 独特的合格判定逻辑：变化量需 >= 100
            if (diff >= 100)
            {
                result.IsPassed = true;
            }
            else
            {
                result.IsPassed = false;
                result.ErrorDescription = "编码器异常";
            }

            return result;
        }
    }

}
