using System;
using System.Threading;
using LogsSearchServer;
using LogsSearchServer.Handlers;
using NetCom;
using NetComModels;
using Serilog;

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

            IUdpListener udpListener = new UdpListener(GlobalProperties.ServerMsgPort, cts.Token);
            IPackageQueue packageQueue = new PackageQueue(udpListener, cts.Token);

            PackageQueueHandler queueHandler = new PackageQueueHandler(packageQueue, cts.Token);

            queueHandler.AddHandler(new SearchInLogsMsgHandler(pathToLogs, cts.Token));
            queueHandler.AddHandler(new SearchInILogsMsgHandler(pathToLogs, cts.Token));
            queueHandler.AddHandler(new SendFileMsgHandler(pathToLogs, cts.Token));
            queueHandler.AddHandler(new PingMsgHandler());

            Thread.Sleep(1000 * 60 * 60);
            return 0;
        }
    }
}