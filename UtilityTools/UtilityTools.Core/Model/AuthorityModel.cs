#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：ca96e75f-dfc7-41d3-b3b2-e7c51ddcb473
 * 文件名：AuthorityModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/6/6 14:22:09
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Interface;

namespace UtilityTools.Core.Model
{
    public class AuthorityItem : BindableBase
    {
        public AuthorityItem(IAuthority authority)
        {
            ModuleName = authority.ModuleName;
            IsActived = authority.IsAuthority;
            IsTrial = authority.IsTrial;
            TrialTime = authority.TrialTime;
        }

        public AuthorityItem(string name, bool isActive, bool isTrial, DateTime validity)
        {
            ModuleName = name;
            IsActived = isActive;
            IsTrial = isTrial;
            TrialTime = validity;
        }

        public AuthorityItem()
        {

        }

        private string? _moduleName;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string? ModuleName
        {
            get { return _moduleName; }
            set { _moduleName = value; RaisePropertyChanged(); }
        }

        private bool _isActived;
        /// <summary>
        /// 是否永久激活
        /// </summary>
        public bool IsActived
        {
            get { return _isActived; }
            set { _isActived = value; RaisePropertyChanged(); }
        }

        private bool _isTrial;
        /// <summary>
        /// 是否是试用期
        /// </summary>
        public bool IsTrial
        {
            get { return _isTrial; }
            set { _isTrial = value; RaisePropertyChanged(); }
        }

        private DateTime _trialTime;
        /// <summary>
        /// 试用期时间
        /// </summary>
        public DateTime TrialTime
        {
            get { return _trialTime; }
            set { _trialTime = value; RaisePropertyChanged(); }
        }

    }

    public class AuthorityModel : BindableBase
    {
        private string? _machineCode;

        public string? MachineCode
        {
            get { return _machineCode; }
            set { _machineCode = value; RaisePropertyChanged(); }
        }

        private string? _privateKey;

        public string? PrivateKey
        {
            get { return _privateKey; }
            set { _privateKey = value; RaisePropertyChanged(); }
        }

        private string? _publicKey;

        public string? PublicKey
        {
            get { return _publicKey; }
            set { _publicKey = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<AuthorityItem>? _authorityItems;

        public ObservableCollection<AuthorityItem>? AuthorityItems
        {
            get { return _authorityItems; }
            set { _authorityItems = value; RaisePropertyChanged(); }
        }


    }
}
