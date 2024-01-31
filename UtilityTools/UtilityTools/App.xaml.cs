using NLog;
using NLog.Config;
using NLog.Targets;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Threading.Tasks;
using System.Windows;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Interface;
using UtilityTools.Core.Mvvm;
using UtilityTools.Services;
using UtilityTools.Services.Interfaces;
using UtilityTools.UserControls.Views;
using UtilityTools.ViewModels;
using UtilityTools.Views;

namespace UtilityTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            //多线程异常
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            return Container.Resolve<MainWindow>();
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //通常全局异常捕捉的都是致命信息
            LogManager.GetCurrentClassLogger().Fatal($"{e.Exception.StackTrace},{e.Exception.Message}");
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Fatal($"{e.Exception.StackTrace},{e.Exception.Message}");
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogManager.GetCurrentClassLogger().Fatal($"{ex.StackTrace},{ex.Message}");

            //记录dump文件
            MiniDump.TryDump($"dumps\\UtilityTools_{DateTime.Now.ToString("HH-mm-ss-ms")}.dmp");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMessageService, MessageService>();
            containerRegistry.RegisterSingleton<IServiceFactory, ServiceFactory>();

            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();

            containerRegistry.RegisterDialog<SerialPortView>();
            containerRegistry.RegisterDialog<NetConfigView>();

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //moduleCatalog.AddModule<NetControllerModule>(NetControllerModule.ModuleName);
            //moduleCatalog.AddModule<VacMonitorModule>(VacMonitorModule.ModuleName);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            //指定模块加载方式为从文件夹中以反射发现并加载module(推荐用法)，并捕获自定义Attribute
            return new CustomModuleCatalog() { ModulePath = @".\Modules" };
        }

        protected override void OnInitialized()
        {
            LogConfig();
            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.InitConfig();
            base.OnInitialized();
        }

        protected void LogConfig()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            var debugTarget = new FileTarget() { Name = "Debug" };
            config.AddTarget("debug", debugTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger}(${callsite:className=False:fileName=False:includeSourcePath=False:methodName=True}:${callsite-linenumber}) | ${message}";

            debuggerTarget.Layout = @"${date:format=HH\:mm\:ss} | ${level:uppercase=true}|${logger}(${callsite:className=False:fileName=False:includeSourcePath=False:methodName=True}:${callsite-linenumber}) | ${message:withexception=true}";

            fileTarget.FileName = "${basedir}/logs/logfile.txt";
            fileTarget.Layout = @"${longdate} | ${level:uppercase=true} | ${logger}(${callsite:className=False:fileName=False:includeSourcePath=False:methodName=True}:${callsite-linenumber}) | ${message:withexception=true}";
            fileTarget.ArchiveFileName = "${basedir}/logs/Archive/Log{#######}.txt";
            fileTarget.MaxArchiveFiles = 30;
            fileTarget.ArchiveEvery = FileArchivePeriod.Day;

            debugTarget.FileName = "${basedir}/logs/debug.txt";
            debugTarget.Layout = @"${longdate} | ${level:uppercase=true} | ${logger}(${callsite:className=False:fileName=False:includeSourcePath=False:methodName=True}:${callsite-linenumber}) | ${message:withexception=true}";
            debugTarget.ArchiveFileName = "${basedir}/logs/Archive/Debug{#######}.txt";
            debugTarget.MaxArchiveFiles = 3;
            debugTarget.ArchiveEvery = FileArchivePeriod.Day;

            // Step 4. Define rules
#if DEBUG
            var rule1 = new LoggingRule("*", LogLevel.Debug, LogLevel.Info, debugTarget);
            config.LoggingRules.Add(rule1);
#endif

            var rule2 = new LoggingRule("*", LogLevel.Warn, fileTarget);
            config.LoggingRules.Add(rule2);

            var rule3 = new LoggingRule("*", LogLevel.Debug, debuggerTarget);
            config.LoggingRules.Add(rule3);

            //var rule3 = new LoggingRule("*", LogLevel.Info, fileTarget);
            //config.LoggingRules.Add(rule3);

            //var rule4 = new LoggingRule("*", LogLevel.Warn, fileTarget);
            //config.LoggingRules.Add(rule4);

            //var rule5 = new LoggingRule("*", LogLevel.Fatal, fileTarget);
            //config.LoggingRules.Add(rule5);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
