using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Services.Interfaces.IServices;
using static UtilityTools.Services.Interfaces.IServices.IThingboardService;

namespace UtilityTools.Services.Services
{
    public class ThingboardService : IThingboardService
    {

        //依赖的组件
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger() ;   //日志记录器
        private IMqttClient _mqttClient;  //MQTT客户端
        private HttpClient _httpClient;   //HTTP客户端


        //实现接口的属性
        public string ServerUrl { get; set ; }
        public string AccessToken { get ; set ; }
        public string MqttBroker { get ; set ; }
        public int MqttPort { get; set; } = 1883;
        public bool EnableMqtt { get; set; } = false;
        public bool EnableHttp { get; set; } = true;

        public bool IsConnected { get; private set; } = true;

        public int TimeoutSeconds { get; set; } = 30;
        public bool AutoReconnect { get; set; } = true;


        public event EventHandler<bool> ConnectionStateChanged;
        public event EventHandler<string> DataUploaded;
        public event EventHandler<UploadFailedEventArgs> UploadFailed;

        public ThingboardService() 
        {
            InitializeHttpClient();
        }

        

        /// <summary>
        /// 初始化HTTP客户端
        /// </summary>

        private void InitializeHttpClient() 
        {
            _httpClient = new HttpClient 
            {
                Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
            };
        }

