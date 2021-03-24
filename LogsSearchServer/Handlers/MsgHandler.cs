using System.Threading;
using NetComModels;

namespace TestServerSocket.Handlers
{
    public abstract class MsgHandler : IMsgHandler
    {
        protected CancellationToken Token;
        protected readonly string PathToLogs;
        public MsgType MsgType { get; }

        public MsgHandler(string pathToLogs, CancellationToken token, MsgType msgType)
        {
            Token = token;
            PathToLogs = pathToLogs;
            MsgType = msgType;
        }

        public abstract void SendAnswer(Packet packet);

        
    }
}