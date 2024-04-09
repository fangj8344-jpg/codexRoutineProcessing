using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public class AllLabelsModel:BindableBase
    {
        #region -----------Constructor-------------
        public AllLabelsModel() {
            this.SendSwitchControlCommand = new DelegateCommand(SendSwitchControl);
            this.DataReadbackControlCommand = new DelegateCommand(DataReadbackControl);
            this.DataDeliveryControlCommand = new DelegateCommand(DataDeliveryControl);
        }
        #endregion

        private void SendSwitchControl()
        {
            SetSendSwitchAction?.Invoke(this.Value,this.Destination,this.Param);
        }
        private void DataReadbackControl()
        {
            DataReadbackControlAction?.Invoke(this.Destination,this.Param);
        }
        private void DataDeliveryControl()
        {
            DataDeliveryControlAction?.Invoke(this.Data,this.Destination,this.Param);
        }

        #region -----------Field-------------------
        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); } 
        }

        private string _destination;

        public string Destination
        {
            get { return _destination; }
            set { _destination = value; RaisePropertyChanged(); }
        }

        private bool _value;
        /// <summary>
        /// 数值
        /// </summary>
        public bool Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private string _data;

        public string Data
        {
            get { return _data; }
            set { _data = value; RaisePropertyChanged(); }
        }

        private string _param;
        /// <summary>
        /// 参数
        /// </summary>
        public string Param
        {
            get { return _param; }
            set { _param = value; RaisePropertyChanged(); }
        }

        public Action<bool,string,string> SetSendSwitchAction { get; set; }

        public Action<string,string> DataReadbackControlAction { get; set; }

        public Action<string,string,string> DataDeliveryControlAction { get; set; }

        public DelegateCommand SendSwitchControlCommand { get; set; }

        public DelegateCommand DataReadbackControlCommand { get; set; }

        public DelegateCommand DataDeliveryControlCommand { get; set; }
        #endregion

        #region ------------Property---------------
        #endregion

        #region ------------PublicMethod-----------
        #endregion

        #region ------------PrivateMethod----------
        #endregion

        #region ------------StaticMethod-----------
        #endregion
    }
}
