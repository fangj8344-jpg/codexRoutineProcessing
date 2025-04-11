#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：38cb7d3a-496e-45c7-af6f-0a24d558dc0c
 * 文件名：LedModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 14:46:58
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

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class LedModel : BindableBase
    {
        #region ------------Constructor------------

        public LedModel() {
           // this.UpdateLightCommand = new DelegateCommand(SetUpdateLight);
        }
        #endregion
        /*
        private void SetUpdateLight()
        {
            byte[] content;
            switch (this.Name.Split(": ")[1])
            {
                case "静态全亮":
                    content = new byte[4];
                    content[0]= 0xCE;
                    content[1] = 0x01;
                    content[2] = 0x01;
                    content[3] = 0xEC;
                    byte len = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len, content);
                    break;
                case "静态全灭":
                    content = new byte[4];
                    content[0] = 0xCE;
                    content[1] = 0x01;
                    content[2] = 0x02;
                    content[3] = 0xEC;
                    byte len1 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len1, content);
                    break;
                case "全亮闪烁":
                    content = new byte[6];
                    content[0] = 0xCE;
                    content[1] = 0x03;
                    content[2] = 0x03;
                    Array.Copy(BitConverter.GetBytes(this.TimeCycle), 0, content, 3, 2);
                    content[5] = 0xEC;
                    byte len2 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len2, content);
                    break;
                case "分段闪烁":
                    content = new byte[8];
                    content[0] = 0xCE;
                    content[1] = 0x05;
                    content[2] = 0x04;
                    Array.Copy(BitConverter.GetBytes(this.TimeCycle), 0, content, 3, 2);
                    //Array.Copy(BitConverter.GetBytes(((ushort)this.DivideLightModel).Revert()), 0, content, 5, 2);
                    content[7] = 0xCE;
                    byte len3 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len3, content);
                    break;
                case "呼吸":
                    content = new byte[6];
                    content[0] = 0xCE;
                    content[1] = 0x03;
                    content[2] = 0x05;
                    Array.Copy(BitConverter.GetBytes(this.TimeCycle), 0, content, 3, 2);
                    content[5] = 0xEC;
                    byte len4 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len4, content);
                    break;
                case "流水（单向）":
                    content = new byte[8];
                    content[0] = 0xCE;
                    content[1] = 0x05;
                    content[2] = 0x06;
                    Array.Copy(BitConverter.GetBytes(this.TimeCycle), 0, content, 3, 2);
                    content[5] = this.LightNumber;
                    content[6] = (byte)this.Orient; 
                    content[7] = 0xEC;
                    byte len5 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len5, content);
                    break;
                case "流水（往复）":
                    content = new byte[7];
                    content[0] = 0xCE;
                    content[1] = 0x04;
                    content[2] = 0x07;
                    Array.Copy(BitConverter.GetBytes(this.TimeCycle), 0, content, 3, 2);
                    content[5] = 0x00;
                    content[6] = 0xEC;
                    byte len6 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len6, content);
                    break;
                case "进度条":
                    content = new byte[5];
                    content[0] = 0xCE;
                    content[1] = 0x02;
                    content[2] = 0x08;
                    content[3] = Value;
                    content[4] = 0xEC;
                    byte len7 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len7, content);
                    break;
                case "静态分段点亮":
                    content = new byte[6];
                    content[0] = 0xCE;
                    content[1] = 0x03;
                    content[2] = 0x09;
                    //Array.Copy(BitConverter.GetBytes(((ushort)this.StaticLightModel).Revert()), 0, content, 3, 2);
                    content[5] = 0xEC;
                    byte len8 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len8, content);
                    break;
                case "默认模式":
                    content = new byte[4];
                    content[0] = 0xCE;
                    content[1] = 0x01;
                    content[2] = 0x0A;
                    content[3] = 0xEC;
                    byte len9 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len9, content);
                    break;
                case "改变颜色":
                    content = new byte[7];
                    content[0] = 0xCE;
                    content[1] = 0x04;
                    content[2] = 0x0B;
                    content[3] = R;
                    content[4] = G; 
                    content[5] = B;
                    content[6] = 0xEC;
                    byte len10 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len10, content);
                    break;
                case "改变亮度":
                    content = new byte[5];
                    content[0] = 0xCE;
                    content[1] = 0x02;
                    content[2] = 0x0C;
                    content[3] = Light;
                    content[4] = 0xEC;
                    byte len11 = (byte)content.Length;
                    UpdateLightFunc?.Invoke(len11, content);
                    break;
            }

           
        }*/
        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }


        //private byte _dataLength;

        //public byte DataLength
        //{
        //    get { return _dataLength; }
        //    set { _dataLength = value; RaisePropertyChanged(); }
        //}

        private byte _patternCode;

        public byte PatternConde
        {
            get { return _patternCode; }
            set { _patternCode = value; RaisePropertyChanged(); }
        }

        private ushort _timeCycle;

        public ushort TimeCycle
        {
            get { return _timeCycle; }
            set { _timeCycle = value; RaisePropertyChanged(); }
        }

        private byte _direction;

        public byte Direction
        {
            get { return _direction; }
            set { _direction = value; RaisePropertyChanged(); }
        }

        private byte _lightNumber;

        public byte LightNumber
        {
            get { return _lightNumber; }
            set { _lightNumber = value; RaisePropertyChanged(); }
        }

        private byte _value;

        public byte Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private EnumOrient _orient = EnumOrient.Left;

        public EnumOrient Orient
        {
            get { return _orient; }
            set { _orient = value; RaisePropertyChanged(); }

        }

        private EnumModels _staticLightModel = EnumModels.Left;

        public EnumModels StaticLightModel
        {
            get { return  _staticLightModel; }
            set { _staticLightModel = value; RaisePropertyChanged(); }

        }

        private EnumFlash _divideLightModel = EnumFlash.OddFlash;
        
        public EnumFlash DivideLightModel
        {
            get { return _divideLightModel; }
            set { _divideLightModel = value; RaisePropertyChanged(); }

        }

        private byte _r;

        public byte R
        {
            get { return _r; }
            set { _r = value; RaisePropertyChanged(); }
        }

        private byte _g;

        public byte G
        {
            get { return _g; }
            set { _g = value; RaisePropertyChanged(); }
        }

        private byte _b;

        public byte B
        {
            get { return _b; }
            set { _b = value; RaisePropertyChanged(); }
        }

        private byte _light;

        public byte Light
        {
            get { return _light; }
            set
            {
                _light = value; RaisePropertyChanged();
            }
        }

        public DelegateCommand UpdateLightCommand { get; set; }


        public Action<byte, byte[]>? UpdateLightFunc { get; set; }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
