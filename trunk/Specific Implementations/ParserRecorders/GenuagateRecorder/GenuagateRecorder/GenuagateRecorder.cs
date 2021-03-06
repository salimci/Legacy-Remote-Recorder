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
using System.ServiceProcess;

namespace Parser
{   
    public class GenuagateRecorder : Parser
    {    
        public GenuagateRecorder()
                : base()
        {
            LogName = "GenuagateRecorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public GenuagateRecorder(String fileName)
            : base(fileName)
        {
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
                        Log.Log(LogType.FILE, LogLevel.ERROR, "  GenuaGateRecorder In dayChangeTimer_Elapsed -->> Cannot find file to parse, please check your path!");
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
                    Log.Log(LogType.FILE, LogLevel.INFORM, " GenuaGateRecorder In dayChangeTimer_Elapsed -->> Day Changed, New File Is, " + FileName);
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
                    Log.Log(LogType.FILE, LogLevel.INFORM, " GenuaGateRecorder In dayChangeTimer_Elapsed -->> File Changed, New File Is : " + FileName);
                }
            }

            dayChangeTimer.Enabled = true;
        }

        static String DatePart(String w3)
        {
            //String dType = arr[3] + " " + arr[4] + " " + arr[5];
            String mounth;

            switch (w3)
            {
                case "Jan":
                    mounth = "01";
                    break;
                case "Feb":
                    mounth = "02";
                    break;
                case "Mar":
                    mounth = "03";
                    break;
                case "Apr":
                    mounth = "04";
                    break;
                case "May":
                    mounth = "05";
                    break;
                case "Jun":
                    mounth = "06";
                    break;
                case "Jul":
                    mounth = "07";
                    break;
                case "Aug":
                    mounth = "08";
                    break;
                case "Sep":
                    mounth = "09";
                    break;
                case "Oct":
                    mounth = "10";
                    break;
                case "Nov":
                    mounth = "11";
                    break;
                case "Dec":
                    mounth = "12";
                    break;
                default:
                    mounth = w3;
                    break;
            }
            return mounth;
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Enter the Function");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Line Is : " + line);
                if (string.IsNullOrEmpty(line))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Line Is Null Or Empty");
                    return true;
                }

                if (!dontSend)
                {
                    CustomBase.Rec rec = new CustomBase.Rec();
                    string[] arr = SpaceSplit(line, true);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Start preparing record");

                    string tempfilename = lastFile;
                    string[] tempfilenamearray = tempfilename.Split('\\');
                    string file_name = tempfilenamearray[tempfilenamearray.Length - 1];
                    string tempdate = file_name.Split('.')[0];
                    string date = tempdate.Split('_')[1];
                    string dateYear = date.Substring(0, 4);
                    string dateMount = DatePart(arr[0]);
                    string dateDay = arr[1];
                    string dateTime = arr[2];
                    string permanentDateTime = dateYear + "." + dateMount + "." + dateDay + " " + dateTime;

                    rec.LogName = "Genuagate Recorder";
                    rec.Datetime = permanentDateTime;
                    rec.EventCategory = arr[8];

                    Dictionary<String, String> dictTemp = new Dictionary<String, String>();

                    switch (rec.EventCategory)
                    {
                        case "request":
                        case "accept":
                        case "connect":
                        case "disconnect":
                            {
                                for (Int32 i = 9; i < arr.Length; i++)
                                {
                                    String[] arrTemp = arr[i].Split('=');
                                    if (arrTemp.Length > 1)
                                    {
                                        dictTemp.Add(arrTemp[0], arrTemp[1]);
                                    }
                                }

                                try
                                {
                                    rec.CustomStr6 = dictTemp["laddr"];
                                }
                                catch
                                {
                                    rec.CustomStr6 = "";
                                }
                                try
                                {
                                    rec.CustomInt1 = Convert.ToInt32(dictTemp["lport"]);
                                }
                                catch
                                {
                                    rec.CustomInt1 = -1;
                                }
                                try
                                {
                                    rec.CustomStr2 = dictTemp["baddr"];
                                }
                                catch
                                {
                                    rec.CustomStr2 = "";
                                }
                                try
                                {
                                    rec.CustomInt2 = Convert.ToInt32(dictTemp["bport"]);
                                }
                                catch
                                {
                                    rec.CustomInt2 = -1;
                                }
                                try
                                {
                                    rec.CustomStr3 = dictTemp["caddr"];
                                }
                                catch
                                {
                                    rec.CustomStr3 = "";
                                }
                                try
                                {
                                    rec.CustomInt3 = Convert.ToInt32(dictTemp["cport"]);
                                }
                                catch
                                {
                                    rec.CustomInt3 = -1;
                                }
                                try
                                {
                                    rec.CustomStr4 = dictTemp["saddr"];
                                }
                                catch
                                {
                                    rec.CustomStr4 = "";
                                }
                                try
                                {
                                    rec.CustomInt4 = Convert.ToInt32(dictTemp["sport"]);
                                }
                                catch
                                {
                                    rec.CustomInt4 = -1;
                                }
                                try
                                {
                                    rec.Description = dictTemp["url"];
                                }
                                catch
                                {
                                    rec.Description = "";
                                }
                                try
                                {
                                    rec.CustomStr5 = dictTemp["duration"];
                                }
                                catch
                                {
                                    rec.CustomStr5 = "";
                                }
                                try
                                {
                                    rec.CustomStr1 = dictTemp["rnum"];
                                }
                                catch
                                {
                                    rec.CustomStr1 = "";
                                }
                                try
                                {
                                    rec.CustomStr7 = dictTemp["status"];
                                }
                                catch
                                {
                                    rec.CustomStr7 = "";
                                }
                                try
                                {
                                    rec.CustomStr8 = dictTemp["type"];
                                }
                                catch
                                {
                                    rec.CustomStr8 = "";
                                }

                                dictTemp.Clear();
                            } break;
                        case "ACCESS":
                            {
                                rec.EventCategory += " " + arr[9];

                                rec.CustomStr10 = "";

                                Int32 i = 12;
                                for (i = 12; i < arr.Length; i++)
                                {
                                    if (Char.IsDigit(arr[i], 0))
                                    {
                                        break;
                                    }
                                    rec.CustomStr10 += arr[i] + " ";
                                }
                                rec.CustomStr10 = rec.CustomStr10.Trim();

                                for (; i < arr.Length; i++)
                                {
                                    if (arr[i].Contains("from"))
                                        break;
                                }
                                i++;

                                String[] arrTemp = arr[i].Split(':');
                                rec.CustomStr3 = arrTemp[0];
                                try
                                {
                                    rec.CustomInt3 = Convert.ToInt32(arrTemp[1]);
                                }
                                catch
                                {
                                }
                                i += 2;

                                arrTemp = arr[i].Split(':');
                                rec.CustomStr2 = arrTemp[0];
                                try
                                {
                                    rec.CustomInt2 = Convert.ToInt32(arrTemp[1]);
                                }
                                catch
                                {
                                }

                            } break;
                    };

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Finish Preparing Record");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Start Sending Data");
                    SetRecordData(rec);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In ParseSpecific() -->> Finish Sending Data");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GenuaGateRecorder In ParseSpecific() -->> Line Is : " + line);
                Log.LogTimed(LogType.FILE, LogLevel.ERROR, " GenuaGateRecorder In ParseSpecific() In Try-Catch -->> " + ex.ToString());
                return false;
            }
        }
        
        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " GenuagateRecorder In ParseFileNameRemote() -->> Enter The Function");

                String stdOut = "";
                String stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> Home Directory is" + Dir);
                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep acctd_";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> SSH command -1 : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    StringReader sr = new StringReader(stdOut);

                    ArrayList arrFileNameList = new ArrayList();
                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].StartsWith("acctd_") == true)
                            arrFileNameList.Add(arr[arr.Length - 1]);
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> Last File Is : " + lastFile);

                    if (!String.IsNullOrEmpty(lastFile))
                    {   
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> LastFile  = " + lastFile);

                        bool bLastFileExist = false;
                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i].ToString()) == base.lastFile)
                            {
                                bLastFileExist = true;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> Last File Is Exist");
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
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuagateRecorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
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
                                        Log.Log(LogType.FILE, LogLevel.ERROR, "  GenuagateRecorder In ParseFileNameRemote() In Try Catch -->> " + ex.Message);
                                    }
                                }
                            }

                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " GenuagateRecorder In ParseFileNameRemote() -->> posTest and position --------------------" + posTest.ToString() + " ----- " + Position.ToString());


                            if (posTest > Position)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " GenuagateRecorder In ParseFileNameRemote() -->> posTest > Position So Continiou With Same File ");

                                FileName = lastFile;

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " GenuagateRecorder In ParseFileNameRemote() -->> LastFile Is " + lastFile);
                            }
                            else
                            {

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                   " GenuagateRecorder In ParseFileNameRemote() -->> Finished Reading The File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " GenuagateRecorder In ParseFileNameRemote() -->> There Is No New File And Continiou With Same File And Waiting For a New Record " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " GenuagateRecorder In ParseFileNameRemote() -->> Finished Reading The File And Continiou With New File " + FileName);
                                            break;

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                 "  GenuagateRecorder In ParseFileNameRemote() -->> LastFile in not exist in Dİrectory: " + lastFile);
                            if (dFileNameList.Length > 0)
                            {
                                FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();// son oluşan dosya atandı.
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "  GenuagateRecorder In ParseFileNameRemote() -->> LastFile Is Is Setted To : " + FileName);
                            }
                        }
                    }
                    else
                    {
                        if (dFileNameList.Length > 0)
                        {
                                FileName = Dir + dFileNameList[0];// ilk oluşan dosyadan başlıyor.
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                    "  GenuagateRecorder In ParseFileNameRemote() -->> LastName Is Null and FileName Is Setted To : " + FileName);
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
                Log.Log(LogType.FILE, LogLevel.ERROR, "  GenuagateRecorder In ParseFileNameRemote() -->> " + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  GenuagateRecorder In ParseFileNameRemote() -->> " + exp.StackTrace);
                return;
            }
            Log.Log(LogType.FILE, LogLevel.INFORM, " GenuagateRecorder In ParseFileNameRemote() -->> Exit The Function");
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  GenuagateRecorder In GetFiles() -->> Error While Getting Files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  " + e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  GenuagateRecorder In GetFiles() -->> Error While Getting Files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  " + e.StackTrace);
                }
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                string tempfilename = arrFileNames[i].ToString().Split('.')[0];
                UInt64 filenumber =Convert.ToUInt64(tempfilename.Split('_')[1]);
                dFileNumberList[i] = filenumber; 
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);

            Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> Sýralanmýþ dosya isimleri yazýlýyor.");
            for (int i = 0; i < dFileNameList.Length; i++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SortFiles() -->> " + dFileNameList[i]);
            }

            return dFileNameList;
        }

        private void SetNextFile(String[] dFileNameList, string sFunction)
        {
            bool fileFound = false;
            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {   
                    string tempfilename = dFileNameList[i].ToString().Split('.')[0];
                    UInt64 filenumber = Convert.ToUInt64(tempfilename.Split('_')[1]);
                    UInt64 lFileNumber = filenumber;

                    string tempfilename2 = Path.GetFileName(lastFile).Split('.')[0];
                    UInt64 filenumber2 = Convert.ToUInt64(tempfilename2.Split('_')[1]);
                    UInt64 lLastFileNumber = filenumber2;

                    if (lFileNumber > lLastFileNumber)
                    {   
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;

                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                            "  GenuaGateRecorder In SetNextFile -->> " + sFunction + " | LastFile Was Deleted , File Not Found,  New File Is : " + FileName);
                        fileFound = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG,
                     "  GenuaGateRecorder In SetNextFile -->> " + ex.Message);
            }
            if(!fileFound)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  GenuaGateRecorder In SetNextFile -->> Last File Is Deleted and Waiting For a New File");
            }
        }
    }
}
