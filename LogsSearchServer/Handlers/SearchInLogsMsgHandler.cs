using System.Threading;
using LogsSearchServer.Filters;
using NetComModels;
using NetComModels.Messages;

namespace LogsSearchServer.Handlers
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