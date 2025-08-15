using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;
using UtilityTools.Modules.NewMotor5Controller.Model;
using UtilityTools.Modules.NewMotor5Controller.Protocol;

namespace UtilityTools.Modules.NewMotor5Controller.Entity
{
    public interface IMotorControl 
    {
        #region ------------ Property ------------
        /// <summary>
        /// X方向电机
        /// </summary>
        MotorModel MotorX { get; set; }

        /// <summary>
        /// Y方向电机
        /// </summary>
        MotorModel MotorY { get; set; }

        /// <summary>
        /// Z方向电机
        /// </summary>
        MotorModel MotorZ { get; set; }

        /// <summary>
        /// R方向电机
        /// </summary>
        MotorModel MotorR { get; set; }

        /// <summary>
        /// T方向电机
        /// </summary>
        MotorModel MotorT { get; set; }

        /// <summary>
        /// 电机列表
        /// </summary>
        ObservableCollection<MotorModel> Motors { get; set; }

        /// <summary>
        /// 是否自动更新电机状态
        /// </summary>
        bool IsAutoUpdateState { get; }
        #endregion

        #region ------------ Method ------------
        /// <summary>
        /// 启动自动定时状态问询，实现后台状态更新
        /// </summary>
        /// <param name="interval">问询间隔，单位ms</param>
        void StartRequest(int interval);

        /// <summary>
        /// 停止状态问询
        /// </summary>
        void StopRequest();

        /// <summary>
        /// 设置电机的激活状态
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="active">激活状态</param>
        void SetMotorActiveEnable(EnumMotorId motorId, bool active);

        /// <summary>
        /// 停止电机
        /// </summary>
        void StopMotor(EnumMotorId motorId);

        /// <summary>
        /// 按脉冲相对移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="pulse">移动脉冲，正为正方向移动，负为负方向移动</param>
        /// <returns></returns>
        void MoveMotorWithPulse(EnumMotorId motorId, int pulse);

        /// <summary>
        /// 按距离相对移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="dis">移动距离，正为正方向移动，负为负方向移动</param>
        /// <returns></returns>
        void MoveMotorWithDis(EnumMotorId motorId, double dis);

        /// <summary>
        /// 按距离绝对移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="dis">目标距离坐标，正为正方向，负为负方向</param>
        /// <param name="obValue">补偿距离</param>
        /// <returns></returns>
        void GotoMotorWithDis(EnumMotorId motorId, double dis, float obValue = 0.0f);

        /// <summary>
        /// 按脉冲绝对移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="pulse">目标脉冲坐标，正为正方向，负为负方向</param>
        /// <param name="obValue">补偿距离</param>
        /// <returns></returns>
        void GotoMotorWithPulse(EnumMotorId motorId, int pulse, float obValue = 0.0f);

        /// <summary>
        /// 按距离速度移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="dis">距离速度，支持正负</param>
        /// <returns></returns>
        void SpeedMotorWithDis(EnumMotorId motorId, double dis);

        /// <summary>
        /// 按脉冲速度移动电机
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <param name="pulse">脉冲速度，支持正负</param>
        /// <returns></returns>
        void SpeedMotorWithPulse(EnumMotorId motorId, int pulse);

        /// <summary>
        /// 设置电机当前位置为零点
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        void SetMotorZero(EnumMotorId motorId);

        /// <summary>
        /// 获取电机当前状态
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        void GetMotorStatus(EnumMotorId motorId);

        /// <summary>
        /// 获取电机当前坐标
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        void GetMotorPos(EnumMotorId motorId);

        /// <summary>
        /// 获取电机当前速度
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        void GetMotorSpeed(EnumMotorId motorId);

        /// <summary>
        /// 脉冲数转距离，距离单位μm
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pulse">脉冲数</param>
        /// <returns></returns>
        double PulseToDis(EnumMotorId motorId, int pulse);

        /// <summary>
        /// 距离转脉冲数，距离单位μm
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="dis">距离，单位μm</param>
        /// <returns></returns>
        int DisToPulse(EnumMotorId motorId, double dis);

        /// 20240517 新增电机控制接口，适配全新的通用电机控制协议指令

        /// <summary>
        /// 设置电机的控制模式
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="mode">控制模式</param>
        /// <returns></returns>
        void SetMotorCtlMode(EnumMotorId motorId, EnumMotorControlMode mode);

        /// <summary>
        /// 设置电机的软限位掩码
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="softLim">软限位掩码</param>
        /// <returns></returns>
        void SetMotorLimMask(EnumMotorId motorId, byte softLim);

        /// <summary>
        /// 设置电机的PID调节阈值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pidThre">PID调节阈值，单位脉冲数</param>
        /// <returns></returns>
        void SetMotorPidThre(EnumMotorId motorId, int pidThre);

        /// <summary>
        /// 设置电机的PID调节次数
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pidTimes">PID调节次数</param>
        /// <returns></returns>
        void SetMotorPidMaxCount(EnumMotorId motorId, int pidTimes);

        /// <summary>
        /// 设置电机的电动阈值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="threshold">点动阈值，单位脉冲</param>
        /// <returns></returns>
        void SetMotorMicroThre(EnumMotorId motorId, int threshold);

