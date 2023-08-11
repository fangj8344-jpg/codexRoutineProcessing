#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Extension
 * 唯一标识：08c469f1-e732-4011-9ae0-d62aa1a35397
 * 文件名：EnumExtension
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:55:19
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace UtilityTools.Core.Extension
{
    /// <summary>
    /// 枚举拓展
    /// </summary>
    public class EnumExtension : MarkupExtension
    {
        #region ------------Constructor------------
        /// <summary>
        /// 枚举拓展构造函数
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <exception cref="ArgumentNullException">枚举类型为空</exception>
        public EnumExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");
            EnumType = enumType;
        }
        #endregion

        #region ------------Field------------
        private Type _enumType;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 枚举类型
        /// </summary>
        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value)
                    return;
                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum");

                _enumType = value;
            }
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 继承实现，返回一个对象，此对象被设置为此标记拓展的目标属性的值
        /// </summary>
        /// <param name="serviceProvider">可以为标记拓展提供服务的对象</param>
        /// <returns>将在拓展应用到的属性上设置的对象值</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);

            return (from object enumValue in enumValues
                    select new EnumerationMember
                    {
                        Value = enumValue,
                        Description = GetDescription(enumValue)
                    }).ToArray();
        }
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 获取枚举描述值
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = EnumType
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString();
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

        /// <summary>
        /// 枚举成员类
        /// </summary>
        public class EnumerationMember
        {
            /// <summary>
            /// 枚举描述
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 枚举数值
            /// </summary>
            public object Value { get; set; }
        }
    }
}
