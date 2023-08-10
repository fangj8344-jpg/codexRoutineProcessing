using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Interface
{
    public interface IModuleBase : IModule
    {
        static string ModuleName { get; }
    }
}
