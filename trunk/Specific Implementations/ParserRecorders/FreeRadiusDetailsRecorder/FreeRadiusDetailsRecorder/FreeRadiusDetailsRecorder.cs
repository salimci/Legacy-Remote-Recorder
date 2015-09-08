using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using Parser;
using Log;
using CustomTools;
using SharpSSH.SharpSsh;
using System.Collections;

namespace Parser
{
    public class FreeRadiusDetailsRecorder : Parser
    {
        Fields fieldsLine;

        public FreeRadiusDetailsRecorder()
            : base()
        {
            LogName = "FreeRadiusDetails Recorder";
            usingKeywords = false;
            lineLimit = 50;
            
        }

        public FreeRadiusDetailsRecorder(String fileName)
            : base(fileName)
        {
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {       
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line" + line);
            if (String.IsNullOrEmpty(line))
            {   
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }
            
            if (!dontSend)
            {   
                try
                {
                        if (line.Contains("="))
                        {
                            string[] propertyandvalue = line.Split('=');
                            string property = propertyandvalue[0].Trim();
                            string value = propertyandvalue[1].Trim().Trim('"');
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Property & Value" + property + " " + value);
                            switch (property)
                            {
                                case "User-Name":
                                    fieldsLine.User_Name = value.Trim();
                                    break;
                                case "NAS-Port":
                                    fieldsLine.Nas_Port = Convert.ToInt32(value);
                                    break;
                                case "NAS-IP-Address":
                                    fieldsLine.Nas_Ip_Address = value;
                                    break;
                                case "Framed-IP-Address":
                                    fieldsLine.Framed_Ip_Address = value;
                                    break;
                                case "NAS-Identifier":
                                    fieldsLine.Nas_Identifier = value;
                                    break;
                                case "Airespace-Wlan-Id":
                                    fieldsLine.Airespace_Wlan_Id = Convert.ToInt32(value);
                                    break;
                                case "Acct-Session-Id":
                                    fieldsLine.Acct_Session_Id = value;
                                    break;
                                case "Acct-Authentic":
                                    fieldsLine.Acct_Authentic = value;
                                    break;
                                case "Tunnel-Type:0":
                                    fieldsLine.Tunnel_Type = value;
                                    break;
                                case "Tunnel-Medium-Type:0":
                                    fieldsLine.Tunnel_Medium_Type = value;
                                    break;
                                case "Tunnel-Private-Group-Id:0":
                                    fieldsLine.Tunnel_Private_Group_Id = Convert.ToInt32(value);
                                    break;
                                case "Acct-Status-Type":
                                    fieldsLine.Acct_Status_Type = value;
                                    break;
                                case "Acct-Input-Octets":
                                    fieldsLine.Acct_Input_Octets = Convert.ToInt32(value);
                                    break;
                                case "Acct-Output-Octets":
                                    fieldsLine.Acct_Output_Octets = Convert.ToInt64(value);
                                    break;
                                case "Acct-Input-Packets":
                                    fieldsLine.Acct_Input_Packets = Convert.ToInt32(value);
                                    break;
                                case "Acct-Output-Packets":
                                    fieldsLine.Acct_Output_Packets = Convert.ToInt32(value);
                                    break;
                                case "Acct-Session-Time":
                                    fieldsLine.Acct_Session_Time = Convert.ToInt32(value);
                                    break;
                                case "Acct-Delay-Time":
                                    fieldsLine.Acct_Delay_Time = Convert.ToInt32(value);
                                    break;
                                case "Calling-Station-Id":
                                    fieldsLine.Calling_Station_Id = value;
                                    break;
                                case "Called-Station-Id":
                                    fieldsLine.Called_Station_Id = value;
                                    break;
                                case "Acct-Unique-Session-Id":
                                    fieldsLine.Acct_Unique_Session_Id = value;
                                    break;
                                case "Realm":
                                    fieldsLine.Realm = value;
                                    break;
                                case "Timestamp":
                                    fieldsLine.Timestamp = Convert.ToInt64(value);
                                    break;
                                case "Request-Authenticator":
                                    fieldsLine.Request_Authenticator = value;
                                    break;
                            }
                        }
                        else 
                        {
                            if (fieldsLine.Date == "")
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Date is empty");
                                string tempdate = line;
                                string[] tempdatepart = tempdate.Split(' ');

                                int count = 0;

                                for (int i = 0; i < tempdatepart.Length; i++)
                                {
                                    if (tempdatepart[i] == "")
                                    {
                                        count++;
                                    }
                                }

                                string[] datepart = new string[tempdatepart.Length - count];
                                int h = 0;
                                for (int k = 0; k < tempdatepart.Length; k++)
                                {
                                    if (tempdatepart[k] != "")
                                    {
                                        datepart[h] = tempdatepart[k];
                                        h++;
                                    }
                                }

                                string _date = datepart[2] + "." + datepart[1] + "." + datepart[4] + " " + datepart[3];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, _date);
                                string permanentDate = Convert.ToDateTime(_date).ToString("yyyy/MM/dd HH:mm:ss");
                                Log.Log(LogType.FILE, LogLevel.DEBUG, permanentDate);
                                fieldsLine.Date = permanentDate;
                            }
                            else 
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Date different from empty");
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Acct_Status_Type Is : " + fieldsLine.Acct_Status_Type.ToString());
                                if (fieldsLine.Acct_Status_Type == "Start" || fieldsLine.Acct_Status_Type == "Stop")
                                {
                                    CustomBase.Rec rec = new CustomBase.Rec();
                                    rec.LogName = LogName;
                                    rec.EventType = fieldsLine.Acct_Status_Type;
                                    rec.CustomInt6 = fieldsLine.Timestamp;
                                    rec.UserName = fieldsLine.User_Name;
                                    rec.Datetime = fieldsLine.Date;
                                    rec.SourceName = fieldsLine.Nas_Ip_Address;
                                    rec.ComputerName = fieldsLine.Acct_Session_Id.Split('/')[1].Trim();
                                    rec.CustomStr1 = fieldsLine.Framed_Ip_Address;
                                    rec.CustomStr2 = fieldsLine.User_Name.Split('@')[1];
                                    SetRecordData(rec);
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Send Data");
                                }

                                fieldsLine.ClearObject();

                                string tempdate = line;
                                string[] tempdatepart = tempdate.Split(' ');

                                int count = 0;

                                for (int i = 0; i < tempdatepart.Length; i++)
                                {
                                    if (tempdatepart[i] == "")
                                    {
                                        count++;
                                    }
                                }

                                string[] datepart = new string[tempdatepart.Length - count];
                                int h = 0;
                                for (int k = 0; k < tempdatepart.Length; k++)
                                {
                                    if (tempdatepart[k] != "")
                                    {
                                        datepart[h] = tempdatepart[k];
                                        h++;
                                    }
                                }

                                string _date = datepart[2] + "." + datepart[1] + "." + datepart[4] + " " + datepart[3];
                                Log.Log(LogType.FILE, LogLevel.DEBUG, _date);
                                string permanentDate = Convert.ToDateTime(_date).ToString("yyyy/MM/dd HH:mm:ss");
                                Log.Log(LogType.FILE, LogLevel.DEBUG, permanentDate);
                                fieldsLine.Date = permanentDate;
                            }
                        }
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

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {   
            dayChangeTimer.Enabled = false;

            if (Today.Day != DateTime.Now.Day || FileName == null)
            {
                String oldFile = FileName;
                DateTime oldTime = Today;
                Int64 oldPosition = Position;
                String oldLastLine = lastLine;
                Today = DateTime.Now;
                Stop();
                ParseFileName();
                if (oldFile == FileName)
                {
                    if (FileName == null)
                    {
                        Today = oldTime;
                        Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> Cannot Find File To Parse, Please Check Your Path!");
                    }
                    else
                    {
                        Today = DateTime.Now;
                        Position = oldPosition;
                        lastLine = oldLastLine;
                        Start();
                    }
                }
                else
                {
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> Day Changed, New File Is, " + FileName);
                }
            }
            else/* if (remoteHost == "")*/
            {
                String fileLast = FileName;
                ParseFileName();
                if (FileName != fileLast)
                {
                    Stop();
                    Position = 0;
                    lastLine = "";
                    FileName = fileLast;
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> File changed, new file is, " + FileName);
                }
            }

            dayChangeTimer.Enabled = true;
        }

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> Enter The Function");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {   
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep detail";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    StringReader sr = new StringReader(stdOut);

                    ArrayList arrFileNameList = new ArrayList();
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].StartsWith("detail") == true && arr[arr.Length - 1].Contains("gz") == false && arr[arr.Length - 1].Contains("bz2") == false)
                            arrFileNameList.Add(arr[arr.Length - 1]);
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);

                    if (!String.IsNullOrEmpty(lastFile))
                    {   
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeRadiusDetailsRecorder In ParseFileNameRemote() -->> LastFile Is = " + lastFile);
                        
                        bool bLastFileExist = false;
                        
                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                break;
                            }
                        }

                        if (bLastFileExist)
                        {
                            stdOut = "";
                            stdErr = "";
                            String commandRead;

                            if (readMethod == "nread")
                            {
                                commandRead = tempCustomVar1 + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
                            }

                            se.RunCommand(commandRead, ref stdOut, ref stdErr);
                            se.Close();

                            StringReader srTest = new StringReader(stdOut);
                            Int64 posTest = Position;
                            String lineTest = "";
                            while ((lineTest = srTest.ReadLine()) != null)
                            {
                                if (lineTest.StartsWith("~?`Position"))
                                {
                                    try
                                    {
                                        String[] arrIn = lineTest.Split('\t');
                                        String[] arrPos = arrIn[0].Split(':');
                                        String[] arrLast = arrIn[1].Split('`');
                                        posTest = Convert.ToInt64(arrPos[1]); // deðiþti Convert.ToUInt32s
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() In Try Catch -->> " + ex.Message);
                                    }
                                }
                            }
                            
                            if (posTest > Position)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> posTest > Position So Continiou With Same File ");

                                FileName = lastFile;

                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> LastFile Is " + lastFile);
                            }
                            else
                            {

                                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Finished Reading The File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " ParseFileNameRemote() -->> There Is No New File And Continiou With Same File And Waiting For a New Record " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameRemote() -->> Finished Reading The File And Continiou With New File " + FileName);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                            SetNextFile(dFileNameList, "ParseFileNameRemote()");
                    }
                    else
                    {
                        bool controlofLastFileIsNull = false;

                        if (String.IsNullOrEmpty(lastFile))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> Last File Is Null");
                            controlofLastFileIsNull = true;
                        }

                        if (dFileNameList.Length > 0)
                        {
                            if (controlofLastFileIsNull)
                            {
                                FileName = Dir + dFileNameList[0].ToString();
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "GenuagateRecorder In ParseFileNameRemote() -->> LastName Is Null and FileName Is Setted To : " + FileName);
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> In The Log Location There Is No Log File");
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " FreeRadiusDetailsRecorder In ParseFileNameRemote() -->>  Exit The Function");
        }   
        
        public override void GetFiles()
        {
            try
            {
                Dir = GetLocation();
                GetRegistry();
                Today = DateTime.Now;
                fieldsLine = new Fields();
                ParseFileName();
            }
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('-')[1]);
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
        }

        private void SetNextFile(String[] dFileNameList, string sFunction)
        {   
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeRadiusDetailsRecorder In SetNextFile() -->> Enter The Function");
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('-')[1]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('-')[1]);
                    
                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, sFunction + " | LastFile Silinmis , Dosya Bulunamadý  Yeni File : " + FileName);
                    }
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeRadiusDetailsRecorder In SetNextFile() -->> Exit The Function");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In SetNextFile() Exception Message -->> " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeRadiusDetailsRecorder In SetNextFile() Exception StackTrace -->> " + ex.StackTrace);
            }
        }
    }
}
