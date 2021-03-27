using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using NetComModels;
using NetComModels.Messages;

namespace NetCom
{
    public class PackageQueue : IPackageQueue
    {
        private CancellationTokenSource _waitPackageTokenSource;
        private readonly CancellationToken _mainToken;
        private readonly ConcurrentQueue<Package> _messages;
        private bool _disposed;

        public event EventHandler<MsgType> NewMessageReceivedEvent;

        public PackageQueue(IUdpListener udpListener, CancellationToken mainToken)
        {
            _mainToken = mainToken;
            _messages = new ConcurrentQueue<Package>();
            _waitPackageTokenSource = new CancellationTokenSource();

            udpListener.ReceivedNewMessageEvent += UdpListenerOnReceivedNewMessageEvent;
        }

        private void UdpListenerOnReceivedNewMessageEvent(object sender, Package package)
        {
            _messages.Enqueue(package);
        }

        public void CancelWaitPackage()
        {
            _waitPackageTokenSource?.Cancel();
            _waitPackageTokenSource = new CancellationTokenSource();
        }

        public void Clear()
        {
            while (_messages.Count > 0)
            {
                _messages.TryDequeue(out _);
            }
        }

        public bool Contains(MsgType type)
        {
            return _messages.IsEmpty == false && _messages.Any(x => x.Type == type);
        }

        public bool IsEmpty => _messages.IsEmpty;

        public bool TryGetPackage(MsgType type, out Package package)
        {
            package = new Package();

            if (_messages.IsEmpty)
                return false;

            for(var i = 0; i < _messages.Count; i++)
            {
                if (_mainToken.IsCancellationRequested)
                {
                    return false;
                }

                if (_messages.TryDequeue(out var msg))
                {
                    if (msg.Type == type)
                    {
                        package = msg;
                        return true;
                    }

                    _messages.Enqueue(msg);
                }
            }

            return false;
        }

        public async Task<bool> WaitPong(IPEndPoint endPoint, int timeout)
        {
            const int interval = 10;
            var waitPackageToken = _waitPackageTokenSource.Token;
            var startDt = DateTime.Now;

            while ((DateTime.Now - startDt).TotalMilliseconds < timeout)
            {
                for (var i = 0; i < _messages.Count; i++)
                {
                    if (_mainToken.IsCancellationRequested || waitPackageToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    if (_messages.TryDequeue(out var msg))
                    {
                        if (msg.Type == MsgType.Pong && Equals(msg.SourceEndPoint.Address, endPoint.Address))
                        {
                            return true;
                        }

                        _messages.Enqueue(msg);
                    }
                }

                await Task.Delay(interval, waitPackageToken);
            }

            throw new TimeoutException("Не удалось получить новое сообщение");
        }

        public async Task<Package> WaitPackageByTypeAsync(MsgType type, int timeout)
        {
            const int interval = 200;
            var startDt = DateTime.Now;
            var waitPackageToken = _waitPackageTokenSource.Token;

            while ((DateTime.Now - startDt).TotalMilliseconds < timeout)
            {
                _mainToken.ThrowIfCancellationRequested();
                waitPackageToken.ThrowIfCancellationRequested();

                if (TryGetPackage(type, out var package))
                {
                    return package;
                }

                await Task.Delay(interval, waitPackageToken);
            }

            throw new TimeoutException("Не удалось получить новое сообщение");
        }

        public async Task<Package> WaitAnyPackageAsync(int timeout)
        {
            const int interval = 5;
            var startDt = DateTime.Now;
            var waitPackageToken = _waitPackageTokenSource.Token;

            while ((DateTime.Now - startDt).TotalMilliseconds < timeout)
            {
                _mainToken.ThrowIfCancellationRequested();
                waitPackageToken.ThrowIfCancellationRequested();

                if (_messages.IsEmpty == false && _messages.TryDequeue(out var msg))
                    return msg;

                await Task.Delay(interval, waitPackageToken);
            }

            throw new TimeoutException("Не удалось получить новое сообщение");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _waitPackageTokenSource?.Cancel();
        }

    }
}