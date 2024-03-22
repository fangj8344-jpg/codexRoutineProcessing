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

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;

namespace UtilityTools.Modules.HvController.Model
{
    public class CustomCmdModel : BindableBase
    {
        #region ------------Constructor------------
        public CustomCmdModel(Action<byte[]> sendMethod)
        {
            SendMethod = sendMethod;
            SendCommand = new DelegateCommand(Send);
        }
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

        /// <summary>
        /// 发送指令回调函数
        /// </summary>
        public Action<byte[]>? SendMethod { get; set; }

        #endregion

        #region ------------Command------------
        /// <summary>
        /// 发送代理函数
        /// </summary>
        public DelegateCommand SendCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        public void Send()
        {
            byte[] cmd = DataTypeCaster.StringToByteArray(CmdString);
            SendMethod?.Invoke(cmd);
        }
        #endregion

        #region ------------PrivateMethod------------

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
