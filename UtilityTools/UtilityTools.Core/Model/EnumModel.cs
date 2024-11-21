#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：b649a380-46fc-4bca-88dc-6acc5428a372
 * 文件名：EnumModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/11/20 10:20:30
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public enum EnumDeviceID
    {
        [Description("主控制板")]
        DeviceID_MAIN_CONTROL_PANEL = 0X0100,

        [Description("真空控制板")]
        DeviceID_VACUUM_CONTROL_PANEL = 0X0101,

        [Description("灯带控制板")]
        DeviceID_LIGHT_BELT_CONTROL_PANEL = 0X0102,

        [Description("20kv高压箱")]
        DeviceID_20KV_HIGH_PRESSURE_BOX = 0X0200,

        [Description("五轴电机控制板")]
        DeviceID_FIVEAXIS_MOTOR_CONTROL_PANEL = 0X0300
    }
}
