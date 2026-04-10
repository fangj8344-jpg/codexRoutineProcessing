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
    /// 满行程测试(保存了脉冲 和UM)
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
            // 🚨 提前获取转换系数（除法，防0）
            if (motorModel.MotorParams.SubRatio == 0)
            {
                result.ErrorDescription = $"严重异常：[{TestName}] 检测到电机转换系数(SubRatio)为 0！硬件配置丢失，测试强制终止！";
                return result;
            }

            // 1. 执行正向测试
            var forwardTest = new StallAndLimitTestItem(true);
            var forwardRes = await forwardTest.ExecuteAsync(motorId, motorModel, motorEntity, ct);
            if (!forwardRes.IsPassed)
            {
                result.ErrorDescription = "正向限位寻找失败：" + forwardRes.ErrorDescription;
                return result;
            }
            result.RealMaxPos = motorModel.MotorParams.Pos;
            result.RealMaxPosUm = Math.Round(motorModel.MotorParams.PosUm, 3); // 直接拿！
            int posForward = motorModel.MotorParams.Pos;
            result.RealMaxPos = posForward; // 塞进口袋
            result.RealMaxPosUm = motorModel.MotorParams.PosUm;
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
            result.RealMinPosUm = motorModel.MotorParams.PosUm;
            result.IsNegativeLimitFound = true;

            // 3. 计算行程
            int fullStroke = posForward - posBackward;
            double ratio = motorModel.MotorParams.SubRatio;
            double fullStrokeUm = Math.Round(fullStroke / ratio, 3);
            result.MeasuredValue = $"{fullStrokeUm:F3}um (脉冲: {fullStroke})";
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
