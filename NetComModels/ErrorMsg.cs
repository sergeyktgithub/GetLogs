namespace NetComModels
{
    public class ErrorMsg : Msg
    {
        public string Message { get; set; }

        public ErrorMsg() : base(MsgType.Error)
        {}

        public ErrorMsg(string message) : this()
        {
            Message = message;
        }
    }
}