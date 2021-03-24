using System;
using System.Collections.Generic;
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

        public static List<string> Extract(string sourceArchiveFileName)
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

            ZipFile.ExtractToDirectory(sourceArchiveFileName, Path.GetDirectoryName(sourceArchiveFileName));
            File.Delete(sourceArchiveFileName);

            return archFiles;
        }

        public static ZipArchive OpenRead(string zipPath)
        {
            return ZipFile.OpenRead(zipPath);
        }
    }
}
