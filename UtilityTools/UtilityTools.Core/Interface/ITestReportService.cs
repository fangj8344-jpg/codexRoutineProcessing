using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Interface
{
    public interface ITestReportService
    {
        // 告诉大家：谁签了这份合同，谁就要负责收 MotorData
        void AddOrUpdateMotorData(MotorData data);

        // 开始新测试（初始化报告）
        void InitReport(string deviceId, string stageId);

        // 结束测试（打上结束时间戳）
        void CompleteReport();
    }
}
