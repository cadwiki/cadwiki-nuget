using Autodesk.AutoCAD.ApplicationServices.Core;
using System;

namespace cadwiki.AC.DocProcessor
{
    public enum SdiMode
    {
        SingleDocument,
        MultiDocument
    }

    public class SdiModeChecker
    {
        public static SdiMode GetSdiMode()
        {
            var sdi_mode = Convert.ToInt32(Application.GetSystemVariable("SDI"));
            if (sdi_mode == 1)
            {
                return SdiMode.SingleDocument;
            }
            else
            {
                return SdiMode.MultiDocument;
            }
        }
    }
}
