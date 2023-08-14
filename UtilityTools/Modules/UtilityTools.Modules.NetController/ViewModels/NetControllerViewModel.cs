#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.ViewModels
 * 唯一标识：d636ff6b-0a6e-400b-8460-6872a380aa4c
 * 文件名：NetControllerViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:19:15
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NetController.Model;
using UtilityTools.Modules.NetController.Views;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.NetController.ViewModels
{
    public class NetControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public NetControllerViewModel(IContainerProvider containerProvider, IMessageService messageService) 
            : base(containerProvider)
        {
            Message = messageService.GetMessage();
            CCS = containerProvider.Resolve<CCSModel>();
            DAC = containerProvider.Resolve<DacModel>();
            Light = containerProvider.Resolve<LightModel>();
            Relay = containerProvider.Resolve<RelayModel>();
            Temperature = containerProvider.Resolve<TemperatureModel>();
            Vacuum = containerProvider.Resolve<VacuumModel>();
        }
        #endregion

        #region ------------Field------------
        private string _message;
        private CCSModel _ccs;
        private DacModel _dac;
        private LightModel _light;
        private RelayModel _relay;
        private TemperatureModel _temperature;
        private VacuumModel _vacuum;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 测试消息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通讯服务
        /// </summary>
        public ISyncRWService Service { get; set; }

        /// <summary>
        /// CCS参数模型
        /// </summary>
        public CCSModel CCS
        {
            get { return _ccs; }
            set { _ccs = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// DAC参数模型
        /// </summary>
        public DacModel DAC
        {
            get { return _dac; }
            set { _dac = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 灯带控制模型
        /// </summary>
        public LightModel Light
        {
            get { return _light; }
            set { _light = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 继电器控制模型
        /// </summary>
        public RelayModel Relay
        {
            get { return _relay; }
            set { _relay = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 温度控制模型
        /// </summary>
        public TemperatureModel Temperature
        {
            get { return _temperature; }
            set { _temperature = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 真空模型
        /// </summary>
        public VacuumModel Vacuum
        {
            get { return _vacuum; }
            set { _vacuum = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
