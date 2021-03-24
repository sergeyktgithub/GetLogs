using System.IO;

namespace TestServerSocket.Filters
{
    public abstract class FileInfoFilter
    {
        public abstract bool Check(FileInfo fileInfo);
    }
}