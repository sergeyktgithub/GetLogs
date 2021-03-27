using System;

namespace LogsSearchServer.Models
{
    public class LogFile
    {
        public string FullName { get;}
        public string Name { get; }
        public DateTime LastWriteTimeUtc { get; }
    }
}