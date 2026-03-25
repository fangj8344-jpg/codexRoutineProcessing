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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        
        /// <summary>
        /// 上传遥测数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UploadTelemetryAsync(string data)
        {

            bool success = false;

            //1.如果启用HTTP， 使用HTTP上传
            if (EnableHttp)
            {
                success = await UploadViaHttpAsync(data);
            }
            //2.如果启用MQTT且已连接，使用MQTT连接
            if (EnableMqtt && _mqttClient.IsConnected)
            {
                var mqttSuccess = await UploadviaMqttAsync(data);
                success = mqttSuccess || success;
            }

            //3.根据结果触发事件
            if (success)
            {
                var json = JsonConvert.SerializeObject(data);
                DataUploaded?.Invoke(this, json);
                _logger.Debug($"数据上传成功:{json}");
            }
            
            return success;
        }

       


        private async Task<bool> UploadViaHttpAsync(string data)
        {
            try
            {
                // 1.构建ThingBoard API URL
                var url = $"{ServerUrl}/api/calibration/";

                //2. 组装
             
                var content = new StringContent(data,Encoding.UTF8,"application/json");

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
                    UploadFailed?.Invoke(this, new UploadFailedEventArgs
                    {
                        FailedData = data,
                        Exception = null,
                        ErrorMessage = response.StatusCode.ToString()
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
    }
}
