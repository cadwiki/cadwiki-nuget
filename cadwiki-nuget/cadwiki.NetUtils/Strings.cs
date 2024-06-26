
namespace cadwiki.NetUtils
{

    public class Strings
    {
        public static bool NotContainsBackSlashVInSubFolder(string s)
        {
            return !s.ToLower().Contains(@"\_v");
        }
    }
}