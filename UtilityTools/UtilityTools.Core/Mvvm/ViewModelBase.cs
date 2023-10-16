using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Mvvm
{
    public abstract class ViewModelBase : BindableBase, IDestructible
    {
        #region 字段
        protected readonly IContainerProvider containerProvider;
        public readonly IEventAggregator aggregator;
        #endregion

        protected ViewModelBase(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            aggregator = containerProvider.Resolve<IEventAggregator>();
        }

        public virtual void Destroy()
        {

        }

        public void UpdateLoading(bool IsOpen, string title = "处理中...")
        {
            aggregator.UpdateLoading(new UpdateModel()
            {
                Title = title,
                IsOpen = IsOpen
            });
        }

       
    }
}
