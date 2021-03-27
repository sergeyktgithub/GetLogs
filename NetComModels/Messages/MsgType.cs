namespace NetComModels.Messages
{
    public enum MsgType
    {
        Ping,
        Pong,
        Begin,
        GetFile,
        SearchByAccIdAndPattern,
        SearchByAccId,
        FilesFound,
        Text,
        SendFile,
        Error,
        AccIdNotFoundError
    }
}