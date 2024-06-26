using System.Collections.Generic;

namespace cadwiki.NetUtils
{
    public class Lists
    {
        public static string StringListToString(List<string> lst, string delimeter)
        {
            string str = "";
            int i = 0;
            foreach (string item in lst)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    str += item;
                }
                if (i < lst.Count - 1 & !string.IsNullOrEmpty(delimeter))
                {
                    str += delimeter;
                }
                i += 1;
            }
            return str;
        }
    }
}