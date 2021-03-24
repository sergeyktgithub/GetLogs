using System;
using System.Collections.Concurrent;
using System.Threading;
using NetCom;
using TestServerSocket.Handlers;
using SimpleTimers = System.Timers;

namespace TestServerSocket
{
    public class MessageQueueHandler : IDisposable
    {
        private SimpleTimers.Timer _timer;
        private ConcurrentStack<IMsgHandler> _sendFileMsgHandlers = new ConcurrentStack<IMsgHandler>();

        protected readonly IMsgUdpListener UdpListener;
        protected CancellationToken Token;

        public MessageQueueHandler(IMsgUdpListener udpListener, CancellationToken token)
        {
            UdpListener = udpListener;
            Token = token;
        }

        public void AddHandler(IMsgHandler msgHandler)
        {
            if (msgHandler == null)
            {
                throw new NullReferenceException();
            }

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
                    var inputMsg = UdpListener.GetPacket(msgHandler.MsgType);

                    if (inputMsg != null)
                        msgHandler.SendAnswer(inputMsg);
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