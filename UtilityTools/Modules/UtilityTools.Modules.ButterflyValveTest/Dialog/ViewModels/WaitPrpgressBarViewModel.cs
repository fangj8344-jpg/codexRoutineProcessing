using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ButterflyValveTest.Dialog.ViewModels
{
    internal class WaitPrpgressBarViewModel : BindableBase, IDialogAware
    {
        public WaitPrpgressBarViewModel() 
        {
            OnConfirmCommand = new DelegateCommand(OnConfirm);
        }

        public string Title => "进度条";

        public event Action<IDialogResult> RequestClose;

        private string _message;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get => _message;
            set { _message = value;RaisePropertyChanged(); }
        }

       
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
           
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //接收外部传递的参数
            if (parameters.ContainsKey("message"))
            {
                Message = parameters.GetValue<string>("message");
            }
        }
        public DelegateCommand OnConfirmCommand { get; set; }
        private void OnConfirm()
        {
            var returnParams = new DialogParameters();
            returnParams.Add("ReturnMsg","OK");
            var dialogResult = new DialogResult(ButtonResult.OK, returnParams);
            RequestClose?.Invoke(dialogResult);
        }
        
    }
}
