using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// 开发板烧录的文件信息
    /// </summary>
    public class DevelopmentBoardMessage
    {
        //板卡类型
        public UInt16 DevelopmentBoardType { get; set; }
        //烧录文件的版本号
        public string VersionNumber { get; set; }
        //版本变更信息
        public string UpdataInformation { get; set; }
        //备注信息
        public string Description { get; set; }
    }
    
}
