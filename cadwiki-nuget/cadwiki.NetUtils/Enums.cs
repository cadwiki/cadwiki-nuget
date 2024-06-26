using System;
using System.Collections.Generic;

namespace cadwiki.NetUtils
{

    public class Enums
    {
        public static List<string> GetStringsFromEnum(Type @type)
        {
            var strings = new List<string>();
            Array items = Enum.GetNames(type);
            foreach (string item in items)
                strings.Add(item);
            return strings;
        }
    }
}