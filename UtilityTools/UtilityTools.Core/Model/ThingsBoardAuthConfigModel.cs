using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    /// <summary>
    /// ThingsBoard 云端平台对接与授权配置
    /// </summary>
    public class ThingsBoardAuthConfigModel
    {
        // ==========================================
        // 1. 基础连接信息
        // ==========================================
        // TB 服务器地址，比如 "http://192.168.1.100:8080" 或真实的域名
        public string ServerUrl { get; set; } = "http://localhost:8080";

        // 你们设备的唯一标识 (通常在 TB 里对应 Device Name 或 Label)
        public string DeviceId { get; set; } = "";

        // ==========================================
        // 2. 身份验证凭证 (Auth)
        // ==========================================
        // 工程师账号 / TB 租户账号
        public string Username { get; set; } = "";

        // 🚨 DPAPI 加密后的密码密文
        public string EncryptedPassword { get; set; } = "";

        // ==========================================
        // 3. 动态令牌 (Token 生命周期管理)
        // ==========================================
        // 从 TB 拿到的 JWT Token (通常很长一串)
        public string JwtToken { get; set; } = "";

        // Refresh Token (TB 支持用这个来无感刷新 Token，你可以先备着这个字段)
        public string RefreshToken { get; set; } = "";

        // Token 的绝对过期时间 (用于拦截器判断)
        public DateTime TokenExpireTime { get; set; }
    }
}
