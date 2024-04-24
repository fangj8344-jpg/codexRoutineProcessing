#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.CmdTestTool.Model
 * 唯一标识：dd27b385-d8d5-4332-95fc-8b23c9842b9f
 * 文件名：CmdModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/1/9 11:47:09
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

using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.CmdTestTool.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CmdModel : BindableBase
    {
        public CmdModel()
        {
            CmdType = "00 00";
			CmdData = string.Empty;

			SendCommand = new DelegateCommand(Send);
        }

        private string _cmdType;
        [JsonProperty]
        public string CmdType
		{
			get { return _cmdType; }
			set { _cmdType = value; RaisePropertyChanged(); }
		}

		private string _cmdData;
        [JsonProperty]
        public string CmdData
		{
			get { return _cmdData; }
			set { _cmdData = value; RaisePropertyChanged(); }
		}

		public DelegateCommand SendCommand { get; set; }

		public event EventHandler<byte[]> RequestEvent;

		private void Send()
		{
			byte[] id = new byte[] { 0x00, 0x00};
			byte[] cmd = DataTypeCaster.StringToByteArray(CmdType);
			byte[] data = DataTypeCaster.StringToByteArray(CmdData);

            // 五轴电机的data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                data = tmp;
            }

            RequestEvent?.Invoke(this, ZepGenericProtocol.GetCmd(id, cmd, data));
        }
    }
}
