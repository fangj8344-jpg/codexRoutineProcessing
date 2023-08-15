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

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.Modules.NetController.Model
{
    public class LightModel : CustomBindableBase
    {
        #region ------------Constructor------------
        public LightModel()
        {
            WorkType = EnumLightWorkType.Default;
            BlinkPeriod = 1000;
            NumMask = 0;
            BreathPeriod = 1000;
            FlowPeriod = 1000;
            FlowCount = 0;
            FlowDirect = EnumFlowDirection.LeftToRight;
            LoadValue = 0;
        }
        #endregion

        #region ------------Field------------
        private EnumLightWorkType _workType;
        private int _blinkPeriod;
        private int _numMask;
        private int _breathPeriod;
        private int _flowPeriod;
        private int _flowCount;
        private EnumFlowDirection _flowDirect;
        private int _loadValue;
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
        #endregion

        #region ------------PublicMethod------------
        
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
