using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;

namespace UtilityTools.Modules.MotorTest.TestItems
{
    //测试成功
    public class PositioningAccuracyTestItem: IMotorTestItem
    {
        private readonly int _threshold = 200; // 判定阈值
        public string TestName => "限位精度测试";

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId, MotorModel motorModel, IMotorEntity motorEntity, CancellationToken ct)
        {
            var result = new MotorTestResult { IsPassed = false };

            // --- 第一轮测试 ---
            var pass1Forward = await new StallAndLimitTestItem(true).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            int pos1F = motorModel.MotorParams.Pos;

            var pass1Backward = await new StallAndLimitTestItem(false).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            int pos1B = motorModel.MotorParams.Pos;

            if (!pass1Forward.IsPassed || !pass1Backward.IsPassed)
            {
                result.ErrorDescription = "第一轮限位寻找失败";
                return result;
            }

            // --- 第二轮测试 ---
            var pass2Forward = await new StallAndLimitTestItem(true).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            int pos2F = motorModel.MotorParams.Pos;

            var pass2Backward = await new StallAndLimitTestItem(false).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            int pos2B = motorModel.MotorParams.Pos;

            if (!pass2Forward.IsPassed || !pass2Backward.IsPassed)
            {
                result.ErrorDescription = "第二轮限位寻找失败";
                return result;
            }

            // --- 计算偏差 (根据你原来的逻辑) ---
            // 注意：根据你原代码，dir[0]通常是正向，dir[1]是负向
            int forwardDiff = Math.Abs(pos2F - pos1F);
            int backwardDiff = Math.Abs(pos2B - pos1B);

            result.MeasuredValue = $"正向偏差:{forwardDiff}, 负向偏差:{backwardDiff}";

            // --- 判定标准 ---
            if (forwardDiff < _threshold && backwardDiff < _threshold)
            {
                result.IsPassed = true;
                result.Description = "限位精度合格";

                // --- 数据库保存逻辑 ---
                //await SaveAccuracyData(motorModel.MotorModelAxis, pos1F, pos1B, pos2F, pos2B);
            }
            else
            {
                result.IsPassed = false;
                result.ErrorDescription = "限位偏差超出200脉冲阈值";
            }

            return result;
        }

       
    }
}
