using System.Text.Json;
using NetComModels;

namespace NetCom.Extensions
{
    public static class PacketExtensions
    {
        public static T Deserialize<T>(this Packet packet) where T : Msg
        {
            return JsonSerializer.Deserialize<T>(packet.Msg);
        }
    }
}