using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    /// <summary>
    /// 消息数据模型
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// 消息过滤器
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 消息体本身
        /// </summary>
        public string Message { get; set; }
    }
}
