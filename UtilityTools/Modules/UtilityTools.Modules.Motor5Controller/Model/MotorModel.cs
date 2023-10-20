using NLog.Fluent;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UtilityTools.Core.Model;

namespace UtilityTools.Modules.Motor5Controller.Model
{
    public class MotorModel : BindableBase
    {
        #region ------------Constructor------------
        public MotorModel()
        {
            SportType = EnumSportsModeTypes.AbsolutePosition;
            MotorStatus = new ObservableCollection<LabelInfoModel>();
            MotorStatus.Add(new LabelInfoModel() { Name = "状态：", Type = "MotorStatus", Channel = 1, Tip = "", Value = "脱机" });
        }
        #endregion

        #region ------------Field------------
        //电机编号
        private int _motorno;
        //电机名称
        private string _motorname;
        //电机状态
        private ObservableCollection<LabelInfoModel> _motorstatus;
        //设置目标位置
        private int _settargetposition;
        //获取目标位置
        private int _obtaintargetposition;
        //获取实时位置
        private int _obtainrealtimeposition;
        //设置原点位置
        private int _setoriginposition;
        //获取原点位置
        private int _obtainoriginposition;
        //设置零点位置
        private int _setzeroposition;
        //设置零点位置
        private int _obtainzeroposition;
        //电机运动模式
        private EnumSportsModeTypes _sportType;
        //电机P参数
        private float _motorpparameter;
        //电机I参数
        private float _motoriparameter;
        //电机D参数
        private float _motordparameter;


        //运动模式（绝对位置目标  相对位置模式  速度模式） -一条
        //pid 设置 读取（组合指令）-- 六条（内存拷贝）
        //停止（锁死停止  断电锁死） 
        //实时位置（定时位置刷新--单独电机）  

        //直流电机
        //电机转动一圈需脉冲数 int
        //传动比
        //丝杆导程 丝杆转一圈位移距离

        //步进电机（步进角 细分数）
        //步进角：度数
        //细分数：一度细分


        #endregion

        #region ------------Property------------
        public int MotorNo {
            get { return _motorno; }
            set
            {
                _motorno = value;
                RaisePropertyChanged();
            }
        }
        public string MotorName
        {
            get { return _motorname; }
            set
            {
                _motorname = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<LabelInfoModel> MotorStatus
        {
            get { return _motorstatus; }
            set { _motorstatus = value; RaisePropertyChanged(); }
        }

        public int SetTargetPosition
        {
            get { return _settargetposition; }
            set
            {
                _settargetposition = value;
                RaisePropertyChanged();
            }
        }

        public int ObtainTargetPosition
        {
            get { return _obtaintargetposition; }
            set
            {
                _obtaintargetposition = value;
                RaisePropertyChanged();
            }
        } 

        public int ObtainRealTimePosition
        {
            get { return _obtainrealtimeposition; }
            set
            {
                _obtainrealtimeposition = value;
                RaisePropertyChanged();
            }
        }

        public int SetOriginPosition
        {
            get { return _setoriginposition; }
            set
            {
                _setoriginposition = value;
                RaisePropertyChanged();
            }
        }

        public int ObtainOriginPosition
        {
            get { return _obtainoriginposition; }
            set
            {
                _obtainoriginposition = value;
                RaisePropertyChanged();
            }
        }

        public int SetZeroPosition
        {
            get { return _setzeroposition; }
            set
            {
                _setzeroposition = value;
                RaisePropertyChanged();
            }
        }

        public int ObtainZeroPosition
        {
            get { return _obtainzeroposition; }
            set
            {
                _obtainzeroposition = value;
                RaisePropertyChanged();
            }
        }


        public EnumSportsModeTypes SportType
        {
            get { return _sportType; }
            set
            {
                _sportType = value;
                RaisePropertyChanged();
                EnumSportsModeTypesChanged?.Invoke(this, value);
            }
        }

        public float MotorPParameter
        {
            get { return _motorpparameter; }
            set
            {
                _motorpparameter = value;
                RaisePropertyChanged();
            }
        }

        public float MotorIParameter
        {
            get { return _motoriparameter; }
            set
            {
                _motoriparameter = value;
                RaisePropertyChanged();
            }
        }

        public float MotorDParameter
        {
            get { return _motordparameter; }
            set
            {
                _motordparameter = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand<object> ButtonEventCommand { get; set; }

        public event EventHandler<EnumSportsModeTypes> EnumSportsModeTypesChanged;
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion


    }
}
