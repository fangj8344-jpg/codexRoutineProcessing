#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：HvController.Model
 * 唯一标识：7043b476-2e03-40ce-814f-773ad8bb8878
 * 文件名：EnumModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/22 17:46:39
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

namespace UtilityTools.Modules.HvController.Model
{
    /// <summary>
    /// 灯丝类型枚举
    /// </summary>
    public enum EnumFilaTypes
    {
        [Description("自焊灯丝")]
        SelfProduct = 0,
        [Description("曹峰提供")]
        CFProduct = 1,
        [Description("六硼化镧")]
        LabFila = 2,
    }

    /// <summary>
    /// 灯丝状态
    /// </summary>
    public enum EnumFilaStates
    {
        [Description("未定义")]
        Undefine,
        [Description("待机状态")]
        Idle,
        [Description("高压错误")]
        HvErr,
        [Description("系统错误")]
        SysErr,
        [Description("高压开启失败")]
        OpenFailed,
        [Description("灯丝耗尽")]
        FilaOut,
        [Description("高压切换失败")]
        ChangFailed,
        [Description("高压开启中")]
        Opening,
        [Description("高压开启")]
        Opened,
        [Description("高压切换中")]
        Changing,
        [Description("高压关闭中")]
        Closing,
    }
}