        /// <summary>
        /// 初始化MQTT客户端
        /// </summary>
        private void InitializeMqttClient()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            //如果启动自动重连，订阅断开事件
            if (AutoReconnect)
            {
                _mqttClient.DisconnectedAsync += async (e) =>
                {
                    _logger.Warn("MQTT 连接断开，参试重连...");

                    //延迟5秒后重连
                    await Task.Delay(5000);

                    try
                    {
                        await ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex,"重连失败");
                    }
                };
            }

           
        }

        /// <summary>
        /// 异步连接到ThingBoard服务器
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            try
            {
                //1.如果启动MQTT，建立连接
                if (EnableMqtt)
                {
                    var options = new MqttClientOptionsBuilder().WithTcpServer(MqttBroker, MqttPort)
                        .WithCredentials(new MqttClientCredentials(AccessToken, null))
                        .WithCleanSession()
                        .Build();

                    await _mqttClient.ConnectAsync(options, CancellationToken.None);

                    _logger.Info($"MQTT 连接成功 :{MqttBroker}:{MqttPort}");


                    //2.更新连接状态 
                    IsConnected = true;

                    //初始事件通知UI
                    ConnectionStateChanged?.Invoke(this, true); 

                }  
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, "连接 ThingBoard 失败");
                IsConnected = false;
                ConnectionStateChanged?.Invoke(this,false);
                throw;//重新抛出异常让调用者知道
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Disconnect()
        {
            //1.如果MQTT已经连接，断开它
            if (EnableMqtt && _mqttClient.IsConnected)
            {
                _mqttClient.DisconnectAsync().Wait();
            }

            //2.更新状态
            IsConnected = false;

            //3.触发事件
            ConnectionStateChanged?.Invoke(this, false);

            _logger.Info("Thingboard 服务已断开");

        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (EnableHttp)
                {
                    var url = $"{ServerUrl}/api/v1/{AccessToken}/attributes";
                    var response = await _httpClient.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }

                //否则测试 MQTT
                if (EnableHttp)
                {
                    var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(MqttBroker, MqttPort)
                        .WithCredentials(new MqttClientCredentials(AccessToken, null)).Build();
                    var result = await _mqttClient.ConnectAsync(options, CancellationToken.None);
                    return result.ResultCode == MqttClientConnectResultCode.Success;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "连接测试失败");
                return false;
            }
        }

      /// <summary>
      /// 上传设备属性
      /// </summary>
      /// <param name="attributes"></param>
      /// <returns></returns>
        public async Task<bool> UploadAttributesAsync(string attributes)
        {
            if (!EnableHttp)
            {
                return false;
            }

            try
            {
                var url = $"{ServerUrl}/api/v1/{AccessToken}/attributes";
                
                var content = new StringContent(attributes, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var success = response.IsSuccessStatusCode;
                if (success)
                {
                    _logger.Info("属性上传成功");
                }
                else
                {
                    _logger.Warn($"属性上传失败：{response.StatusCode}");
                }
                return success;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "上传属性异常");
                UploadFailed?.Invoke(this, new UploadFailedEventArgs
                {
                    FailedData = attributes,
                    Exception = ex,
                    ErrorMessage = ex.Message
                });
                return false;
            }
        }

        

       


        private async Task<bool> UploadViaHttpAsync(string data)
        {
            try
            {
                // 1.构建ThingBoard API URL
                var url = $"{ServerUrl}/api/calibration/";

                //2. 组装
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

                //3.发送 POST 请求
                var response = await _httpClient.PostAsync(url,content);

                //4.检测响应状态
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.Debug($"HTTP 上传成功: {data}");
                    return true;
                }
                else
                {
                    _logger.Warn($"HTTP 上传失败: {response.StatusCode}");
                    string errorDetail = await response.Content.ReadAsStringAsync();
                    UploadFailed?.Invoke(this, new UploadFailedEventArgs
                    {
                        FailedData = data,
                        Exception = null,
                        ErrorMessage = $"[{response.StatusCode}] {errorDetail}"
                    });
                    return false;
                }
                
            }
            catch(Exception ex) 
            {
                _logger.Error(ex, "HTTP 上传异常");

                //触发失败事件
                UploadFailed?.Invoke(this, new UploadFailedEventArgs
                {
                    FailedData =  data,
                    Exception = ex,
                    ErrorMessage = ex.Message
                });

                return false;
            }
        }

        private async Task<bool> UploadviaMqttAsync(string data)
        {
            try
            {
                //1.检测连接状态
                if (!_mqttClient.IsConnected)
                {
                    _logger.Warn("MQTT 未连接，无法上传");
                    return false;
                }

                //2.构建MQTT Topic
                var topic = "v1/devices/me/telemetry";

                //3.
           

                var payload = Encoding.UTF8.GetBytes(data);

                //4.构建并发布消息
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build();

                await _mqttClient.PublishAsync(message, CancellationToken.None);

                _logger.Debug($"MQTT 上传成功:{data}");
                return true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MQTT 上传异常");
                UploadFailed?.Invoke(this, new UploadFailedEventArgs 
                {
                    FailedData = data,
                    Exception = ex,
                    ErrorMessage = ex.Message
                });
                return false;
            }
        }


        // ==========================================
        // 🔑 1. 登录换票接口 (/api/auth/login/)
        // ==========================================
        public async Task<(bool IsSuccess, string Token, int ExpiresIn, string ErrorMsg)> LoginAsync(string username, string password)
        {
            try
            {
                ServerUrl = ThingsBoardAuthManager.Current.ServerUrl;
                if (string.IsNullOrWhiteSpace(ServerUrl)) return (false, "", 0, "服务器地址未配置");

                // 🚨 完全按文档拼接路由
                string requestUrl = $"{ServerUrl.TrimEnd('/')}/api/auth/login/";

                var loginPayload = new { username = username, password = password };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(loginPayload);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds > 0 ? TimeoutSeconds : 10);
                    HttpResponseMessage response = await client.PostAsync(requestUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(responseBody))
                        {
                            string token = doc.RootElement.GetProperty("token").GetString();
                            this.AccessToken = token;

                            // 🚨 文档规定有效期为 1 天 = 86400 秒
                            return (true, token, 86400, string.Empty);
                        }
                    }
                    else
                    {
                        string errorStr = await response.Content.ReadAsStringAsync();
                        _logger.Error($"登录失败{response.Content}");
                        return (false, "", 0, $"登录失败 (HTTP {(int)response.StatusCode}): {errorStr}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "", 0, $"网络通讯异常: {ex.Message}");
            }
        }
        // ==========================================
        // 🛡️ 2. 起飞前检查保镖 (静默换票)
        // ==========================================
        private async Task<bool> EnsureAuthReadyAsync()
        {
            
            var config = ThingsBoardAuthManager.Current;

            // 预留 5 分钟缓冲期
            if (!string.IsNullOrEmpty(config.JwtToken) && config.TokenExpireTime > DateTime.Now.AddMinutes(5))
            {
                return true;
            }

            string pwd = ThingsBoardAuthManager.DecryptPassword(config.EncryptedPassword);
            if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(pwd)) return false;

            // 登录
            var result = await LoginAsync(config.Username, pwd);
            if (result.IsSuccess)
            {
                config.JwtToken = result.Token;
                config.TokenExpireTime = DateTime.Now.AddSeconds(result.ExpiresIn);
                ThingsBoardAuthManager.SaveConfig(); // 保存新票
                return true;
            }

            return false;
        }

        // ==========================================
        // 🚀 3. 标定数据上传接口 (/api/calibration/)
        // ==========================================
        public async Task<bool> UploadTelemetryAsync(string jsonPayload)
        {
            // 没票直接拦截
            if (!await EnsureAuthReadyAsync())
            {
                UploadFailed?.Invoke(this, new UploadFailedEventArgs { ErrorMessage = "授权令牌无效或已过期，自动补票失败！" });
                return false;
            }

            var config = ThingsBoardAuthManager.Current;
            string requestUrl = $"{config.ServerUrl.TrimEnd('/')}/api/calibration/";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds > 0 ? TimeoutSeconds : 10);

                    // 🚨 极其关键的一步：加上 Authorization: Bearer <token>
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.JwtToken);

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(requestUrl, content);


                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        // 文档说 200 是覆盖更新，201 是新建成功，都算成功
                        DataUploaded?.Invoke(this, "数据已成功送达服务器");
                        _logger.Debug($"数据上传成功{jsonPayload}");
                        return true;
                    }
                    else
                    {
                        string errorStr = await response.Content.ReadAsStringAsync();
                        UploadFailed?.Invoke(this, new UploadFailedEventArgs { ErrorMessage = $"上传失败 (HTTP {(int)response.StatusCode}): {errorStr}" });
                        _logger.Debug($"数据上传失败{(int)response.StatusCode}): {errorStr}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                UploadFailed?.Invoke(this, new UploadFailedEventArgs { ErrorMessage = $"网络通讯异常: {ex.Message}" });
                return false;
            }
        }

        public async Task<(bool Success, bool HasData, string RawJson, string ErrorMsg)> QueryCalibrationExistsAsync(string sampleStageId)
        {
            if (!await EnsureAuthReadyAsync())
                return (false, false, string.Empty, "授权令牌无效或已过期，请先在工程师配置界面登录");

            var config = ThingsBoardAuthManager.Current;
            string requestUrl = $"{config.ServerUrl.TrimEnd('/')}/api/calibration/?sample_stage_id={Uri.EscapeDataString(sampleStageId)}";

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.JwtToken);

                var response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(body);
                    bool hasData = doc.RootElement.GetArrayLength() > 0;
                    return (true, hasData, body, string.Empty);
                }
                else
                {
                    string err = await response.Content.ReadAsStringAsync();
                    return (false, false, string.Empty, $"查询失败 (HTTP {(int)response.StatusCode}): {err}");
                }
            }
            catch (Exception ex)
            {
                return (false, false, string.Empty, $"网络异常: {ex.Message}");
            }
        }
    }
}
