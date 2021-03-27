namespace NetComModels.Messages
{
    public class PongMsg : Msg
    {
        public PongMsg() : base(MsgType.Pong)
        {
        }
    }
}