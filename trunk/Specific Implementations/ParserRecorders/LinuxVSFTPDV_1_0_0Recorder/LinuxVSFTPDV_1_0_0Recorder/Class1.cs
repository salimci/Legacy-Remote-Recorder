using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public struct Fields
    {
        public Int64 tempPosition;
        public string fileName;
        public int maxRecordSendCounter;
    }
    public class LinuxVSFTPDV_1_0_0Recorder : Parser
    {
        private Fields RecordFields;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";



        public LinuxVSFTPDV_1_0_0Recorder()
            : base()
        {
            LogName = "LinuxVSFTPDV_1_0_0Recorder";
            RecordFields = new Fields();
            Position = lastTempPosition;
        }

        public override void Init()
        {
            GetFiles();
        }

        public LinuxVSFTPDV_1_0_0Recorder(String fileName)
            : base(fileName)
        {
        }

        private long GetLinuxFileSizeControl(string fileName)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetLinuxFileSizeControl() -->> Is started. ");

            long position = 0;
            try
            {
                string stdOut = "";
                string stdErr = "";
                String wcArg = "";
                String wcCmd = "";

                wcCmd = "wc";
                wcArg = "-l";
                String commandRead;
                String lineCommand = wcCmd + " " + wcArg + " " + FileName;
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + lineCommand);

                if (!se.Connected)
                {
                    se.Connect();
                }

                se.RunCommand(lineCommand, ref stdOut, ref stdErr);
                se.Close();

                String[] arr = SpaceSplit(stdOut, false);
                //RecordFields.tempPosition = Convert.ToInt64(arr[0]);
                position = Convert.ToInt64(arr[0]);
                //Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + RecordFields.tempPosition.ToString());
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetLinuxFileSizeControl -->> Getting Line Count With Command : " + position);
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetLinuxFileSizeControl() -->>  : " + exception);
            }
            return position;
        } // GetLinuxFileSizeControl


        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After


        /// <summary
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b, int type)
        {
            //type = 1 first index
            //type = 0 middle index
            //type = 2 last index

            int posA = 0;
            int posB = 0;

            if (type == 0)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
                posB = value.LastIndexOf(b, System.StringComparison.Ordinal);
            }

            if (type == 1)
            {
                posA = value.IndexOf(a, System.StringComparison.Ordinal);
                posB = value.IndexOf(b, System.StringComparison.Ordinal);
            }

            if (type == 2)
            {
                posA = value.LastIndexOf(a, System.StringComparison.Ordinal);
                posB = value.LastIndexOf(b, System.StringComparison.Ordinal);
            }

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between


        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line: " + line);
            long fileLength = 0;
            try
            {
                Rec r = new Rec();
                //r.Datetime = DateTime.Now.ToString(dateFormat);
                r.Description = line;
                r.LogName = LogName;
                r.SourceName = remoteHost;
                string[] lineArr1 = SpaceSplit(line, false);
                //
                try
                {
                    string month = lineArr1[1];
                    string day = lineArr1[2];
                    string year = lineArr1[4];
                    string time = lineArr1[3];
                    DateTime dt;
                    string myDateTimeString = month + ", " + day + "," + year + "  ," + time;
                    dt = Convert.ToDateTime(myDateTimeString);
                    string date = dt.ToString(dateFormat);
                    r.Datetime = date;
                }//
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "DateTime Error: " + exception.Message);
                }


                if (line.Contains("CONNECT:"))
                {
                    r.EventCategory = lineArr1[7].Replace(':', ' ').Trim();
                    r.CustomStr1 = lineArr1[6].Replace(']', ' ').Trim();
                    r.CustomStr3 = lineArr1[9].Replace('"', ' ').Trim();
                }

                else if (line.Contains("LOGIN:"))
                {
                    r.EventCategory = lineArr1[9].Replace(':', ' ').Trim();
                    r.UserName = Between(lineArr1[7], "[", "]");
                    r.CustomStr1 = lineArr1[6].Replace(']', ' ').Trim();
                    r.CustomStr3 = lineArr1[11].Replace('"', ' ').Trim();
                    r.EventType = lineArr1[8];
                }

                else
                {
                    r.EventCategory = lineArr1[9].Replace(':', ' ').Trim();
                    r.UserName = Between(lineArr1[7], "[", "]");
                    r.EventType = lineArr1[8];
                    r.CustomStr1 = lineArr1[6].Replace(']', ' ').Trim();
                    r.CustomStr3 = lineArr1[11].Replace('"', ' ').Replace(',', ' ').Trim();
                    r.CustomStr5 = lineArr1[12].Replace('"', ' ').Replace(',', ' ').Trim(); 
                    try
                    {
                        r.CustomInt3 = Convert.ToInt32(lineArr1[13]);
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 Error: " + exception.Message);
                        r.CustomInt3 = 0;
                    }
                    r.CustomStr6 = lineArr1[15];
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "maxRecordSendCounter: " + RecordFields.maxRecordSendCounter);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "lineLimit: " + lineLimit);

                //if (RecordFields.maxRecordSendCounter == lineLimit)
                {
                    RecordFields.maxRecordSendCounter = 0;
                    fileLength = GetLinuxFileSizeControl(RecordFields.fileName);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "fileLength: " + fileLength);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "position: " + Position);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "lineLimit: " + lineLimit);

                    if (Position > fileLength)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Position is greater than the length file position will be reset.");
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Position = 0 ");
                    }
                }

                if (Position < fileLength)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record is sending now.");
                    SetRecordData(r);
                    RecordFields.maxRecordSendCounter++;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "maxRecordSendCounter." + RecordFields.maxRecordSendCounter);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Position." + Position);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Record sended.");
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                return false;
            }
            return true;
        }



        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                String day = "secure";
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                foreach (String file in Directory.GetFiles(Dir))
                {
                    if (file.Equals(day))
                    {
                        FileName = file;
                        break;
                    }
                }
            }
            else
                FileName = Dir;
        }

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> Enter The ParseFileNameRemote Function");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() --> Directory | " + Dir);
                    se.Connect();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() --> Remote Host Already Connected ");
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep secure";
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> SSH command : " + command + " Result : " + stdOut);
                    //stdout : -rwxrw-rw- 1 ibrahim ibrahim 1920 2011-07-04 11:17 secure

                    StringReader sr = new StringReader(stdOut);
                    Boolean fileExistControl = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].Equals("secure") == true)
                            fileExistControl = true;
                    }

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        if (fileExistControl)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> Secure File is Exist");
                            stdOut = "";
                            stdErr = "";
                            String commandRead;

                            if (readMethod == "nread")
                            {
                                commandRead = tempCustomVar1 + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
                            }

                            Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() --> Position : " + Position);
                            se.RunCommand(commandRead, ref stdOut, ref stdErr);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() --> Result : " + stdOut);
                            //Jun 26 04:34:14 SAMBASERVER sshd[20314]: Accepted password for natek from 172.16.1.14 port 55200 ssh2

                            StringReader srTest = new StringReader(stdOut);
                            se.Close();

                            if (String.IsNullOrEmpty(stdOut))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() --> Restart The Position");
                                Position = 0;
                            }
                        }
                        else
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->> Secure File is NOT Exist");

                    }
                    else
                    {
                        if (fileExistControl)
                        {
                            FileName = Dir + "secure";
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                "  LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->>  Last File Is Null And Setted The File To " + FileName);

                        }
                    }

                    stdOut = "";
                    stdErr = "";
                    se.Close();
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " LinuxVSFTPDV_1_0_0Recorder In ParseFileNameRemote() -->>  Exit The Function");
        }

        public override void GetFiles()
        {
            try
            {
                Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In GetFiles() ");
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  LinuxVSFTPDV_1_0_0Recorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }
    }
}
