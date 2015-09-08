using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NatekLogService
{
    class WriteLog
    {
        private static String dateFormat = "yyyy/MM/dd HH:mm:ss";

        public  static void Write(String functionName, Exception ex)
        {
            StreamWriter LogFile = File.AppendText(ControlProcessorTypex86() + "\\Natek Alert GUI.log");
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine(DateTime.Now.ToString(dateFormat) + " " + functionName + "  : " + ex.ToString());
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine();
            LogFileSizeControl();
            LogFile.Close();
            LogFile = null;

        }

        public static void Write(String functionName, String msg)
        {
            StreamWriter LogFile = File.AppendText(ControlProcessorTypex86() + "\\Natek Alert GUI.log");
            LogFile.WriteLine(DateTime.Now.ToString(dateFormat) + " " + functionName + "  : " + msg);
            LogFile.Close();
            LogFileSizeControl();
            LogFile = null;
        }

        public static void LogFileSizeControl()
        {
            FileInfo fileInfo = new FileInfo(ControlProcessorTypex86() + "\\Natek Alert GUI.log");
            if (fileInfo.Length > 5242880)
            {
                fileInfo.Delete();
            }
        }

        public static String ControlProcessorTypex86()
        {
            if (Directory.Exists(@"C:\Program Files (x86)"))
            {
                return @"C:\Program Files (x86)\Natek\Natek Log Alert";
            }
            else
            {
                return @"C:\Program Files\Natek\Natek Log Alert";
            }
        }
    }
}
