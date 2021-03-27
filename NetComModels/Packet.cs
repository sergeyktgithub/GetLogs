using System;
using System.Net;
using System.Text.Json.Serialization;
using NetComModels.Messages;
using ZipExtension;

namespace NetComModels
{
    [Serializable]
    public class Package
    {
        public byte[] CompressedMessage { get; set; }
        public MsgType Type { get; set; }

        [JsonConverter(typeof(JsonIPEndPointConverter))]
        public IPEndPoint SourceEndPoint { get; set; }

        [JsonIgnore]
        public bool IsEmpty => CompressedMessage.Length == 0;

        public Package()
        {
        }

        public Package(string json, MsgType msgType) : this(json, msgType, 0)
        {
        }

        public Package(string json, MsgType msgType, int sourcePort)
        {
            if (string.IsNullOrEmpty(json))
                throw new Exception(nameof(json) + "is null or empty");

            CompressedMessage = Zip.CompressText(json);
            Type = msgType;
            SourceEndPoint = new IPEndPoint(IPAddress.None, sourcePort);
        }

        public string GetMessageText()
        {
            if (CompressedMessage.Length == 0)
                throw new Exception(nameof(CompressedMessage) + " is empty");

            return Zip.ExtractFromBytes(CompressedMessage);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MsgType.Error:
                    return $"Ip={SourceEndPoint.Address}, Port={SourceEndPoint.Port}, Msg={GetMessageText()}, Type={Enum.GetName(typeof(MsgType), Type)}, Error={GetMessageText()}";
            }
            
            return $"Ip={SourceEndPoint.Address}, Port={SourceEndPoint.Port}, Msg={GetMessageText()}, Type={Enum.GetName(typeof(MsgType), Type)}";
        }
    }
}