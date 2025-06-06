using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Interface
{
    public interface IAuthority
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        string ModuleName { get; set; }

        /// <summary>
        /// 是否永久授权
        /// </summary>
        bool IsAuthority { get; set; }

        /// <summary>
        /// 是否是试用状态
        /// </summary>
        bool IsTrial { get; set; }

        /// <summary>
        /// 试用时间
        /// </summary>
        DateTime TrialTime { get; set; }

    }
}
