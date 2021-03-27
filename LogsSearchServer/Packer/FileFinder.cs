using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogsSearchServer.Filters;
using LogsSearchServer.Models;
using NetComModels.Messages;
using Serilog;
using ZipExtension;

namespace LogsSearchServer.Packer
{
    public class FileFinder
    {
        private const string NameDirLogs = "Logs";

        private readonly string _accountIdPath;
        private readonly string _baseDirName;
        private readonly List<FileInfoFilter> _fileInfoFilters;

        public List<FoundFile> FoundFiles { get; }
        public bool IsEmpty => FoundFiles.Count == 0;

        public event EventHandler<string> ProcessMsgEvent;

        public FileFinder(string requiredAccountId, string logPath, string baseDirName, List<FileInfoFilter> fileInfoFilters)
        {
            _baseDirName = baseDirName;
            _fileInfoFilters = fileInfoFilters;
            _accountIdPath = Path.Combine(logPath, requiredAccountId);

            FoundFiles = new List<FoundFile>();
        }

        public List<FoundFile> Search()
        {
            FoundFiles.AddRange(PreparingFiles());
            return FoundFiles;
        }

        private List<FoundFile> PreparingFiles()
        {
            var logsPath = Path.Combine(_accountIdPath, NameDirLogs);
            var resultFoundFiles = new List<FoundFile>();
            bool haveLogs = false;

            foreach (var subDir in Directory.GetDirectories(logsPath))
            {
                foreach (var dirAccLog in Directory.GetDirectories(subDir))
                {
                    var srcPath = Path.Combine(dirAccLog, _baseDirName);
                    if (Directory.Exists(srcPath) == false)
                    {
                        continue;
                    }

                    var extractedArchives = ExtractAllArchives(srcPath);

                    var foundFiles = SearchByFilter(srcPath);
                    if (foundFiles.Count > 0)
                    {
                        if (haveLogs == false)
                        {
                            haveLogs = true;
                            SwearInLog("Логи есть, подготовка списка");
                        }

                        resultFoundFiles.AddRange(foundFiles);
                    }
                    else if (extractedArchives.Count > 0)
                    {
                        CompressAllArchives(extractedArchives);
                    }
                }
            }

            SwearInLog($"Нашел {resultFoundFiles.Count} файл(ов)");

            return resultFoundFiles;
        }

        private void SwearInLog(string msg)
        {
            OnProcessMsgEvent(msg);
            Log.Debug(msg);
        }

        private static Dictionary<string, List<string>> ExtractAllArchives(string sourcePath)
        {
            Dictionary<string, List<string>> resultExtract = new Dictionary<string, List<string>>();

            foreach (var sourceFilePath in Directory.GetFiles(sourcePath, "*.zip"))
            {
                var sourceFileInfo = new FileInfo(sourceFilePath);
                if (sourceFileInfo.Extension == ".zip")
                {
                    resultExtract[sourceFilePath] = Zip.ExtractToArchiveDirectory(sourceFilePath);
                }
            }

            return resultExtract;
        }

        private void CompressAllArchives(Dictionary<string, List<string>> extractedArchives)
        {
            foreach (var pair in extractedArchives)
            {
                var dirName = Path.GetDirectoryName(pair.Key);
                var archName = Path.GetFileNameWithoutExtension(pair.Key);
                var dstDir = Path.Combine(dirName ?? throw new InvalidOperationException(), archName);

                Directory.CreateDirectory(dstDir);

                foreach (var file in pair.Value)
                {
                    if (File.Exists(file) == false)
                    {
                        throw new FileNotFoundException("Файл не существует: " + file);
                    }

                    var dstPath = Path.Combine(dstDir, Path.GetFileName(file) ?? throw new InvalidOperationException());
                    File.Copy(file, dstPath);
                    File.Delete(file);
                }

                Zip.CreateFromDirectory(dstDir, pair.Key);
                Directory.Delete(dstDir, true);
            }
        }

        private List<FoundFile> SearchByFilter(string path)
        {
            var foundFiles = new List<FoundFile>();

            foreach (var filePath in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(filePath);

                if (_fileInfoFilters.Any(x => x.Check(fileInfo) == false)) continue;

                foundFiles.Add(new FoundFile(filePath));
            }

            return foundFiles;
        }

        protected virtual void OnProcessMsgEvent(string msg)
        {
            ProcessMsgEvent?.Invoke(this, msg);
        }
    }
}