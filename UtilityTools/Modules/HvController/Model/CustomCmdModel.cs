#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.HvController.Model
 * 唯一标识：01812bee-4bad-4c3c-a741-2de7e1373f33
 * 文件名：CustomCmdModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/24 17:06:27
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

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.HvController.Model
{
    public class CustomCmdModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private bool _isAutoSend;
        /// <summary>
        /// 是否自动发送
        /// </summary>
        public bool IsAutoSend
        {
            get { return _isAutoSend; }
            set { _isAutoSend = value; RaisePropertyChanged(); }
        }

        private string _cmdString;
        /// <summary>
        /// 指令字符串
        /// </summary>
        public string CmdString
        {
            get { return _cmdString; }
            set { _cmdString = value; RaisePropertyChanged(); }
        }


        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
