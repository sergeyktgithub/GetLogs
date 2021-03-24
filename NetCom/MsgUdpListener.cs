using System;
using System.Collections.Concurrent;
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
    public class MsgUdpListener : IMsgUdpListener
    {
        private readonly int _listenPort;
        private readonly CancellationToken _token;
        private ConcurrentStack<Packet> _msg;
        private System.Timers.Timer _timer;
        private UdpClient _listener;
        private bool _disposed;

        public event EventHandler<MsgType> NewMessageReceivedEvent;

        public MsgUdpListener(int listenPort, CancellationToken token)
        {
            _listenPort = listenPort;
            _token = token;
            _msg = new ConcurrentStack<Packet>();
        }

        public void Start()
        {
            Log.Debug("Start listener");

            _listener = new UdpClient(_listenPort);
            _listener.EnableBroadcast = true;

            _timer = new System.Timers.Timer(10);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            ReceiveMsg();

            if (_token.IsCancellationRequested) return;
            if (_disposed) return;

            _timer?.Start();
        }

        public Packet GetPacket(MsgType type)
        {
            if (_msg.IsEmpty)
                return null;

            for(int i = 0; i < _msg.Count; i++)
            {
                if (_token.IsCancellationRequested)
                {
                    return null;
                }

                if (_msg.TryPop(out var msgClient))
                {
                    if (msgClient.Type == type)
                        return msgClient;

                    _msg.Push(msgClient);
                }
            }

            return null;
        }

        public async Task<Packet> GetPacketAsync(MsgType type, int timeout = 3000)
        {
            const int interval = 200;
            var counterTik = 0;
            while (true)
            {
                _token.ThrowIfCancellationRequested();

                var msg = GetPacket(type);
                if (msg != null)
                {
                    return msg;
                }

                await Task.Delay(interval);
                counterTik += interval;

                if (counterTik >= timeout)
                {
                    throw new TimeoutException("Не удалось получить новое сообщение");
                }
            }
        }

        private void ReceiveMsg()
        {
            try
            {
                Log.Debug("Жду сообщений");

                IPEndPoint remoteIp = null;
                byte[] bytes = _listener.Receive(ref remoteIp);

                Log.Debug($"Получил сообщение от {remoteIp} :");

                _token.ThrowIfCancellationRequested();

                try
                {
                    var msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    Log.Debug($"Получил сообщение:\n {msg}");

                    var packet = JsonSerializer.Deserialize<Packet>(msg);

                    Log.Debug($"Десерилизовал сообщение:\n {packet}");

                    _msg.Push(packet);

                    Log.Debug($"Добавил в очередь, длина {_msg.Count}");

                    OnNewMessageReceivedEvent(packet.Type);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Не удалось десериализовать сообщение от клиента");
                }
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Ошибка сокета");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _timer?.Stop();
            _timer?.Dispose();

            _listener?.Dispose();
        }


        protected virtual void OnNewMessageReceivedEvent(MsgType e)
        {
            NewMessageReceivedEvent?.Invoke(this, e);
        }
    }
}