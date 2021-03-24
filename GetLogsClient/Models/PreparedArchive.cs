using System.Collections.Generic;
using System.Net;
using System.Windows.Shapes;
using NetComModels;

namespace GetLogsClient.Models
{
    public class PreparedArchive
    {
        private readonly FoundFilesMsg _msg;

        public IPEndPoint Source { get; set; }
        public string Name => System.IO.Path.GetFileName(_msg.FullPath);
        public string FullPath => _msg.FullPath;
        public List<string> Files => _msg.Files;
        public long TotalSize => _msg.TotalSize;

        public bool IsEmpty => _msg == null || 
                               string.IsNullOrEmpty(_msg.FullPath) ||
                               _msg.Files.Count == 0;

        public PreparedArchive(IPEndPoint source, FoundFilesMsg msg)
        {
            Source = source;
            _msg = msg;
        }
    }
}