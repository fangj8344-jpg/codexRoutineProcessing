using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Interface;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MotorTest.Service
{
    public class FirmwareCheckResult
    {
        public bool IsLatest { get; set; }
        public bool UpgradeTriggered { get; set; }
        public bool UpgradeSucceeded { get; set; }
        public bool UserSkippedUpgrade { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CurrentVersionText { get; set; } = string.Empty;
        public string LatestVersionText { get; set; } = string.Empty;
        public string LatestPackageUrl { get; set; } = string.Empty;
    }

    public interface IFirmwareUpgradeCoordinator
    {
        Task<FirmwareCheckResult> CheckAndUpgradeIfNeededAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            string indexUrl,
            IDialogHostService dialogHostService,
            string dialogHostName,
            CancellationToken ct = default);
    }
}
