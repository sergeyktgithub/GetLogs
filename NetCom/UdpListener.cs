using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using NetComModels;
using Serilog;

namespace NetCom
{
    public class UdpListener : IUdpListener
    {
        private System.Timers.Timer _timer;
        private UdpClient _listener;
        private bool _disposed;
        private readonly CancellationToken _mainToken;
        public event EventHandler<Package> ReceivedNewMessageEvent;

        public UdpListener(int listenPort, CancellationToken mainToken)
        {
            _mainToken = mainToken;

            _listener = new UdpClient(listenPort)
            {
                EnableBroadcast = true
            };

            _timer = new System.Timers.Timer(1);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            ReceiveMsg();

            if (_mainToken.IsCancellationRequested) return;
            if (_disposed) return;

            _timer?.Start();
        }

        private void ReceiveMsg()
        {
            try
            {
                Log.Debug("Жду сообщений");

                IPEndPoint remoteIp = null;
                byte[] bytes = _listener.Receive(ref remoteIp);

                _mainToken.ThrowIfCancellationRequested();

                SendPackageAsync(bytes, remoteIp);
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Ошибка сокета");
            }
        }

        private void SendPackageAsync(byte[] bytes, IPEndPoint remoteIp)
        {
            Task.Run(() =>
            {
                try
                {
                    var msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Log.Debug($"Получил сообщение:\n {msg} отправитель {remoteIp}");

                    var package = JsonSerializer.Deserialize<Package>(msg);
                    var sourceIp = remoteIp.Address;
                    var sourcePort = package.SourceEndPoint.Port;
                    package.SourceEndPoint = new IPEndPoint(sourceIp, sourcePort);

                    Log.Debug($"Десериализовал ообщение:\n {package}");

                    OnNewPackageReceivedEvent(package);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Не удалось десериализовать сообщение от клиента {remoteIp}");
                }
            }, _mainToken);
        }


        protected virtual void OnNewPackageReceivedEvent(Package package)
        {
            ReceivedNewMessageEvent?.Invoke(this, package);
        }

        public void Dispose()
        {
            if (_disposed == true)
                return;

            _disposed = true;
            _listener?.Dispose();
        }
    }
}