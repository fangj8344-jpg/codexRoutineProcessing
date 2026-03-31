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
    /// <summary>
    /// 满行程测试
    /// </summary>
    public class FullTravelTestItem: IMotorTestItem
    {
        private readonly (int min, int max) _standardRange;
        public string TestName => "满行程测试";

        public FullTravelTestItem((int min, int max) standardRange)
        {
            _standardRange = standardRange;
        }

        public async Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel, 
            IMotorEntity motorEntity, 
            CancellationToken ct)
        {
            var result = new TravelTestResult { IsPassed = false };

            // 1. 执行正向测试
            var forwardTest = new StallAndLimitTestItem(true);
            var forwardRes = await forwardTest.ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (!forwardRes.IsPassed)
            {
                result.ErrorDescription = "正向限位寻找失败：" + forwardRes.ErrorDescription;
                return result;
            }
            int posForward = motorModel.MotorParams.Pos;
            result.RealMaxPos = posForward; // 塞进口袋
            result.IsPositiveLimitFound = true;

            // 2. 执行反向测试
            var backwardTest = new StallAndLimitTestItem(false);
            var backwardRes = await backwardTest.ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (!backwardRes.IsPassed)
            {
                result.ErrorDescription = "反向限位寻找失败：" + backwardRes.ErrorDescription;
                return result;
            }
            int posBackward = motorModel.MotorParams.Pos;
            result.RealMinPos = posBackward; // 塞进口袋
            result.IsNegativeLimitFound = true;

            // 3. 计算行程
            int fullStroke = posForward - posBackward;
            result.MeasuredValue = fullStroke.ToString();

            // 4. 判定标准
            if (fullStroke < _standardRange.max && fullStroke > _standardRange.min)
            {
                result.IsPassed = true;

                // --- 这里是计算中心点，可以存在结果里或者通过其他方式返回 ---
                int centerPoint = posBackward + (fullStroke / 2);
                result.ErrorDescription = $"行程合格。建议中心点位置: {centerPoint}";

            }
            else
            {
                result.IsPassed = false;
                result.Description = $"行程 {fullStroke} 不在标准范围 [{_standardRange.min}-{_standardRange.max}] 内";
            }

            return result;
        }

       
    }
}
