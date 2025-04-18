using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Mvvm;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    public class SimpleTestUnit : BindableBase, ITestUnit
    {
        public SimpleTestUnit(IAsynRWService service, byte[] cmd, Func<byte[], bool> testFunc)
        {
            _service = service;//服务 IAsynRWService的派生类  UdpNetAsyncDevice TcpNetAsynDevice等
            _cmd = cmd;//报文
            _testFunc = testFunc;//传入的测试函数 在Service_UpdateResponse中调用

            if (_service != null)
            {
                _service.UpdateResponse += Service_UpdateResponse;//service收到数据后会调用UpdateResponse
            }

            //TestCommand = new DelegateCommand(TestCom);//同WPF绑定的响应命令 点击按钮后调用发送命令
        }

        ~SimpleTestUnit()
        {
            Dispose();
        }

        private bool _isDisposed;//链接释放标志
        private IAsynRWService _service;
        private byte[] _cmd;
        private Func<byte[], bool> _testFunc;

        private bool _isOK;
        public bool IsOK
        {
            get => _isOK; 
            set
            {
                _isOK = value;
                RaisePropertyChanged();
            }
        }

        //public DelegateCommand TestCommand { get; set; }

        public void TestCom()
        {
            IsOK = false;
            _service.Open();
            _service?.SendMsg(_cmd);
        }

        private void Service_UpdateResponse(object? sender, byte[] e)
        {
            IsOK = _testFunc(e);
        }

        public void Dispose()
        {//释放资源 关闭连接
            if (_isDisposed) return;

            _isDisposed = true;
            _service.Close();
            
            GC.SuppressFinalize(this); // 避免Finalizer被调用
        }
    }
}
