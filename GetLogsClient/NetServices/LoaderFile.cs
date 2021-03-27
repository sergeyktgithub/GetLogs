using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCom;
using NetCom.Extensions;
using NetComModels;
using NetComModels.ErrorMessages;
using NetComModels.Messages;

namespace GetLogsClient.NetServices
{
    public sealed class LoaderFile : IDisposable
    {
        private readonly IPEndPoint _localMsg;
        private readonly IPEndPoint _localFile;
        private readonly IPackageQueue _packageQueue;
        private CancellationToken _token;

        public delegate void ProgressEventHandler(object sender, int bytesReceived, long totalBytes);

        public event EventHandler<string> BeginDownloadEvent; 
        public event EventHandler<string> EndDownloadEvent; 
        public event EventHandler<string> ErrorEvent; 
        public event EventHandler<string> GeneralMsgEvent; 
        public event ProgressEventHandler ProgressEvent; 

        public LoaderFile(IPEndPoint localMsg, IPEndPoint localFile, IPackageQueue packageQueue, CancellationToken token)
        {
            _localMsg = localMsg;
            _localFile = localFile;
            _packageQueue = packageQueue;
            _token = token;

            _packageQueue.NewMessageReceivedEvent += PackageQueueOnNewMessageReceivedEvent;
        }

        private void PackageQueueOnNewMessageReceivedEvent(object sender, MsgType msgType)
        {
            if (msgType == MsgType.Error && _packageQueue.TryGetPackage(MsgType.Error, out var package))
            {
                var errorMsg = JsonSerializer.Deserialize<ErrorMsg>(package.GetMessageText());
                OnErrorEvent($"Ошибка загрузки файла: {errorMsg.Message}");
            }
        }

        public void Dispose()
        {
            _packageQueue?.Dispose();
        }

        public async Task FileRequestAsync(string fileName, string savePath, IPEndPoint destinationMsg)
        {
            _packageQueue.Clear();

            OnGeneralMsgEvent("Старт создание пакета");

            var broadcastMessageUdp = new UdpMessage(_localMsg, destinationMsg);
            broadcastMessageUdp.Send(new GetFileMsg(fileName, _localFile.Port));

            var package = await _packageQueue.WaitPackageByTypeAsync(MsgType.SendFile, 1000 * 60 * 5);
            var sendFileMsg = package.Deserialize<SendFileMsg>();

            OnBeginDownloadEvent($"Загружаю файл: {savePath}");

            await FileUploadAsync(_localFile, savePath, sendFileMsg.FileSize);

            OnEndDownloadEvent($"Загрузил файл: {savePath}");
        }

        private async Task FileUploadAsync(IPEndPoint local, string savePath, long lengthInputFile)
        {
            var listener = new TcpListener(local.Address, local.Port);
            try
            {
                listener.Start();

                using (_token.Register(() => listener.Stop()))
                using (var client = await listener.AcceptTcpClientAsync())
                using (var stream = client.GetStream())
                using (var output = File.Create(savePath))
                {
                    var buffer = new byte[1024 * 8];
                    int bytesRead;
                    int bytesReceived = 0;

                    while (_token.IsCancellationRequested == false 
                           && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                        bytesReceived += bytesRead;

                        ProgressEvent?.Invoke(this, bytesReceived, lengthInputFile);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private void OnBeginDownloadEvent(string text)
        {
            BeginDownloadEvent?.Invoke(this, text);
        }

        private void OnEndDownloadEvent(string test)
        {
            EndDownloadEvent?.Invoke(this, test);
        }

        private void OnErrorEvent(string text)
        {
            ErrorEvent?.Invoke(this, text);
        }

        private void OnGeneralMsgEvent(string text)
        {
            GeneralMsgEvent?.Invoke(this, text);
        }
    }
}