namespace NetComModels.Messages
{
    public class PingMsg : Msg
    {
        public PingMsg() : base(MsgType.Ping)
        {
        }
    }
}