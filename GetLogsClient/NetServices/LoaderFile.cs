using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetCom;
using NetCom.Extensions;
using NetComModels;

namespace GetLogsClient.NetServices
{
    public sealed class LoaderFile : IDisposable
    {
        private readonly IPEndPoint _localMsg;
        private readonly IPEndPoint _localFile;
        private readonly IMsgUdpListener _listener;
        private CancellationToken _token;

        public event EventHandler<string> BeginDownloadEvent; 
        public event EventHandler<string> EndDownloadEvent; 
        public event EventHandler<string> ErrorEvent; 

        public LoaderFile(IPEndPoint localMsg, IPEndPoint localFile, IMsgUdpListener listener, CancellationToken token)
        {
            _localMsg = localMsg;
            _localFile = localFile;
            _listener = listener;
            _token = token;

            _listener.NewMessageReceivedEvent += ListenerOnNewMessageReceivedEvent;
        }

        private void ListenerOnNewMessageReceivedEvent(object sender, MsgType msgType)
        {
            if (msgType == MsgType.Error)
            {
                ErrorMsg errorMsg = _listener.GetPacket(MsgType.Error).Deserialize<ErrorMsg>();
                OnErrorEvent($"Получил ошибку: {errorMsg.Message}");
            }
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }

        public async Task FileRequestAsync(string fileName, string savePath, IPEndPoint destinationMsg)
        {
            var broadcastMessageUdp = new MessageUdp(Environment.MachineName, _localMsg, destinationMsg);
            broadcastMessageUdp.Send(new GetFileMsg(fileName, _localFile.Port));

            if (await _listener.GetPacketAsync(MsgType.Ok) != null)
            {
                await FileUploadAsync(_localFile, savePath);
            }
        }

        private async Task FileUploadAsync(IPEndPoint local, string savePath)
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
                    OnBeginDownloadEvent($"Загружаю файл: {savePath}");

                    var buffer = new byte[1024];
                    int bytesRead;
                    while (_token.IsCancellationRequested == false && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }

                    OnEndDownloadEvent($"Загрузил файл: {savePath}");
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

        private void OnErrorEvent(string e)
        {
            ErrorEvent?.Invoke(this, e);
        }
    }
}