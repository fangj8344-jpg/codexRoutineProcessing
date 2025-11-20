
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;

using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class MotorTypeModel : BindableBase
    {
        public MotorTypeModel(ThreeAxisTestModel motorModel, IContainerProvider containerProvider)
        {
            _motorModel = motorModel;
            _containerProvider = containerProvider;
            _motorModel.Motors = new ObservableCollection<FiveAxisModel> { };
            motorTypeUpdate(); 
            
        }
        private IContainerProvider _containerProvider;
        private ThreeAxisTestModel _motorModel;
        private EnumMotorAxisType? _enumMotorAxisType = Protocol.EnumMotorAxisType.TwoAxisMotor;
        
        /// <summary>
        /// 电机类型
        /// </summary>
        public EnumMotorAxisType? EnumMotorAxisType
        {
            get { return _enumMotorAxisType; }
            set 
            {
                if (_enumMotorAxisType != value)
                {
                    _enumMotorAxisType = value;
                    motorAxisUpdate();
                }
               
                RaisePropertyChanged(); 
                
            }
        }
        private bool _isZem18 = true;
        /// <summary>
        /// 是否是zem18系列
        /// </summary>
        public bool IsZem18
        {
            get { return _isZem18; }
            set { _isZem18 = value; RaisePropertyChanged(); }
        }

        private bool _isFiveAxisMotor = false;
        /// <summary>
        /// 是否是五轴电机
        /// </summary>
        public bool IsFiveAxisMotor
        {
            get { return _isFiveAxisMotor; }
            set
            {
                _isFiveAxisMotor = value;
                if (value)
                {
                    EnumMotorAxisType = Protocol.EnumMotorAxisType.FiveAxisMotor;
                    motorTypeUpdate();
                }
               
                RaisePropertyChanged();
            }
        }

        private bool _isThreeAxisMotor;
        /// <summary>
        /// 是否是三轴电机
        /// </summary>
        public bool IsThreeAxisMotor
        {
            get { return _isThreeAxisMotor; }
            set
            {
                _isThreeAxisMotor = value;
                if (value)
                {
                    EnumMotorAxisType = Protocol.EnumMotorAxisType.ThreeAxisMotor;
                    motorTypeUpdate();
                }
                RaisePropertyChanged();
            }


        }
        private bool _isTwoAxisMotor = true;
        /// <summary>
        /// 是否是两轴电机
        /// </summary>
        public bool IsTwoAxisMotor
        {
            get { return _isTwoAxisMotor; }
            set
            {
                _isTwoAxisMotor = value;
                if (value)
                {
                    EnumMotorAxisType = Protocol.EnumMotorAxisType.TwoAxisMotor;
                    motorTypeUpdate();
                }
                RaisePropertyChanged();
            }
        }
        private bool _isXMotor;
        /// <summary>
        /// 是否是X轴电机
        /// </summary>
        public bool IsXMotor
        {
            get { return _isXMotor; }
            set 
            {
                if (value != _isXMotor)
                {
                    _isXMotor = value;
                    motorAxisUpdate();
                   
                }
                RaisePropertyChanged();


            }
        }
        private bool _isYMotor;
        /// <summary>
        /// 是否是Y轴电机
        /// </summary>
        public bool IsYMotor
        {
            get { return _isYMotor; }
            set 
            {
                if (value != _isYMotor)
                {
                    _isYMotor = value;
                    motorAxisUpdate();
                  
                }
                RaisePropertyChanged();
            }
        }
        private bool _isZMotor;
        /// <summary>
        /// 是否是Z轴电机
        /// </summary>
        public bool IsZMotor
        {
            get { return _isZMotor; }
            set 
            {
                if (value != _isZMotor)
                {
                    _isZMotor = value;
                    motorAxisUpdate();
                }
                 RaisePropertyChanged(); 
            }
        }
        private bool _isTMotor;
        /// <summary>
        /// 是否是T轴电机
        /// </summary>
        public bool IsTMotor
        {
            get { return _isTMotor; }
            set 
            {
                if (value != _isTMotor)
                {
                    _isTMotor = value;
                    motorAxisUpdate();
                }
                RaisePropertyChanged();
            }
        }
        private bool _isRMotor;
        /// <summary>
        /// 是否是R轴电机
        /// </summary>
        public bool IsRMotor
        {
            get { return _isRMotor; }
            set 
            {
                if (value != _isRMotor)
                {
                    _isRMotor = value;
                    motorAxisUpdate();
                }
                    RaisePropertyChanged(); 
            }
        }
        private void motorTypeUpdate()
        {
            switch (EnumMotorAxisType)
            {
                case Protocol.EnumMotorAxisType.TwoAxisMotor:
                    IsXMotor = true;
                    IsYMotor = true;
                    IsZMotor = false;
                    IsTMotor = false;
                    IsRMotor = false;
                    break;
                case Protocol.EnumMotorAxisType.ThreeAxisMotor:
                    IsXMotor = true;
                    IsYMotor = true;
                    IsZMotor = false;
                    IsTMotor = false;
                    IsRMotor = false;
                    break;
                case Protocol.EnumMotorAxisType.FiveAxisMotor:
                    IsXMotor = true;
                    IsYMotor = true;
                    IsZMotor = true;
                    IsTMotor = true;
                    IsRMotor = true;
                    break;
            }
        }
        private void motorAxisUpdate()
        {

            if (EnumMotorAxisType == Protocol.EnumMotorAxisType.TwoAxisMotor)
            {
                _motorModel.Motors.Clear();
                _motorModel.MotorplotModel.Series.Clear();
                _motorModel.MotorSpeedplotModel.Series.Clear();
                if (IsXMotor == true)
                {
                    if (_motorModel.XAxis == null)
                    {
                        _motorModel.XAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_1, EnumMotorModel.MOTOR_x, _motorModel, "X轴") { };
                    }
                    else
                    {
                        _motorModel.XAxis.EnumMotorId = EnumMotorId.MOTOR_1;
                    }
                    _motorModel.Motors.Add(_motorModel.XAxis);
            
                }
                if (IsYMotor == true)
                {
                    if (_motorModel.YAxis == null)
                    {
                        _motorModel.YAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_2, EnumMotorModel.MOTOR_y, _motorModel, "Y轴") { };
                    }
                    else
                    {
                        _motorModel.YAxis.EnumMotorId = EnumMotorId.MOTOR_2;
                    }
                    _motorModel.Motors.Add(_motorModel.YAxis);
                }
             
            }
            else
            {
                _motorModel.Motors.Clear();
                _motorModel.MotorplotModel.Series.Clear();
                _motorModel.MotorSpeedplotModel.Series.Clear();
                if (IsXMotor == true)
                {
                    if (_motorModel.XAxis == null)
                    {
                        _motorModel.XAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_2, EnumMotorModel.MOTOR_x, _motorModel, "X轴") { };
                    }
                    else
                    {
                        _motorModel.XAxis.EnumMotorId = EnumMotorId.MOTOR_2;
                    }
                    _motorModel.Motors.Add(_motorModel.XAxis);
                }
                
                if (IsYMotor == true)
                {
                    if (_motorModel.YAxis == null)
                    {
                        _motorModel.YAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_1, EnumMotorModel.MOTOR_y, _motorModel, "Y轴") { };
                    }
                    else
                    {
                        _motorModel.YAxis.EnumMotorId = EnumMotorId.MOTOR_1;
                    }
                    _motorModel.Motors.Add(_motorModel.YAxis);
                }
            }
            if (IsZMotor == true)
            {
                if (_motorModel.ZAxis == null)
                {
                    _motorModel.ZAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_4, EnumMotorModel.MOTOR_z, _motorModel, "Z轴") { };
                }
                
                _motorModel.Motors.Add(_motorModel.ZAxis);
            }
            if (IsTMotor == true)
            {
                if (_motorModel.TAxis == null)
                {
                    _motorModel.TAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_3, EnumMotorModel.MOTOR_t, _motorModel, "T轴") { };
                }
               
                _motorModel.Motors.Add(_motorModel.TAxis);
            }
            
            if (IsRMotor == true)
            {
                if (_motorModel.RAxis == null)
                {
                    _motorModel.RAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_5, EnumMotorModel.MOTOR_r, _motorModel, "R轴") { };
                }
                
                _motorModel.Motors.Add(_motorModel.RAxis);
            }
            AxisUpdate();
        }
        

        private void AxisUpdate()
        {
            RemoveLine();
            if (_motorModel.Motors != null && _motorModel.Motors.Count > 0)
            {
                for (int i = 0; i < _motorModel.Motors.Count; i++)
                {
                    _motorModel.Motors[i].SpeedLine.ItemsSource = _motorModel.Motors[i].MotorModel.SpeedList;
                    _motorModel.Motors[i].SpeedLine.DataFieldX = "SpeedDate";
                    _motorModel.Motors[i].SpeedLine.DataFieldY = "Speed";
                    _motorModel.Motors[i].PosLine.ItemsSource = _motorModel.Motors[i].MotorModel.PointList;
                    _motorModel.Motors[i].PosLine.DataFieldX = "Date";
                    _motorModel.Motors[i].PosLine.DataFieldY = "Point";
                    _motorModel.MotorplotModel.Series.Add(_motorModel.Motors[i].PosLine);
                    _motorModel.MotorSpeedplotModel.Series.Add(_motorModel.Motors[i].SpeedLine);
                   var x =  _motorModel.Motors[i].MotorModel.MotorModelAxis;

                    _motorModel.Motors[i].ConfirmTheStandardStroke();
                }
                
            }
        }

        private void RemoveLine()
        {
            if (_motorModel.XAxis!= null&&_motorModel.MotorplotModel.Series.Contains(_motorModel.XAxis.PosLine))
            {
                _motorModel.MotorplotModel.Series.Remove(_motorModel.XAxis.PosLine);
            }
            if (_motorModel.XAxis != null && _motorModel.MotorSpeedplotModel.Series.Contains(_motorModel.XAxis.SpeedLine))
            {
                _motorModel.MotorSpeedplotModel.Series.Remove(_motorModel.XAxis.SpeedLine);
            }

            if (_motorModel.YAxis != null && _motorModel.MotorplotModel.Series.Contains(_motorModel.YAxis.PosLine))
            {
                _motorModel.MotorplotModel.Series.Remove(_motorModel.YAxis.PosLine);
            }
            if (_motorModel.YAxis != null && _motorModel.MotorSpeedplotModel.Series.Contains(_motorModel.YAxis.SpeedLine))
            {
                _motorModel.MotorSpeedplotModel.Series.Remove(_motorModel.YAxis.SpeedLine);
            }

            if (_motorModel.ZAxis != null && _motorModel.MotorplotModel.Series.Contains(_motorModel.ZAxis.PosLine))
            {
                _motorModel.MotorplotModel.Series.Remove(_motorModel.ZAxis.PosLine);
            }
            if (_motorModel.ZAxis != null && _motorModel.MotorSpeedplotModel.Series.Contains(_motorModel.ZAxis.SpeedLine))
            {
                _motorModel.MotorSpeedplotModel.Series.Remove(_motorModel.ZAxis.SpeedLine);
            }

            if (_motorModel.TAxis != null && _motorModel.MotorplotModel.Series.Contains(_motorModel.TAxis.PosLine))
            {
                _motorModel.MotorplotModel.Series.Remove(_motorModel.TAxis.PosLine);
            }
            if (_motorModel.TAxis != null && _motorModel.MotorSpeedplotModel.Series.Contains(_motorModel.TAxis.SpeedLine))
            {
                _motorModel.MotorSpeedplotModel.Series.Remove(_motorModel.TAxis.SpeedLine);
            }

            if (_motorModel.RAxis != null && _motorModel.MotorplotModel.Series.Contains(_motorModel.RAxis.PosLine))
            {
                _motorModel.MotorplotModel.Series.Remove(_motorModel.RAxis.PosLine);
            }
            if (_motorModel.RAxis != null && _motorModel.MotorSpeedplotModel.Series.Contains(_motorModel.RAxis.SpeedLine))
            {
                _motorModel.MotorSpeedplotModel.Series.Remove(_motorModel.RAxis.SpeedLine);
            }
        }
       
    }
}
