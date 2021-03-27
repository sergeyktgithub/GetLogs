using System;

namespace NetComModels.Messages
{
    public class SearchByAccIdAndPatternMsg : SearchByAccIdMsg
    {
        public string Pattern { get; set; }

        public SearchByAccIdAndPatternMsg() : this("", "", DateTime.MinValue, DateTime.MinValue)
        { }

        public SearchByAccIdAndPatternMsg(string pattern, string accId, DateTime fromDateTime, DateTime toDateTime) : base(accId, fromDateTime, toDateTime, MsgType.SearchByAccIdAndPattern)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            AccId = accId ?? throw new ArgumentNullException(nameof(accId));
            FromDateTimeUtc = fromDateTime.ToUniversalTime();
            ToDateTimeUtc = toDateTime.ToUniversalTime();
        }
    }
}