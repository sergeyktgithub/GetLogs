using NetComModels;
using NetComModels.Messages;

namespace NetCom
{
    public interface IUdpMessage
    {
        void Send<T>(T msg) where T : Msg;
    }
}