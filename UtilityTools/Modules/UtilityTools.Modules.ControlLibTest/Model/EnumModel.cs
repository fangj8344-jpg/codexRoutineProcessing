using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ControlLibTest.Model
{
    public enum EnumFlash
    {
        /// <summary>
        /// 奇数闪灯
        /// </summary>
        OddFlash = 0x01,

        /// <summary>
        /// 偶数闪灯
        /// </summary>
        EvenFlash = 0x02,

        /// <summary>
        /// 偶数闪灯
        /// </summary>
        AlternateFlash = 0x03,
    }


    public enum EnumModels
    {
        /// <summary>
        /// 奇数闪灯
        /// </summary>
        Left = 0x01,

        /// <summary>
        /// 偶数闪灯
        /// </summary>
        Middle = 0x02,


        Right = 0x03
    }

    public enum EnumOrient
    {
        /// <summary>
        /// 奇数闪灯
        /// </summary>
        Left = 0x01,

        /// <summary>
        /// 偶数闪灯
        /// </summary>
        Right = 0x02
    }
}
