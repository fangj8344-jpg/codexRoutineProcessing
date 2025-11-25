using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.FDC12CHVBox.Model
{
    public static class NetMethodModel
    {
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static string IncrementIpLastSegment(string originalIp)
        {
            // 1. 分割 IP 地址
            string[] ipParts = originalIp.Split('.');

            // 2. 验证 IP 格式是否正确
            if (ipParts.Length != 4)
            {
                throw new ArgumentException("无效的 IP 地址格式。");
            }

            // 3. 提取最后一段并尝试转换为整数
            if (int.TryParse(ipParts[3], out int lastPart))
            {
                // 4. 对最后一段数字加 1
                lastPart++;

                // 5. 将加 1 后的数字转换回字符串，并替换数组中的最后一个元素
                ipParts[3] = lastPart.ToString();

                // 6. 重新组合成新的 IP 地址字符串
                return string.Join('.', ipParts);
            }
            else
            {
                throw new ArgumentException("IP 地址的最后一段不是有效的数字。");
            }
        }
    }
}
