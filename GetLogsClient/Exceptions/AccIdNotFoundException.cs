using System;
using System.Net;
using System.Runtime.Serialization;

namespace GetLogsClient.NetServices
{
    [Serializable]
    public class AccIdNotFoundException : Exception
    {
        public AccIdNotFoundException()
        {
        }

        public AccIdNotFoundException(IPEndPoint endPoint, string accId) : base($"{endPoint} : AccId: {accId} не найден")
        {
        }

        public AccIdNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AccIdNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}