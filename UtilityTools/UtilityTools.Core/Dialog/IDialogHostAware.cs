using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Dialog
{
    public interface IDialogHostAware
    {
        /// <summary>
        /// DialogHost名称
        /// </summary>
        public string DialogHostName { get; set; }
        /// <summary>
        /// 确定
        /// </summary>
        public DelegateCommand SaveCommand { get; set; }
        /// <summary>
        /// 取消
        /// </summary>
        public DelegateCommand CancelCommand { get; set; }
        /// <summary>
        /// 打开过程中执行
        /// </summary>
        /// <param name="parameters"></param>
        void OnDialogOpend(IDialogParameters parameters);
    }
}
