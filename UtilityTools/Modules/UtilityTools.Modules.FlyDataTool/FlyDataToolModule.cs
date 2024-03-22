#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.FlyDataTool
 * 唯一标识：5a5c860e-c729-4c8a-9fea-52115171cdef
 * 文件名：FlyDataToolModule
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 15:58:36
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

using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.FlyDataTool.Views;

namespace UtilityTools.Modules.FlyDataTool
{
    [CustomModule(ModuleName = "FlyDataTool", Title = "灯丝工具", Tip = "数据分析", Icon = "FlashTriangle")]
    public class FlyDataToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FlyDataToolView>();
        }
    }
}
