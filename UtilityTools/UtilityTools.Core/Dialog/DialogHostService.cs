using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UtilityTools.Core.Dialog
{
    public class DialogHostService : DialogService, IDialogHostService
    {

        #region 字段
        private readonly IContainerExtension _containerExtension;
        #endregion

        #region PublicFunction
        public DialogHostService(IContainerExtension containerExtension) : base(containerExtension)
        {
            this._containerExtension = containerExtension;
        }

        public async Task<IDialogResult> ShowDialog(string name, IDialogParameters parameters, string dialogHostName = "Root")
        {
            if (parameters == null)
                parameters = new DialogParameters();

            // 从容器中取出弹出窗口的实例
            var content = _containerExtension.Resolve<object>(name);

            // 验证窗口的合法性
            if (!(content is FrameworkElement dialogContent))
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            if (content is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
            {
                ViewModelLocator.SetAutoWireViewModel(view, true);
            }

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
            {
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogHostAware interface");
            }

            viewModel.DialogHostName = dialogHostName;
            DialogOpenedEventHandler eventHandler = (s, e) =>
            {
                if (viewModel is IDialogHostAware dialogHostAware)
                {
                    dialogHostAware.OnDialogOpend(parameters);
                }
                e.Session.UpdateContent(content);
            };

            var result = await DialogHost.Show(dialogContent, viewModel.DialogHostName, eventHandler);
            return (IDialogResult)result;
        }
        #endregion

    }
}
