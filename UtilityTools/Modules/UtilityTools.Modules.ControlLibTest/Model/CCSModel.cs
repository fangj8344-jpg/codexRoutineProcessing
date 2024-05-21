#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：60ca7fee-b83c-4623-99f4-844aeb3031b8
 * 文件名：CCSModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 14:32:41
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeptools.CommonLib.Model;

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class CCSModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private string _name = "";
        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private string _realName = "";
        /// <summary>
        /// 实际物理含义
        /// </summary>
        public string RealName
        {
            get { return _realName; }
            set { _realName = value; RaisePropertyChanged(); }
        }

        private byte _channel;
        /// <summary>
        /// 通道信息
        /// </summary>
        public byte Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        private string _relayName = "";
        /// <summary>
        /// 继电器名称
        /// </summary>
        public string RelayName
        {
            get { return _relayName; }
            set { _relayName = value; RaisePropertyChanged(); }
        }

        private byte _relayChannel;
        /// <summary>
        /// 继电器通道
        /// </summary>
        public byte RelayChannel
        {
            get { return _relayChannel; }
            set { _relayChannel = value; RaisePropertyChanged(); }
        }

        private bool _hasRelay;
        /// <summary>
        /// 是否有继电器
        /// </summary>
        public bool HasRelay
        {
            get { return _hasRelay; }
            set { _hasRelay = value;  }
        }

        private bool _relayState;
        /// <summary>
        /// 继电器状态
        /// </summary>
        public bool RelayState
        {
            get { return _relayState; }
            set { _relayState = value; RaisePropertyChanged(); }
        }

        private ushort _value;
        /// <summary>
        /// DAC输出幅值
        /// </summary>
        public ushort Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private ushort _minValue;
        /// <summary>
        /// DAC输出最小幅值
        /// </summary>
        public ushort MinValue
        {
            get { return _minValue; }
            set { _minValue = value; RaisePropertyChanged(); }
        }

        private ushort _maxValue;
        /// <summary>
        /// DAC输出最大幅值
        /// </summary>
        public ushort MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 设置数值方法
        /// </summary>
        public Func<ushort, ResponseProto>? SetValueFunc { get; set; } = null;

        /// <summary>
        /// 设置继电器状态方法
        /// </summary>
        public Func<bool, ResponseProto>? SetStateFunc { get; set; } = null;
        #endregion


        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
