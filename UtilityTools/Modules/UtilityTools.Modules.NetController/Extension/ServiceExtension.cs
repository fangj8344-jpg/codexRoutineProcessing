#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Extension
 * 唯一标识：cb8a233c-68a3-4a15-bbe6-7489b6b03630
 * 文件名：ServiceExtension
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/15 9:49:51
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

using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Modules.NetController.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.NetController.Extension
{
    public static class ServiceExtension
    {
        /// <summary>
        /// 设置CCS继电器状态
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="channel">通道信息</param>
        /// <param name="enable">使能状态</param>
        /// <returns></returns>
        public static bool SetCCSRelay(this ISyncRWService service, int channel, bool enable)
        {
            string cmdName = $"SetCCSk{channel}";
            List<string> inParams = new List<string>();
            inParams.Add(enable ? "1" : "0");
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置压缩镜值
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="value">压缩镜数值</param>
        /// <returns></returns>
        public static bool SetCCSFocusValue(this ISyncRWService service, int value)
        {
            string cmdName = "SetOB";
            List<string> inParams = new List<string>();
            inParams.Add(value.ToString());
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置压缩镜值
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="value">压缩镜数值</param>
        /// <returns></returns>
        public static bool SetCCSCompressValue(this ISyncRWService service, int channel, int value)
        {
            string cmdName = $"SetCompress{channel:X}";
            List<string> inParams = new List<string>();
            inParams.Add(value.ToString());
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置对中线圈数值
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="value">对中线圈数值</param>
        /// <returns></returns>
        public static bool SetCCSCenterValue(this ISyncRWService service, int channel, int value)
        {
            string cmdName = $"SetAligX1";
            switch (channel)
            {
                case 0: cmdName = $"SetAligX1"; break;
                case 1: cmdName = $"SetAligX2"; break;
                case 2: cmdName = $"SetAligY1"; break;
                case 3: cmdName = $"SetAligY2"; break;
            }
            List<string> inParams = new List<string>();
            inParams.Add(value.ToString());
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置像散线圈数值
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="value">像散线圈数值</param>
        /// <returns></returns>
        public static bool SetCCSAstigValue(this ISyncRWService service, int channel, int value)
        {
            string cmdName = $"SetAstig{channel:X}";
            List<string> inParams = new List<string>();
            inParams.Add(value.ToString());
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置DAC输出数值
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="value">DAC输出数值</param>
        /// <returns></returns>
        public static bool SetDACValue(this ISyncRWService service, int channel, int value)
        {
            string cmdName = $"T_SetDAC";
            List<string> inParams = new List<string>();
            inParams.Add(channel.ToString("X"));
            inParams.Add(value.ToString());
            bool result = service.SendSetCommand(cmdName, inParams);
            if (result == false)
            {
                LogManager.GetCurrentClassLogger().Error(String.Format("SendGetCommand({0}) return false", cmdName));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 发送握手协议
        /// </summary>
        /// <param name="service">服务实例</param>
        /// <param name="cmd">握手指令</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool SendHandshake(this ISyncRWService service)
        {
            var netConfig = service.GetHandle() as NetConfigModel;
            if (netConfig == null) 
            {
                LogManager.GetCurrentClassLogger().Error($"{service.Name} is NOT Net Service!");
                return false;
            }
            string cmdName = "SendHandshake";
            byte[] cmd = NetControllerProtocol.PackBytesHandShake(netConfig.HostIp, netConfig.HostPort);
            byte[] response;
            int resultLen;

            service.Request(cmdName, cmd, out response, out resultLen, 2000);

            if (resultLen > 0)
            {
                if (!NetControllerProtocol.CheckResponse(response, resultLen))
                {
                    LogManager.GetCurrentClassLogger().Error($"{service.Name} CheckResponse Failed: {service.GetCmdString(response, resultLen)}");
                    return false;
                }

                string resStr = Encoding.Default.GetString(response, 0, resultLen);
                if (resStr.Contains("Error Code"))
                {
                    string[] errList = resStr.Split(':', ',');
                    if (errList.Length < 5)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resultLen)}");
                        return false;
                    }

                    int errCode = int.Parse(errList[3], System.Globalization.NumberStyles.HexNumber);
                    return NetControllerProtocol.ParseErrCode(errCode);
                }
                else
                {
                    string[] pList = resStr.Split(':', ',');
                    if (pList.Length < 4)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resultLen)}");
                        return false;
                    }

                    if (HardwareMethod.IsIPAddress(pList[0])) 
                    {
                        netConfig.TargetIp = pList[0];
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 发送设置指令
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="paramList"></param>
        /// <param name="outTime"></param>
        /// <returns></returns>
        public static bool SendSetCommand(this ISyncRWService service, string cmdName, List<string> paramList, int outTime = 2000)
        {
            var netConfig = service.GetHandle() as NetConfigModel;
            if (netConfig == null)
            {
                LogManager.GetCurrentClassLogger().Error($"{service.Name} is NOT Net Service!");
                return false;
            }

            byte[] cmd = NetControllerProtocol.PackageBytesCmd(cmdName, paramList, netConfig.HostIp, netConfig.HostPort);
            byte[] response;
            int resLen;
            LogManager.GetCurrentClassLogger().Debug(cmd);
            service.Request(cmdName, cmd, out response, out resLen, outTime);
            if(resLen > 0)
            {
                if (!NetControllerProtocol.CheckResponse(response, resLen))
                {
                    LogManager.GetCurrentClassLogger().Error($"{service.Name} CheckResponse Failed: {service.GetCmdString(response, resLen)}");
                    return false;
                }

                string responseStr = Encoding.UTF8.GetString(response, 0, resLen);
                if (responseStr.Contains("Error Code"))
                {
                    string[] errList = responseStr.Split(':', ',');
                    if (errList.Length < 4)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resLen)}");
                        return false;
                    }

                    int errCode = int.Parse(errList[2], System.Globalization.NumberStyles.HexNumber);
                    return NetControllerProtocol.ParseErrCode(errCode);
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 发送不带参的获取指令
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="paramList"></param>
        /// <param name="outTime"></param>
        /// <returns></returns>
        public static bool SendSetCommand(this ISyncRWService service, string cmdName, out List<string> paramList, int outTime = 2000)
        {
            paramList = new List<string>();

            var netConfig = service.GetHandle() as NetConfigModel;
            if (netConfig == null)
            {
                LogManager.GetCurrentClassLogger().Error($"{service.Name} is NOT Net Service!");
                return false;
            }

            byte[] cmd = NetControllerProtocol.PackageBytesCmd(cmdName, paramList, netConfig.HostIp, netConfig.HostPort);
            byte[] response;
            int resLen;
            LogManager.GetCurrentClassLogger().Debug(cmd);
            service.Request(cmdName, cmd, out response, out resLen, outTime);
            
            if (resLen > 0)
            {
                if (!NetControllerProtocol.CheckResponse(response, resLen))
                {
                    LogManager.GetCurrentClassLogger().Error($"{service.Name} CheckResponse Failed: {service.GetCmdString(response, resLen)}");
                    return false;
                }

                string responseStr = Encoding.UTF8.GetString(response, 0, resLen);
                if (responseStr.Contains("Error Code"))
                {
                    string[] errList = responseStr.Split(':', ',');
                    if (errList.Length < 4)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resLen)}");
                        return false;
                    }

                    int errCode = int.Parse(errList[2], System.Globalization.NumberStyles.HexNumber);
                    return NetControllerProtocol.ParseErrCode(errCode);
                }
                else
                {
                    string[] pList = responseStr.Split(':', ',');
                    if (pList.Length < 3)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resLen)}");
                        return false;
                    }

                    for (int i = 1; i < pList.Length - 3; ++i)
                    {
                        if (pList[i] == netConfig.TargetIp)
                            break;
                        if (pList[i] == "")
                        {
                            paramList.Add("0");
                        }
                        else
                        {
                            paramList.Add(pList[i]);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 发送带参的获取指令
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cmdName"></param>
        /// <param name="inparamList"></param>
        /// <param name="paramList"></param>
        /// <param name="outTime"></param>
        /// <returns></returns>
        public static bool SendSetCommand(this ISyncRWService service, string cmdName, List<string> inparamList, out List<string> paramList, int outTime = 2000)
        {
            paramList = new List<string>();

            var netConfig = service.GetHandle() as NetConfigModel;
            if (netConfig == null)
            {
                LogManager.GetCurrentClassLogger().Error($"{service.Name} is NOT Net Service!");
                return false;
            }

            byte[] cmd = NetControllerProtocol.PackageBytesCmd(cmdName, inparamList, netConfig.HostIp, netConfig.HostPort);
            byte[] response;
            int resLen;
            LogManager.GetCurrentClassLogger().Debug(cmd);
            service.Request(cmdName, cmd, out response, out resLen, outTime);

            if (resLen > 0)
            {
                if (!NetControllerProtocol.CheckResponse(response, resLen))
                {
                    LogManager.GetCurrentClassLogger().Error($"{service.Name} CheckResponse Failed: {service.GetCmdString(response, resLen)}");
                    return false;
                }

                string responseStr = Encoding.UTF8.GetString(response, 0, resLen);
                if (responseStr.Contains("Error Code"))
                {
                    string[] errList = responseStr.Split(':', ',');
                    if (errList.Length < 4)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resLen)}");
                        return false;
                    }

                    int errCode = int.Parse(errList[2], System.Globalization.NumberStyles.HexNumber);
                    return NetControllerProtocol.ParseErrCode(errCode);
                }
                else
                {
                    string[] pList = responseStr.Split(':', ',');
                    if (pList.Length < 3)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{service.Name} Response Format Error: {service.GetCmdString(response, resLen)}");
                        return false;
                    }

                    for (int i = 1; i < pList.Length - 3; ++i)
                    {
                        if (pList[i] == netConfig.TargetIp)
                            break;
                        if (pList[i] == "")
                        {
                            paramList.Add("0");
                        }
                        else
                        {
                            paramList.Add(pList[i]);
                        }
                    }
                }
            }

            return false;
        }
    }
}
