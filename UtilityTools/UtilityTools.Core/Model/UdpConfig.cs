#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：a4c68a79-e0b0-4fbf-82fc-a13f6e9ba50d
 * 文件名：UdpConfig
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:36:06
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

namespace UtilityTools.Core.Model
{
    public class UdpConfig : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        private string _targetIp;
        private int _targetPort;
        private string _hostIp;
        private int _hostPort;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 目标IP
        /// </summary>
        public string TargetIp
        {
            get { return _targetIp; }
            set { _targetIp = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 目标端口号
        /// </summary>
        public int TargetPort
        {
            get { return _targetPort; }
            set { _targetPort = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 本地IP
        /// </summary>
        public string HostIp
        {
            get { return _hostIp; }
            set { _hostIp = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 本地端口号
        /// </summary>
        public int HostPort
        {
            get { return _hostPort; }
            set { _hostPort = value; RaisePropertyChanged(); }
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
