using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace NetworkAdapter.Services
{
    public class UserSession
    {
        public DateTime lastSentPing = DateTime.UtcNow;
        public DateTime lastReceive = DateTime.UtcNow;
        public bool connected = false;
        public bool left = false;
        public long sendId = 0;
        public long recId = 0;
        public string sessionId;
        public Queue<NetworkRoomPacket> sendQ = new Queue<NetworkRoomPacket>();
        public bool server = true;
        public int pingTimeout = 12000;
        public int sessionTimeout = 5 * 60 * 1000;

        [JsonIgnore]
        const int backPressure = 100;

        public event Action<NetworkRoomPacket> OnSend;

        public event Action<string, bool> OnMessage;

        public event Action OnConnect;

        public event Action OnDisconnect;

        public event Action OnLeave;

        public void SetupServer(int pingTimeout, int sessionTimeout)
        {
            this.pingTimeout = pingTimeout;
            this.sessionTimeout = sessionTimeout;
            server = true;
        }

        public void SetupClient()
        {
            server = false;
            sessionId = Guid.NewGuid().ToString("N");
        }

        public void UpdateTime()
        {
            var now = DateTime.UtcNow;
            var deltaSent = (now - lastSentPing).TotalMilliseconds;
            var deltaReceived = (now - lastReceive).TotalMilliseconds;
            if (server && deltaSent >= pingTimeout / 2)
            {
                SendPacket(NetworkRoomPacket.Ping(recId, sendId));
                lastSentPing = now;
            }
            if (connected && deltaReceived >= pingTimeout)
                disconnect();
            if (!left && deltaReceived >= sessionTimeout)
                Leave();
        }

        public void HandlePacket(NetworkRoomPacket packet)
        {
            if (server && packet.type == NetworkRoomPacket.TYPE_CONNECT)
            {
                if (sessionId != null && packet.data != sessionId)
                {
                    resetSession();
                }
                sessionId = packet.data;
                connect();
                SendPacket(NetworkRoomPacket.Accept(sessionId, pingTimeout, sessionTimeout));
                return;
            }
            if (server && packet.type == NetworkRoomPacket.TYPE_DISCONNECT)
            {
                disconnect();
                return;
            }
            if (server && packet.type == NetworkRoomPacket.TYPE_LEAVE)
            {
                leaveLocal();
                return;
            }
            if (!server && packet.type == NetworkRoomPacket.TYPE_ACCEPT)
            {
                var dct = packet.GetDataDictionary();
                var sId = dct["sessionId"];
                if (sessionId != null && sId != sessionId)
                {
                    resetSession();
                }
                sessionId = sId;
                pingTimeout = int.Parse(dct["pingTimeout"]);
                sessionTimeout = int.Parse(dct["sessionTimeout"]);
                connect();
                return;
            }
            if (!server && packet.type == NetworkRoomPacket.TYPE_REJECT)
            {
                leaveLocal();
                return;
            }

            lastReceive = DateTime.UtcNow;

            connect();

            if (packet.HasAckId())
            {
                //Consume ack-ed messages
                while (sendQ.Count > 0)
                {
                    if (packet.ackId < sendQ.Peek().msgId)
                        break;
                    sendQ.Dequeue();
                }
            }

            if (packet.type == NetworkRoomPacket.TYPE_RELIABLE_MESSAGE)
            {
                if (packet.msgId <= recId) //Old message
                    return;
                if (packet.msgId > recId + 1) //Missed message
                {
                    SendPacket(NetworkRoomPacket.Resend(recId));
                    return;
                }
                recId = packet.msgId;
                OnMessage?.Invoke(packet.data, true);
                return;
            }

            if (packet.type == NetworkRoomPacket.TYPE_SKIP)
            {
                if (packet.msgId > recId)
                    recId = packet.msgId;
                return;
            }

            if (packet.type == NetworkRoomPacket.TYPE_PING)
            {
                SendPacket(NetworkRoomPacket.Pong(recId, sendId));
                if (packet.msgId > recId) //Missed message
                {
                    SendPacket(NetworkRoomPacket.Resend(recId));
                }
                return;
            }

            if (packet.type == NetworkRoomPacket.TYPE_PONG)
            {
                if (packet.msgId > recId) //Missed message
                {
                    SendPacket(NetworkRoomPacket.Resend(recId));
                }
                return;
            }

            if (packet.type == NetworkRoomPacket.TYPE_RESEND)
            {
                if (sendQ.Count == 0)
                {
                    //No data to resend, skip id to current
                    SendPacket(NetworkRoomPacket.Skip(recId, sendId));
                    return;
                }
                var first = sendQ.Peek();
                if (first.msgId > packet.ackId + 1)
                {
                    //Skip the missing messages
                    SendPacket(NetworkRoomPacket.Skip(recId, first.msgId - 1));
                }
                foreach (var item in sendQ)
                {
                    var x = item;
                    x.ackId = recId;
                    SendPacket(x);
                }
                return;
            }

            if (packet.type == NetworkRoomPacket.TYPE_MESSAGE)
            {
                OnMessage?.Invoke(packet.data, false);
                return;
            }
        }

        public void SendMessage(string message, bool reliable)
        {
            if (reliable)
            {
                var packet = NetworkRoomPacket.ReliableMessage(recId, ++sendId, message);
                sendQ.Enqueue(packet);
                if (sendQ.Count > backPressure)
                    sendQ.Dequeue(); //Drop the oldest message
                SendPacket(packet);
            }
            else
            {
                SendPacket(NetworkRoomPacket.Message(message));
            }
        }

        public void SendPacket(NetworkRoomPacket packet)
        {
            OnSend?.Invoke(packet);
        }

        public void HandleClientConnect()
        {
            SendPacket(NetworkRoomPacket.Connect(sessionId));
        }

        public void HandleClientDisconnect()
        {
            disconnect();
        }

        public void Leave()
        {
            SendPacket(NetworkRoomPacket.Leave());
            leaveLocal();
        }

        void resetSession()
        {
            sendId = 0;
            recId = 0;
            sendQ.Clear();
        }

        void leaveLocal()
        {
            if (!left)
            {
                left = true;
                OnLeave?.Invoke();
            }
        }

        void connect()
        {
            if (!connected)
            {
                connected = true;
                OnConnect?.Invoke();
            }
        }

        void disconnect()
        {
            if (connected)
            {
                connected = false;
                OnDisconnect?.Invoke();
            }
        }
    }
}
