using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Serilog;

namespace logsService
{
    public class LogArch
    {
        private CancellationTokenSource _cancellationTokenSource;

        public LogArch(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public void ClearLogs(string pathToLog, int keepHours)
        {
            Log.Information($"Очистка логов, удаляем все что старше {keepHours} час.");

            //GMC.logs
            var accountDirectories = Directory.GetDirectories(pathToLog);
            foreach (var accountDirectory in accountDirectories)
            {
                var logsMainDir = Path.Combine(accountDirectory, "Logs");
                if (Directory.Exists(logsMainDir))
                {
                    //GMC.logs\<accId>\Logs\
                    var logsMainDirectories = Directory.GetDirectories(logsMainDir);
                    foreach (var logsMainDirectory in logsMainDirectories)
                    {
                        var accountLogDirectories = Directory.GetDirectories(logsMainDirectory);
                        foreach (var accountLogDirectory in accountLogDirectories)
                        {
                            var creationTime = (new FileInfo(accountLogDirectory)).CreationTime;
                            var minDate = DateTime.Now - TimeSpan.FromHours(keepHours);
                            if (creationTime < minDate)
                            {
                                if (_cancellationTokenSource.IsCancellationRequested)
                                {
                                    return;
                                }

                                Log.Debug($"Удалил: {accountLogDirectory}, creationTime: {creationTime}, borderDate: {minDate}, keepHours {keepHours}");
                                Directory.Delete(accountLogDirectory, true);
                            }
                        }
                    }
                }
                else
                {
                    Log.Debug($"Директория {logsMainDir} не существует");
                }
            }
        }

        public void ArchiveLogFiles(string pathToLog)
        {
            Log.Debug($"Старт архивирования");

            //GMC.logs
            var accountDirectories = Directory.GetDirectories(pathToLog);
            foreach (var accountDirectory in accountDirectories)
            {
                var logsMainDir = Path.Combine(accountDirectory, "Logs");
                if (Directory.Exists(logsMainDir))
                {
                    //GMC.logs\<accId>\Logs\
                    var logsMainDirectories = Directory.GetDirectories(logsMainDir);
                    foreach (var logsMainDirectory in logsMainDirectories)
                    {
                        var accountLogDirectories = Directory.GetDirectories(logsMainDirectory);
                        foreach (var accountLogDirectory in accountLogDirectories)
                        {
                            if (_cancellationTokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            ArchSubDirs(accountLogDirectory, "Logs", "*.txt");
                            ArchSubDirs(accountLogDirectory, "ILog", "*.ilog");
                            ArchSubDirs(accountLogDirectory, "BussLogs", "*.txt");
                        }
                    }
                }
                else
                {
                    Log.Debug($"Директория {logsMainDir} не существует");
                }
            }
        }

        private void ArchSubDirs(string logDir, string mainSubDir, string extension)
        {
            Log.Debug($"Вход: logDir={logDir}, mainSubDir={mainSubDir}, extension={extension}");

            var logAccountDir = Path.Combine(logDir, mainSubDir);
            if (Directory.Exists(logAccountDir))
            {
                var accountLogFiles = Directory.GetFiles(logAccountDir, extension);
                if (accountLogFiles.Length > 0)
                {
                    var creationTime = accountLogFiles.Max(x => (new FileInfo(x)).LastWriteTime);

                    if (creationTime >= DateTime.Now.Date)
                    {
                        Log.Debug($"Файлы еще свежие: {creationTime}");
                        return;
                    }

                    var date = creationTime.Date.ToString("dd_MM_yyyy");
                    var time = DateTime.Now.ToString("hh_mm_ss");
                    var archDirName = Path.Combine(logAccountDir, date + "__" + time);

                    if (!Directory.Exists(archDirName))
                        Directory.CreateDirectory(archDirName);

                    int countCopyFiles = 0;

                    foreach (var logFile in accountLogFiles)
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        var logFileName = Path.GetFileName(logFile);
                        var destFile = Path.Combine(archDirName, logFileName);

                        File.Copy(logFile, destFile, true);
                        File.Delete(logFile);

                        countCopyFiles++;

                        Log.Debug($"Скопировал: {logFile}");
                    }

                    if (countCopyFiles > 0)
                    {
                        var zipPath = Path.Combine(archDirName, archDirName + ".zip");
                        ZipFile.CreateFromDirectory(archDirName, zipPath);

                        Log.Debug($"Сжал: {zipPath}");
                    }

                    Directory.Delete(archDirName, true);
                    Log.Debug($"Удалил временную директорию: {archDirName}");
                }
            }
        }
    }
}