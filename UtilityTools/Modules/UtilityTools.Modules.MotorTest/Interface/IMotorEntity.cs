using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.Interface
{
    public interface IMotorEntity
    {
        void SetMotorEnableCommand(EnumMotorId motorId, EnumMotorEnable enable);
        void SetMotorOperatingStatusCommand(EnumMotorId motorId, EnumMotorOperatingState operatingStatus);
        void SetMotorGoToCommand(EnumMotorId motorId, EnumMotorUnit unit, float distance);
        void SetMotorZeroCommand(EnumMotorId motorId);
        void SetMotorControlModeCommand(EnumMotorId motorId, EnumMotorCtrType controlMode);
        void SetMotorControlMinClsCommand(EnumMotorId motorId, int minSpeed);
        void SetMotorControlMaxClsCommand(EnumMotorId motorId, int maxSpeed);
        void SetMotorLimitEnableCommand(EnumMotorId motorId, byte limitEnableMask);
        void SetAxTypeCommand(EnumMotorId motorId, EnumMotorMoveType moveType);

        // --- 获取类指令 ---
        void GetMotorPosCommand(EnumMotorId motorId);
        void GetMotorStatusCommand(EnumMotorId motorId);
        void GetMotorSpeedCommand(EnumMotorId motorId);
        void GetMotorLimitEnableCommand(EnumMotorId motorId);
        void GetAxTypeCommand(EnumMotorId motorId);
    }
}
