using System.Text.Json;
using NetComModels;
using NetComModels.Messages;

namespace NetCom.Extensions
{
    public static class PackageExtensions
    {
        public static T Deserialize<T>(this Package package) where T : Msg
        {
            return JsonSerializer.Deserialize<T>(package.GetMessageText());
        }
    }
}