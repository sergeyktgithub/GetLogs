using System.Threading;
using NetCom;
using NetCom.Helpers;
using NetComModels;
using NetComModels.Messages;

namespace LogsSearchServer.Handlers
{
    public class PingMsgHandler : IMsgHandler
    {
        public MsgType MsgType { get; }

        public PingMsgHandler()
        {
            MsgType = MsgType.Ping;
        }

        public void SendAnswer(Package package)
        {
            var pairNet = new TwoEndPoints(NetHelper.GetLocalIpAddress(), GlobalProperties.ServerMsgPort, package.SourceEndPoint);
            UdpMessage.Send(pairNet, new PongMsg());
        }
    }
}