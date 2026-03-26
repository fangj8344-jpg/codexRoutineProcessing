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
    public class PositioningAccuracyTestItem: IMotorTestItem
    {
        private readonly int _threshold = 200; // 判定阈值
        public string TestName => "定位精度测试";

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
                result.Description = "定位精度合格";

                // --- 数据库保存逻辑 ---
                await SaveAccuracyData(motorModel.MotorModelAxis, pos1F, pos1B, pos2F, pos2B);
            }
            else
            {
                result.IsPassed = false;
                result.ErrorDescription = "定位偏差超出200脉冲阈值";
            }

            return result;
        }

        private async Task SaveAccuracyData(EnumMotorModel axis, int f1, int b1, int f2, int b2)
        {
            // 使用信号量保护，防止并发写入冲突
            await SpliteOperate.MotorMessageSemaphore.WaitAsync();
            try
            {
                using (var db = new MotorMessageDbContextBase())
                {
                    var m1 = new MotorMessage { MotorModelAxis = axis, TotalDistance = f1 - b1, LeftLimitPosition = f1, RightLimitPosition = b1 };
                    var m2 = new MotorMessage { MotorModelAxis = axis, TotalDistance = f2 - b2, LeftLimitPosition = f2, RightLimitPosition = b2 };

                    await SpliteOperate.AddMotorMessageAsync(m1, db);
                    await SpliteOperate.AddMotorMessageAsync(m2, db);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"定位精度数据保存失败：{ex}");
            }
            finally
            {
                SpliteOperate.MotorMessageSemaphore.Release();
            }
        }
    }
}
