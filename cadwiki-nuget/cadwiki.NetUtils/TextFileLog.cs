using System;
using System.IO;
using Microsoft.VisualBasic;

namespace cadwiki.NetUtils
{

    public abstract class TextFileLog
    {
        public virtual string LogName
        {
            get
            {
                return _logName;
            }
            set
            {
                _logName = value;
            }
        }

        private static string _logDir = @"C:\Temp";

        public virtual string LogDir
        {
            get
            {
                return _logDir;
            }
            set
            {
                _logDir = value;
                CreateNewLogFile();
            }
        }

        private string _logName = "TextFileLog";
        private string _logExt = ".txt";
        private string _logFilePath = "";

        public virtual string LogFilePath
        {
            get
            {
                return _logFilePath;
            }
            set
            {
                _logFilePath = value;
            }
        }

        private int _daysToKeepLogFile = 30;

        public TextFileLog()
        {
        }

        public void CreateNewLogFile()
        {
            try
            {
                string dir = _logDir;
                string fileName = LogName;
                string fileExt = _logExt;

                if (!Directory.Exists(dir))
                {
                    var di = Directory.CreateDirectory(dir);
                }

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd");
                _logFilePath = string.Concat(dir, @"\", fileName, "_", timeStamp, fileExt);

                if (!File.Exists(_logFilePath))
                {
                    var stream = File.Create(_logFilePath);
                    stream.Close();
                    Write("Log created.");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public bool Write(string message)
        {
            try
            {
                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                timeStamp.PadRight(19);
                File.AppendAllText(_logFilePath, Environment.NewLine + timeStamp + " =>" + Constants.vbTab + message);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
            }
        }

        public void Exception(Exception ex)
        {
            Write("-----------------------------------------------------------------------------");

            if (ex is not null)
            {
                Write(ex.GetType().FullName);

                if (ex.Message is not null)
                {
                    Write("Message".PadRight(22) + " : " + ex.Message);
                }

                if (ex.StackTrace is not null)
                {
                    Write("StackTrace".PadRight(22) + " : " + ex.StackTrace);
                }

                if (ex.InnerException is not null && ex.InnerException.Message is not null)
                {
                    Write("InnerException Message" + " : " + ex.InnerException.Message);
                }
            }

            Write("-----------------------------------------------------------------------------");
        }

        public void Delete()
        {
            try
            {
                string dir = Path.GetDirectoryName(_logFilePath);

                if (Directory.Exists(dir))
                {
                    if (File.Exists(_logFilePath))
                        File.Delete(_logFilePath);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void DeleteOldLogFiles()
        {
            try
            {
                var thresholdDate = DateTime.Now.AddDays(-1 * _daysToKeepLogFile);
                var directoryInfo = new DirectoryInfo(_logDir);
                FileInfo[] files = directoryInfo.GetFiles();

                foreach (FileInfo @file in files)
                {

                    if ((@file.Extension.ToLower() ?? "") == (_logExt ?? "") && IsLogFileOlderThanThreshold(@file.Name, thresholdDate))
                    {

                        try
                        {
                            @file.Delete();
                            Write("Deleted outdated log file: " + @file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Exception(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        private bool IsLogFileOlderThanThreshold(string fileName, DateTime thresholdDate)
        {
            try
            {
                string dateString = fileName.Replace(_logName, "").Replace(_logExt, "").Replace("_", "");
                DateTime logDate = default;

                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out logDate))
                {
                    return logDate < thresholdDate;
                }

                return false;
            }
            catch (Exception ex)
            {

            }

            return default;

        }
    }
}