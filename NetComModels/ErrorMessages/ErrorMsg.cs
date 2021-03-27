using NetComModels.Messages;

namespace NetComModels.ErrorMessages
{
    public class ErrorMsg : Msg
    {
        public string Message { get; set; }

        public ErrorMsg() : this("", MsgType.Error)
        {
        }

        public ErrorMsg(string message) : this(message, MsgType.Error)
        {
        }

        protected ErrorMsg(string message, MsgType msgType) : base(msgType)
        {
            Message = message;
        }
    }
}