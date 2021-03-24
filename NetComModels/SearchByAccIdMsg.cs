using System;

namespace NetComModels
{
    public class SearchByAccIdMsg : Msg
    {
        public string AccId { get; set; }
        public DateTime FromDateTimeUtc { get; set; }
        public DateTime ToDateTimeUtc { get; set; }

        public SearchByAccIdMsg() : this("", DateTime.MinValue, DateTime.MinValue)
        { }

        public SearchByAccIdMsg(string accId, DateTime fromDateTime, DateTime toDateTime) : base(MsgType.SearchByAccId)
        {
            AccId = accId ?? throw new ArgumentNullException(nameof(accId));
            FromDateTimeUtc = fromDateTime.ToUniversalTime();
            ToDateTimeUtc = toDateTime.ToUniversalTime();
        }

        public SearchByAccIdMsg(string accId, DateTime fromDateTime, DateTime toDateTime, MsgType msgType) : base(msgType)
        {
            AccId = accId ?? throw new ArgumentNullException(nameof(accId));
            FromDateTimeUtc = fromDateTime.ToUniversalTime();
            ToDateTimeUtc = toDateTime.ToUniversalTime();
        }
    }
}