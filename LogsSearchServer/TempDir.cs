using System;
using System.IO;

namespace TestServerSocket
{
    public class TempDir
    {
        public static readonly string Label = "_temp";

        public string Name { get; set; }
        public string FullPath { get; set; }
        public string ParentPath { get; set; }

        public TempDir()
        {
        }

        private TempDir(string name, string fullPath, string parentPath)
        {
            Name = name;
            FullPath = fullPath;
            ParentPath = parentPath;
        }

        private TempDir(string path)
        {
            Name = Guid.NewGuid().ToString("N") + Label;
            FullPath = System.IO.Path.Combine(path, Name);
            ParentPath = path;
        }

        public static TempDir Create(string path)
        {
            return new TempDir(path);
        }

        public static TempDir CreateFromFullPath(string fullPath)
        {
            var path = Path.GetDirectoryName(fullPath);
            return new TempDir(
                Path.GetFileName(path), path, Path.GetDirectoryName(path)
            );
        }

    }
}