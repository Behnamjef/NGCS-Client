using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NetworkAdapter.Brokers;
using NetworkAdapter.Tools;

namespace NetworkAdapter.Services
{

    public class RoomSocketServices : IDisposable
    {
        NetworkHttpBrocker httpClient;
        CancellationTokenSource cancelSource = new CancellationTokenSource();

        public event Action OnConnect;
        public event Action OnReconect;
        public event Action OnDisconnect;
        public event Action OnRoomNotFound;
        public event Action OnRoomJoinReject;
        public event Action OnAuthorizationFailed;
        public event Action OnLeave;
        public event Action OnNotConnected;
        public event Action<string, bool> OnMessage;
        public bool IsConnected { get; private set; }
        public bool IsInit { get; private set; }

        string url;
        WebSocketBrocker socketAdapter;
        UserSession session;
        Timer timer;

        public RoomSocketServices(NetworkHttpBrocker client, string url)
        {
            this.httpClient = client;
            this.url = getWebSocketUri(url);
            session = new UserSession();
            timer = new Timer(_ =>
            {
                lock (session)
                    session.UpdateTime();
            });

        }

        public void Init()
        {
            session.OnConnect += invokeConnect;
            session.OnDisconnect += invokeDisconnect;
            session.OnSend += m => sendToSocket(m.Serialize(), false);
            session.OnMessage += invokeMessage;
            session.OnLeave += Session_OnLeave;
            session.SetupClient();
            runLoop().LogExceptions();
            timer.Change(session.pingTimeout / 4, session.pingTimeout / 4);
            sendConnectSocket(session.sessionId);
            ConnectToRoomTimeOut();
        }

        private async void ConnectToRoomTimeOut()
        {
            await Task.Delay(10000);
            if (!IsConnected)
            {
                OnNotConnected?.Invoke();
            }
        }

        private void Session_OnLeave()
        {
            OnLeave?.Invoke();
            ForceClose();
        }


        public void RequestLeave()
        {
            lock (session)
                session.Leave();
        }

        //--Socket--        
        public void Send(string data, bool reliable)
        {
            lock (session)
                session.SendMessage(data, reliable);
        }

        void sendToSocket(string s, bool bypassIsConnected)
        {
            //Send to socket
            if (!IsConnected && !bypassIsConnected)
                return;
            //Debug.Log($"RoomClient: Sending to socket -> {s}");
            socketAdapter.Send(s);
        }
        //----
        void sendConnectSocket(string s)
        {
            socketAdapter.Send(s);
        }
        //--Helpers--

        private bool shouldListen = true;

