using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.Motor5Controller.Model
{
    /// <summary>
    /// 运动模式枚举
    /// </summary>
    public enum EnumSportsModeTypes
    {
        [Description("绝对位置")]
        AbsolutePosition = 0,
        [Description("相对位置")]
        RelativePosition = 1,
        [Description("速度模式")]
        SpeedMode = 2,
    }
}
