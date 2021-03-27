using System;
using System.Collections.Generic;
using System.IO;
using LogsSearchServer.Extensions;
using LogsSearchServer.Models;
using Serilog;
using ZipExtension;

namespace LogsSearchServer.Packer
{
    public class FilesPacker
    {
        private readonly TempDir _tempDir;

        public string ZipName => _tempDir.Name + ".zip";
        public string ZipPath => Path.Combine(_tempDir.ParentPath, ZipName);

        public FilesPacker(TempDir tempDir)
        {
            _tempDir = tempDir;
        }

        public void Create()
        {
            DeleteOldZipFiles();

            _tempDir.ReadFoundFilesConfig(out var foundFiles);
            _tempDir.DeleteFoundFilesConfig();

            PreparingFiles(foundFiles);

            Zip.CreateFromDirectory(_tempDir.FullPath, ZipPath);
            _tempDir.Delete();
        }

        private void PreparingFiles(List<FoundFile> foundFiles)
        {
            foreach (var foundFile in foundFiles)
            {
                if (File.Exists(foundFile.FullPath))
                {
                    var sourceFilePath = foundFile.FullPath;
                    var destFilePath = string.Empty;
                    try
                    {
                        var fileDir = Path.GetDirectoryName(Path.GetDirectoryName(sourceFilePath));
                        var dirName = Path.GetFileName(fileDir);
                        var fileName = Path.GetFileName(sourceFilePath);
                        var destDir = Path.Combine(_tempDir.FullPath, dirName);
                        destFilePath = Path.Combine(destDir, fileName);

                        Directory.CreateDirectory(destDir);
                        File.Copy(sourceFilePath, destFilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Ошибка при копировании файла лога: {sourceFilePath} в {destFilePath}");
                    }
                }
            }
        }

        private void DeleteOldZipFiles()
        {
            foreach (var file in Directory.GetFiles(_tempDir.ParentPath, "*.zip"))
            {
                File.Delete(file);
            }
        }
    }
}