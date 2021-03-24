using System;
using System.Net;
using System.Text.Json.Serialization;

namespace NetComModels
{
    [Serializable]
    public class Packet
    {
        public string Name { get; set; }
        public string Msg { get; set; }
        public MsgType Type { get; set; }

        [JsonConverter(typeof(JsonIPEndPointConverter))]
        public IPEndPoint SourceEndPoint { get; set; }

        public Packet()
        {}

        public Packet(string name, string msg, MsgType msgType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Msg = msg ?? throw new ArgumentNullException(nameof(msg));
            Type = msgType;
        }

        public Packet(string name, string msg, MsgType msgType, IPEndPoint sourceEndPoint)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Msg = msg ?? throw new ArgumentNullException(nameof(msg));
            Type = msgType;
            SourceEndPoint = sourceEndPoint;
        }

        public override string ToString()
        {
            return $"Name={Name}, Ip={SourceEndPoint.Address}, Port={SourceEndPoint.Port}, Msg={Msg}, Type={Type}";
        }
    }
}