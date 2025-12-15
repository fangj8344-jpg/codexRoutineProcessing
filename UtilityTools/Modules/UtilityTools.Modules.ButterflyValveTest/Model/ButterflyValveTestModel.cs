using CsvHelper;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IDialogService _dialogService;
        
        private ObservableCollection<ButterflyValveModel> _butterflyValveModels;
        public ObservableCollection<ButterflyValveModel> ButterflyValveModels
        {
            get { return _butterflyValveModels; }
            set { _butterflyValveModels = value;RaisePropertyChanged(); }
        }
        private void InitProperty()
        {
            ButterflyValveModels = new ObservableCollection<ButterflyValveModel>();
            for (int i = 0; i < 1; i++)
            {
                var model = new ButterflyValveModel(this);
                ButterflyValveModels.Add(model);
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
