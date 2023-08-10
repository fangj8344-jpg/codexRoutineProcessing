using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.ViewModels
{
    internal class HomeViewModel : RegionViewModelBase
    {
        public HomeViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {

        }
    }
}
