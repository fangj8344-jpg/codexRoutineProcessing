using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using Prism.Mvvm;
using UtilityTools.Services.Interfaces.IServices;
using Prism.Services.Dialogs;
using Prism.Commands;
using UtilityTools.Modules.Test485ChipTool.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.Test485ChipTool.Protocol;
using System.Threading;

namespace UtilityTools.Modules.Test485ChipTool.ViewModels
{
    public class Test485ChipToolViewModel:RegionViewModelBase
    {
        #region---------------------Construct-------------------
        public Test485ChipToolViewModel(IContainerProvider containerProvider)
          : base(containerProvider)
        {
            InitProperty();
            InitCommand();
        }
        #endregion

        #region ------------Field------------

        #endregion

        #region ------------Property------------
        
        private Test485Model _model1;
        /// <summary>
        /// 串口1
        /// </summary>
        public Test485Model Model1
        {
            get { return _model1; }
            set { _model1 = value; RaisePropertyChanged(); }
        }

        private Test485Model _model2;
        /// <summary>
        /// 串口2
        /// </summary>
        public Test485Model Model2
        {
            get { return _model2; }
            set { _model2 = value; RaisePropertyChanged(); }
        }

        private Test485Model _model3;
        /// <summary>
        /// 串口3
        /// </summary>
        public Test485Model Model3
        {
            get { return _model3; }
            set { _model3 = value; RaisePropertyChanged(); }
        }

        private Test485Model _model4;
        /// <summary>
        /// 串口4
        /// </summary>
        public Test485Model Model4
        {
            get { return _model4; }
            set { _model4 = value; RaisePropertyChanged(); }
        }

        private Test485Model _model5;
        /// <summary>
        /// 串口5
        /// </summary>
        public Test485Model Model5
        {
            get { return _model5; }
            set { _model5 = value; RaisePropertyChanged(); }
        }

       
        #endregion

        #region ------------Command------------
        public DelegateCommand<string> TestCommand { get; set; }

        public DelegateCommand ClearCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            TestCommand = new DelegateCommand<string>(Test);
            ClearCommand = new DelegateCommand(Clear);
        }

       


        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            var hostIp = FreePort.FindIpv4IP().ToString();
            _model1 = new Test485Model(hostIp, FreePort.FindNextAvailableUDPPort(5000),"192.168.1.88",5001);
            _model2 = new Test485Model(hostIp, FreePort.FindNextAvailableUDPPort(5000), "192.168.1.88", 5002);
            _model3 = new Test485Model(hostIp, FreePort.FindNextAvailableUDPPort(5000), "192.168.1.88", 5003);
            _model4 = new Test485Model(hostIp, FreePort.FindNextAvailableUDPPort(5000), "192.168.1.88", 5004);
            _model5 = new Test485Model(hostIp, FreePort.FindNextAvailableUDPPort(5000), "192.168.1.88", 5005);
        }

        private void Test(string obj)
        {
            
            switch (obj)
            {
                case "1": _model1.CheckTest(); break;
                case "2": _model2.CheckTest(); break;
                case "3": _model3.CheckTest(); break;
                case "4": _model4.CheckTest(); break;
                case "5": _model5.CheckTest(); break;
                case "0":
                    _model1.CheckTest();
                    Thread.Sleep(200);
                    _model2.CheckTest();
                    Thread.Sleep(200);
                    _model3.CheckTest();
                    Thread.Sleep(200);
                    _model4.CheckTest();
                    Thread.Sleep(200);
                    _model5.CheckTest();
                    break;
            }
        }
        private void Clear()
        {
            _model1.IsCheckOK = null;
            _model2.IsCheckOK = null;
            _model3.IsCheckOK = null;
            _model4.IsCheckOK = null;
            _model5.IsCheckOK = null;
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
