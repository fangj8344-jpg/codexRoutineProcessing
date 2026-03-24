using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Services.Interfaces.IServices
{
    /// <summary>
    /// Thingboard 数据上传服务接口
    /// </summary>
    public interface IThingboardService
    {

        #region Event
        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event EventHandler<bool> ConnectionStateChanged;

        /// <summary>
        /// 数据上传成功事件
        /// </summary>
        event EventHandler<string> DataUploaded;

        /// <summary>
        /// 数据上传失败事件
        /// </summary>
        event EventHandler UploadFailed;
        #endregion

        #region Properties

        /// <summary>
        /// ThingBoard 服务器地址(HTTP)
        /// </summary>
        string ServerUrl { get; set; }


        /// <summary>
        /// 认证token
        /// </summary>
        public string AccessToken {get; set;}

        /// <summary>
        /// MQTT Broker 服务器地址
        /// </summary>
        string MqttBroker { get; set; }

        /// <summary>
        /// MQTT 端口号
        /// </summary>
        int MqttPort { get; set; }

        /// <summary>
        /// 是否启用MQTT协议
        /// </summary>
        bool EnableMqtt { get; set; }

        /// <summary>
        /// 是否启动HTTP协议
        /// </summary>
        bool EnableHttp { get; set; }

        /// <summary>
        /// 当前连接状态(只读)
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接超时时间(秒)
        /// </summary>
        int TimeoutSeconds { get; set; }

        /// <summary>
        /// 是否自动重连
        /// </summary>
        bool AutoReconnect { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 连接到 ThingBoard 服务器(异步)
        /// </summary>
        /// <returns>连接任务</returns>
        /// <remarks>
        /// 1.启动程序时调用
        /// 2.断开后重连
        /// - 连接成功/失败会触发 ConnectionStateChanged 事件
        /// <remarks>
        Task ConnectAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 上传遥测数据(JSON格式)
        /// </summary>
        /// <param name="data"></param>
        /// <returns>是否成功</returns>
        Task<bool> UploadTelemetryAsync(string data);

        

        /// <summary>
        /// 上传设备属性
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// 使用场景：上传设备固件版本、序列号等不常变化的属性
        /// 
        /// 示例：
        /// var attrs = new Dictionary<string, object>
        /// {
        ///     { "firmwareVersion", "1.0.0" },
        ///     { "serialNumber", "SN123456" }
        /// };
        /// await service.UploadAttributesAsync(attrs);
        /// </remarks>
        Task<bool> UploadAttributesAsync(string attributes);

      

        /// <summary>
        /// 测试连接(用于配置界面)
        /// </summary>
        /// <returns>是否连接成功</returns>
        /// /// <remarks>
        /// 使用场景：
        /// 用户在配置界面输入服务器地址和 AccessToken 后，
        /// 点击"测试连接"按钮调用此方法
        /// </remarks>
        Task<bool> TestConnectionAsync();
        #endregion

        /// <summary>
        /// 上传失败事件参数类
        /// </summary>
        /// <remarks>
        /// 当上传失败时，是通过事件传递失败信息
        /// </remarks>
        public class UploadFailedEventArgs : EventArgs
        {
            /// <summary>
            /// 失败的消息
            /// </summary>
            public string ErrorMessage { get; set; } 
            /// <summary>
            /// 失败的数据(可以用于重试)
            /// </summary>
            public string FailedData { get; set; }
            /// <summary>
            /// 原始异常
            /// </summary>
            public Exception Exception { get; set; }
        }
    
      
    }
}
