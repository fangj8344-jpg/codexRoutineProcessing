#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.InstrumentDataRetriever
 * 唯一标识：f8e7d6c5-b4a3-4c2d-9e1f-0a9b8c7d6e5f
 * 文件名：InstrumentDataRetrieverModule
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 15:58:36
 * 版本：V1.0.0
 * 描述：仪器数据检索模块
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
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.InstrumentDataRetriever.Services;
using UtilityTools.Modules.InstrumentDataRetriever.ViewModels;
using UtilityTools.Modules.InstrumentDataRetriever.Views;

namespace UtilityTools.Modules.InstrumentDataRetriever
{
    [CustomModule(ModuleName = "InstrumentDataRetriever", Title = "仪器数据检索", Tip = "数据库检索与分析", Icon = "DatabaseSearch")]
    public class InstrumentDataRetrieverModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InstrumentDataRetrieverView, InstrumentDataRetrieverViewModel>();
            containerRegistry.RegisterSingleton<IInstrumentDataService, InstrumentDataService>();
        }
    }
} 