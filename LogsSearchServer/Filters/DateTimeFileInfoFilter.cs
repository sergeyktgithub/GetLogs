using System;
using System.IO;

namespace TestServerSocket.Filters
{
    public class DateTimeFileInfoFilter : FileInfoFilter
    {
        public DateTime FromUtc { get; }
        public DateTime ToUtc { get; }

        public DateTimeFileInfoFilter(DateTime fromUtc, DateTime toUtc)
        {
            FromUtc = fromUtc;
            ToUtc = toUtc;
        }

        public override bool Check(FileInfo fileInfo)
        {
            var res = FromUtc < fileInfo.LastWriteTimeUtc && fileInfo.LastWriteTimeUtc < ToUtc;
            return res;
        }
    }
}