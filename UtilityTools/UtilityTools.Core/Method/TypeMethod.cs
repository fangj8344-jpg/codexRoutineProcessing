using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Method
{
    /// <summary>
    /// 类型获取
    /// </summary>
    public class TypeMethod
    {
        /// <summary>
        /// 获取指定接口得全部实现
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <returns>接口列表</returns>
        public static IEnumerable<Type> GetInterfaceTypes(Type interfaceType)
        {
            var allAss = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAss)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract) continue;
                    foreach (var t in type.GetInterfaces())
                    {
                        if (t == interfaceType)
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定基类的所有派生类
        /// </summary>
        /// <param name="derivedType">基类类型</param>
        /// <returns>派生类列表</returns>
        public static IEnumerable<Type> GetDerivedTypes(Type baseType)
        {
            var allAss = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAss)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (baseType.IsAssignableFrom(type) && type != baseType)
                    {
                        yield return type;
                    }
                }
            }
        }

        /// <summary>
        /// 得到枚举的显示Attribute
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetEnumDisplay(Enum e)
        {
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DisplayAttribute displayAttribute = EnumInfo.GetCustomAttribute<DisplayAttribute>();
            string result = string.Empty;
            if (displayAttribute != null)
            {
                result = displayAttribute.GetName();
            }
            else
            {
                result = e.ToString();
            }

            return result;
        }

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
    }
}
