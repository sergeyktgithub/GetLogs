namespace NetComModels.Messages
{
    public class BeginMsg : Msg
    {
        public string Msg { get; set; }

        public BeginMsg() : this("")
        {
        }

        public BeginMsg(string msg) : base(MsgType.Begin)
        {
            Msg = msg;
        }
    }
}