using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using NetCom;
using NetComModels;
using Serilog;
using TestServerSocket.Extensions;
using TestServerSocket.Packer;

namespace TestServerSocket.Handlers
{
    public class SendFileMsgHandler : MsgHandler
    {
        public SendFileMsgHandler(string pathToLogs, CancellationToken token) 
            : base(pathToLogs,token, NetComModels.MsgType.GetFile)
        {
        }

        private static void SendFile(string filePath, IPEndPoint endPoint)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(endPoint);

            Log.Debug("Sending {0} to the host.", filePath);
            client.SendFile(filePath);

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public override void SendAnswer(Packet packet)
        {
            var pairNet = new TwoEndPoints(NetHelper.GetLocalIpAddress(), GlobalProperties.ServerMsgPort, packet.SourceEndPoint);
            var getFileMsg = JsonSerializer.Deserialize<GetFileMsg>(packet.Msg);

            if (File.Exists(getFileMsg.FilePath) == false)
            {
                MessageUdp.Send(pairNet, new ErrorMsg($"Не удалось найти файл: {getFileMsg.FilePath}"));
            }
            else
            {
                MessageUdp.Send(pairNet, new OkMessage());

                TempDir readTempDir = TempDir.CreateFromFullPath(getFileMsg.FilePath);
                FilesPacker filesPacker = new FilesPacker(readTempDir);
                filesPacker.Create();

                SendFile(filesPacker.ZipPath, new IPEndPoint(packet.SourceEndPoint.Address, getFileMsg.DestPort));
            }
        }
    }
}