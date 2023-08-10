using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Event;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Extension
{
    public static class DialogExtension
    {
        /// <summary>
        /// 询问窗口
        /// </summary>
        /// <param name="dialogHost">指定的DialogHost会话主机</param>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="dialogHostName">会话主机名称（唯一的）</param>
        /// <returns></returns>
        public static async Task<IDialogResult> Question(this IDialogHostService dialogHost, string title, string content, string dialogHostName = "Root")
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", title);
            dialogParameters.Add("Content", content);
            dialogParameters.Add("dialogHostName", dialogHostName);

            var dialogResult = await dialogHost.ShowDialog("MsgView", dialogParameters, dialogHostName);
            return dialogResult;
        }

        /// <summary>
        /// 推送等待消息
        /// </summary>
        /// <param name="aggregator"></param>
        /// <param name="model"></param>
        public static void UpdateLoading(this IEventAggregator aggregator, UpdateModel model)
        {
            aggregator.GetEvent<UpdateLoadingEvent>().Publish(model);
        }

        /// <summary>
        /// 注册等待消息
        /// </summary>
        /// <param name="aggregator"></param>
        /// <param name="action"></param>
        public static void Register(this IEventAggregator aggregator, Action<UpdateModel> action)
        {
            aggregator.GetEvent<UpdateLoadingEvent>().Subscribe(action);
        }

        /// <summary>
        /// 发送提示消息
        /// </summary>
        /// <param name="aggregator"></param>
        /// <param name="message"></param>
        /// <param name="filterName"></param>
        public static void SendMessage(this IEventAggregator aggregator, string message, string filterName = "Main")
        {
            aggregator.GetEvent<MessageEvent>().Publish(new MessageModel()
            {
                Filter = filterName,
                Message = message
            });
        }

        /// <summary>
        /// 注册提示消息
        /// </summary>
        /// <param name="aggregator"></param>
        /// <param name="action"></param>
        /// <param name="filterName"></param>
        public static void RegisterMessage(this IEventAggregator aggregator, Action<MessageModel> action, string filterName = "Main")
        {
            aggregator.GetEvent<MessageEvent>().Subscribe(action,
                ThreadOption.PublisherThread, true, (m) =>
                {
                    return m.Filter.Equals(filterName);
                });
        }
    }
}
