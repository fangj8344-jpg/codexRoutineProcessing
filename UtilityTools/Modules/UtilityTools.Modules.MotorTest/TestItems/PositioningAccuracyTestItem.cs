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
        private readonly int _threshold; // 判定阈值
        public string TestName => "限位精度测试";

        public PositioningAccuracyTestItem(int threshold = 200)
        {
            _threshold = threshold;
        }

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
            double ratio = motorModel.MotorParams.SubRatio;
            double forwardDiffUm = ratio > 0 ? Math.Round(forwardDiff / ratio, 3) : 0;
            double backwardDiffUm = ratio > 0 ? Math.Round(backwardDiff / ratio, 3) : 0;

            result.MeasuredValue = $"正向:{forwardDiffUm:F3}um ({forwardDiff}脉冲), 负向:{backwardDiffUm:F3}um ({backwardDiff}脉冲)";

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
                double thresholdUm = ratio > 0 ? Math.Round(_threshold / ratio, 3) : 0;
                result.ErrorDescription = $"限位偏差超限(>{thresholdUm:F3}um / {_threshold}脉冲)";
            }

            return result;
        }

       
    }
}
