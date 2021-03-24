using NetComModels;

namespace TestServerSocket.Handlers
{
    public interface IMsgHandler
    {
        MsgType MsgType { get; }

        void SendAnswer(Packet packet);
    }
}