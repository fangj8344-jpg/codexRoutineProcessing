using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    internal interface ITestUnit : IDisposable
    {
        bool IsOK { get; set; }

        void TestCom();

        //DelegateCommand TestCommand { get; set; }
    }
}
