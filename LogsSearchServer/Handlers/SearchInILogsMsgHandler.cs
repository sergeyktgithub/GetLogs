using System.Threading;
using LogsSearchServer.Filters;
using NetComModels;
using NetComModels.Messages;

namespace LogsSearchServer.Handlers
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