        /// <summary>
        /// 设置电机的点动距离
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="microLen">点动距离，单位脉冲</param>
        /// <returns></returns>
        void SetMotorMicroLen(EnumMotorId motorId, int microLen);

        /// <summary>
        /// 设置电机的PID参数
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pid">PID参数</param>
        /// <returns></returns>
        void SetMotorPid(EnumMotorId motorId, PidModel pid);

        /// <summary>
        /// 设置电机的软限位最大值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="sLimMax">软限位最大值，单位脉冲</param>
        /// <returns></returns>
        void SetMotorSLimMax(EnumMotorId motorId, int sLimMax);

        /// <summary>
        /// 设置电机的软限位最小值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="sLimMin">软限位最小值，单位脉冲</param>
        /// <returns></returns>
        void SetMotorSLimMin(EnumMotorId motorId, int sLimMin);

        /// <summary>
        /// 设置电机的最大速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="maxSpeed">最大速度，单位脉冲/s</param>
        /// <returns></returns>
        void SetMotorMaxSpeed(EnumMotorId motorId, int maxSpeed);

        /// <summary>
        /// 设置电机的最小速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="minSpeed">最小速度，单位脉冲/s</param>
        /// <returns></returns>
        void SetMotorMinSpeed(EnumMotorId motorId, int minSpeed);

        /// <summary>
        /// 获取电机的控制模式
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="mode">控制模式</param>
        /// <returns></returns>
        void GetMotorCtlMode(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的软限位状态
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <returns></returns>
        void GetMotorLimMask(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的PID调节阈值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pidThre">PID调节阈值，单位脉冲数</param>
        /// <returns></returns>
        void GetMotorPidThre(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的PID调节次数
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="pidTimes">PID调节次数</param>
        /// <returns></returns>
        void GetMotorPidMaxCount(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的电动阈值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="threshold">点动阈值，单位脉冲</param>
        /// <returns></returns>
        void GetMotorMicroThre(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的点动距离
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="microLen">点动距离，单位脉冲</param>
        /// <returns></returns>
        void GetMotorMicroLen(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的PID参数
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="kp">参数P</param>
        /// <param name="ki">参数I</param>
        /// <param name="kd">参数D</param>
        /// <returns></returns>
        void GetMotorPid(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的软限位最大值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <returns></returns>
        void GetMotorMaxPos(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的软限位最小值
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <returns></returns>
        void GetMotorMinPos(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的最大速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <returns></returns>
        void GetMotorMaxSpeed(EnumMotorId motorId);

        /// <summary>
        /// 获取电机的最小速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <returns></returns>
        void GetMotorMinSpeed(EnumMotorId motorId);

        /// <summary>
        /// 设置电机的轴类型
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="type">轴类型</param>
        /// <returns></returns>
        void SetMotorAxtype(EnumMotorId motorId, EnumMotorAxisType type);

        /// <summary>
        /// 设置电机的轴单位类型
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="unit">轴的单位类型</param>
        /// <returns></returns>
        void SetMotorAxUnit(EnumMotorId motorId, EnumMotorUnitType unit);

        /// <summary>
        /// 设置电机的类型
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="type">电机类型</param>
        /// <returns></returns>
        void SetMotorType(EnumMotorId motorId, EnumMotorType type);

        /// <summary>
        /// 设置电机轴的转换系数
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="type">电机的转换系数</param>
        /// <returns></returns>
        void SetMotorAxCoef(EnumMotorId motorId, float coef);

        /// <summary>
        /// 获取电机最大加速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        void GetMotorMaxAcc(EnumMotorId motorId);

        /// <summary>
        /// 设置电机最大加速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="maxAcc">最大加速度脉冲</param>
        void SetMotorMaxAcc(EnumMotorId motorId, int maxAcc);

        /// <summary>
        /// 获取电机最大减速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        void GetMotorMaxDec(EnumMotorId motorId);

        /// <summary>
        /// 设置电机最大减速度
        /// </summary>
        /// <param name="motorId">电机通道信息</param>
        /// <param name="maxDec">最大减速度脉冲</param>
        void SetMotorMaxDec(EnumMotorId motorId, int maxDec);

        /// <summary>
        /// 设置电机补偿时候的臂长
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="length">电机臂长</param>
        void SetMotorTLink(EnumMotorId motorId, float length);

        /// <summary>
        /// 获取电机补偿时候的臂长
        /// </summary>
        /// <param name="motorId">电机ID</param>
        void GetMotorTLink(EnumMotorId motorId);

        /// <summary>
        /// 设置软限位内向补偿距离
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="nailH">距离1</param>
        /// <param name="heigh">距离2</param>
        void SetCompensationLength(EnumMotorId motorId, float nailH, float heigh);

        /// <summary>
        /// 获取软限位内向补偿距离
        /// </summary>
        /// <param name="motorId">电机ID</param>
        void GetCompensationLength(EnumMotorId motorId);

        /// <summary>
        /// 设置电机相对坐标信息
        /// </summary>
        /// <param name="motorId">电机ID</param>
        /// <param name="length">相对坐标位置</param>
        void SetRelativeCoorInfo(EnumMotorId motorId, float length);

        /// <summary>
        /// 获取电机相对坐标信息
        /// </summary>
        /// <param name="motorId">电机ID</param>
        void GetRelativeCoorInfo(EnumMotorId motorId);
        #endregion
    }
  
}
