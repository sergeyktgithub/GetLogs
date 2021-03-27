namespace NetComModels.Messages
{
    public class SendFileMsg : Msg
    {
        public string FilePath { get; set; }
        public long FileSize { get; set; }

        public SendFileMsg() : this("", 0)
        {
        }

        public SendFileMsg(string filePath, long fileSize) : base(MsgType.SendFile)
        {
            FilePath = filePath;
            FileSize = fileSize;
        }
    }
}