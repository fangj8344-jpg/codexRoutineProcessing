using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UtilityTools.Core.Model;
using System.Threading.Tasks;
using UtilityTools.Modules.FZBPumpController.Protocol;
using Prism.Ioc;

namespace UtilityTools.Modules.FZBPumpController.Model
{
    internal class SwitchControlModel:BindableBase
    {

        #region ----------Constructor------------
        public SwitchControlModel(IContainerProvider containerProvider) {
            
            AllLabelsModels = new ObservableCollection<AllLabelsModel>();
            AllLabelsModels.Add(new AllLabelsModel() { Name = "待机", Destination = "001", Value = false , Data = "", Param = "002" , SetSendSwitchAction= SendSwitchControlMessage }) ;
            AllLabelsModels.Add(new AllLabelsModel() { Name = "泵组", Destination = "001" , Value = false, Data = "" , Param = "010" , SetSendSwitchAction = SendSwitchControlMessage });
            AllLabelsModels.Add(new AllLabelsModel() { Name = "电动泵" , Destination = "001" , Value = false , Data = "" ,Param = "023" , SetSendSwitchAction = SendSwitchControlMessage });
            AllLabelsModels.Add(new AllLabelsModel() { Name = "转速设置模式" , Destination = "001" , Value = false , Data = "" ,Param = "026", SetSendSwitchAction = SendSwitchControlMessage });
            FbModel = new FZBModel(containerProvider);
        }
        #endregion

        #region ----------Field------------------
        private ObservableCollection<AllLabelsModel> _allLabelsModels;

        private FZBModel _fbModel;
        #endregion

        #region ----------Property---------------
        public ObservableCollection<AllLabelsModel> AllLabelsModels
        {
            get { return _allLabelsModels; }
            set { _allLabelsModels = value; RaisePropertyChanged();  }
        }

        public FZBModel FbModel 
        {
            get { return _fbModel; }
            set { _fbModel = value; RaisePropertyChanged(); } 
        }
        #endregion

        #region ----------PublicMethod-----------
        #endregion

        #region ----------PrivateMethod----------
        private void SendSwitchControlMessage(bool IsOpen,string Destination,string Param)
        {
            byte[] bytes = FZBPumpControllerProtocol.SendSwitchControlMessage(IsOpen,Destination,Param);
            FbModel.SendMsg(bytes);
        }
        #endregion

        #region ----------StaticMethod-----------

        #endregion
    }
}
