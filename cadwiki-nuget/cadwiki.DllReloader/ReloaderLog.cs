using cadwiki.NetUtils;

namespace cadwiki.DllReloader
{

    public class ReloaderLog : TextFileLog
    {

        public override string LogName
        {
            get
            {
                return base.LogName;
            }
            set
            {
                base.LogName = value;
            }
        }

        public override string LogDir
        {
            get
            {
                return base.LogDir;
            }
            set
            {
                base.LogDir = value;
            }
        }

        public ReloaderLog()
        {
            LogName = "ReloaderLog";
            CreateNewLogFile();
            DeleteOldLogFiles();
        }
    }
}