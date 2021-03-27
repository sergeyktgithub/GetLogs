using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetComModels;
using NetComModels.Messages;

namespace GetLogsClient.NetServices
{
    public class AboutILogsOnList : AboutFiles<SearchByAccIdMsg>
    {
        public AboutILogsOnList(List<TwoEndPoints> twoEndPointsList, IPackageQueue packageQueue) : base(twoEndPointsList, packageQueue)
        {
        }

        public async Task<List<PreparedArchive>> CheckExistILogsAsync(string accId, DateTime fromDateTime, DateTime toDateTime)
        {
            return await CheckExistFilesAsync(new SearchByAccIdMsg()
            {
                AccId = accId,
                FromDateTimeUtc = fromDateTime.ToUniversalTime(),
                ToDateTimeUtc = toDateTime.ToUniversalTime()
            });
        }
    }
}