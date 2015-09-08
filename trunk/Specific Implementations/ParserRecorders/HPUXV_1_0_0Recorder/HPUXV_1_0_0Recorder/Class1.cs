using System;
using System.IO;
using Log;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class HPUXV_1_0_0Recorder : Parser
    {
        public HPUXV_1_0_0Recorder()
            : base()
        {
            LogName = "HPUXV_1_0_0Recorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            startLineCheckCount = 1;
            GetFiles();
        }

        public HPUXV_1_0_0Recorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false);

                try
                {
                    Rec r = new Rec();

                    if (arr.Length < 6)
                    {
                        Log.Log(LogType.FILE, LogLevel.WARN, "Different message on parse, moving to description: " + line);

                        DateTime dt = DateTime.Now;
                        r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;

                        r.Description = line;
                    }
                    else
                    {
                        String[] dateArr = arr[1].Split('/');
                        r.Datetime = DateTime.Now.Year + "/" + Convert.ToInt32(dateArr[0]) + "/" + Convert.ToInt32(dateArr[1]) + " " + arr[2] + ":00";

                        r.EventCategory = arr[0];
                        r.CustomStr1 = arr[3];
                        r.CustomStr2 = arr[4];
                        String[] lastArr = arr[5].Split('-');
                        if (lastArr.Length > 1)
                        {
                            r.CustomStr3 = lastArr[0];
                            r.CustomStr4 = lastArr[1];
                        }
                        else
                            r.CustomStr3 = arr[5];
                    }

                    r.LogName = LogName;

                    SetRecordData(r);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }
            }
            return true;
        }

        protected override void ParseFileNameLocal()
        {
            /*DateTime dtLast = new DateTime(1970, 12, 12);
            String fileLast = "";
            foreach (String file in Directory.GetFiles(Dir))
            {
                DateTime dt = File.GetLastAccessTime(file);
                if (dt > dtLast)
                {
                    fileLast = file;
                    dtLast = dt;
                }
            }
            FileName = fileLast;*/
        }

        protected override void ParseFileNameRemote()
        {
            String stdOut = "";
            String stdErr = "";
            se = new SshExec(remoteHost, user);
            se.Password = password;
            se.Connect();
            se.SetTimeout(Int32.MaxValue);
            String command = "ls " + Dir + " -lt | grep ^-";
            Log.Log(LogType.FILE, LogLevel.DEBUG, "SSH command is : " + command);
            se.RunCommand(command, ref stdOut, ref stdErr);
            StringReader sr = new StringReader(stdOut);
            String line = "";
            bool first = true;
            while ((line = sr.ReadLine()) != null)
            {
                if (first)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Command returned : " + line);
                    first = false;
                }
                String[] arr = line.Split(' ');
                if (arr[arr.Length - 1].Contains("syslog"))
                {
                    FileName = Dir + arr[arr.Length - 1];
                    break;
                }
            }
            stdOut = "";
            stdErr = "";
            se.Close();

            //FileName = Dir + "sulog";
        }

        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception e)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
