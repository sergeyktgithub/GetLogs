namespace NetComModels
{
    public class GetFileMsg : Msg
    {
        public int DestPort { get; set; }
        public string FilePath { get; set; }

        public GetFileMsg() : base(MsgType.GetFile)
        {
        }

        public GetFileMsg(string filePath, int destPort) : this()
        {
            FilePath = filePath;
            DestPort = destPort;
        }
    }
}