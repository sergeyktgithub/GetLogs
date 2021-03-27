using NetComModels;
using NetComModels.Messages;

namespace LogsSearchServer.Handlers
{
    public interface IMsgHandler
    {
        MsgType MsgType { get; }

        void SendAnswer(Package package);
    }
}