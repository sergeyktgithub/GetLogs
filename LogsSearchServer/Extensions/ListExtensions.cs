using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LogsSearchServer.Models;

namespace LogsSearchServer.Extensions
{
    public static class ListExtensions
    {
        public static void WriteFoundFilesConfig(this List<FoundFile> foundFiles, TempDir tempDir, out string fileName)
        {
            fileName = Path.Combine(tempDir.FullPath, tempDir.Name + ".json");
            var jsonDesc = JsonSerializer.Serialize(foundFiles);

            File.WriteAllText(fileName, jsonDesc);
        }

        public static void ReadFoundFilesConfig(this TempDir tempDir, out List<FoundFile> foundFiles)
        {
            foundFiles = new List<FoundFile>();

            var fileName = Path.Combine(tempDir.FullPath, tempDir.Name + ".json");
            if (File.Exists(fileName))
            {
                var jsonText = File.ReadAllText(fileName);
                foundFiles = JsonSerializer.Deserialize<List<FoundFile>>(jsonText);
            }
        }

        public static void DeleteFoundFilesConfig(this TempDir tempDir)
        {
            var fileName = Path.Combine(tempDir.FullPath, tempDir.Name + ".json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}