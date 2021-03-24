using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using NetCom;
using NetComModels;
using TestServerSocket.Filters;
using TestServerSocket.Packer;

namespace TestServerSocket.Handlers
{
    public class SearchInLogsMsgHandler : SearchAccIdMsgHandler<SearchByAccIdAndPatternMsg>
    {
        public SearchInLogsMsgHandler(string pathToLogs, CancellationToken token) 
            : base(pathToLogs, "Logs", token, MsgType.SearchByAccIdAndPattern)
        {
        }

        protected override void SetFilters(SearchByAccIdAndPatternMsg msg)
        {
            FileInfoFilters.Add(new DateTimeFileInfoFilter(msg.FromDateTimeUtc, msg.ToDateTimeUtc));
            if (string.IsNullOrEmpty(msg.Pattern) == false)
                FileInfoFilters.Add(new SearchTextByRegExFileInfoFilter(msg.Pattern));
        }
    }
}