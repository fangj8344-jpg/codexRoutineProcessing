using Prism.Commands;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls.Primitives;
using UtilityTools.Core;
using UtilityTools.Core.Interface;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UtilityTools.ViewModels
{
    public class MainWindowViewModel : BindableBase, IConfigureService
    {
        #region Property
        private string _title = "Zeptools Application";
        /// <summary>
        /// 主题软件标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private ObservableCollection<CustomModuleInfo> _modules;
        /// <summary>
        /// 导航菜单列表
        /// </summary>
        public ObservableCollection<CustomModuleInfo> Modules
        {
            get => _modules ?? (_modules = new ObservableCollection<CustomModuleInfo>());
            set { _modules = value; RaisePropertyChanged(); }
        }

        private int _selectedIndex;
        /// <summary>
        /// 导航菜单列表选中索引
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Constructor
        public MainWindowViewModel(IRegionManager regionManager, IModuleCatalog moduleCatalog)
        {
            this._regionManager = regionManager;
            this._moduleCatalog = moduleCatalog;
            InitCommand();
        }
        #endregion

        #region Field
        private readonly IRegionManager _regionManager;
        private readonly IModuleCatalog _moduleCatalog;
        private IRegionNavigationJournal _journal;
        #endregion

        #region Command
        public DelegateCommand<IModuleInfo> NavigateCommand { get; set; }
        public DelegateCommand GoBackCommand { get; set; }
        public DelegateCommand GoForwardCommand { get; set; }
        public DelegateCommand GoHomeCommand { get; set; }
        public DelegateCommand LoadedWindowCommand { get; set; }
        public DelegateCommand ClosingWindowCommand { get; set; }
        #endregion

        #region PublicMethod
        /// <summary>
        /// 初始化配置函数
        /// </summary>
        public void InitConfig()
        {
            _regionManager.Regions[RegionNames.MainWindowRegionName].RequestNavigate("HomeView");
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            NavigateCommand = new DelegateCommand<IModuleInfo>(Navigate);
            GoBackCommand = new DelegateCommand(GoBack);
            GoForwardCommand = new DelegateCommand(GoForward);
            GoHomeCommand = new DelegateCommand(GoHome);
            LoadedWindowCommand = new DelegateCommand(LoadedWindow);
            ClosingWindowCommand = new DelegateCommand(ClosingWindow);
        }

        /// <summary>
        /// 导航到指定界面
        /// </summary>
        /// <param name="taskBar">导航</param>
        private void Navigate(IModuleInfo taskBar)
        {
            if (taskBar == null || string.IsNullOrWhiteSpace(taskBar.ModuleName))
                return;
            _regionManager.Regions[RegionNames.MainWindowRegionName].RequestNavigate($"{taskBar.ModuleName}View", back =>
            {
                _journal = back.Context.NavigationService.Journal;
            });
        }

        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private void GoBack()
        {
            if (_journal != null && _journal.CanGoBack)
                _journal.GoBack();
        }

        /// <summary>
        /// 回到之前界面
        /// </summary>
        private void GoForward()
        {
            if (_journal != null && _journal.CanGoForward)
                _journal.GoForward();
        }

        /// <summary>
        /// 返回主界面
        /// </summary>
        private void GoHome()
        {
            _regionManager.Regions[RegionNames.MainWindowRegionName].RequestNavigate("HomeView", back =>
            {
                _journal = back.Context.NavigationService.Journal;
                SelectedIndex = 0;
            });
        }

        /// <summary>
        /// 页面加载完成
        /// </summary>
        private void LoadedWindow()
        {
            Modules.Add(new CustomModuleInfo("Home", "Default") { Title = "首页", Tip="首页", Icon="Home"});
            foreach (var module in _moduleCatalog.Modules)
            {
                var customModule = module as CustomModuleInfo;
                if(customModule != null)
                    Modules.Add(customModule);
            }
            /*
            _regionManager.Regions[RegionNames.MainWindowRegionName].RequestNavigate($"FDC12CHVBoxView", back =>
            {
                _journal = back.Context.NavigationService.Journal;
            });
            */


        }

        /// <summary>
        /// 关闭窗口释放资源
        /// </summary>
        private void ClosingWindow()
        { 
            
        }
        #endregion
    }
}