        async Task runLoop()
        {
            //Debug.Log("Starting room socket");
            using (cancelSource = new CancellationTokenSource())
            {
                int fails = 0;
                while (!cancelSource.IsCancellationRequested)
                {
                    if (!shouldListen)
                    {
                        lock (session)
                            session.HandleClientDisconnect();

                        await Task.Delay(7_000);
                        shouldListen = true;
                        continue;
                    }

                    try
                    {
                        using (socketAdapter = new WebSocketBrocker())
                        {
                            // http client will fill in api service. 
                            // always remember to call init and login befor create a room socket
                            socketAdapter.headers.Add("Authorization", "Bearer " + httpClient.userInfo.token);
                            socketAdapter.headers.Add("X-Language", httpClient.userInfo.lang);
                            socketAdapter.headers.Add("ClientVersion", httpClient.userInfo.clientVersion);
                            socketAdapter.headers.Add("Compressed", "true");

                            socketAdapter.readTimeout = TimeSpan.FromMilliseconds(session.pingTimeout);
                            await socketAdapter.ConnectAsync(new Uri(url));

                            cancelSource.Token.Register(() => socketAdapter.Dispose());

                            //Reset fail counter
                            fails = 0;

                            lock (session)
                            {
                                session.HandleClientConnect();
                            }

                            socketAdapter.OnMessage += m =>
                            {
                                var msg = new NetworkRoomPacket(m);
                                //Debug.Log($"RoomClient: Data received -> {m}");
                                lock (session)
                                    session.HandlePacket(msg);
                                if (msg.type == NetworkRoomPacket.TYPE_ACCEPT)
                                {
                                    socketAdapter.readTimeout = TimeSpan.FromMilliseconds(session.pingTimeout);
                                    timer.Change(1000, session.pingTimeout / 4);
                                }
                            };

                            var closeReason = await socketAdapter.Process();
                            if (closeReason == "ROOM-NOT-FOUND")
                            {
                                invokeRoomNotFound();
                                lock (session)
                                    session.HandleClientDisconnect();
                                Dispose();
                                break;
                            }
                            if (closeReason == "UNAUTHORIZED")
                            {
                                //Debug.LogError("Unauthorized token");
                                invokeAuthorizationFailed();
                                lock (session)
                                    session.HandleClientDisconnect();
                                Dispose();
                                break;
                            }
                            if (closeReason == "REJECT")
                            {
                                invokeRoomJoinReject();
                                lock (session)
                                    session.HandleClientDisconnect();
                                Dispose();
                                break;
                            }

                            if (closeReason == "FORCE-CLOSE")
                            {
                                lock (session)
                                    session.HandleClientDisconnect();
                                Dispose();
                                break;
                            }
                            if (closeReason == "MOVE")
                            {
                                shouldListen = false;
                                continue;
                            }
                            if (closeReason == "TIME-OUT")
                            {
                                lock (session)
                                    session.HandleClientDisconnect();
                            }
                            if (closeReason == "DUPLICATE")
                            {
                                //Debug.LogError("DUPLICATE user");
                            }
                            if (closeReason == "DUPLICATE")
                            {
                                lock (session)
                                    session.HandleClientDisconnect();
                            }

                            lock (session)
                                session.HandleClientDisconnect();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        //Debug.LogException(ex);
                    }
                    catch (WebSocketException ex)
                    {
                        Debug.LogException(ex);
                    }
                    catch (OperationCanceledException ex)
                    {
                        //Debug.LogException(ex);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogException(ex);
                    }


                    lock (session)
                        session.HandleClientDisconnect();
                    fails++;
                    await Task.Delay(Math.Min(5000, 1000));
                }
            }
        }

        string getWebSocketUri(string uri)
        {
            if (uri.StartsWith("http", StringComparison.Ordinal) && uri.Length > 4)
            {
                return "ws" + uri.Substring(4);
            }
            return uri;
        }

        void invokeConnect()
        {
            if (IsConnected)
                return;
            //Debug.Log("RoomClient: Connected");
            IsConnected = true;
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    if (IsInit)
                        OnReconect?.Invoke();
                    else
                    {
                        OnConnect?.Invoke();
                        IsInit = true;
                    }

                });
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
        }

        void invokeRoomNotFound()
        {
            //Debug.Log("RoomClient: Room not found");
            IsConnected = false;
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnRoomNotFound?.Invoke();
                });
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
        }

        void invokeRoomJoinReject()
        {
            //Debug.Log("RoomClient: Room join rejected");
            try
            {
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnRoomJoinReject?.Invoke();
                });
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
        }

        void invokeAuthorizationFailed()
        {
            //Debug.Log("RoomClient: Authorization failed");
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

        void invokeDisconnect()
        {
            try
            {
                socketAdapter?.Dispose();
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
            if (!IsConnected)
                return;
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
                //Debug.LogException(ex);
            }
        }

        void invokeMessage(string message, bool reliable)
        {
            try
            {
                //Debug.Log($"RoomClient: Message -> {message}");
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() =>
                {
                    OnMessage?.Invoke(message, reliable);
                });
            }
            catch (Exception ex)
            {
                //Debug.LogException(ex);
            }
        }
        //----

        public void ForceClose()
        {
            if (IsInit) IsInit = false;
            shouldListen = false;
            socketAdapter?.Dispose();
        }

        public void Dispose()
        {
            session.OnConnect -= invokeConnect;
            session.OnDisconnect -= invokeDisconnect;
            session.OnSend -= m => sendToSocket(m.Serialize(), false);
            session.OnMessage -= invokeMessage;
            session.OnLeave -= Session_OnLeave;

            if (cancelSource != null)
            {
                if (!cancelSource.IsCancellationRequested)
                    try { cancelSource.Cancel(); } catch { }
                cancelSource = null;
                //Debug.Log("Stopping room socket");
            }
            timer?.Dispose();
            if (IsInit) IsInit = false;
        }
    }
}
