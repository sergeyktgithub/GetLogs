using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IniSettings
{
    public class IniFile
    {
        readonly string _path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);

        public IniFile(string path)
        {
            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException(path);
            }

            _path = path;
        }

        public string Read(string section, string key)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retVal, 255, _path);
            return retVal.ToString();
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _path);
        }

        public void DeleteKey(string key, string section = null)
        {
            Write(section, key, null);
        }
        
        public void DeleteSection(string section = null)
        {
            Write(section, null, null);
        }

        public bool KeyExists(string section, string key)
        {
            return Read(section, key).Length > 0;
        }
    }
}
