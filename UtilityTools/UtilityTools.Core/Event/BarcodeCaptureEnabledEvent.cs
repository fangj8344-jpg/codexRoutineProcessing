using Prism.Events;

namespace UtilityTools.Core.Event
{
    /// <summary>
    /// 控制全局扫码采集开关：true 开启、false 关闭。
    /// </summary>
    public class BarcodeCaptureEnabledEvent : PubSubEvent<bool>
    {
    }
}
