using System;
using NetComModels;

namespace NetCom
{
    public interface IUdpListener : IDisposable
    {
        event EventHandler<Package> ReceivedNewMessageEvent;
    }
}