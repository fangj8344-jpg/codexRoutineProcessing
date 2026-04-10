namespace UtilityTools.Core.Interface
{
    /// <summary>
    /// 提供当前测试样品台型号（用于按型号读取阈值）。
    /// </summary>
    public interface ITestStageTypeProvider
    {
        string GetCurrentStageType();
    }
}
