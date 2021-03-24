using System;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetComModels;

namespace GetLogsClient.NetServices
{
    public class AboutILogs : AboutFiles<SearchByAccIdMsg>
    {
        public AboutILogs(TwoEndPoints twoEndPoints, IMsgUdpListener msgUdpListener) : base(twoEndPoints, msgUdpListener)
        {
        }

        public async Task<PreparedArchive> CheckExistLogsAsync(string accId, DateTime fromDateTime, DateTime toDateTime, int timeout = 1000 * 10)
        {
            return await CheckExistFilesAsync(new SearchByAccIdMsg()
            {
                AccId = accId,
                FromDateTimeUtc = fromDateTime.ToUniversalTime(),
                ToDateTimeUtc = toDateTime.ToUniversalTime(),
            });
        }
    }
}