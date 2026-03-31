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
    public class SmoothnessTestItem: IMotorTestItem
    {
        //private readonly (int min, int max) _fullStrokeRange;
        public string TestName => "丝杆测试";

        public SmoothnessTestItem()
        {
           
        }

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId, 
            MotorModel motorModel,
            IMotorEntity motorEntity,
            CancellationToken ct)
        {
            // 1. 定义一个提取稳定段并算标准差的私有函数
            double CalculateStableStdDev(int startIdx, int endIdx)
            {
                // 获取这一段的所有原始速度点（取绝对值）
                var rawSpeeds = motorModel.SpeedList
                    .Skip(startIdx)
                    .Take(endIdx - startIdx)
                    .Select(s => Math.Abs(s.Speed))
                    .ToList();

                if (rawSpeeds.Count < 10) return 0; // 点太少没意义

                // --- 精妙之处：掐头去尾 (按 20% 比例剔除) ---
                int skipCount = (int)(rawSpeeds.Count * 0.2);
                var stableSpeeds = rawSpeeds
                    .Skip(skipCount)             // 去掉开头加速段
                    .Take(rawSpeeds.Count - 2 * skipCount) // 去掉末尾减速/碰撞段
                    .ToList();

                if (!stableSpeeds.Any()) return 0;

                double avg = stableSpeeds.Average();
                double sumOfSquares = stableSpeeds.Sum(v => Math.Pow(v - avg, 2));
                return Math.Sqrt(sumOfSquares / stableSpeeds.Count);
            }

            // --- 开始正式测试 ---
            var result = new SmoothnessTestResult();
            // A 段：正向
            int startF = motorModel.SpeedList.Count;
            var fRes = await new StallAndLimitTestItem(true,true).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (!fRes.IsPassed)
            {
                result.IsPassed = false;
                result.ErrorDescription = $"正向运行异常: {fRes.ErrorDescription}";
                return result;
            }
            int endF = motorModel.SpeedList.Count;
            double stdDevF = CalculateStableStdDev(startF, endF);

            // B 段：反向
            int startB = motorModel.SpeedList.Count;
            var bRes = await new StallAndLimitTestItem(false,true).ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (!bRes.IsPassed)
            {
                result.IsPassed = false;
                result.ErrorDescription = $"反向运行异常: {bRes.ErrorDescription}";
                return result;
            }
            int endB = motorModel.SpeedList.Count;
            double stdDevB = CalculateStableStdDev(startB, endB);

            // --- 综合评价 ---
         

            if (fRes.IsPassed && bRes.IsPassed)
            {
                result.IsPassed = true;
                result.ForwardStdDev = stdDevF;
                result.BackwardStdDev = stdDevB;


                result.MeasuredValue = $"stdDevF：{stdDevF}:stdDevB：{stdDevB}";
                result.Description = $"正向波动:{stdDevF:F2}, 反向波动:{stdDevB:F2}";
            }
            return result;
        }

       
    }
}
