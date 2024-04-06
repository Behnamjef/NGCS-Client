using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NetworkAdapter.Tools;
using UnityEngine;

namespace NetworkAdapter.Brokers
{
    public class WebSocketBrocker : IDisposable
    {
        const int bufferSize = 4 * 1024;

        public Dictionary<string, string> headers = new Dictionary<string, string>();
        public TimeSpan connectTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan readTimeout = TimeSpan.FromSeconds(60);
        public TimeSpan sendTimeout = TimeSpan.FromSeconds(5);
        ArraySegment<byte> bufferCmpArraySegment = new ArraySegment<byte>(new byte[2048]);

        public event Action<string> OnMessage;

        WebSocket socket;
        AsyncBuffer<string> writeBuffer = new AsyncBuffer<string>();
        byte[] readBuffer = new byte[bufferSize];
        TaskCompletionSource<bool> disposeTaskSource = new TaskCompletionSource<bool>();

        public async Task ConnectAsync(Uri uri)
        {
            if (disposeTaskSource.Task.IsCompleted)
                throw new ObjectDisposedException(nameof(WebSocketBrocker));
            var ws = new ClientWebSocket();
            ws.Options.Cookies = new System.Net.CookieContainer();
            foreach (var item in headers)
            {
                ws.Options.SetRequestHeader(item.Key, item.Value);
            }
            socket = ws;
            await ws.ConnectAsync(uri, CancellationToken.None).WithTimeout(connectTimeout);
        }

        /// <summary>
        /// Processes the socket
        /// </summary>
        /// <returns>Close reason</returns>
        public async Task<string> Process()
        {
            if (disposeTaskSource.Task.IsCompleted)
                throw new ObjectDisposedException(nameof(WebSocketBrocker));
            try
            {
                await Task.WhenAny(disposeTaskSource.Task,
                    Task.WhenAll(
                        runReader(CancellationToken.None),
                        runWriter(CancellationToken.None)
                    ));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return socket.CloseStatusDescription;
        }

        public void Send(string data)
        {
            writeBuffer.Enqueue(data);
        }


        #region Reader

        async Task runReader(CancellationToken cts)
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var str = await read(cts);
                    if (str == null)
                        return;
                    invokeMessage(str);
                }
            }
            catch (TimeoutException)
            {

                Debug.LogError("Read time-out");
            }
            catch (WebSocketException ex)
            {
                Debug.LogError($"WebSocketException: Code {ex.WebSocketErrorCode} - {ex}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
            finally
            {
                writeBuffer.Enqueue(null);
            }
        }

        async Task<string> read(CancellationToken cancellationToken)
        {
            if (headers.ContainsKey("Compressed"))
            {
                if (headers["Compressed"] == "true")
                {
                    return await readCompressed(cancellationToken);
                }
                else
                {
                    return await readUnCompressed(cancellationToken);
                }
            }
            else
            {
                return await readUnCompressed(cancellationToken);
            }
        }

        [Obsolete]
        async Task<string> readUnCompressed(CancellationToken cancellationToken)
        {
            var result = await socket
                .ReceiveAsync(new ArraySegment<byte>(readBuffer, 0, readBuffer.Length), cancellationToken)
                .WithTimeout<WebSocketReceiveResult>(readTimeout, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                return null;
            if (result.EndOfMessage)
                return Encoding.UTF8.GetString(readBuffer, 0, result.Count);

            //Large message, use stream
            using (var ms = new MemoryStream())
            {
                ms.Write(readBuffer, 0, result.Count);
                while (!cancellationToken.IsCancellationRequested)
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(readBuffer, 0, readBuffer.Length),
                            cancellationToken)
                        .WithTimeout<WebSocketReceiveResult>(readTimeout, cancellationToken);
                    ms.Write(readBuffer, 0, result.Count);
                    if (result.MessageType == WebSocketMessageType.Close)
                        return null;
                    if (result.EndOfMessage)
                        return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            return null;
        }

        async Task<string> readCompressed(CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(bufferCmpArraySegment, cancellationToken);
                ms.Write(bufferCmpArraySegment.Array, bufferCmpArraySegment.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);


            var arrRaw = ms.ToArray();
            if (result.MessageType == WebSocketMessageType.Close)
                return null;
            else
            {
                var insCompress = new DeflateCompression();
                var arrCmpBuff = insCompress.Decompress(arrRaw);
                return Encoding.UTF8.GetString(arrCmpBuff);
            }
        }

        #endregion

        #region Writer

        async Task runWriter(CancellationToken cancellationToken)
        {
            if (headers.ContainsKey("Compressed"))
            {
                if (headers["Compressed"] == "true")
                {
                    await writerCompressed(cancellationToken);
                }
                else
                {
                    await writerUnCompressed(cancellationToken);
                }
            }
            else
            {
                await writerUnCompressed(cancellationToken);
            }
        }

        async Task writerCompressed(CancellationToken cts)
        {
            try
            {
                const int chunkSize = 2048;
                while (socket.State == WebSocketState.Open)
                {
                    var s = await writeBuffer.DequeueAsync(cts).ConfigureAwait(false);
                    if (socket.State != WebSocketState.Open)
                        return;
                    if (s == null)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal", cts)
                            .WithTimeout(sendTimeout).ConfigureAwait(false);
                        return;
                    }

                    byte[] buff = Encoding.UTF8.GetBytes(s);
                    var insCompression = new DeflateCompression();
                    buff = insCompression.Compress(buff);

                    int offset = 0;
                    while (offset < buff.Length)
                    {
                        int count = Math.Min(chunkSize, buff.Length - offset);
                        var segment = new ArraySegment<byte>(buff, offset, count);
                        await socket.SendAsync(segment, WebSocketMessageType.Binary, offset + count == buff.Length, CancellationToken.None).ConfigureAwait(false);
                        offset += count;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                socket.Dispose();
            }
        }

        [Obsolete]
        async Task writerUnCompressed(CancellationToken cts)
        {
            try
            {
                byte[] sendBuffer = new byte[bufferSize];
                while (socket.State == WebSocketState.Open)
                {
                    var s = await writeBuffer.DequeueAsync(cts);
                    if (socket.State != WebSocketState.Open)
                        return;
                    if (s == null)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal", cts)
                            .WithTimeout(sendTimeout);
                        return;
                    }

                    long len;
                    byte[] buff;
                    if (sendBuffer.Length >= Encoding.UTF8.GetMaxByteCount(s.Length))
                    {
                        len = Encoding.UTF8.GetBytes(s, 0, s.Length, sendBuffer, 0);
                        buff = sendBuffer;
                    }
                    else
                    {
                        buff = Encoding.UTF8.GetBytes(s);
                        len = buff.Length;
                    }

                    for (long i = 0; i < len; i += bufferSize)
                    {
                        var m = new ArraySegment<byte>(buff, (int)i, (int)Math.Min(len - i, bufferSize));
                        await socket.SendAsync(m, WebSocketMessageType.Text, i + bufferSize >= len, cts)
                            .WithTimeout(sendTimeout);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                socket.Dispose();
            }
        }

        #endregion

        void invokeMessage(string message)
        {
            try
            {
                OnMessage?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void Dispose()
        {
            disposeTaskSource.TrySetResult(true);
            writeBuffer.Enqueue(null);
            Task.Delay(2000);
            socket.Abort();
            socket.Dispose();
        }
    }
}