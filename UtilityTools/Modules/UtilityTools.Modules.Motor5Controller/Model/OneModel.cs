using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Modules.Motor5Controller.Model
{
    public class OneModel : BindableBase
    {
        #region ------------Constructor------------
        public OneModel()
        {
            //电机状态
            MotorStatus = "失能";

            //设置目标位置
            SetTargetPosition = 1;
            //获取目标位置
            ObtainTargetPosition = 2;
            //获取实时位置
            ObtainRealTimePosition = 3;
            //设置原点位置
            SetOriginPosition = 4;
            //获取原点位置
            ObtainOriginPosition = 5 ;
            //设置零点位置
            SetZeroPosition = 6;
            //设置零点位置
            ObtainZeroPosition = 7;
        }
        #endregion

        #region ------------Field------------
        private string _motorstatus;
        private int _settargetposition;
        private int _obtaintargetposition;
        private int _obtainrealtimeposition;
        private int _setoriginposition;
        private int _obtainoriginposition;
        private int _setzeroposition;
        private int _obtainzeroposition;
        #endregion

        #region ------------Property------------
        /// <summary>
        ///电机状态
        /// </summary>
        public string MotorStatus
        {
            get { return _motorstatus; }
            set
            {
                _motorstatus = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///设置目标位置
        /// </summary>
        public int SetTargetPosition
        {
            get { return _settargetposition; }
            set
            {
                _settargetposition = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///设置目标位置
        /// </summary>
        public int ObtainTargetPosition
        {
            get { return _obtaintargetposition; }
            set
            {
                _obtaintargetposition = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///获取实时位置
        /// </summary>
        public int ObtainRealTimePosition
        {
            get { return _obtainrealtimeposition; }
            set
            {
                _obtainrealtimeposition = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///设置原点位置
        /// </summary>
        public int SetOriginPosition
        {
            get { return _setoriginposition; }
            set
            {
                _setoriginposition = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///获取原点位置
        /// </summary>
        public int ObtainOriginPosition
        {
            get { return _obtainoriginposition; }
            set
            {
                _obtainoriginposition = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        ///设置零点位置
        /// </summary>
        public int SetZeroPosition
        {
            get { return _setzeroposition; }
            set
            {
                _setzeroposition = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///设置零点位置
        /// </summary>
        public int ObtainZeroPosition
        {
            get { return _obtainzeroposition; }
            set
            {
                _obtainzeroposition = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand<object> ButtonEventCommand { get; set; }

        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion


    }
}
