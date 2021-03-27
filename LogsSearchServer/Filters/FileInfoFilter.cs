using System.IO;

namespace LogsSearchServer.Filters
{
    public abstract class FileInfoFilter
    {
        public abstract bool Check(FileInfo fileInfo);
    }
}