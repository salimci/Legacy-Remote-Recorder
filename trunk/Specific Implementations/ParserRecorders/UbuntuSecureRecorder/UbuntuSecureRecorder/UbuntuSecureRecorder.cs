using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;

namespace Parser
{
    public class UbuntuSecureRecorder : Parser
    {
        public UbuntuSecureRecorder()
            : base()
        {
            LogName = "UbuntuSecureRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }
        public UbuntuSecureRecorder(String fileName)
            : base(fileName)
        {
        }
        public override bool ParseSpecific(String line, bool dontSend)
        {
            bool isInformMassage = false;
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;
            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, true);
                if (arr.Length < 5)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count 10, found " + arr.Length + ", line: " + line + "!");                    
                    return true;
                }
                try
                {
                    Rec r = new Rec();
                    DateTime dt = DateTime.Parse(DateTime.Now.Year + " " + arr[0] + " " + arr[1] + " " + arr[2]);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                    r.SourceName = arr[3];
                    r.CustomStr6 = arr[4];
                    r.Description = "";
                    r.EventType = "Success";
                    r.EventCategory = null;                    
                    if (r.CustomStr6.StartsWith("sshd["))
                    {
                        //Accepted password for toor from 127.0.0.1 port 40649 ssh2
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
                        if(arr[5].StartsWith("Accepted")
                            || arr[5].StartsWith("Failed"))
                        {
                            r.EventCategory = arr[5] + " " + arr[6];
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
                        }
                        else if(arr[6].StartsWith("reverse")
                            || arr[6].StartsWith("Received"))
                        {
                            Int32 i = 6;
                            if (arr.Length > 6)
                                r.EventCategory = arr[6] + " " + arr[7];                            
                        }
                        else if(arr[6].StartsWith("Did"))
                        {
                            if (arr.Length > 6)
                                r.EventCategory = arr[6] + " " + arr[7];                         
                        }
                        else if (arr[6].StartsWith("Connection") || arr[6].StartsWith("Session") || arr[6].StartsWith("session"))
                        {
                            if (arr.Length > 6)
                                r.EventCategory = arr[6] + " " + arr[7];
                            for (int i = 6; i < arr.Length - 1; i++)
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
                        r.CustomStr6 = "sshd";
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
                    else if (r.CustomStr6.StartsWith("su[") || r.CustomStr6.StartsWith("sudo[") || r.CustomStr6.StartsWith("login["))
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
                        if (r.UserName == "" ||r.UserName == null)
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
                                    if(r.UserName != "" && r.UserName != null)
                                        break;
                                }
                                else if (arr[i].Contains("user="))
                                {
                                    r.UserName = arr[i].Split('=')[1];                                    
                                }
                            }
                        }                        
                        if(arr.Length > 7 && arr[5].StartsWith("pam_unix"))
                            r.EventCategory = arr[6] + " " + arr[7].Trim(';');
                        else if(arr[5] == "Successful")
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                        r.CustomStr6 = arrEv[0].ToString();
                    }
                    else if (r.CustomStr6.StartsWith("useradd"))
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

                        if (arr.Length > 6)
                            r.EventCategory = arr[5] + " " + arr[6].Trim(':');
                        r.CustomStr6 = arrEv[0].ToString();
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
                            if (arr[5].Contains("Remove"))
                            {
                                r.EventCategory = arr[5] + " " + arr[6];
                                if (arr[6].Contains("user"))
                                    r.UserName = arr[7].Trim('\'').Remove(0, 1);
                                else if (arr[6].Contains("group"))
                                    r.CustomStr9 = arr[7].Trim('\'').Remove(0, 1);                                
                            }
                            else
                            {
                                r.EventCategory = arr[5] + " " + arr[6];
                                r.UserName = arr[6].Trim('\'').Remove(0, 1);
                                r.CustomStr9 = arr[8].Trim('\'').Remove(0, 1);
                            }
                        }
                        else if (arr.Length > 7)
                        { //removed group `test3' owned by `test3' 
                            r.EventCategory = arr[5] + " " + arr[6].Trim(';');
                            if (arr[6].Contains("user"))
                                r.UserName = arr[7].Trim('\'').Remove(0, 1);
                            else if (arr[6].Contains("group"))
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
                            r.UserName = natekArr2[0];
                            r.CustomStr9 = natekArr2[1];
                        }
                        else
                            r.UserName = natekArr[1];
                        r.EventCategory = natekArr[2];
                        for (int h = 6; h < arr.Length; h++)
                        {
                            r.EventCategory += " " + arr[h];
                            if(arr[h].Contains("."))
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
                                if(arr[i].StartsWith("user="))
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
                        r.EventCategory = arr[6] + arr[7].Trim(';');

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
                        isInformMassage = true;
                        r.CustomStr6 = "";
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
                    if(r.EventCategory != null)
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
                    if (isInformMassage)
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
                    if (file.Contains(day))
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
            String stdOut = "";
            String stdErr = "";
            se = new SshExec(remoteHost, user);
            se.Password = password;
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                se.Connect();
                se.SetTimeout(Int32.MaxValue);
                String command = "ls -lt " + Dir + " | grep ^-";
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
                    if (arr[arr.Length - 1].Contains("secure"))
                    {
                        FileName = Dir + arr[arr.Length - 1];
                        break;
                    }
                }
                stdOut = "";
                stdErr = "";
                se.Close();
            }
            else
                FileName = Dir;
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
    }
}
