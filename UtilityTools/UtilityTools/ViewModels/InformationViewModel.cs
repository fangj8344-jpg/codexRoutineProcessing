#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.ViewModels
 * 唯一标识：1a3611a2-f5c4-455f-86f7-09d16b1e656c
 * 文件名：InformationViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/12 15:47:04
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;

namespace UtilityTools.ViewModels
{
    public class InformationViewModel : BindableBase, IDialogHostAware
    {
        #region ------------Constructor------------
        public InformationViewModel()
        {
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Command------------
        public DelegateCommand SaveCommand { get; set; }

        public DelegateCommand CancelCommand { get; set; }
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 会话主机名称
        /// </summary>
        public string DialogHostName { get; set; }

        private string _title;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private string _infomation;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Infomation
        {
            get { return _infomation; }
            set { _infomation = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        public void OnDialogOpend(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Title"))
                Title = parameters.GetValue<string>("Title");

            if (parameters.ContainsKey("Content"))
                Infomation = parameters.GetValue<string>("Content");

            if (parameters.ContainsKey("DialogHostName"))
                DialogHostName = parameters.GetValue<string>("DialogHostName");
        }
        #endregion

        #region ------------PrivateMethod------------
        private void Save()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, parameters));
            }
        }

        private void Cancel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters parameters = new DialogParameters();
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Cancel, parameters));
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
