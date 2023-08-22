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
    public class MotorModel : BindableBase
    {
        #region ------------Constructor------------
        public MotorModel()
        {
            MotorStatus = new ObservableCollection<LabelInfoModel>();
            MotorStatus.Add(new LabelInfoModel() { Name = "状态：", Type = "MotorStatus", Channel = 1, Tip = "", Value = "失能" });
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
