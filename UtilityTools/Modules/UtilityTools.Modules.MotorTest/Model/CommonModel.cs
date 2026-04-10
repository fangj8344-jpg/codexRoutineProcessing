using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MotorTest.Model
{
    public enum MotorTestProgressState
    {
        NotStarted = 0,
        Running = 1,
        Passed = 2,
        Failed = 3
    }

    public class CommonModel
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        public const string MotorTestRegionName = "MotorTest";
        public const string ThreeAxisTestModelWindowsViewModelRegionName = "ThreeAxisTestModelWindowsViewModel";
        
        #endregion
    }

    public class PIDModel : BindableBase
    {
        private float _p;

        public float P
        {
            get { return _p; }
            set { _p = value; RaisePropertyChanged(); }
        }

        private float _i;

        public float I
        {
            get { return _i; }
            set { _i = value; RaisePropertyChanged(); }
        }

        private float _d;

        public float D
        {
            get { return _d; }
            set { _d = value; RaisePropertyChanged(); }
        }
    }
    public class  MotorTestMessage:BindableBase
    {
        public string AxisName { get; set; }
        private MotorTestProgressState _progressState = MotorTestProgressState.NotStarted;
        public MotorTestProgressState ProgressState
        {
            get { return _progressState; }
            set { _progressState = value; RaisePropertyChanged(); }
        }
        private string _testProject;
        /// <summary>
        /// 项目名称
        /// </summary>
        public string TestProject
        {
            get { return _testProject; }
            set { _testProject = value; RaisePropertyChanged(); }
        }
        private string _testResult;
        /// <summary>
        /// 测试结果
        /// </summary>
        public string TestResult
        {
            get { return _testResult; }
            set
            {
                _testResult = value;
                if (value == "合格")
                {
                    ProgressState = MotorTestProgressState.Passed;
                }
                else if (value == "不合格")
                {
                    ProgressState = MotorTestProgressState.Failed;
                }
                RaisePropertyChanged();
            }
        }

        private string _testValue;
        /// <summary>
        /// 测试值
        /// </summary>
        public string TestValue
        {
            get { return _testValue; }
            set { _testValue = value; RaisePropertyChanged(); }
        }

        private string _standardValue;
        /// <summary>
        /// 标准值
        /// </summary>
        public string StandardValue
        {
            get { return _standardValue; }
            set { _standardValue = value;RaisePropertyChanged(); }
        }

        private string _description;
        /// <summary>
        /// 说明
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(); }
        }
    }
    public class RestartableSender
    {
        // 发送线程
        private Thread _senderThread;
        // 控制线程是否运行的标志
        private bool _isRunning;
        // 最大重启次数，防止无限重启
        private readonly int _maxRestartCount;
        // 当前重启次数
        private int _currentRestartCount;
        // 线程名称
        private readonly string _threadName;
        // 发送逻辑的委托
        private readonly Action _sendAction;
        // 重启延迟时间(毫秒)
        private readonly int _restartDelay;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="threadName">线程名称</param>
        /// <param name="sendAction">发送逻辑</param>
        /// <param name="maxRestartCount">最大重启次数</param>
        /// <param name="restartDelay">重启延迟时间(毫秒)</param>
        public RestartableSender(string threadName, Action sendAction,
                                int maxRestartCount = 10, int restartDelay = 1000)
        {
            _threadName = threadName;
            _sendAction = sendAction ?? throw new ArgumentNullException(nameof(sendAction));
            _maxRestartCount = maxRestartCount;
            _restartDelay = restartDelay;
            _currentRestartCount = 0;
        }

        /// <summary>
        /// 启动发送线程
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                Console.WriteLine($"{_threadName} 已经在运行中");
                return;
            }

            _isRunning = true;
            _currentRestartCount = 0;
            CreateAndStartThread();
        }

        /// <summary>
        /// 停止发送线程
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            if (_senderThread != null && _senderThread.IsAlive)
            {
                // 可以根据需要决定是否使用Abort()
                // 注意：Abort()可能会导致资源无法正确释放
                // _senderThread.Abort();
                Console.WriteLine($"{_threadName} 正在停止...");
            }
        }

        /// <summary>
        /// 创建并启动线程
        /// </summary>
        private void CreateAndStartThread()
        {
            _senderThread = new Thread(ThreadProc)
            {
                Name = _threadName,
                IsBackground = true // 设为后台线程，避免程序无法退出
            };

            _senderThread.Start();
            Console.WriteLine($"{_threadName} 已启动");
        }

        /// <summary>
        /// 线程执行的方法
        /// </summary>
        private void ThreadProc()
        {
            try
            {
                // 执行发送逻辑
                _sendAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_threadName} 发生异常: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // 线程结束时检查是否需要重启
                CheckAndRestart();
            }
        }

        /// <summary>
        /// 检查并重启线程
        /// </summary>
        private void CheckAndRestart()
        {
            // 如果线程应该继续运行
            if (_isRunning)
            {
                // 检查是否达到最大重启次数
                if (_currentRestartCount < _maxRestartCount)
                {
                    _currentRestartCount++;
                    Console.WriteLine($"{_threadName} 将在 {_restartDelay}ms 后重启，当前重启次数: {_currentRestartCount}/{_maxRestartCount}");

                    // 延迟重启，避免频繁重启
                    Thread.Sleep(_restartDelay);
                    CreateAndStartThread();
                }
                else
                {
                    Console.WriteLine($"{_threadName} 已达到最大重启次数 {_maxRestartCount}，停止重启");
                    _isRunning = false;
                }
            }
            else
            {
                Console.WriteLine($"{_threadName} 已正常停止");
            }
        }
    }

}
