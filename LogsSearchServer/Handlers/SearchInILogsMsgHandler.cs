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
    public class SearchInILogsMsgHandler : SearchAccIdMsgHandler<SearchByAccIdMsg>
    {
        public SearchInILogsMsgHandler(string pathToLogs, CancellationToken token)
            : base(pathToLogs, "ILog", token, MsgType.SearchByAccId)
        {
        }

        protected override void SetFilters(SearchByAccIdMsg msg)
        {
            FileInfoFilters.Add(new DateTimeFileInfoFilter(msg.FromDateTimeUtc, msg.ToDateTimeUtc));
        }
    }
}