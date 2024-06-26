﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace cadwiki.NetUtils
{

    public class Exceptions
    {
        public static List<string> GetStackTraceLines(Exception ex)
        {
            string stackTrace = ex.StackTrace;
            char[] seperator = Environment.NewLine.ToCharArray();
            return stackTrace.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static List<string> GetPrettyStringList(Exception ex)
        {
            var list = new List<string>();
            list.Add("-----------------------------------------------------------------------------");

            if (!string.IsNullOrEmpty(ex.Message))
            {
                list.Add("Message : ".PadLeft(26) + ex.Message);
            }
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                list.Add("StackTrace : ".PadLeft(26) + ex.StackTrace);
            }
            if (!string.IsNullOrEmpty(ex.InnerException.Message))
            {
                list.Add("InnerException Message : " + ex.InnerException.Message);
            }
            // If (Not String.IsNullOrEmpty(ex.Message)) Then
            // End If

            list.Add("-----------------------------------------------------------------------------");
            return list;
        }
    }
}