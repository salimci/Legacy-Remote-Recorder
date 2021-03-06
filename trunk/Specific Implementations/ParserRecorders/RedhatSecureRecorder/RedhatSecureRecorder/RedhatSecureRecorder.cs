using System;
using System.Collections.Generic;
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
    public class RedhatSecureRecorder : Parser
    {
        public RedhatSecureRecorder()
            : base()
        {
            LogName = "RedhatSecureRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public RedhatSecureRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            bool isInformMessage = false;
            if (line == "")
                return true;
            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true);
                if (arr.Length < 5)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your Squid Logger before messing with developer! Parsing will continue...");
                    return true;
                }
                try
                {  //Nov 12 15:54:36 itim last message repeated 2 times
                    Rec r = new Rec();
                    DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                    r.SourceName = arr[3];
                    r.CustomStr6 = arr[4];
                    r.Description = "";
                    r.EventType = "Success";
                    r.EventCategory = null;
                    if (r.CustomStr6.StartsWith("sshd[") || r.CustomStr6.StartsWith("login["))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                        if (arr[5].StartsWith("Accepted")
                            || arr[5].StartsWith("Failed")
                            || arr[5].StartsWith("pam_unix") || arr[5].StartsWith("FAILED"))
                        {
                            Int32 i = 5;
                            for (i = 5; i < arr.Length; i++)
                            {
                                if ((i + 2) < arr.Length)
                                {
                                    if (arr[i + 2] == "from"
                                        || arr[i + 2] == "by")
                                    {
                                        r.UserName = arr[i + 1];
                                        i += 2;
                                        break;
                                    }
                                }
                            }
                            if (i < arr.Length)
                            {
                                r.CustomStr1 = arr[i];
                                i++;
                                r.CustomStr3 = arr[i];
                                i++;
                            }
                            if ((arr[5].StartsWith("pam_unix")))
                            {
                                r.EventCategory = arr[6] + " " + arr[7];
                            }
                            else
                            {
                                r.EventCategory = arr[5] + " " + arr[6];
                            }
                        }
                        else if (arr[5].StartsWith("reverse")
                            || arr[5].StartsWith("Received"))
                        {
                            Int32 i = 5;
                            for (i = 5; i < arr.Length; i++)
                            {
                                if (arr[i].EndsWith(":"))
                                    break;

                                r.EventCategory += arr[i] + " ";
                            }
                            if (i < arr.Length)
                            {
                                r.EventCategory = r.EventCategory.Trim();

                                r.CustomStr1 = arr[i].TrimEnd(':');
                                i++;

                                for (; i < arr.Length; i++)
                                {
                                    r.CustomStr3 += arr[i] + " ";
                                }
                                r.CustomStr3 = r.CustomStr3.Trim();
                            }
                        }
                        else if (arr[5].StartsWith("Did"))
                        {
                            r.CustomStr1 = arr[arr.Length - 1];
                        }
                        else if (arr[5].StartsWith("Connection") || arr[5].Contains("ession"))
                        {
                            if (arr.Length > 6)
                                r.EventCategory = arr[5] + " " + arr[6];
                            for (int i = 5; i < arr.Length - 1; i++)
                            {
                                if (arr[i] == "by")
                                {
                                    r.CustomStr3 = arr[i + 1].Trim();
                                    break;
                                }
                                else if (arr[i] == "user")
                                {
                                    r.UserName = arr[i + 1].Trim();
                                }
                            }
                        }
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("passwd(pam_unix)"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }

                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (arr[i] == "for")
                            {
                                if (i + 1 <= arr.Length)
                                {
                                    r.UserName = arr[i + 1];
                                    break;
                                }
                            }
                        }
                        if (arr.Length > 7)
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("su(pam_unix)") || r.CustomStr6.StartsWith("sudo(pam_unix)") || r.CustomStr6.StartsWith("login(pam_unix)"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }

                        bool end = false;
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (!end)
                            {
                                if ((i + 2) < arr.Length)
                                {
                                    if (arr[i + 2] == "by")
                                    {
                                        r.UserName = arr[i + 1];
                                        i++;
                                        end = true;
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                        if (r.UserName == "" || r.UserName == null)
                        {
                            for (Int32 i = 5; i < arr.Length; i++)
                            {
                                if (arr[i] == "user")
                                {
                                    if (i + 1 <= arr.Length)
                                    {
                                        r.UserName = arr[i + 1];
                                        break;
                                    }
                                }
                                else if (arr[i].Contains("ruser="))
                                {
                                    r.UserName = arr[i].Split('=')[1];
                                    break;
                                }
                            }
                        }
                        if (arr.Length > 7)
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("adduser") || r.CustomStr6.StartsWith("useradd"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                        if (r.UserName == "" || r.UserName == null)
                        {
                            for (Int32 i = 7; i < arr.Length; i++)
                            {
                                if (arr[i].Contains("name="))
                                {
                                    if (arr[6] == "user:")
                                        r.UserName = arr[i].Split('=')[1].Trim(',');
                                    if (arr[6] == "group:")
                                        r.CustomStr9 = arr[i].Split('=')[1].Trim(',');
                                }
                            }
                        }

                        if (arr.Length > 7)
                            r.EventCategory = arr[5] + " " + arr[6].Trim(':');
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("webmin[") || r.CustomStr6.StartsWith("portmap["))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                    }
                    else if (r.CustomStr6.StartsWith("userdel"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                        if (arr.Length > 9)
                        {
                            r.EventCategory = arr[5] + " " + arr[7] + " " + arr[9];
                            r.UserName = arr[6].Trim('\'').Remove(0, 1);
                            r.CustomStr9 = arr[8].Trim('\'').Remove(0, 1);
                        }
                        else if (arr.Length > 7)
                        {
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                            if (arr[6].Contains("user"))
                                r.UserName = arr[7].Trim('\'').Remove(0, 1);
                            else
                                r.CustomStr9 = arr[7].Trim('\'').Remove(0, 1);
                        }
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("<nateklog>"))
                    {
                        String[] natekArr = arr[5].Split(':');
                        r.CustomStr5 = natekArr[0];
                        String[] natekArr2 = natekArr[1].Split('|');
                        if (natekArr2.Length > 1)
                        {
                            r.UserName = natekArr2[1];
                            r.CustomStr9 = natekArr2[0];
                        }
                        else
                            r.UserName = natekArr[1];
                        r.EventCategory = natekArr[2];
                        for (int h = 6; h < arr.Length; h++)
                        {
                            r.EventCategory += " " + arr[h];
                            if (arr[h].Contains("."))
                                break;
                        }
                    }
                    else if (r.CustomStr6.StartsWith("usermod"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                        if (arr.Length > 10)
                        {
                            r.EventCategory = arr[5] + " " + arr[7] + " " + arr[8] + " " + arr[9];
                            if (r.EventCategory == "add to shadow group")
                            {
                                r.UserName = arr[6].Trim('\'').Remove(0, 1);
                                r.CustomStr9 = arr[10].Trim('\'').Remove(0, 1);
                            }
                        }
                        else if (arr.Length > 9)
                        {
                            r.EventCategory = arr[5] + " " + arr[7] + " " + arr[8];
                            if (r.EventCategory == "add to group")
                            {
                                r.UserName = arr[6].Trim('\'').Remove(0, 1);
                                r.CustomStr9 = arr[9].Trim('\'').Remove(0, 1);
                            }
                        }
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("sshd(pam_unix)"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }

                        bool end = false;
                        for (Int32 i = 5; i < arr.Length; i++)
                        {
                            if (!end)
                            {
                                if ((i + 2) < arr.Length)
                                {
                                    if (arr[i + 2] == "by")
                                    {
                                        r.UserName = arr[i + 1];
                                        i++;
                                        end = true;
                                    }
                                    else
                                        r.EventCategory += arr[i] + " ";
                                }
                            }
                        }

                        if (arr[5] == "authentication")
                        {
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                            for (int i = 5; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("user="))
                                {
                                    r.UserName = arr[i].Split('=')[1];
                                    break;
                                }
                            }
                        }
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("dovecot-auth"))
                    {
                        r.UserName = "";
                        Int32 i = 6;
                        for (i = 6; i < arr.Length; i++)
                        {
                            if (arr[i].EndsWith(";"))
                            {
                                r.EventCategory += arr[i].TrimEnd(';');
                                break;
                            }

                            r.EventCategory += arr[i] + " ";
                        }
                        for (; i < arr.Length; i++)
                        {
                            if (arr[i].StartsWith("user"))
                                break;
                        }
                        for (; i < arr.Length; i++)
                            r.UserName += arr[i] + " ";

                        r.UserName = r.UserName.Trim();

                        r.CustomStr6 = "dovecot-auth: pam_unix(dovecot:auth):";
                    }
                    else if (r.CustomStr6.StartsWith("remote(pam_unix)"))
                    {
                        String[] arrEv = arr[4].Split('[');
                        if (arrEv.Length > 1)
                        {
                            arrEv[1] = arrEv[1].TrimEnd(':', ']');
                            try
                            {
                                r.CustomStr7 = (arrEv[1]);
                            }
                            catch
                            {
                                r.CustomStr7 = "";
                            }
                        }
                        r.EventCategory = arr[5] + arr[6].Trim(';');

                        for (Int32 i = 7; i < arr.Length; i++)
                        {
                            if (arr[i].Split('=')[0] == "user")
                            {
                                try
                                {
                                    r.UserName = arr[i].Split('=')[1];
                                }
                                catch { }
                            }
                        }
                        r.CustomStr6 = "remote(pam_unix)";
                    }
                    else
                    {
                        isInformMessage = true;
                    }

                    if (remoteHost != "")
                        r.ComputerName = remoteHost;
                    else
                    {
                        String[] arrLocation = Dir.Split('\\');
                        if (arrLocation.Length > 1)
                            r.ComputerName = arrLocation[2];
                    }

                    r.LogName = LogName;
                    r.CustomStr10 = Environment.MachineName;
                    r.CustomStr1 = r.UserName;

                    //Nov 12 15:54:36 itim last message repeated 2 times                    
                    if (r.EventCategory != null)
                    {
                        if (r.EventCategory.Contains("Fail") || r.EventCategory.Contains("fail"))
                        {
                            r.EventType = "Failure";
                        }
                    }
                    else
                    {
                        r.EventType = "";
                    }
                    if (isInformMessage)
                    {
                        for (int j = 4; j < arr.Length; j++)
                        {
                            r.Description += arr[j] + " ";
                        }
                    }
                    else
                    {
                        for (int j = 5; j < arr.Length; j++)
                        {
                            r.Description += arr[j] + " ";
                        }
                    }
                    SetRecordData(r);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return false;
                }
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
                Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureRecorder In ParseFileNameRemote() -->> Enter The ParseFileNameRemote Function");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() --> Directory | " + Dir);
                    se.Connect();
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() --> Remote Host Already Connected ");
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep secure";
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> SSH command : " + command + " Result : " + stdOut);
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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> Secure File is Exist");
                            stdOut = "";
                            stdErr = "";
                            String commandRead;

                            if (readMethod == "nread")
                            {
                                commandRead = tempCustomVar1 + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
                            }

                            Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() --> Position : " + Position);
                            se.RunCommand(commandRead, ref stdOut, ref stdErr);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() --> Result : " + stdOut);
                            //Jun 26 04:34:14 SAMBASERVER sshd[20314]: Accepted password for natek from 172.16.1.14 port 55200 ssh2

                            StringReader srTest = new StringReader(stdOut);
                            se.Close();

                            if (String.IsNullOrEmpty(stdOut))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() --> Restart The Position");
                                Position = 0;
                            }
                        }
                        else
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> Secure File is NOT Exist");

                    }
                    else
                    {
                        if (fileExistControl)
                        {
                            FileName = Dir + "secure";
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                "  RedhatSecureRecorder In ParseFileNameRemote() -->>  Last File Is Null And Setted The File To " + FileName);

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
                Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureRecorder In ParseFileNameRemote() -->>  Exit The Function");
        }

        //protected override void ParseFileNameRemote()
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureRecorder In ParseFileNameRemote() -->> Enter The Function");

        //        String stdOut = "";
        //        String stdErr = "";
        //        String line = "";

        //        se = new SshExec(remoteHost, user);
        //        se.Password = password;

        //        if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

        //            se.Connect();
        //            se.SetTimeout(Int32.MaxValue);
        //            String command = "ls -lt " + Dir + " | grep ^-";
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> SSH command : " + command);
        //            se.RunCommand(command, ref stdOut, ref stdErr);
        //            StringReader sr = new StringReader(stdOut);

        //            ArrayList arrFileNameList = new ArrayList();

        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                String[] arr = line.Split(' ');
        //                if (arr[arr.Length - 1].Contains("secure") == true && arr[arr.Length - 1].Contains("gz") == false && arr[arr.Length - 1].Contains("bz2") == false)
        //                    arrFileNameList.Add(arr[arr.Length - 1]);
        //            }

        //            String[] dFileNameList = SortFiles(arrFileNameList);

        //            if (!String.IsNullOrEmpty(lastFile))
        //            {
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> LastFile Is = " + lastFile);

        //                bool bLastFileExist = false;

        //                for (int i = 0; i < dFileNameList.Length; i++)
        //                {
        //                    if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
        //                    {
        //                        bLastFileExist = true;
        //                        break;
        //                    }
        //                }

        //                if (bLastFileExist)
        //                {
        //                    stdOut = "";
        //                    stdErr = "";
        //                    String commandRead;

        //                    if (readMethod == "nread")
        //                    {
        //                        commandRead = tempCustomVar1 + " -n " + Position + "," + lineLimit + "p " + lastFile;
        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
        //                    }
        //                    else
        //                    {
        //                        commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
        //                        Log.Log(LogType.FILE, LogLevel.DEBUG, " RedhatSecureRecorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
        //                    }

        //                    se.RunCommand(commandRead, ref stdOut, ref stdErr);
        //                    se.Close();

        //                    StringReader srTest = new StringReader(stdOut);
        //                    Int64 posTest = Position;
        //                    String lineTest = "";
        //                    while ((lineTest = srTest.ReadLine()) != null)
        //                    {
        //                        if (lineTest.StartsWith("~?`Position"))
        //                        {
        //                            try
        //                            {
        //                                String[] arrIn = lineTest.Split('\t');
        //                                String[] arrPos = arrIn[0].Split(':');
        //                                String[] arrLast = arrIn[1].Split('`');
        //                                posTest = Convert.ToInt64(arrPos[1]); // de�i�ti Convert.ToUInt32s
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Log.Log(LogType.FILE, LogLevel.ERROR, " RedhatSecureRecorder In ParseFileNameRemote() In Try Catch -->> " + ex.Message);
        //                            }
        //                        }
        //                    }

        //                    if (posTest > Position)
        //                    {
        //                        Log.Log(LogType.FILE, LogLevel.INFORM,
        //                            " RedhatSecureRecorder In ParseFileNameRemote() -->> posTest > Position So Continiou With Same File ");

        //                        FileName = lastFile;

        //                        Log.Log(LogType.FILE, LogLevel.INFORM,
        //                            " RedhatSecureRecorder In ParseFileNameRemote() -->> LastFile Is " + lastFile);
        //                    }
        //                    else
        //                    {

        //                        Log.Log(LogType.FILE, LogLevel.INFORM,
        //                           " RedhatSecureRecorder In ParseFileNameRemote() -->> Finished Reading The File");

        //                        for (int i = 0; i < dFileNameList.Length; i++)
        //                        {
        //                            if (Dir + dFileNameList[i].ToString() == lastFile)
        //                            {
        //                                if (i + 1 == dFileNameList.Length)
        //                                {
        //                                    FileName = lastFile;
        //                                    Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                        " RedhatSecureRecorder In ParseFileNameRemote() -->> There Is No New File And Continiou With Same File And Waiting For a New Record " + FileName);
        //                                    break;
        //                                }
        //                                else
        //                                {
        //                                    FileName = Dir + dFileNameList[(i + 1)].ToString();
        //                                    Position = 0;
        //                                    lastFile = FileName;
        //                                    Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                        " RedhatSecureRecorder In ParseFileNameRemote() -->> Finished Reading The File And Continiou With New File " + FileName);
        //                                    break;

        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                    SetNextFile(dFileNameList, "ParseFileNameRemote()");

        //            }
        //            else
        //            {
        //                if (dFileNameList.Length > 0)
        //                {
        //                    FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
        //                    lastFile = FileName;
        //                    Position = 0;
        //                    Log.Log(LogType.FILE, LogLevel.INFORM,
        //                        "  RedhatSecureRecorder In ParseFileNameRemote() -->>  Last File Is Null And Setted The File To " + FileName);

        //                }
        //            }

        //            stdOut = "";
        //            stdErr = "";
        //            se.Close();
        //        }
        //        else
        //        {
        //            FileName = Dir;
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
        //        return;
        //    }

        //    Log.Log(LogType.FILE, LogLevel.INFORM, " RedhatSecureRecorder In ParseFileNameRemote() -->>  Exit The Function");
        //}

        public override void GetFiles()
        {
            try
            {
                Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  RedhatSecureRecorder In GetFiles() ");
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  RedhatSecureRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  RedhatSecureRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  RedhatSecureRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }
    }
}
