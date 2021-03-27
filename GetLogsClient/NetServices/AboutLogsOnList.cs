using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GetLogsClient.Models;
using NetCom;
using NetComModels;
using NetComModels.Messages;

namespace GetLogsClient.NetServices
{
    public class AboutLogsOnList : AboutFiles<SearchByAccIdAndPatternMsg>
    {
        public AboutLogsOnList(List<TwoEndPoints> twoEndPointsList, IPackageQueue packageQueue) : base(twoEndPointsList, packageQueue)
        {
        }

        public async Task<List<PreparedArchive>> CheckExistLogsAsync(string pattern, string accId, DateTime fromDateTime, DateTime toDateTime)
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