#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：9c3cc66c-d205-43e7-b2d4-1b47e593d334
 * 文件名：PropertyInfoModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/25 14:30:48
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
    public class PropertyInfoModel : BindableBase
    {
        #region ------------Constructor------------
        public PropertyInfoModel()
        {

        }

        public PropertyInfoModel(string des, string name, string value)
        {
            Description = des;
            Name = name;
            Value = value;
        }

        public PropertyInfoModel(int key)
        {
            Key = key;
            if (KeyNameDict.ContainsKey(key))
                Name = KeyNameDict[key];
            if (KeyDesDict.ContainsKey(key))
                Description = KeyDesDict[key];
        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private string _name;
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private int _key;
        /// <summary>
        /// TiffTagID的key值，取未定义的那部分区域 1000-2000
        /// </summary>
        public int Key
        {
            get { return _key; }
            set { _key = value; RaisePropertyChanged(); }
        }

        private string _description;
        /// <summary>
        /// 中文描述
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(); }
        }


        private string _value;
        /// <summary>
        /// 属性值
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        public static Dictionary<int, string> KeyNameDict = new Dictionary<int, string>()
        {
            { 0x1001, "Name"},
            { 0x1002, "Version"},
            { 0x1003, "Date"},
            { 0x1004, "Detecter"},
            { 0x1005, "OB"},
            { 0x1006, "Mag"},
            { 0x1007, "HighVol"},
            { 0x1008, "PixelLength"},
            { 0x1009, "Title"},
            { 0x100A, "Note"},
            { 0x100B, "Position"},
            { 0x100C, "AveragePoints"},
            { 0x100D, "Frequency"},
            { 0x100E, "DataFlag"},
        };

        public static Dictionary<int, string> KeyDesDict = new Dictionary<int, string>()
        {
            { 0x1001, "名字"},
            { 0x1002, "版本"},
            { 0x1003, "日期"},
            { 0x1004, "检测器"},
            { 0x1005, "物镜值"},
            { 0x1006, "放大倍数"},
            { 0x1007, "加速电压"},
            { 0x1008, "像素长度"},
            { 0x1009, "标题"},
            { 0x100A, "备注"},
            { 0x100B, "坐标"},
            { 0x100C, "平均点数"},
            { 0x100D, "采样率"},
            { 0x100E, "数据标识位"},
        };
        #endregion
    }
}
