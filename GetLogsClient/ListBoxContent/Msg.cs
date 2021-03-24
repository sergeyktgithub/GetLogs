namespace GetLogsClient.ListBoxContent
{
    public class Msg
    {
        public string Text { get; set; }

        public Msg()
        {
        }

        public Msg(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}