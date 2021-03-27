using System;

namespace NetComModels.Messages
{
    public class TextMsg : Msg
    {
        public string Msg { get; set; }

        public TextMsg() : this("")
        {
        }

        public TextMsg(string msg) : base(MsgType.Text)
        {
            Msg = msg;
        }
    }
}