using System;
using System.Collections.Concurrent;
using System.Threading;
using LogsSearchServer.Handlers;
using NetCom;
using SimpleTimers = System.Timers;

namespace LogsSearchServer
{
    public class PackageQueueHandler : IDisposable
    {
        private SimpleTimers.Timer _timer;
        private ConcurrentStack<IMsgHandler> _sendFileMsgHandlers = new ConcurrentStack<IMsgHandler>();

        protected readonly IPackageQueue UdpListener;
        protected CancellationToken Token;

        public PackageQueueHandler(IPackageQueue udpListener, CancellationToken token)
        {
            UdpListener = udpListener;
            Token = token;
        }

        public void AddHandler(IMsgHandler msgHandler)
        {
            if (msgHandler == null) throw new ArgumentNullException(nameof(msgHandler));

            _sendFileMsgHandlers.Push(msgHandler);

            if (_timer == null || _timer.Enabled == false)
            {
                StartTimer();
            }
        }

        private void StartTimer()
        {
            _timer = new SimpleTimers.Timer(100) {AutoReset = false};
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, SimpleTimers.ElapsedEventArgs e)
        {
            if (Token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                foreach (var msgHandler in _sendFileMsgHandlers)
                {
                    if (UdpListener.TryGetPackage(msgHandler.MsgType, out var package))
                        msgHandler.SendAnswer(package);
                }
            }
            finally
            {
                _timer.Start();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            UdpListener?.Dispose();
        }
    }
}