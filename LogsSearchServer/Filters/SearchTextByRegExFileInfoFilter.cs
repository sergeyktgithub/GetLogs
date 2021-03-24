using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TestServerSocket.Filters
{
    public class SearchTextByRegExFileInfoFilter : FileInfoFilter
    {
        private Regex _regex;

        public SearchTextByRegExFileInfoFilter(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new Exception("pattern не должен быть пустым");
            }

            _regex = new Regex(pattern);
        }

        public override bool Check(FileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.FullName))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (_regex.IsMatch(line))
                    {
                        return true;
                    }

                    line = reader.ReadLine();
                }
            }

            return false;
        }
    }
}