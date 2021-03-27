using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using LogsSearchServer.Extensions;
using LogsSearchServer.Filters;
using LogsSearchServer.Packer;
using NetCom;
using NetCom.Extensions;
using NetCom.Helpers;
using NetComModels;
using NetComModels.ErrorMessages;
using NetComModels.Messages;
using ZipExtension;

namespace LogsSearchServer.Handlers
{
    public abstract class SearchAccIdMsgHandler<T> : MsgHandler where T : SearchByAccIdMsg
    {
        private readonly string _baseDirName;
        protected List<FileInfoFilter> FileInfoFilters { get; }

        protected SearchAccIdMsgHandler(string pathToLogs, string baseDirName, CancellationToken token, MsgType msgType) : base(pathToLogs, token, msgType)
        {
            _baseDirName = baseDirName;
            FileInfoFilters = new List<FileInfoFilter>();
        }

        protected List<string> GetAccIdDirectoryList(string pathToLogs)
        {
            return Directory.GetDirectories(pathToLogs).Select(Path.GetFileName).ToList();
        }

        public override void SendAnswer(Package package)
        {
            var pairNet = new TwoEndPoints(
                NetHelper.GetLocalIpAddress(), GlobalProperties.ServerMsgPort,
                package.SourceEndPoint
            );

            UdpMessage.Send(pairNet, new BeginMsg());

            var msgSearchInLogs = package.Deserialize<T>();
            var accountIdDirectoryList = GetAccIdDirectoryList(PathToLogs);

            if (accountIdDirectoryList.Any(x => x == msgSearchInLogs.AccId))
            {
                FileInfoFilters.Clear();
                SetFilters(msgSearchInLogs);

                var fileFinder = new FileFinder(msgSearchInLogs.AccId, PathToLogs, _baseDirName, FileInfoFilters);
                fileFinder.ProcessMsgEvent += (sender, msg) =>
                {
                    UdpMessage.Send(pairNet, new TextMsg(msg));
                };
                fileFinder.Search();

                if (fileFinder.IsEmpty)
                {
                    UdpMessage.Send(pairNet, new FoundFilesMsg());
                }
                else
                {
                    var accountIdPath = Path.Combine(PathToLogs, msgSearchInLogs.AccId);
                    var tempDir = TempDir.Create(accountIdPath);
                    tempDir.DeleteAllTempDir();
                    tempDir.CreateDirectory();

                    fileFinder.FoundFiles.WriteFoundFilesConfig(tempDir, out var fileName);

                    UdpMessage.Send(pairNet, new FoundFilesMsg(fileName, fileFinder.FoundFiles.Select(x => x.FullPath).ToList(), fileFinder.FoundFiles.Sum(x => x.Size)));
                }
            }
            else
            {
                UdpMessage.Send(pairNet, new AccIdNotFoundErrorMsg(msgSearchInLogs.AccId));
            }
        }

        protected abstract void SetFilters(T msg);
    }
}