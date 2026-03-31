using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Interface;
using UtilityTools.Core.Model;

namespace UtilityTools.Modules.MultiAxisTest.Services
{
    /// <summary>
    /// 实现接口，收取信息
    /// </summary>
    class TestReportManager : ITestReportService
    {
        public static Lazy<TestReportManager> _instance = new(() => new TestReportManager());
        public static TestReportManager Instance => _instance.Value;

        public UploadInformation CurrentUpload { get; private set; }

        public void InitReport(string deviceId, string stageId)
        {
            CurrentUpload = new UploadInformation
            {
                DeviceId = deviceId,
                SampleStageId = stageId,
                Content = new SampleStageReport
                {
                    StageId = stageId,
                    StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Motors = new List<MotorData>()
                }
            };
        }

        public void AddOrUpdateMotorData(MotorData data)
        {
            lock (CurrentUpload)
            {
                var existing = CurrentUpload.Content.Motors.FirstOrDefault(m => m.AxisType == data.AxisType);
                if (existing != null) CurrentUpload.Content.Motors.Remove(existing);
                CurrentUpload.Content.Motors.Add(data);
            }
        }

        public void CompleteReport()
        {
            CurrentUpload.Content.EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
