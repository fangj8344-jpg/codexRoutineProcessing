using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MultiChannelHV.ViewModels
{
    class PasswordDialogViewModel: BindableBase, IDialogAware
    {
        // 对话框标题（IDialogAware 要求的属性）
        private string _title = "密码验证";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        // 密码输入值（绑定到视图的PasswordBox）
        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // 命令：确认
        public DelegateCommand ConfirmCommand { get; private set; }
        // 命令：取消
        public DelegateCommand CancelCommand { get; private set; }

        // 对话框关闭请求事件（IDialogAware 要求的事件）
        public event Action<IDialogResult> RequestClose;

        public PasswordDialogViewModel()
        {
            // 初始化命令
            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        // 确认逻辑
        private void OnConfirm()
        {
            // 构造返回结果（通过DialogParameters传递数据）
            var parameters = new DialogParameters();
            parameters.Add("EnteredPassword", Password); // 传递用户输入的密码

            // 关闭对话框，返回"成功"结果
            RequestClose?.Invoke(new Prism.Services.Dialogs.DialogResult(ButtonResult.OK, parameters));
        }

        // 取消逻辑
        private void OnCancel()
        {
            // 关闭对话框，返回"取消"结果
            RequestClose?.Invoke(new Prism.Services.Dialogs.DialogResult(ButtonResult.Cancel));
        }

        // 对话框打开时调用（处理传入的参数）
        public void OnDialogOpened(IDialogParameters parameters)
        {
            // 接收从外部传递的参数（例如标题、提示信息等）
            if (parameters.ContainsKey("Title"))
            {
                Title = parameters.GetValue<string>("Title");
            }
        }

        // 对话框关闭前调用（可验证是否允许关闭）
        public bool CanCloseDialog()
        {
            
            return true; // 允许关闭
        }

        // 对话框关闭后调用（清理资源）
        public void OnDialogClosed()
        {
            // 可选：释放资源、重置状态等
        }
    }
}
