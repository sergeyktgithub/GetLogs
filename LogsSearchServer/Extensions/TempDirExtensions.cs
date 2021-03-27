using System.IO;

namespace LogsSearchServer.Extensions
{
    public static class TempDirExtensions
    {
        public static void CreateDirectory(this TempDir tempDir)
        {
            Directory.CreateDirectory(tempDir.FullPath);
        }

        public static void Delete(this TempDir tempDir)
        {
            Directory.Delete(tempDir.FullPath, true);
        }

        public static void DeleteAllTempDir(this TempDir tempDir)
        {
            foreach (var dir in Directory.GetDirectories(tempDir.ParentPath))
            {
                if (ContainsLabel(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        private static bool ContainsLabel(string path)
        {
            var name = System.IO.Path.GetFileName(path);
            return name != null && name.Contains(TempDir.Label);
        }
    }
}