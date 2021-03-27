using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using LogsSearchServer.Packer;
using NetCom;
using NetCom.Extensions;
using NetCom.Helpers;
using NetComModels;
using NetComModels.ErrorMessages;
using NetComModels.Messages;
using Serilog;

namespace LogsSearchServer.Handlers
{
    public class SendFileMsgHandler : MsgHandler
    {
        public SendFileMsgHandler(string pathToLogs, CancellationToken token) 
            : base(pathToLogs,token, MsgType.GetFile)
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

        public override void SendAnswer(Package package)
        {
            var pairNet = new TwoEndPoints(NetHelper.GetLocalIpAddress(), GlobalProperties.ServerMsgPort, package.SourceEndPoint);
            var getFileMsg = package.Deserialize<GetFileMsg>();

            if (File.Exists(getFileMsg.FilePath) == false)
            {
                UdpMessage.Send(pairNet, new ErrorMsg($"Не удалось найти файл: {getFileMsg.FilePath}"));
            }
            else
            {
                TempDir readTempDir = TempDir.CreateFromFullPath(getFileMsg.FilePath);
                FilesPacker filesPacker = new FilesPacker(readTempDir);
                filesPacker.Create();

                UdpMessage.Send(pairNet, new SendFileMsg(filesPacker.ZipPath, (new FileInfo(filesPacker.ZipPath)).Length));

                Thread.Sleep(1000);

                SendFile(filesPacker.ZipPath, new IPEndPoint(package.SourceEndPoint.Address, getFileMsg.DestPort));
            }
        }
    }
}