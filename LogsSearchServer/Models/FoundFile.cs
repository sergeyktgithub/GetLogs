using System.IO;

namespace LogsSearchServer.Models
{
    public class FoundFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }

        public FoundFile()
        {
        }

        public FoundFile(string fullPath)
        {
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
            Size = (new FileInfo(fullPath)).Length;
        }
    }
}