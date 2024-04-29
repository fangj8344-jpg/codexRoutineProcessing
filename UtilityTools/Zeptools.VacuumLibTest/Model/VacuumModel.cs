#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.VacuumLibTest.Model
 * 唯一标识：32406ff7-bc18-4b10-94dd-9e4e2d750f96
 * 文件名：VacuumModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/28 14:13:52
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeptools.CommonLib.Model;

namespace UtilityTools.VacuumLibTest.Model
{
    internal class VacuumModel : NotificationModel
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private float _vacValue1;
        /// <summary>
        /// 通道1真空值
        /// </summary>
        public float VacValue1
        {
            get { return _vacValue1; }
            set { _vacValue1 = value; RaisePropertyChanged(); }
        }

        private float _vacValue2;
        /// <summary>
        /// 通道2真空值
        /// </summary>
        public float VacValue2
        {
            get { return _vacValue2; }
            set { _vacValue2 = value; RaisePropertyChanged(); }
        }

        private float _vacValue3;
        /// <summary>
        /// 通道3真空值
        /// </summary>
        public float VacValue3
        {
            get { return _vacValue3; }
            set { _vacValue3 = value; RaisePropertyChanged(); }
        }

        private float _vacValue4;
        /// <summary>
        /// 通道4真空值
        /// </summary>
        public float VacValue4
        {
            get { return _vacValue4; }
            set { _vacValue4 = value; RaisePropertyChanged(); }
        }

        private float _lowVacValue;
        /// <summary>
        /// 低真空目标值
        /// </summary>
        public float LowVacValue
        {
            get { return _lowVacValue; }
            set { _lowVacValue = value; RaisePropertyChanged(); }
        }

        private float _gateValveValue;
        /// <summary>
        /// 闸板阀开闭量值
        /// </summary>
        public float GateValveValue
        {
            get { return _gateValveValue; }
            set { _gateValveValue = value; RaisePropertyChanged(); }
        }

        private float _leakValveValue;
        /// <summary>
        /// 针阀开闭量值
        /// </summary>
        public float LeakValveValue
        {
            get { return _leakValveValue; }
            set { _leakValveValue = value; RaisePropertyChanged(); }
        }

        private float _p;
        /// <summary>
        /// PID的P参数
        /// </summary>
        public float P
        {
            get { return _p; }
            set { _p = value; RaisePropertyChanged(); }
        }

        private float _i;
        /// <summary>
        /// PID的I参数
        /// </summary>
        public float I
        {
            get { return _i; }
            set { _i = value; RaisePropertyChanged(); }
        }

        private float _d;
        /// <summary>
        /// PID的D参数
        /// </summary>
        public float D
        {
            get { return _d; }
            set { _d = value; RaisePropertyChanged(); }
        }

        private float _caliVol1;
        /// <summary>
        /// 标定输入电压通道1
        /// </summary>
        public float CaliVol1
        {
            get { return _caliVol1; }
            set { _caliVol1 = value; RaisePropertyChanged(); }
        }

        private float _caliVol2;
        /// <summary>
        /// 标定输入电压通道2
        /// </summary>
        public float CaliVol2
        {
            get { return _caliVol2; }
            set { _caliVol2 = value; RaisePropertyChanged(); }
        }

        private float _caliVol3;
        /// <summary>
        /// 标定输入电压通道3
        /// </summary>
        public float CaliVol3
        {
            get { return _caliVol3; }
            set { _caliVol3 = value; RaisePropertyChanged(); }
        }

        private float _caliVol4;
        /// <summary>
        /// 标定输入电压通道4
        /// </summary>
        public float CaliVol4
        {
            get { return _caliVol4; }
            set { _caliVol4 = value; RaisePropertyChanged(); }
        }

        private float _caliParam1;
        /// <summary>
        /// 标定参数通道1
        /// </summary>
        public float CaliParam1
        {
            get { return _caliParam1; }
            set { _caliParam1 = value; RaisePropertyChanged(); }
        }

        private float _caliParam2;
        /// <summary>
        /// 标定参数通道2
        /// </summary>
        public float CaliParam2
        {
            get { return _caliParam2; }
            set { _caliParam2 = value; RaisePropertyChanged(); }
        }

        private float _caliParam3;
        /// <summary>
        /// 标定参数通道3
        /// </summary>
        public float CaliParam3
        {
            get { return _caliParam3; }
            set { _caliParam3 = value; RaisePropertyChanged(); }
        }

        private float _caliParam4;
        /// <summary>
        /// 标定参数通道4
        /// </summary>
        public float CaliParam4
        {
            get { return _caliParam4; }
            set { _caliParam4 = value; RaisePropertyChanged(); }
        }

        private bool _ioState0;
        /// <summary>
        /// IO输出通道0状态
        /// </summary>
        public bool IOState0
        {
            get { return _ioState0; }
            set { _ioState0 = value; RaisePropertyChanged(); }
        }

        private bool _ioState1;
        /// <summary>
        /// IO输出通道1状态
        /// </summary>
        public bool IOState1
        {
            get { return _ioState1; }
            set { _ioState1 = value; RaisePropertyChanged(); }
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
