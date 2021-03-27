using System;

namespace NetComModels.Messages
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

        public override string ToString()
        {
            return Enum.GetName(typeof(MsgType), MsgType);
        }
    }
}