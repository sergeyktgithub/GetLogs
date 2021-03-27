using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipExtension
{
    public static class Zip
    {
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);
        }

        public static string CreateFromDirectory(string sourceDirectoryName)
        {
            var name = Path.GetFileName(sourceDirectoryName);
            var srcDirName = Path.GetDirectoryName(sourceDirectoryName);
            var archiveFileName = Path.Combine(srcDirName ?? throw new InvalidOperationException(), name + ".zip");
            ZipFile.CreateFromDirectory(sourceDirectoryName, archiveFileName);

            return archiveFileName;
        }

        public static void ExtractToDirectoryByArchiveName(string sourceArchiveFileName)
        {
            var name = Path.GetFileNameWithoutExtension(sourceArchiveFileName);
            var srcDirName = Path.GetDirectoryName(sourceArchiveFileName);
            var destPath = Path.Combine(srcDirName, name);
            Directory.CreateDirectory(destPath);
            ZipFile.ExtractToDirectory(sourceArchiveFileName, destPath);
        }

        public static List<string> ExtractToArchiveDirectory(string sourceArchiveFileName)
        {
            if (!File.Exists(sourceArchiveFileName))
                throw new FileNotFoundException("Архив не существует: " + sourceArchiveFileName);

            var archFiles = new List<string>();
            var archiveDir = Path.GetDirectoryName(sourceArchiveFileName);

            using (var archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                foreach (var entry in archive.Entries)
                {
                    var filePath = Path.Combine(archiveDir ?? throw new InvalidOperationException(), entry.FullName);

                    archFiles.Add(filePath);

                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }

            ZipFile.ExtractToDirectory(sourceArchiveFileName, archiveDir);
            File.Delete(sourceArchiveFileName);

            return archFiles;
        }

        public static ZipArchive OpenRead(string zipPath)
        {
            return ZipFile.OpenRead(zipPath);
        }

        public static byte[] CompressText(string text)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry("file.txt");

                    using (var entryStream = demoFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(text);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static string ExtractFromBytes(byte[] bytes)
        {
            using (var zippedStream = new MemoryStream(bytes))
            {
                using (var archive = new ZipArchive(zippedStream))
                {
                    var entry = archive.Entries.FirstOrDefault();
                    if (entry == null) throw new NullReferenceException(nameof(entry) + "is null");

                    using (var unzippedEntryStream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            unzippedEntryStream.CopyTo(ms);
                            var unzippedArray = ms.ToArray();

                            return Encoding.Default.GetString(unzippedArray);
                        }
                    }
                }
            }
        }
    }
}
