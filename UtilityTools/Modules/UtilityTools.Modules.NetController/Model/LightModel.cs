#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：dc2973cb-c077-40e8-9e72-30ddca1ed2b0
 * 文件名：LightModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:34:05
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

using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NetController.Extension;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.NetController.Model
{
    public class LightModel : BindableBase
    {
        #region ------------Constructor------------
        public LightModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _aggregator = containerProvider.Resolve<IEventAggregator>();
            _service = _containerProvider.Resolve<IServiceFactory>().GetSyncRWService("UNCB");
            WorkType = EnumLightWorkType.Default;
            BlinkPeriod = 1000;
            NumMask = 0;
            BreathPeriod = 1000;
            FlowPeriod = 1000;
            FlowCount = 0;
            FlowDirect = EnumFlowDirection.LeftToRight;
            LoadValue = 0;
            MotorEnable = new ToggleInfoModel() { Name = "电机开关", Type = "Motor", Tip = "", Channel = 1, Enable = false };

            UpdateLightCommand = new DelegateCommand(UpdateLight);
        }
        #endregion

        #region ------------Field------------
        private readonly IContainerProvider _containerProvider;
        public readonly IEventAggregator _aggregator;
        private ISyncRWService _service;
        private EnumLightWorkType _workType;
        private int _blinkPeriod;
        private int _numMask;
        private int _breathPeriod;
        private int _flowPeriod;
        private int _flowCount;
        private EnumFlowDirection _flowDirect;
        private int _loadValue;
        private ToggleInfoModel _motorEnable;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 灯光组工作模式
        /// </summary>
        public EnumLightWorkType WorkType
        {
            get { return _workType; }
            set
            {
                _workType = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 灯光闪烁周期
        /// </summary>
        public int BlinkPeriod
        {
            get { return _blinkPeriod; }
            set
            {
                _blinkPeriod = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 工作灯光编号掩码
        /// </summary>
        public int NumMask
        {
            get { return _numMask; }
            set
            {
                _numMask = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 呼吸灯周期
        /// </summary>
        public int BreathPeriod
        {
            get { return _breathPeriod; }
            set
            {
                _breathPeriod = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 流动周期
        /// </summary>
        public int FlowPeriod
        {
            get { return _flowPeriod; }
            set
            {
                _flowPeriod = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 流动灯数量
        /// </summary>
        public int FlowCount
        {
            get { return _flowCount; }
            set
            {
                _flowCount = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 流动方向
        /// </summary>
        public EnumFlowDirection FlowDirect
        {
            get { return _flowDirect; }
            set
            {
                _flowDirect = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 加载数值
        /// </summary>
        public int LoadValue
        {
            get { return _loadValue; }
            set
            {
                _loadValue = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 电机使能
        /// </summary>
        public ToggleInfoModel MotorEnable
        {
            get { return _motorEnable; }
            set
            {
                _motorEnable = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand UpdateLightCommand { get; set; }

        private void NewUpdateLight()
        {
            string cmdName = "SetLightMsg";
            ByteWriter writer = new ByteWriter(16);
            writer.Write((byte)0xCE);
            switch (WorkType)
            {
                case EnumLightWorkType.AllOn:
                    writer.Write((byte)0x01);
                    writer.Write((byte)0x01);
                    break;
                case EnumLightWorkType.AllOff:
                    writer.Write((byte)0x01);
                    writer.Write((byte)0x02);
                    break;
                case EnumLightWorkType.AllBlink:
                    writer.Write((byte)0x03);
                    writer.Write((byte)0x03);
                    writer.Write((ushort)BlinkPeriod);
                    break;
                case EnumLightWorkType.PartBlink:
                    writer.Write((byte)0x05);
                    writer.Write((byte)0x04);
                    writer.Write((ushort)BlinkPeriod);
                    writer.Write((byte)0x03);
                    break;
                case EnumLightWorkType.Breath:
                    writer.Write((byte)0x03);
                    writer.Write((byte)0x05);
                    writer.Write((ushort)BreathPeriod);
                    break;
                case EnumLightWorkType.SingleFlow:
                    writer.Write((byte)0x05);
                    writer.Write((byte)0x06);
                    writer.Write((ushort)FlowPeriod);
                    writer.Write((byte)FlowCount);
                    writer.Write((byte)FlowDirect);
                    break;
                case EnumLightWorkType.BothwayFlow:
                    writer.Write((byte)0x04);
                    writer.Write((byte)0x07);
                    writer.Write((ushort)BlinkPeriod);
                    writer.Write((byte)0x00);
                    break;
                case EnumLightWorkType.Loading:
                    writer.Write((byte)0x02);
                    writer.Write((byte)0x08);
                    writer.Write((byte)LoadValue);
                    break;
                case EnumLightWorkType.PartOn:
                    writer.Write((byte)0x03);
                    writer.Write((byte)0x09);
                    writer.Write((ushort)2);
                    break;
                case EnumLightWorkType.Default:
                    writer.Write((byte)0x01);
                    writer.Write((byte)0x0A);
                    break;
            }
            writer.Write((byte)0xEC);
            try
            {
                var result = _service.SendLightCommand(cmdName, writer.EndWrite(true));
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error($"更新灯光状态失败：{e.Message}");
            }
        }

        private void UpdateLight()
        {
            string cmdName = "SetAllLight";
            List<string> inParams = new List<string>();
            switch (WorkType)
            {
                case EnumLightWorkType.AllOn:
                    cmdName = "SetAllLight";
                    break;
                case EnumLightWorkType.AllOff:
                    cmdName = "SetAllOFF";
                    break;
                case EnumLightWorkType.AllBlink:
                    cmdName = "SetAllBlink";
                    inParams.Add(string.Format("{0}", BlinkPeriod));
                    break;
                case EnumLightWorkType.PartBlink:
                    cmdName = "SetPartBlink";
                    inParams.Add(string.Format("{0}", BlinkPeriod));
                    inParams.Add(string.Format("{0}", NumMask));
                    break;
                case EnumLightWorkType.PartOn:
                    cmdName = "SetPartLight";
                    inParams.Add(string.Format("{0}", NumMask));
                    break;
                case EnumLightWorkType.Breath:
                    cmdName = "SetBreath";
                    inParams.Add(string.Format("{0}", BreathPeriod));
                    break;
                case EnumLightWorkType.SingleFlow:
                    cmdName = "SetFlow";
                    inParams.Add(string.Format("{0}", FlowPeriod));
                    inParams.Add(string.Format("{0}", FlowCount));
                    inParams.Add(string.Format("{0}", (int)FlowDirect));
                    break;
                case EnumLightWorkType.BothwayFlow:
                    cmdName = "SetTwoWayFlow";
                    inParams.Add(string.Format("{0}", FlowPeriod));
                    inParams.Add(string.Format("{0}", FlowCount));
                    break;
                case EnumLightWorkType.Loading:
                    cmdName = "SetLoad";
                    inParams.Add(string.Format("{0}", LoadValue));
                    break;
                case EnumLightWorkType.Default:
                    cmdName = "SetDefault";
                    break;
            }
            try
            {
                bool result = _service.SendSetCommand(cmdName, inParams);
                if (result == false)
                {
                    LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                    this._aggregator.SendMessage("更新灯光状态失败!");
                }
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.Message);
                this._aggregator.SendMessage(e.Message);
            }
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 设置属性变更回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void SetPropertyChangedHandle(PropertyChangedEventHandler handler)
        {
            MotorEnable.PropertyChanged += handler;
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
