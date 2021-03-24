using System;
using System.Text.Json;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetComModels;

namespace GetLogsClient.NetServices
{
    public abstract class AboutFiles<T> where T : SearchByAccIdMsg
    {
        private readonly TwoEndPoints _twoEndPoints;
        private readonly IMsgUdpListener _msgUdpListener;

        public AboutFiles(TwoEndPoints twoEndPoints, IMsgUdpListener msgUdpListener)
        {
            _twoEndPoints = twoEndPoints;
            _msgUdpListener = msgUdpListener ?? throw new ArgumentNullException(nameof(msgUdpListener));
        }

        protected virtual async Task<PreparedArchive> CheckExistFilesAsync(T msg, int timeout = 1000 * 10)
        {
            var foundFilesPacket = _msgUdpListener.GetPacket(MsgType.FilesFound);
            if (foundFilesPacket == null)
            {
                MessageUdp broadcastMessageUdp = new MessageUdp(Environment.MachineName, _twoEndPoints);
                broadcastMessageUdp.Send(msg);

                foundFilesPacket = await _msgUdpListener.GetPacketAsync(MsgType.FilesFound, timeout);
            }

            return new PreparedArchive(foundFilesPacket.SourceEndPoint, JsonSerializer.Deserialize<FoundFilesMsg>(foundFilesPacket.Msg));
        }
    }
}