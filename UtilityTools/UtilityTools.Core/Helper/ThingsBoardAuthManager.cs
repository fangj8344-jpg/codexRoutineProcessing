using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// ThingsBoard 云端平台对接与授权配置
    /// </summary>
    public class ThingsBoardAuthManager
    {
        // 文件名叫 tb_auth_config.json
        // 1. 先定义专属的配置文件夹路径 (在软件运行目录下的 Configs 文件夹)
        private static readonly string ConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        private static readonly string ConfigFilePath = Path.Combine(ConfigFolder, "tb_auth_config.json");
        // 固定密钥，内部工具够用
        private static readonly byte[] _aesKey = Encoding.UTF8.GetBytes("ZepTools@2025Key");  // 16字节
        private static readonly byte[] _aesIv = Encoding.UTF8.GetBytes("ZepTools@2025_IV");  // 16字节

        // 全局单例对象，随处可用 ThingsBoardAuthManager.Current.JwtToken
        public static ThingsBoardAuthConfigModel Current { get; private set; } = new ThingsBoardAuthConfigModel();
        // ==========================================
        // 1. 保存配置到本地 JSON
        // ==========================================
        public static void SaveConfig()
        {
            try
            {
                // 🚨 核心防坑：既然放在了子文件夹里，保存前必须先检查文件夹存不存在！
                // 如果不存在就新建一个，否则 File.WriteAllText 会直接报错闪退。
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                }
                // 美化 JSON，方便实施工程师在现场用记事本修改 ServerUrl 或 DeviceId
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(ConfigFilePath, jsonString);
            }
            catch (Exception ex)
            {
                // 记录日志
                NLog.LogManager.GetCurrentClassLogger().Error($"保存 TB 授权配置文件失败: {ex.Message}");
            }
        }

        // ==========================================
        // 2. 从本地读取配置
        // ==========================================
        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string jsonString = File.ReadAllText(ConfigFilePath);
                    Current = JsonSerializer.Deserialize<ThingsBoardAuthConfigModel>(jsonString) ?? new ThingsBoardAuthConfigModel();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"读取 TB 授权配置文件损坏，已重置: {ex.Message}");
                Current = new ThingsBoardAuthConfigModel();
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncryptPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = _aesKey;
            aes.IV = _aesIv;
            byte[] encrypted = aes.CreateEncryptor().TransformFinalBlock(
            Encoding.UTF8.GetBytes(plainText), 0, Encoding.UTF8.GetBytes(plainText).Length);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>

        public static string DecryptPassword(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return "";
            try
            {
                using var aes = System.Security.Cryptography.Aes.Create();
                aes.Key = _aesKey;
                aes.IV = _aesIv;
                byte[] bytes = Convert.FromBase64String(encryptedText);
                byte[] decrypted = aes.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return "";
            }
        }

    }
}
