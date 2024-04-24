#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：4d96f197-cdf7-4f78-981e-b6a089bf1644
 * 文件名：EnumMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/11/27 15:41:09
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public class EnumMethod
    {
        /// <summary>
        /// 得到枚举的中文注释
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetEnumDesc(Enum e)
        {
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.
                GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (EnumAttributes.Length > 0)
            {
                return EnumAttributes[0].Description;
            }
            return e.ToString();
        }


        public static IList EnumToDescList(Type type)
        {
            ArrayList list = new ArrayList();
            Array array = Enum.GetValues(type);
            foreach (Enum value in array)
            {
                list.Add(new KeyValuePair<Enum, string>(value, GetEnumDesc(value)));
            }
            return list;
        }
    }
}
