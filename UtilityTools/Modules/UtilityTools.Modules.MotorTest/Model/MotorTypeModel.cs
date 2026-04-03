using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using UtilityTools.Modules.MotorTest.Protocol;

namespace UtilityTools.Modules.MotorTest.Model
{
    // 定义一个专门用来存“单轴硬件配置”的小结构体
    public class AxisHardwareConfig
    {
        public EnumMotorId ChannelId { get; set; }        // 插在哪个物理孔
        public (int Min, int Max) StrokeRange { get; set; } // 丝杆有效行程
    }

    // 定义咱们的四个标准机型
    public enum MachineProfile
    {
        CompactTwoAxis,     // 紧凑型双轴
        StandardTwoAxis,    // 标准型双轴
        HeavyDutyThreeAxis, // 重载型三轴
        UniversalFiveAxis   // 全动型五轴
    }

    public class MotorTypeModel : BindableBase
    {
        private readonly IContainerProvider _containerProvider;
        private readonly ThreeAxisTestModel _motorModel;
        // 在 MotorTypeModel.cs 中增加
        // 1. 给这个“翻译官”换个更有辨识度的名字，避免跟类型名掐架
        public Protocol.EnumMotorAxisType SelectedAxisType
        {
            get
            {
                return _currentProfile switch
                {
                    MachineProfile.CompactTwoAxis => Protocol.EnumMotorAxisType.TwoAxisMotor,
                    MachineProfile.StandardTwoAxis => Protocol.EnumMotorAxisType.TwoAxisMotor,
                    MachineProfile.HeavyDutyThreeAxis => Protocol.EnumMotorAxisType.ThreeAxisMotor,
                    MachineProfile.UniversalFiveAxis => Protocol.EnumMotorAxisType.FiveAxisMotor,
                    _ => Protocol.EnumMotorAxisType.TwoAxisMotor
                };
            }
        }

        // 2. 这里引用上面的 SelectedAxisType 属性名，编译器就不会懵圈了
        public string CurrentDbName => SelectedAxisType switch
        {
            Protocol.EnumMotorAxisType.TwoAxisMotor => "TwoMotorTetsMessages.db",
            Protocol.EnumMotorAxisType.ThreeAxisMotor => "ThreeMotorTetsMessages.db",
            Protocol.EnumMotorAxisType.FiveAxisMotor => "FiveMotorTetsMessages.db",
            _ => "DefaultMotorTetsMessages.db"
        };
        // 当前选中的机型（核心状态，全剧唯一的真理）
        private MachineProfile _currentProfile;

        // ==========================================
        // 🌟 终极核心：机型配置矩阵表 (The Truth Matrix)
        // ==========================================
        private static readonly Dictionary<MachineProfile, Dictionary<EnumMotorModel, AxisHardwareConfig>> _machineConfigTable = new()
        {
            // 1. 紧凑型双轴（原小仓 Zem18：X短，Y稍长）
            [MachineProfile.CompactTwoAxis] = new Dictionary<EnumMotorModel, AxisHardwareConfig>
            {
                [EnumMotorModel.MOTOR_x] = new() { ChannelId = EnumMotorId.MOTOR_1, StrokeRange = (120000, 130000) },
                [EnumMotorModel.MOTOR_y] = new() { ChannelId = EnumMotorId.MOTOR_2, StrokeRange = (160000, 170000) }
            },

            // 2. 标准型双轴（原大仓 Zem20：X极长，Y极长）
            [MachineProfile.StandardTwoAxis] = new Dictionary<EnumMotorModel, AxisHardwareConfig>
            {
                [EnumMotorModel.MOTOR_x] = new() { ChannelId = EnumMotorId.MOTOR_1, StrokeRange = (235000, 255000) },
                [EnumMotorModel.MOTOR_y] = new() { ChannelId = EnumMotorId.MOTOR_2, StrokeRange = (215000, 225000) }
            },

            // 3. 重载型三轴（统一 0-100万 量程）
            [MachineProfile.HeavyDutyThreeAxis] = new Dictionary<EnumMotorModel, AxisHardwareConfig>
            {
                // 注意：硬件上，除了双轴机型，其余机型 X 插 2 孔，Y 插 1 孔（保持你原有的硬件路由逻辑）
                [EnumMotorModel.MOTOR_x] = new() { ChannelId = EnumMotorId.MOTOR_2, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_y] = new() { ChannelId = EnumMotorId.MOTOR_1, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_z] = new() { ChannelId = EnumMotorId.MOTOR_4, StrokeRange = (0, 1000000) }
            },

            // 4. 全动型五轴（统一 0-100万 量程）
            [MachineProfile.UniversalFiveAxis] = new Dictionary<EnumMotorModel, AxisHardwareConfig>
            {
                [EnumMotorModel.MOTOR_x] = new() { ChannelId = EnumMotorId.MOTOR_2, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_y] = new() { ChannelId = EnumMotorId.MOTOR_1, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_z] = new() { ChannelId = EnumMotorId.MOTOR_4, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_t] = new() { ChannelId = EnumMotorId.MOTOR_3, StrokeRange = (0, 1000000) },
                [EnumMotorModel.MOTOR_r] = new() { ChannelId = EnumMotorId.MOTOR_5, StrokeRange = (0, 1000000) }
            }
        };

