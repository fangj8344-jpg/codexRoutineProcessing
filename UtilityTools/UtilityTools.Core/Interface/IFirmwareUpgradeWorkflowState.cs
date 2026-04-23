namespace UtilityTools.Core.Interface
{
    /// <summary>
    /// 多轴流程中的固件检查/升级状态（跨模块共享）。
    /// </summary>
    public interface IFirmwareUpgradeWorkflowState
    {
        bool FirmwareCheckCompleted { get; set; }
        bool FirmwareUpgradeRequired { get; set; }
        bool FirmwareUpgradeSkipped { get; set; }
        bool FirmwareUpgradeInProgress { get; set; }
        string FirmwareCheckMessage { get; set; }
        string CurrentFirmwareVersion { get; set; }
        string LatestFirmwareVersion { get; set; }
        string LatestFirmwareUrl { get; set; }
        string FirmwareIndexUrl { get; set; }

        void ResetFirmwareCheckState();
    }
}
