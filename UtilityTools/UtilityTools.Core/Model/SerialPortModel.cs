#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：7b182b1f-2f08-464f-8940-921129111ae6
 * 文件名：SerialPortModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:39:58
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
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public class SerialPortModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        private SerialPort _serialPort = new SerialPort();
        private string _portName;
        private int _baudRate = 115200;
        private Parity _parityType = Parity.None;
        private StopBits _stopBitsType = StopBits.One;
        private int _dataBits = 8;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 串口实例
        /// </summary>
        public SerialPort SerialPort
        {
            get { return _serialPort; }
            set { _serialPort = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 校验码
        /// </summary>
        public Parity ParityType
        {
            get { return _parityType; }
            set { _parityType = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBitsType
        {
            get { return _stopBitsType; }
            set { _stopBitsType = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; RaisePropertyChanged(); }
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
