using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NetworkAdapter.Brokers
{
    /// <summary>
    /// Multi-Writer - Single-Reader async buffer
    /// </summary>
    public class AsyncBuffer<T>
    {
        Queue<T> q = new Queue<T>();
        TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
        object sync = new object();

        public void Enqueue(T item)
        {
            lock (sync)
            {
                q.Enqueue(item);
                source.TrySetResult(true);
            }
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            lock (sync)
            {
                if (q.Count > 0)
                    return q.Dequeue();
                source = new TaskCompletionSource<bool>();
                cancellationToken.Register(() => source.TrySetCanceled());
            }
            await source.Task;
            return await DequeueAsync(cancellationToken);
        }

        public void Clear()
        {
            lock (sync)
            {
                source.TrySetCanceled();
                q.Clear();
            }
        }
    }
}