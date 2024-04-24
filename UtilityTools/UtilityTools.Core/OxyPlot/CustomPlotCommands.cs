#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.OxyPlot
 * 唯一标识：4551e046-d8f7-425c-9506-fc33ba5bde7f
 * 文件名：CustomPlotCommands
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/22 16:10:48
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

using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.OxyPlot
{
    public static class CustomPlotCommands
    {
        #region ------------Constructor------------
        static CustomPlotCommands()
        {
            SelectRectCommand = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new SelectRectManipulatot(view), args));
        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        /// <summary>
        /// Gets the zoom rectangle command.
        /// </summary>
        public static IViewCommand<OxyMouseDownEventArgs> SelectRectCommand { get; private set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
