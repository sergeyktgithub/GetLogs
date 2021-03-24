namespace NetComModels
{
    public class MsgText : Msg
    {
        public string Msg { get; set; }

        public MsgText() : base(MsgType.Text)
        {}

        public MsgText(string msg) : this()
        {
            Msg = msg;
        }
    }
}