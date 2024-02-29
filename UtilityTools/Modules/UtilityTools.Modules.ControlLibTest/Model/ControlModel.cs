#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：89ada8ee-71dc-4026-8bc6-1fce46249d4a
 * 文件名：ControlModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 14:31:15
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

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class ControlModel : BindableBase
    {
        #region ------------Constructor------------
        public ControlModel()
        {
            CCSModels.Add(new CCSModel() { Name = "CHA0", RelayName = "CCSk2", RelayChannel = (byte)0x02, Channel = (byte)0xA0, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHA1", RelayName = "CCSk3", RelayChannel = (byte)0x03, Channel = (byte)0xA1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHA2", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xA2, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB0", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB0, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB1", RelayName = "CCSk6", RelayChannel = (byte)0x06, Channel = (byte)0xB1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB2", RelayName = "CCSk7", RelayChannel = (byte)0x07, Channel = (byte)0xB2, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB3", RelayName = "CCSk5", RelayChannel = (byte)0x05, Channel = (byte)0xB3, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB4", RelayName = "CCSk4", RelayChannel = (byte)0x04, Channel = (byte)0xB4, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB5", RelayName = "CCSk0", RelayChannel = (byte)0x00, Channel = (byte)0xB5, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            //CCSModels.Add(new CCSModel() { Name = "CHB6", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB6, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB7", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB7, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0xFFFF, RelayState = false });
            CCSModels.Add(new CCSModel() { Name = "CHB8", RelayName = "CCSk1", RelayChannel = (byte)0x01, Channel = (byte)0xB8, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false });
        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private ObservableCollection<CCSModel> _ccsModels = new ObservableCollection<CCSModel>();

        public ObservableCollection<CCSModel> CCSModels
        {
            get { return _ccsModels; }
            set { _ccsModels = value; RaisePropertyChanged(); }
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
