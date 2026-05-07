using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.Interface
{
    internal interface IMotorTestItem
    {
        string TestName { get; }

        /// <summary>
        /// 执行测试逻辑
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="motorModel">电机模型数据</param>
        /// <param name="motorEntity">你的控制实体（用于发指令）</param>
        /// <param name="ct">取消令牌</param>
        Task<MotorTestResult> ExecuteAsync(
            EnumMotorId motorId,
            MotorModel motorModel,
            IMotorEntity motorEntity, // 假设你的控制类有接口
            CancellationToken ct);
    }
    // 统一的测试结果包装类
    public class MotorTestResult
    {
        public bool IsPassed { get; set; }
        public string MeasuredValue { get; set; }
        public string ErrorDescription { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// 是否属于“限位异常”类失败（用于流程中止判定）。
        /// </summary>
        public bool IsLimitAbnormal { get; set; }
    }
}
