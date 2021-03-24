using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using NetCom;
using NetComModels;
using TestServerSocket.Extensions;
using TestServerSocket.Filters;
using TestServerSocket.Packer;

namespace TestServerSocket.Handlers
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

        public override void SendAnswer(Packet packet)
        {
            var pairNet = new TwoEndPoints(
                NetHelper.GetLocalIpAddress(), GlobalProperties.ServerMsgPort,
                packet.SourceEndPoint
            );
            var msgSearchInLogs = JsonSerializer.Deserialize<T>(packet.Msg);
            var accountIdDirectoryList = GetAccIdDirectoryList(PathToLogs);

            if (accountIdDirectoryList.Any(x => x == msgSearchInLogs.AccId))
            {
                FileInfoFilters.Clear();
                SetFilters(msgSearchInLogs);

                var filesPacker = new FileFinder(msgSearchInLogs.AccId, PathToLogs, _baseDirName, FileInfoFilters);
                filesPacker.Search();

                if (filesPacker.IsEmpty)
                {
                    MessageUdp.Send(pairNet, new FoundFilesMsg());
                }
                else
                {
                    var accountIdPath = Path.Combine(PathToLogs, msgSearchInLogs.AccId);
                    var tempDir = TempDir.Create(accountIdPath);
                    tempDir.DeleteAllTempDir();
                    tempDir.CreateDirectory();

                    filesPacker.FoundFiles.WriteFoundFilesConfig(tempDir, out var fileName);

                    MessageUdp.Send(pairNet, new FoundFilesMsg(fileName, filesPacker.FoundFiles.Select(x => x.FullPath).ToList(), filesPacker.FoundFiles.Sum(x => x.Size)));
                }
            }
        }

        protected abstract void SetFilters(T msg);
    }
}