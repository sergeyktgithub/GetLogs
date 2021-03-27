using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetComModels.Messages
{
    public class FoundFilesMsg : Msg
    {
        public string FullPath { get; set; }
        public List<string> Files { get; set; }
        public long TotalSize { get; set; }

        [JsonIgnore]
        public bool IsEmpty => Files.Count == 0;

        public FoundFilesMsg() : base(MsgType.FilesFound)
        {
            Files = new List<string>();
        }

        public FoundFilesMsg(string fullPath, List<string> files, long totalSize) : this()
        {
            FullPath = fullPath;
            Files = files;
            TotalSize = totalSize;
        }
    }
}