using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MotorTest.Model
{
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
            set { _testResult = value; RaisePropertyChanged(); }
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

}
