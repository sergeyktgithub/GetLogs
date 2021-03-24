using System;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetComModels;

namespace GetLogsClient.NetServices
{
    public class AboutLogs : AboutFiles<SearchByAccIdAndPatternMsg>
    {
        public AboutLogs(TwoEndPoints twoEndPoints, IMsgUdpListener msgUdpListener) : base(twoEndPoints, msgUdpListener)
        {
        }

        public async Task<PreparedArchive> CheckExistLogsAsync(string pattern, string accId, DateTime fromDateTime, DateTime toDateTime, int timeout = 1000 * 10)
        {
            return await CheckExistFilesAsync(new SearchByAccIdAndPatternMsg()
            {
                AccId = accId,
                FromDateTimeUtc = fromDateTime.ToUniversalTime(),
                ToDateTimeUtc = toDateTime.ToUniversalTime(),
                Pattern = pattern
            });
        }
    }
}