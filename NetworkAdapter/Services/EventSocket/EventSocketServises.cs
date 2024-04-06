using NetworkAdapter.Tools;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using NetworkAdapter.Brokers;
using UnityEngine;
using System.Threading;

namespace NetworkAdapter.Services.EventSocket
{
    
    public enum MethodType
    {
        MatchMaking,
    }

    public class RestSocketModel
    {
        public MethodType type { get; set; }
        public List<KeyValuePair<string, string>> header { get; set; }
        public object data { get; set; }
        public string errorMessage { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class EventSocketServices : IDisposable
    {

        NetworkHttpBrocker httpClient;
        Task socketTask;
        Timer timer;
        CancellationTokenSource cancelSource;
        WebSocketBrocker socketAdapter;
        TimeSpan pingTimeout = TimeSpan.FromMinutes(1);

        public EventSocketListeners EventListeners { get; set; } = new EventSocketListeners();

        private event Action<string> OnEventReceived;
        private event Action<string> OnApiDataRecieved;
        private event Action<string> OnSendApiData;
        private event Action OnAuthorizationFailed;

        public event Action OnConnect;
        public event Action OnDisconnect;

        public bool IsConnected { get; set; }

        string url = "";

        public struct EventPushMessage
        {
            public string sender;
            public long id;
            public bool ack;
            public string name;
            public JToken data;
        }

        public struct EventAckMessage
        {
            public string sender;
            public long id;
        }

        public EventSocketServices(NetworkHttpBrocker client, string url)
        {
            this.url = getWebSocketUri(url + "core/ws/event");
            this.httpClient = client;
            timer = new Timer(timerCallback);
            OnEventReceived += EventListeners.CompileEvent;
        }

        //--Socket--

        public async void Send(RestSocketModel message)
        {
            //Debug.Log($"Socket Request Sending => Type : {message.type} | Data : {JsonConvert.SerializeObject(message.data)}");

            var serializeMessage = NetworkEventPacket.API(httpClient.userInfo.userId, message.Serialize()).Serialize();
            if (IsConnected)
            {
                socketAdapter?.Send(serializeMessage);
            }
            else
            {
                await Utils.WaitUntil(IsConnect, 20, 20000);
                socketAdapter?.Send(serializeMessage);
            }
        }

        private bool IsConnect() => IsConnected;

        public void StartSocket()
        {
            if (socketTask == null)
            {
                socketTask = runLoop().LogExceptions();
                timer.Change((int)pingTimeout.TotalMilliseconds / 2, (int)pingTimeout.TotalMilliseconds / 2);
            }
        }

        public void StopSocket()
        {
            if (socketTask == null)
                return;
            if (cancelSource != null && !cancelSource.IsCancellationRequested)
                cancelSource.Cancel();
            socketTask = null;
            timer.Change(int.MaxValue, int.MaxValue);
            //Debug.Log("Stopped event push socket");
        }

        void timerCallback(object state)
        {
            socketAdapter?.Send(NetworkEventPacket.Ping().Serialize());
        }

        async Task runLoop()
        {
            //Debug.Log("Starting event push socket");
            using (cancelSource = new CancellationTokenSource())
            {
                int fails = 0;
                while (!cancelSource.IsCancellationRequested)
                {
                    try
                    {
                        //Debug.Log($"EventPush: Connecting to {url}");
                        using (socketAdapter = new WebSocketBrocker())
                        {
                            // http client will fill in api service. 
                            // always remember to call init and login befor create a room socket
                            socketAdapter.headers.Add("Authorization", "Bearer " + httpClient.userInfo.token);
                            socketAdapter.headers.Add("X-Language", httpClient.userInfo.lang);
                            socketAdapter.headers.Add("ClientVersion", httpClient.userInfo.clientVersion);
                            socketAdapter.headers.Add("Compressed", "true");

                            socketAdapter.readTimeout = pingTimeout;
                            await socketAdapter.ConnectAsync(new Uri(url));

                            cancelSource.Token.Register(() => socketAdapter.Dispose());

                            //Reset fail counter
                            fails = 0;

                            invokeConnect();

                            socketAdapter.OnMessage += m =>
                            {
                                handleMessage(m);
                            };

                            var closeReason = await socketAdapter.Process();

                            //Debug.Log($"Socket close reason: {closeReason}");

                            if (closeReason == "UNAUTHORIZED")
                            {
                                invokeAuthorizationFailed();
                                Dispose();
                                break;
                            }
                            if (closeReason == "FORCE-CLOSE")
                            {
                                invokeDisconnect();
                                Dispose();
                                break;
                            }
                            if (closeReason == "MOVE")
                            {
                                await Task.Delay(7_000);
                            }
                            if (closeReason == "TIME-OUT")
                            {
                                invokeDisconnect();
                                Dispose();
                                break;
                            }
                            if (closeReason == "DUPLICATE")
                            {
                                invokeAuthorizationFailed();
                                Dispose();
                                break;
                            }
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                    catch (WebSocketException ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                    invokeDisconnect();
                    fails++;
                    await Task.Delay(Math.Min(5000, fails * 500));
                }
            }
        }

        void invokeAuthorizationFailed()
        {
            ////Debug.Log("RoomClient: Authorization failed");
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnAuthorizationFailed?.Invoke();
                });
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
        }
        void invokeConnect()
        {
            if (IsConnected)
                return;
            //Debug.Log("EventPush: Connected");
            IsConnected = true;
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnConnect?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        void invokeDisconnect()
        {
            if (!IsConnected)
                return;
            //Debug.Log("EventPush: Disconnected");
            IsConnected = false;
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnDisconnect?.Invoke();
                });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }


        void handleMessage(string str)
        {
            var packet = new NetworkEventPacket(str);
            switch (packet.type)
            {
                case NetworkEventPacket.TYPE_PONG:
                    break;
                case NetworkEventPacket.TYPE_PING:
                    socketAdapter?.Send(NetworkEventPacket.Pong().Serialize());
                    break;
                case NetworkEventPacket.TYPE_SETTINGS:
                    var dct = packet.GetDataDictionary();
                    pingTimeout = TimeSpan.FromMilliseconds(int.Parse(dct["timeout"]));
                    socketAdapter.readTimeout = pingTimeout;
                    timer.Change(1000, (int)pingTimeout.TotalMilliseconds / 2);
                    break;
                case NetworkEventPacket.TYPE_EVENT:
                    invokeCallback(packet.data);
                    break;
                case NetworkEventPacket.TYPE_ACKED_EVENT:
                    invokeCallback(packet.data);
                    socketAdapter?.Send(NetworkEventPacket.EventAck(packet.id).Serialize());
                    break;
                case NetworkEventPacket.TYPE_QUERY:
                    invokeCallback(packet.data);
                    socketAdapter?.Send(NetworkEventPacket.QueryAck(packet.id, httpClient.userInfo.userId).Serialize());
                    break;
                case NetworkEventPacket.TYPE_QUERY_ACK:
                    invokeCallback(packet.data);
                    socketAdapter?.Send(NetworkEventPacket.Query(packet.id, httpClient.userInfo.userId).Serialize());
                    break;
                case NetworkEventPacket.TYPE_API:
                    OnApiDataRecieved(packet.data);
                    break;
                default:
                    if (str.Length > 1)
                        invokeCallback(str);
                    break;

            }
        }
        //----


        //--Helpers--
        string getWebSocketUri(string uri)
        {
            if (uri.StartsWith("http", StringComparison.Ordinal) && uri.Length > 4)
            {
                return "ws" + uri.Substring(4);
            }
            return uri;
        }

        void invokeCallback(string data)
        {
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnEventReceived?.Invoke(data);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }


        //----
        public void Dispose()
        {
            StopSocket();
            timer?.Dispose();
        }
    }
}
