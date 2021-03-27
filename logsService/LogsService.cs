using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Timers = System.Timers;
using Serilog;
using IniSettings;
using LogsSearchServer;
using LogsSearchServer.Handlers;
using NetCom;
using NetComModels;

namespace logsService
{
    public partial class LogsService : ServiceBase
    {
        private const int DefaultKeepHours = 24 * 7;
        private const int DefaultCheckInterval = 12;
        private readonly string _serviceDir;
        private Timers.Timer _timerArchLogs;
        private LogArch _logArch;
        private CancellationTokenSource _cancellationTokenSource;

        private string _pathToLog = string.Empty;
        private int _keepHours = DefaultKeepHours;
        private int _checkInterval = DefaultKeepHours;

        public LogsService()
        {
            InitializeComponent();

            Process thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.Idle;

            _serviceDir = System.Reflection.Assembly.GetAssembly(typeof(LogsService)).Location;
            string logPath = Path.Combine(Path.GetDirectoryName(_serviceDir) ?? throw new InvalidOperationException(), "log.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath, 
                    flushToDiskInterval: TimeSpan.FromSeconds(5),
                    rollingInterval:RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;

            Log.Information("Initialize service");
        }

        protected override void OnStart(string[] args)
        {
            Log.Information("Start service");

            try
            {
                ReadConfigIni();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _logArch = new LogArch(_cancellationTokenSource);

            InitLogsSearchServer();
            InitArchLogsTimer();
        }

        private void ReadConfigIni()
        {
            string configPath = Path.Combine(Path.GetDirectoryName(_serviceDir) ?? throw new InvalidOperationException(), "config.ini");

            Log.Debug($"pathToConfigIni = {configPath}");

            var configIniFile = new IniFile(configPath);
            ParseKeyPathToLog(configIniFile);
            ParseKeyKeepHours(configIniFile);
            ParseKeyCheckInterval(configIniFile);

            Log.Information($"Загрузил из config.ini");
            Log.Information($"pathToLog={_pathToLog}");
            Log.Information($"keepHours={_keepHours}");
            Log.Information($"checkInterval={_checkInterval}");
        }

        private void ParseKeyCheckInterval(IniFile configIniFile)
        {
            if (configIniFile.KeyExists("LogConfig", "checkInterval"))
            {
                try
                {
                    _checkInterval = int.Parse(configIniFile.Read("LogConfig", "checkInterval"));
                }
                catch
                {
                    Log.Debug($"Не удалось получить checkInterval, ставлю по умолчанию {DefaultCheckInterval}");
                    _checkInterval = DefaultCheckInterval;
                }
            }
            else
            {
                throw new Exception("LogConfig или checkInterval не найдена в config.ini");
            }

            _checkInterval *= 60 * 60 * 1000;
        }

        private void ParseKeyKeepHours(IniFile configIniFile)
        {
            if (configIniFile.KeyExists("LogConfig", "keepHours"))
            {
                try
                {
                    _keepHours = int.Parse(configIniFile.Read("LogConfig", "keepHours"));
                }
                catch
                {
                    Log.Debug($"Не удалось получить keepHours, ставлю по умолчанию {DefaultKeepHours}");
                    _keepHours = DefaultKeepHours;
                }
            }
            else
            {
                throw new Exception("LogConfig или keepHours не найдена в config.ini");
            }
        }

        private void ParseKeyPathToLog(IniFile configIniFile)
        {
            if (configIniFile.KeyExists("Paths", "pathToLog"))
            {
                _pathToLog = configIniFile.Read("Paths", "pathToLog");
            }
            else
            {
                throw new Exception("Section или pathToLog не найдена в config.ini");
            }
        }

        private void InitArchLogsTimer()
        {
            Log.Debug("Begin InitArchLogsTimer");

            _timerArchLogs = new Timers.Timer(1000) {AutoReset = false};
            _timerArchLogs.Elapsed += TimerArchLogsOnElapsed;
            _timerArchLogs.Start();

            Log.Debug("End InitArchLogsTimer");
        }

        private void InitLogsSearchServer()
        {
            Log.Debug("Begin InitLogsSearchServer");

            IUdpListener udpListener = new UdpListener(GlobalProperties.ServerMsgPort, _cancellationTokenSource.Token);
            IPackageQueue packageQueue = new PackageQueue(udpListener, _cancellationTokenSource.Token);

            PackageQueueHandler packageQueueHandler = new PackageQueueHandler(packageQueue, _cancellationTokenSource.Token);
            packageQueueHandler.AddHandler(new SearchInLogsMsgHandler(_pathToLog, _cancellationTokenSource.Token));
            packageQueueHandler.AddHandler(new SearchInILogsMsgHandler(_pathToLog, _cancellationTokenSource.Token));
            packageQueueHandler.AddHandler(new SendFileMsgHandler(_pathToLog, _cancellationTokenSource.Token));
            packageQueueHandler.AddHandler(new PingMsgHandler());

            Log.Debug("End InitLogsSearchServer");
        }

        private void TimerArchLogsOnElapsed(object sender, Timers.ElapsedEventArgs e)
        {
            Log.Debug("Старт просмотра логов");

            try
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _timerArchLogs.Stop();
                    return;
                }

                _logArch.ClearLogs(_pathToLog, _keepHours);
                _logArch.ArchiveLogFiles(_pathToLog);
            }
            finally
            {
                _timerArchLogs.Interval = _checkInterval * 60 * 60 * 1000;
                _timerArchLogs.Start();
            }
        }

        protected override void OnStop()
        {
            _cancellationTokenSource.Cancel();
            _timerArchLogs.Stop();
            Log.Information("Stop service");
        }
    }
}
