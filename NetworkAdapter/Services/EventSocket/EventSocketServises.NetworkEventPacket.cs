using NetworkAdapter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Services.EventSocket
{

    public struct NetworkEventPacket
    {
        public const char TYPE_PING = 'P';
        public const char TYPE_QUERY = 'Q';
        public const char TYPE_QUERY_ACK = 'R';
        public const char TYPE_PONG = 'O';
        public const char TYPE_SETTINGS = 'S';
        public const char TYPE_EVENT = 'E';
        public const char TYPE_ACKED_EVENT = 'A';
        public const char TYPE_EVENT_ACK = 'K';
        public const char TYPE_API = 'I';

        public char type;
        public string id;
        public string data;

        public NetworkEventPacket(string str)
        {
            var cursor = new StringCursor(str);
            type = cursor.Read(1)[0];
            id = null;
            data = null;
            switch (type)
            {
                case TYPE_SETTINGS:
                case TYPE_EVENT:
                    data = cursor.ReadToEnd();
                    break;
                case TYPE_EVENT_ACK:
                    id = cursor.ReadToEnd();
                    break;
                case TYPE_ACKED_EVENT:
                case TYPE_QUERY:
                case TYPE_QUERY_ACK:
                    id = cursor.ReadTo(';', false);
                    data = cursor.ReadToEnd();
                    break;
                case TYPE_API:
                    id = cursor.ReadTo(';', false);
                    data = cursor.ReadToEnd();
                    break;
                default:
                    break;
            }
        }

        public Dictionary<string, string> GetDataDictionary() =>
            data.Split(';').Select(x => x.Split('=')).ToDictionary(x => x[0], y => y[1]);

        public static NetworkEventPacket Ping() => new NetworkEventPacket { type = TYPE_PING };
        public static NetworkEventPacket Pong() => new NetworkEventPacket { type = TYPE_PONG };

        public static NetworkEventPacket Query(string replySubject, string userId) => new NetworkEventPacket
        {
            type = TYPE_QUERY,
            id = replySubject,
            data = userId
        };

        public static NetworkEventPacket QueryAck(string replySubject, string userId) => new NetworkEventPacket
        {
            type = TYPE_QUERY_ACK,
            id = replySubject,
            data = userId
        };

        public static NetworkEventPacket Event(string data) => new NetworkEventPacket
        {
            type = TYPE_EVENT,
            data = data
        };

        public static NetworkEventPacket AckedEvent(string id, string data) => new NetworkEventPacket
        {
            type = TYPE_ACKED_EVENT,
            id = id,
            data = data
        };

        public static NetworkEventPacket EventAck(string id) => new NetworkEventPacket
        {
            type = TYPE_EVENT_ACK,
            id = id,
        };

        public static NetworkEventPacket Settings(int pingTimeout) => new NetworkEventPacket
        {
            type = TYPE_SETTINGS,
            data = $"timeout={pingTimeout}"
        };
        public static NetworkEventPacket API(string id, string data) => new NetworkEventPacket
        {
            type = TYPE_API,
            id = id,
            data = data
        };
        public string Serialize()
        {
            switch (type)
            {
                case TYPE_PING:
                case TYPE_PONG:
                    return type.ToString();
                case TYPE_EVENT_ACK:
                    return $"{type}{id}";
                case TYPE_ACKED_EVENT:
                case TYPE_QUERY:
                case TYPE_QUERY_ACK:
                case TYPE_API:
                    return $"{type}{id};{data}";
                default:
                    return $"{type}{data}";
            }
        }
    }
}
