using System.Threading;
using NetComModels;
using NetComModels.Messages;

namespace LogsSearchServer.Handlers
{
    public abstract class MsgHandler : IMsgHandler
    {
        protected CancellationToken Token;
        protected readonly string PathToLogs;
        public MsgType MsgType { get; }

        protected MsgHandler(string pathToLogs, CancellationToken token, MsgType msgType)
        {
            Token = token;
            PathToLogs = pathToLogs;
            MsgType = msgType;
        }

        public abstract void SendAnswer(Package package);

        
    }
}