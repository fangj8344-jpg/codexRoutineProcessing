using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UtilityTools.Core.Event
{
    public class PubSub
    {
        public PubSub()
        {
            handles = new ConcurrentDictionary<string, EventWaitHandle>();
            results = new ConcurrentDictionary<string, object>();
        }

        public Task<object> Sub(string key, int time_out)
        {
            if (handles.TryGetValue(key, out _))
            {
                throw new Exception("重复订阅");
            }

            EventWaitHandle handle = new AutoResetEvent(false);
            handles.TryAdd(key, handle);

            return Task.Run(() =>
            {
                if (!handle.WaitOne(time_out))
                {
                    // 清理
                    handles.TryRemove(key, out _);

                    throw new Exception("请求超时");
                }

                if (results.TryGetValue(key, out object? ret))
                {
                    // 清理
                    results.TryRemove(key, out _);
                    handles.TryRemove(key, out _);
                }

                return ret;
            });
        }

        public void Pub(string key, object result)
        {
            EventWaitHandle handle;
            if (handles.TryGetValue(key, out handle))
            {
                results.TryAdd(key, result);
                handle.Set();
            }
        }

        private ConcurrentDictionary<string, EventWaitHandle> handles;
        private ConcurrentDictionary<string, object> results;
    }
}
