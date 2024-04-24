#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：8c1cfa9f-58b6-4297-be07-705e00691d99
 * 文件名：ScanModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 16:58:09
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

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class ScanModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private ushort _minValue = 0;

        public ushort MinValue
        {
            get { return _minValue; }
            set { _minValue = value; RaisePropertyChanged(); }
        }

        private ushort _maxValue = 0xFFFF;

        public ushort MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; RaisePropertyChanged(); }
        }

        private ushort _ampX;

        public ushort AmpX
        {
            get { return _ampX; }
            set { _ampX = value; RaisePropertyChanged(); }
        }

        private ushort _ampY;

        public ushort AmpY
        {
            get { return _ampY; }
            set { _ampY = value; RaisePropertyChanged(); }
        }

        private ushort _ampTX;

        public ushort AmpTX
        {
            get { return _ampTX; }
            set { _ampTX = value; RaisePropertyChanged(); }
        }

        private ushort _ampTY;

        public ushort AmpTY
        {
            get { return _ampTY; }
            set { _ampTY = value; RaisePropertyChanged(); }
        }

        private ushort _analogSpinA;

        public ushort AnalogSpinA
        {
            get { return _analogSpinA; }
            set { _analogSpinA = value; RaisePropertyChanged(); }
        }

        private ushort _analogSpinB;

        public ushort AnalogSpinB
        {
            get { return _analogSpinB; }
            set { _analogSpinB = value; RaisePropertyChanged(); }
        }

        private ushort _analogSpinC;

        public ushort AnalogSpinC
        {
            get { return _analogSpinC; }
            set { _analogSpinC = value; RaisePropertyChanged(); }
        }

        private ushort _analogSpinD;

        public ushort AnalogSpinD
        {
            get { return _analogSpinD; }
            set { _analogSpinD = value; RaisePropertyChanged(); }
        }

        private bool _k1;

        public bool K1
        {
            get { return _k1; }
            set { _k1 = value; RaisePropertyChanged(); }
        }

        private bool _k2;

        public bool K2
        {
            get { return _k2; }
            set { _k2 = value; RaisePropertyChanged(); }
        }

        private bool _k3;

        public bool K3
        {
            get { return _k3; }
            set { _k3 = value; RaisePropertyChanged(); }
        }

        private bool _k4;

        public bool K4
        {
            get { return _k4; }
            set { _k4 = value; RaisePropertyChanged(); }
        }

        private bool _sw;

        public bool Sw
        {
            get { return _sw; }
            set { _sw = value; RaisePropertyChanged(); }
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
