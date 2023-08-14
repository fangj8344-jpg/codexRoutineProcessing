#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：2f7bcad8-8ff5-494a-9a23-8412e7a2a342
 * 文件名：UsbInfoModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:34:31
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    /// <summary>
    /// USB系统信息
    /// </summary>
    public class UsbInfoModel : BindableBase
    {
        #region ------------Constructor------------
        public UsbInfoModel(int pid, int vid)
        {
            Pid = pid;
            Vid = vid;
            Info = string.Format("USB\\VID_{0:X4}&PID_{1:X4}", Vid, Pid);
        }
        #endregion

        #region ------------Field------------
        private int _pid;
        private int _vid;
        private string _info;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// USB的PID
        /// </summary>
        public int Pid
        {
            get { return _pid; }
            set { _pid = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// USB的VID
        /// </summary>
		public int Vid
        {
            get { return _vid; }
            set { _vid = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// USB的格式信息
        /// </summary>
        public string Info
        {
            get { return _info; }
            set { _info = value; RaisePropertyChanged(); }
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