        public MotorTypeModel(ThreeAxisTestModel motorModel, IContainerProvider containerProvider, MachineProfile sampleModel = MachineProfile.StandardTwoAxis)
        {
            _motorModel = motorModel;
            _containerProvider = containerProvider;
            _motorModel.Motors = new ObservableCollection<FiveAxisModel>();

          
            switch (sampleModel)
            {
                case MachineProfile.CompactTwoAxis:
                    IsCompactTwoAxis = true;
                    break;

                case MachineProfile.StandardTwoAxis:
                    IsStandardTwoAxis = true;
                    break;

                case MachineProfile.HeavyDutyThreeAxis:
                    IsHeavyDutyThreeAxis = true;
                    break;

                case MachineProfile.UniversalFiveAxis:
                    IsUniversalFiveAxis = true;
                    break;

                default:
                    IsStandardTwoAxis = true; // 兜底保护，万一传了个未知的进来，默认切到紧凑型
                    break;
            }
            ChangeProfile(sampleModel);
        }

        // ==========================================
        // UI 绑定属性 (纯粹的触发器)
        // ==========================================
        public bool IsCompactTwoAxis
        {
            get => _currentProfile == MachineProfile.CompactTwoAxis;
            set { if (value) ChangeProfile(MachineProfile.CompactTwoAxis); }
        }

        public bool IsStandardTwoAxis
        {
            get => _currentProfile == MachineProfile.StandardTwoAxis;
            set { if (value) ChangeProfile(MachineProfile.StandardTwoAxis); }
        }

        public bool IsHeavyDutyThreeAxis
        {
            get => _currentProfile == MachineProfile.HeavyDutyThreeAxis;
            set { if (value) ChangeProfile(MachineProfile.HeavyDutyThreeAxis); }
        }

        public bool IsUniversalFiveAxis
        {
            get => _currentProfile == MachineProfile.UniversalFiveAxis;
            set { if (value) ChangeProfile(MachineProfile.UniversalFiveAxis); }
        }

        // ==========================================
        // 核心执行方法：查表 + 组装 (0个 if-else 分支！)
        // ==========================================
        private void ChangeProfile(MachineProfile newProfile)
        {
            if (_currentProfile == newProfile && _motorModel.Motors.Count > 0) return;

            _currentProfile = newProfile;
            RaisePropertyChanged(nameof(IsCompactTwoAxis));
            RaisePropertyChanged(nameof(IsStandardTwoAxis));
            RaisePropertyChanged(nameof(IsHeavyDutyThreeAxis));
            RaisePropertyChanged(nameof(IsUniversalFiveAxis));

            // 1. 暴力清空图表和集合
            _motorModel.Motors.Clear();
            _motorModel.MotorplotModel.Series.Clear();
            _motorModel.MotorSpeedplotModel.Series.Clear();

            // 2. 去矩阵表里，拿出当前机型的专属配置单
            if (_machineConfigTable.TryGetValue(_currentProfile, out var axisConfigs))
            {
                // 3. 遍历配置单里的每一个轴，直接无脑生成！
                foreach (var kvp in axisConfigs)
                {
                    EnumMotorModel logicalAxis = kvp.Key;        // 逻辑轴名(X/Y/Z)
                    AxisHardwareConfig config = kvp.Value;       // 插孔与行程参数

                    // 实例化轴
                    var axis = new FiveAxisModel(_containerProvider, config.ChannelId, logicalAxis, _motorModel, $"{logicalAxis.ToString().Replace("MOTOR_", "").ToUpper()}轴");

                    // 注入行程参数
                    axis.FullStrokeRange = config.StrokeRange;

                    // 挂载到集合和图表
                    _motorModel.Motors.Add(axis);
                    axis.SpeedLine.ItemsSource = axis.MotorModel.SpeedList;
                    axis.PosLine.ItemsSource = axis.MotorModel.PointList;
                    _motorModel.MotorplotModel.Series.Add(axis.PosLine);
                    _motorModel.MotorSpeedplotModel.Series.Add(axis.SpeedLine);

                    // 通知单轴自己去画红线（单轴类里直接用 FullStrokeRange 即可，不需要任何判断）
                    axis.ConfirmTheStandardStroke();
                }
            }

            // 4. 刷新UI
            _motorModel.MotorplotModel.InvalidatePlot(true);
            _motorModel.MotorSpeedplotModel.InvalidatePlot(true);
        }
    }

}