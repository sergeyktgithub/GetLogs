namespace GetLogsClient.ListBoxContent
{
    public class FileLogMsg : Msg
    {
        public string Path { get; }
        public FileLogMsg(string filePath)
        {
            Text = System.IO.Path.GetFileName(filePath);
            Path = filePath;
        }
    }
}