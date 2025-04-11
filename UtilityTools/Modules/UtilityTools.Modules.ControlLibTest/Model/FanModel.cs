#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：f06977f7-55ba-4346-bbf9-509e70536958
 * 文件名：FanModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 14:37:14
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

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class FanModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private byte _channel;

        public byte Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        private byte _speed;

        public byte Speed
        {
            get { return _speed; }
            set { _speed = value; RaisePropertyChanged(); }
        }

        private byte _minSpeed;

        public byte MinSpeed
        {
            get { return _minSpeed; }
            set { _minSpeed = value; RaisePropertyChanged(); }
        }

        private byte _maxSpeed;

        public byte MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; RaisePropertyChanged(); }
        }

        public Action<byte>? SetValueFunc { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
