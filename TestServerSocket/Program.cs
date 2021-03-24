using System;
using System.Threading;
using NetCom;
using NetComModels;
using Serilog;
using TestServerSocket.Handlers;

namespace TestServerSocket
{
    public class SocketListener
    {
        public static int Main(String[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose().
                WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            string pathToLogs = "c:\\GMC.logs";

            CancellationTokenSource cts = new CancellationTokenSource();

            IMsgUdpListener msgUdpListener = new MsgUdpListener(GlobalProperties.ServerMsgPort, cts.Token);
            msgUdpListener.Start();

            MessageQueueHandler queueHandler = new MessageQueueHandler(msgUdpListener, cts.Token);

            queueHandler.AddHandler(new SearchInLogsMsgHandler(pathToLogs, cts.Token));
            queueHandler.AddHandler(new SearchInILogsMsgHandler(pathToLogs, cts.Token));
            queueHandler.AddHandler(new SendFileMsgHandler(pathToLogs, cts.Token));

            Thread.Sleep(1000 * 60 * 60);
            return 0;
        }
    }
}