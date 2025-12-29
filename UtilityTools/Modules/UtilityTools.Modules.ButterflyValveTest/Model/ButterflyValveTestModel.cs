using CsvHelper;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.ButterflyValveTest.Model
{
    public class ButterflyValveTestModel:BindableBase
    {
        public ButterflyValveTestModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            InitProperty();
        }

        private bool _isOldButterflyValve = true;
        public bool IsOldButterflyValve
        {
            get { return _isOldButterflyValve; }
            set 
            {
                ButterflyValveModels?.Clear();
                OldButterflyValveModels?.Clear();
                if (value == true)
                {
                   
                    for (int i = 0; i < 1; i++)
                    {
                        var model = new OldButterflyValveModel(this);
                        OldButterflyValveModels.Add(model);
                    }
                }
                else
                {
                    for (int i = 0; i < 1; i++)
                    {
                        var model = new ButterflyValveModel(this);
                        ButterflyValveModels.Add(model);
                    }
                }
                _isOldButterflyValve = value;
                RaisePropertyChanged(); 
            }
        }
        private IDialogService _dialogService;
        
        private ObservableCollection<ButterflyValveModel> _butterflyValveModels;
        public ObservableCollection<ButterflyValveModel> ButterflyValveModels
        {
            get { return _butterflyValveModels; }
            set { _butterflyValveModels = value;RaisePropertyChanged(); }
        }
        private ObservableCollection<OldButterflyValveModel> _oldButterflyValveModels;
        public ObservableCollection<OldButterflyValveModel> OldButterflyValveModels
        {
            get { return _oldButterflyValveModels; }
            set { _oldButterflyValveModels = value; RaisePropertyChanged(); }
        }
        private void InitProperty()
        {
            ButterflyValveModels = new ObservableCollection<ButterflyValveModel>();
            OldButterflyValveModels = new ObservableCollection<OldButterflyValveModel>();
            for (int i = 0; i < 1; i++)
            {
                var model = new OldButterflyValveModel(this);
                OldButterflyValveModels.Add(model);
            }


        }
        public string ShowDialog(string message)
        {
          
            string returnMsg = null;
            var parameters = new DialogParameters();
            parameters.Add("message", message);
            _dialogService.ShowDialog("WaitPrpgressBarView", parameters, result =>
            {
                if (result.Parameters.ContainsKey("ReturnMsg"))
                {
                    returnMsg = result.Parameters.GetValue<string>("ReturnMsg");
                }
              
            });
            return returnMsg;


        }
    }
}
