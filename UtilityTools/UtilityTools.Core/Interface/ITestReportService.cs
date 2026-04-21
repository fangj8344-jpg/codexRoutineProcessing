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
        object MotorKindObj { get; set; }

        /// <summary>
        /// 无扫码快捷键进入五轴机型时，仅保留 X/Y 轴参与界面与测试（Z/T/R 不显示不测）。
        /// </summary>
        bool DevShortcutXyOnlyAxes { get; set; }
        // 告诉大家：谁签了这份合同，谁就要负责收 MotorData
        void AddOrUpdateMotorData(MotorData data);

        // 初始化测试（初始化报告）
        void InitReport();

        //开始测试（打上开始时间戳）
        void StartReport();

        // 结束测试（打上结束时间戳）
        void CompleteReport();


        
    }
}
