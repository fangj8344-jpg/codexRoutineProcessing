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
        /// 同时为 true 时，才允许使用「随机重复精度」页的填充虚拟数据功能。
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

        /// <summary>
        /// 报告抬头用：完整扫码原文（多段用「/」拼接，与扫码页及上传中的样品台标识一致）；未扫码时返回 null。
        /// </summary>
        string? GetReportScanText();

        /// <summary>
        /// 报告文件名等用：扫码解析得到的序列号（二维码末段中生产日期之后的部分）；未扫码时返回 null。
        /// </summary>
        string? GetReportSampleSerialNumber();
    }
}
