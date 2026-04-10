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
    public class EncoderTestItem : IMotorTestItem
    {
        public string TestName => "编码器测试";

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            var result = new MotorTestResult { IsPassed = false };

            // 1. 初始化电机状态 (闭环位置模式并使能)
            motorEntity.SetMotorControlModeCommand(motorId, EnumMotorCtrType.CloseLoopPosCtr);
            motorEntity.SetMotorEnableCommand(motorId, EnumMotorEnable.Enable);
            await Task.Delay(500, ct);

            // ==========================================
            // 2. 【防撞避让逻辑】先向反向（左）移动，腾出测试空间
            // ==========================================
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, motorModel.MotorParams.Pos - 1000000);

            // 向左跑 4 秒
            await Task.Delay(4000, ct);

            // 停止并等待稳定
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
            await Task.Delay(1000, ct);

            // ==========================================
            // 3. 【正式编码器测试逻辑】
            // ==========================================
            // 记录安全的起始位置
            var posStart = motorModel.MotorParams.Pos;

            // 正向移动一个较大的脉冲量
            motorEntity.SetMotorGoToCommand(motorId, EnumMotorUnit.Pulse, posStart + 1000000);

            // 向右跑 3 秒
            await Task.Delay(3000, ct);

            // 停止并等待稳定
            motorEntity.SetMotorOperatingStatusCommand(motorId, EnumMotorOperatingState.Stop);
            await Task.Delay(3000, ct);

            // 4. 获取最终位置并判定
            var posEnd = motorModel.MotorParams.Pos;

            // 注意：这里没有加 Math.Abs，因为编码器不仅要变，还要“变对方向”
            // 向正向发指令，posEnd 必须大于 posStart
            var diff = posEnd - posStart;
            double ratio = motorModel.MotorParams.SubRatio;
            double diffUm = ratio > 0 ? Math.Round(diff / ratio, 3) : 0;

            result.MeasuredValue = $"编码器变化：{diffUm:F3}um (脉冲: {diff})";

            // 独特的合格判定逻辑：变化量需 >= 100
            if (diff >= 100)
            {
                result.IsPassed = true;
                result.Description = $"起始:{posStart} 结束:{posEnd}";
            }
            else
            {
                result.IsPassed = false;
                result.ErrorDescription = $"编码器异常或方向错误 (预期>=100, 实际:{diff})";
            }

            return result;
        }
    }

}
