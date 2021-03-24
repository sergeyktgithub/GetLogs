using System;

namespace GetLogsClient.Models
{
    public class LastState
    {
        public int AccIdIndex { get; set; }
        public string[] AccIdList { get; set; }

        public int SelectedPatternIndex { get; set; }
        public string[] PatternList { get; set; }

        public DateTime FromDateTime { get; set; }
        public DateTime ToDateTime { get; set; }
        public string SavePath { get; set; }
    }
}