using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NetworkAdapter.Tools
{
    public static class Utils
    {
        // <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }

        static readonly HttpClient httpClient = new HttpClient();
        public static Task LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                {
                    // #if !NO_SPAM
                    Debug.Log("Exception in Task \n" + exception.Message + '\n' + exception.StackTrace);
                    // #endif
                }
            },
                TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }


        public static async Task<T> WithTimeout<T>(this Task task, TimeSpan timeout, CancellationToken ct = default)
        {
            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                var t = await Task.WhenAny(task, Task.Delay(timeout, cancel.Token));
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                cancel.Cancel();
                if (!task.IsCompleted)
                    throw new TimeoutException();
                return await (Task<T>)t;
            }
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout, CancellationToken ct = default)
        {
            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                var t = await Task.WhenAny(task, Task.Delay(timeout, cancel.Token));
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                cancel.Cancel();
                if (!task.IsCompleted)
                    throw new TimeoutException();
                await t;
            }
        }

        public static bool TryRemove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(
                new KeyValuePair<TKey, TValue>(key, value));
        }

        public static Dictionary<TK, TV> GetSnapshot<TK, TV>(this ConcurrentDictionary<TK, TV> dictionary)
        {
            return dictionary.ToDictionary(x => x.Key, y => y.Value);
        }

        public static async Task<Sprite> LoadRemoteImageAsync(string url)
        {
            try
            {
                var data = await httpClient.GetByteArrayAsync(url);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = "remoteImage";
                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return null;
        }

        public static void CopyProperties(this object source, object target)
        {
            var sourceProperties = source.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();

            foreach (var sp in sourceProperties)
            {
                foreach (var tp in targetProperties)
                {
                    if (sp.Name == tp.Name && sp.PropertyType == tp.PropertyType)
                    {
                        tp.SetValue(target, sp.GetValue(source));
                        break;
                    }
                }
            }
        }

        public static void CopyFields(this object source, object target)
        {
            var sourceFields = source.GetType().GetFields();
            var targetFields = target.GetType().GetFields();

            foreach (var sf in sourceFields)
            {
                foreach (var tf in targetFields)
                {
                    if (sf.Name == tf.Name && sf.FieldType == tf.FieldType)
                    {
                        tf.SetValue(target, sf.GetValue(source));
                        break;
                    }
                }
            }
        }

        public static string HashSHA256(this string str)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(str.GetUTF8Bytes()));
        }

        public static byte[] GetUTF8Bytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string GetUTF8String(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
