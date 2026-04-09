using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Modules.MotorTest.Interface;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class TravelTestResult:MotorTestResult
    {
        // 专门存这一轴跑出来的真实边界
        public float RealMinPos { get; set; }
        public float RealMaxPos { get; set; }
        public double RealMaxPosUm { get; set; }
        public double RealMinPosUm { get; set; }
        public bool IsPositiveLimitFound { get; set; }
        public bool IsNegativeLimitFound { get; set; }
    }


    // 1. 丝杆测试专用桶
    public class SmoothnessTestResult : MotorTestResult
    {
        public double ForwardStdDev { get; set; }  // 正向标准差
        public double BackwardStdDev { get; set; } // 反向标准差
        public double ForwardStdDevUm { get; set; }  // 正向标准差
        public double BackwardStdDevUm { get; set; } // 反向标准差
    }

    // 2. 线性测试专用桶
    public class LinearTestResult : MotorTestResult
    {
        public double FinalStdDev { get; set; }
        public double FinalStdDevUm { get; set; }
        // 【最关键】这98个点的名单就在这里传出去！
        public List<PositionError> PositionErrors { get; set; } = new List<PositionError>();
    }

}
