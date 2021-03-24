namespace NetComModels
{
    public class Msg
    {
        public MsgType MsgType { get; set; }

        public Msg()
        {}

        public Msg(MsgType msgType)
        {
            MsgType = msgType;
        }
    }
}