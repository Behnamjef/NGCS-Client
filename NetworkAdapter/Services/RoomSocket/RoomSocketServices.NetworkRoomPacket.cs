using NetworkAdapter.Tools;
using System.Collections.Generic;
using System.Linq;

namespace NetworkAdapter.Services { 
    public struct NetworkRoomPacket
    {
        public const char TYPE_PING = 'P';
        public const char TYPE_PONG = 'O';
        public const char TYPE_RELIABLE_MESSAGE = 'R';
        public const char TYPE_MESSAGE = 'M';
        public const char TYPE_RESEND = 'E';
        public const char TYPE_SKIP = 'S';
        public const char TYPE_CONNECT = 'C';
        public const char TYPE_DISCONNECT = 'D';
        public const char TYPE_REJECT = 'J';
        public const char TYPE_LEAVE = 'L';
        public const char TYPE_ACCEPT = 'A';

        public char type;
        public long msgId;
        public long ackId;
        public string data;

        public bool HasAckId() =>
            type == TYPE_PING ||
            type == TYPE_PONG ||
            type == TYPE_RELIABLE_MESSAGE ||
            type == TYPE_RESEND ||
            type == TYPE_SKIP;

        public bool HasMsgId() =>
            type == TYPE_PING ||
            type == TYPE_PONG ||
            type == TYPE_RELIABLE_MESSAGE ||
            type == TYPE_SKIP;

        public NetworkRoomPacket(string str)
        {
            var cursor = new StringCursor(str);
            type = cursor.Read(1)[0];
            msgId = 0;
            ackId = 0;
            data = null;
            if (HasAckId() || HasMsgId())
            {
                msgId = long.Parse(cursor.ReadTo(';', false));
                ackId = long.Parse(cursor.ReadTo(';', false));
            }
            if (!cursor.IsEnded)
                data = cursor.ReadToEnd();
        }

        public Dictionary<string, string> GetDataDictionary() =>
            data.Split(';').Select(x => x.Split('=')).ToDictionary(x => x[0], y => y[1]);

        public static char PeekType(string str) => str[0];

        public static NetworkRoomPacket Ping(long ackId, long msgId) => new NetworkRoomPacket
        {
            type = TYPE_PING,
            ackId = ackId,
            msgId = msgId,
        };

        public static NetworkRoomPacket Pong(long ackId, long msgId) => new NetworkRoomPacket
        {
            type = TYPE_PONG,
            ackId = ackId,
            msgId = msgId
        };

        public static NetworkRoomPacket Message(string message) => new NetworkRoomPacket
        {
            type = TYPE_MESSAGE,
            data = message,
        };

        public static NetworkRoomPacket ReliableMessage(long ackId, long msgId, string message) => new NetworkRoomPacket
        {
            type = TYPE_RELIABLE_MESSAGE,
            data = message,
            ackId = ackId,
            msgId = msgId
        };

        public static NetworkRoomPacket Disconnect() => new NetworkRoomPacket
        {
            type = TYPE_DISCONNECT
        };

        public static NetworkRoomPacket Leave() => new NetworkRoomPacket
        {
            type = TYPE_LEAVE
        };

        public static NetworkRoomPacket Connect(string sessionId) => new NetworkRoomPacket
        {
            type = TYPE_CONNECT,
            data = sessionId
        };

        public static NetworkRoomPacket Accept(string sessionId, int pingTimeout, int sessionTimeout) => new NetworkRoomPacket
        {
            type = TYPE_ACCEPT,
            data = $"sessionId={sessionId};pingTimeout={pingTimeout};sessionTimeout={sessionTimeout}"
        };

        public static NetworkRoomPacket Reject() => new NetworkRoomPacket
        {
            type = TYPE_REJECT
        };

        public static NetworkRoomPacket Resend(long ackId) => new NetworkRoomPacket
        {
            type = TYPE_RESEND,
            ackId = ackId,
        };

        public static NetworkRoomPacket Skip(long ackId, long msgId) => new NetworkRoomPacket
        {
            type = TYPE_SKIP,
            ackId = ackId,
            msgId = msgId
        };

        public string Serialize()
        {
            if (HasAckId() || HasMsgId())
                return $"{type}{msgId};{ackId};{data}";
            if (data == null)
                return type.ToString();
            return $"{type}{data}";
        }
    }
}
