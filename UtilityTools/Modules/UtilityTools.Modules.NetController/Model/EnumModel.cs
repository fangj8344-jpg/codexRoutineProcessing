#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：7d6e8b55-6b4c-4f96-9721-17fba539e7e0
 * 文件名：EnumModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:32:13
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.NetController.Model
{
    /// <summary>
    /// 灯光工作类型
    /// </summary>
    public enum EnumLightWorkType
    {
        /// <summary>
        /// 所有灯打开
        /// </summary>
        [Description("全部点亮")] AllOn,

        /// <summary>
        /// 所有灯熄灭
        /// </summary>
        [Description("全部熄灭")] AllOff,

        /// <summary>
        /// 所有灯闪烁
        /// </summary>
        [Description("全部闪烁")] AllBlink,

        /// <summary>
        /// 部分灯闪烁
        /// </summary>
        [Description("部分闪烁")] PartBlink,

        /// <summary>
        /// 部分点亮
        /// </summary>
        [Description("部分点亮")] PartOn,

        /// <summary>
        /// 呼吸灯
        /// </summary>
        [Description("呼吸灯模式")] Breath,

        /// <summary>
        /// 单向流动
        /// </summary>
        [Description("单向流动")] SingleFlow,

        /// <summary>
        /// 双向流动
        /// </summary>
        [Description("双向流动")] BothwayFlow,

        /// <summary>
        /// 加载进度模式
        /// </summary>
        [Description("加载进度模式")] Loading,

        /// <summary>
        /// 默认模式
        /// </summary>
        [Description("默认模式")] Default
    }

    /// <summary>
    /// 单向流动模式下的流动方向
    /// </summary>
    public enum EnumFlowDirection
    {
        /// <summary>
        /// 流动方向从左到右
        /// </summary>
        LeftToRight = 0,

        /// <summary>
        /// 流动方向从右到左
        /// </summary>
        RightToLeft = 1,
    }
}
