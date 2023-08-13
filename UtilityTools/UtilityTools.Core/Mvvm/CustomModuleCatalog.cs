#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Mvvm
 * 唯一标识：e16e19a1-a90b-4edb-a821-d957f9bcd535
 * 文件名：CustomModuleCatalog
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 9:48:27
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

using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Mvvm
{
    public class CustomModuleCatalog : ModuleCatalog
    {
        private class InnerModuleInfoLoader : MarshalByRefObject
        {
            internal CustomModuleInfo[] GetModuleInfos(string path)
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                ResolveEventHandler value = (object sender, ResolveEventArgs args) => OnReflectionOnlyResolve(args, directory);
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += value;
                CustomModuleInfo[] result = GetNotAlreadyLoadedModuleInfos(IModuleType: AppDomain.CurrentDomain.GetAssemblies().First((Assembly asm) => asm.FullName == typeof(IModule).Assembly.FullName).GetType(typeof(IModule).FullName), directory: directory).ToArray();
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= value;
                return result;
            }

            private static IEnumerable<CustomModuleInfo> GetNotAlreadyLoadedModuleInfos(DirectoryInfo directory, Type IModuleType)
            {
                List<Assembly> list = new List<Assembly>();
                Assembly[] alreadyLoadedAssemblies = (from p in AppDomain.CurrentDomain.GetAssemblies()
                                                      where !p.IsDynamic
                                                      select p).ToArray();
                foreach (FileInfo item in (from file in directory.GetFiles("*.dll")
                                           where alreadyLoadedAssemblies.FirstOrDefault((Assembly assembly) => string.Compare(Path.GetFileName(assembly.Location), file.Name, StringComparison.OrdinalIgnoreCase) == 0) == null
                                           select file).ToList())
                {
                    try
                    {
                        list.Add(Assembly.LoadFrom(item.FullName));
                    }
                    catch (BadImageFormatException)
                    {
                    }
                }

                return list.SelectMany((Assembly assembly) => from t in assembly.GetExportedTypes().Where(new Func<Type, bool>(IModuleType.IsAssignableFrom))
                                                              where t != IModuleType
                                                              where !t.IsAbstract
                                                              select t into type
                                                              select CreateModuleInfo(type));
            }

            private static Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
            {
                Assembly assembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault((Assembly asm) => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
                if (assembly != null)
                {
                    return assembly;
                }

                AssemblyName assemblyName = new AssemblyName(args.Name);
                string text = Path.Combine(directory.FullName, assemblyName.Name + ".dll");
                if (File.Exists(text))
                {
                    return Assembly.ReflectionOnlyLoadFrom(text);
                }

                return Assembly.ReflectionOnlyLoad(args.Name);
            }

            internal void LoadAssemblies(IEnumerable<string> assemblies)
            {
                foreach (string assembly in assemblies)
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoadFrom(assembly);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
            }

            private static CustomModuleInfo CreateModuleInfo(Type type)
            {
                string name = type.Name;
                string title = type.Name;
                string tip = "";
                string icon = "Tools";
                List<string> list = new List<string>();
                bool flag = false;
                CustomAttributeData customAttributeData = CustomAttributeData.GetCustomAttributes(type).FirstOrDefault((CustomAttributeData cad) 
                    => cad.Constructor.DeclaringType!.FullName == typeof(CustomModuleAttribute).FullName);
                if (customAttributeData != null)
                {
                    foreach (CustomAttributeNamedArgument namedArgument in customAttributeData.NamedArguments)
                    {
                        switch (namedArgument.MemberInfo.Name)
                        {
                            case "ModuleName":
                                name = (string)namedArgument.TypedValue.Value;
                                break;
                            case "OnDemand":
                                flag = (bool)namedArgument.TypedValue.Value;
                                break;
                            case "StartupLoaded":
                                flag = !(bool)namedArgument.TypedValue.Value;
                                break;
                            case "Title":
                                title = (string)namedArgument.TypedValue.Value;
                                break;
                            case "Tip":
                                tip = (string)namedArgument.TypedValue.Value;
                                break;
                            case "Icon":
                                icon = (string)namedArgument.TypedValue.Value;
                                break;
                        }
                    }
                }

                foreach (CustomAttributeData item in from cad in CustomAttributeData.GetCustomAttributes(type)
                                                     where cad.Constructor.DeclaringType!.FullName == typeof(ModuleDependencyAttribute).FullName
                                                     select cad)
                {
                    list.Add((string)item.ConstructorArguments[0].Value);
                }

                CustomModuleInfo obj = new CustomModuleInfo(name, type.AssemblyQualifiedName)
                {
                    InitializationMode = (flag ? InitializationMode.OnDemand : InitializationMode.WhenAvailable),
                    Ref = type.Assembly.EscapedCodeBase,
                    Title = title,
                    Tip = tip,
                    Icon = icon,
                };
                obj.DependsOn.AddRange(list);
                return obj;
            }
        }

        //
        // 摘要:
        //     Directory containing modules to search for.
        public string ModulePath { get; set; }

        //
        // 摘要:
        //     Drives the main logic of building the child domain and searching for the assemblies.
        protected override void InnerLoad()
        {
            if (string.IsNullOrEmpty(ModulePath))
            {
                throw new InvalidOperationException("ModulePathCannotBeNullOrEmpty");
            }

            if (!Directory.Exists(ModulePath))
            {
                throw new InvalidOperationException($"DirectoryNotFound: {ModulePath}");
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            try
            {
                List<string> list = new List<string>();
                IEnumerable<string> collection = from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                 where !(assembly is AssemblyBuilder) && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder" && !string.IsNullOrEmpty(assembly.Location)
                                                 select assembly.Location;
                list.AddRange(collection);
                Type typeFromHandle = typeof(InnerModuleInfoLoader);
                if (typeFromHandle.Assembly != null)
                {
                    InnerModuleInfoLoader innerModuleInfoLoader = (InnerModuleInfoLoader)currentDomain.CreateInstanceFrom(typeFromHandle.Assembly.Location, typeFromHandle.FullName)!.Unwrap();
                    base.Items.AddRange(innerModuleInfoLoader.GetModuleInfos(ModulePath));
                }
            }
            catch (Exception innerException)
            {
                throw new Exception("There was an error loading assemblies.", innerException);
            }
        }
    }
}
