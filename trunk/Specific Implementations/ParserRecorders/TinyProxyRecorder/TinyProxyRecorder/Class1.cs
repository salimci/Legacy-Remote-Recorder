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
    public class TinyProxyRecorder : Parser
    {
        public struct Fields
        {
            public string ConnectionId,
                          EVENTCATEGORY,
                          EVENTTYPE,
                          DATETIME,
                          CUSTOMSTR1,
                          CUSTOMSTR3,
                          CUSTOMSTR4,
                          CUSTOMSTR5,
                          CUSTOMSTR6,
                          CUSTOMSTR7,
                          DESCRIPTION;

            public int CUSTOMINT1;
            public bool IsInsertData;
        }


        private Fields RecordFields;
        public TinyProxyRecorder()
            : base()
        {
            LogName = "TinyProxyRecorder";
            usingKeywords = false;
        }

        public override void Init()
        {
            RecordFields = new Fields();
            GetFiles();
        }

        public TinyProxyRecorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

            if (line == "")
                return true;

            String[] arr = SpaceSplit(line, false);
            ArrayList arrList = new ArrayList();
            for (int i = 0; i < arr.Length; i++)
            {
                if (!string.IsNullOrEmpty(arr[i]))
                {
                    arrList.Add(arr[i].Trim());
                }
            }

            try
            {
                Rec r = new Rec();

                for (int i = 0; i < arrList.Count; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ArrList : " + arrList[i]);
                }
                try
                {
                    r.EventCategory = arrList[0].ToString();
                    DateTime df = DateTime.Now;
                    DateTime dt;
                    string myDateTimeString = arrList[1] + (string)arrList[2] + "," + df.Year + "," + arrList[3];
                    dt = Convert.ToDateTime(myDateTimeString);
                    r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    r.CustomInt1 = Convert.ToInt32(arr[4].Replace("[", " ").Replace("]", " ").Replace(":", " ").Trim());
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "1 : " + ex.Message);
                }

                if (line.StartsWith("CONNECT"))
                {
                    for (int i = 0; i < arrList.Count; i++)
                    {
                        if (arrList[i].ToString().Contains("http:"))
                        {
                            r.CustomStr1 = arrList[5].ToString();
                            r.CustomStr5 = arrList[i].ToString();
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
                            r.EventType = arrList[i - 1].ToString();

                            if (arrList[i].ToString().Contains("[") || arrList[i].ToString().Contains("]"))
                            {
                                r.CustomStr3 = arrList[i].ToString().Replace("[", " ").Replace("]", " ");
                            }
                            r.CustomStr6 = Between(line, "(", ")");
                        }

                        if (i == arrList.Count - 1)
                        {
                            if (!arrList[i].ToString().Contains("[") || !arrList[i].ToString().Contains("]"))
                            {
                                if (!string.IsNullOrEmpty(r.EventType))
                                {
                                    r.CustomStr7 = arrList[i].ToString();//7-3     
                                }
                            }
                            //Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + arrList[i]);
                        }

                        if (!line.Contains("(") || !line.Contains(")"))
                        {
                            line = line.Replace('"', '|');
                            for (int jI = 0; jI < arrList.Count; jI++)
                            {
                                if (jI > 4)
                                {
                                    r.CustomStr1 += string.Concat(string.Format("{0} ", arrList[jI]));
                                }
                            }

                            String[] conArray = r.CustomStr1.Split('"');
                            r.CustomStr1 = conArray[0];
                            r.CustomStr4 = conArray[1];
                            r.CustomStr6 = conArray[2];
                        }

                        if (arrList[i].ToString().Contains("["))
                        {
                            if (arrList[i].ToString().Contains("."))
                            {
                                r.CustomStr1 = arrList[5].ToString();
                                r.CustomStr3 = arrList[i].ToString().Replace("[", "").Replace("]", "").Trim();
                                r.CustomStr6 = Between(line, "(", ")");
                            }
                        }
                    }
                }

                if (line.StartsWith("INFO"))
                {
                    try
                    {
                        for (int i = 0; i < arrList.Count; i++)
                        {
                            if (i > 4)
                            {
                                if (i > 4)
                                {
                                    r.CustomStr1 += string.Concat(string.Format("{0} ", arrList[i]));
                                }
                            }
                        }
                        try
                        {
                            string[] adf = r.CustomStr1.Split('"');
                            if (adf.Length > 0)
                            {
                                r.CustomStr1 = adf[0];
                                r.CustomStr4 = adf[1];
                            }
                        }
                        catch (Exception)
                        {
                            string[] d = r.CustomStr1.Split(' ');
                            for (int i = 0; i < d.Length; i++)
                            {
                                if (d[i].Contains("."))
                                {
                                    r.CustomStr4 += string.Concat(d[i]);
                                }
                                else
                                {
                                    r.CustomStr1 += string.Concat(d[i] + " ");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "Info line Exception : " + ex.Message);
                    }
                }


                r.ComputerName = remoteHost;
                r.LogName = LogName;
                r.Description = line;

                //if (line.Length > 900)
                //{
                //    r.Description = line.Substring(0, 899);
                //}
                //else
                //{
                //    r.Description = line;
                //}
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Record is sending now.");
                SetRecordData(r);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                return true;
            }
            return true;
        }

        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);


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

        protected override void ParseFileNameLocal()
        {

        }


        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " TinyProxyRecorder In ParseFileNameRemote() -->> Enter The Function");

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
                    String command = "ls -lt " + Dir + " | grep messages";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " TinyProxyRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    StringReader sr = new StringReader(stdOut);

                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] arr = line.Split(' ');
                        if (arr[arr.Length - 1].Contains("messages") == true && arr[arr.Length - 1].Contains("gz") == false && arr[arr.Length - 1].Contains("bz2") == false)
                            arrFileNameList.Add(arr[arr.Length - 1]);
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " TinyProxyRecorder In ParseFileNameRemote() -->> LastFile Is = " + lastFile);

                        bool bLastFileExist = false;

                        for (int i = 0; i < dFileNameList.Length; i++)
                        {
                            if ((base.Dir + dFileNameList[i]) == base.lastFile)
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
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " TinyProxyRecorder In ParseFileNameRemote() -->> commandRead For nread Is : " + commandRead);
                            }
                            else
                            {
                                commandRead = readMethod + " -n " + Position + "," + lineLimit + "p " + lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, " TinyProxyRecorder In ParseFileNameRemote() -->> commandRead For sed Is : " + commandRead);
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
                                        posTest = Convert.ToInt64(arrPos[1]); // değişti Convert.ToUInt32s
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.ERROR, " TinyProxyRecorder In ParseFileNameRemote() In Try Catch -->> " + ex.Message);
                                    }
                                }
                            }

                            if (posTest > Position)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " TinyProxyRecorder In ParseFileNameRemote() -->> posTest > Position So Continiou With Same File ");

                                FileName = lastFile;

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                    " TinyProxyRecorder In ParseFileNameRemote() -->> LastFile Is " + lastFile);
                            }
                            else
                            {

                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                   " TinyProxyRecorder In ParseFileNameRemote() -->> Finished Reading The File");

                                for (int i = 0; i < dFileNameList.Length; i++)
                                {
                                    if (Dir + dFileNameList[i].ToString() == lastFile)
                                    {
                                        if (i + 1 == dFileNameList.Length)
                                        {
                                            FileName = lastFile;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " TinyProxyRecorder In ParseFileNameRemote() -->> There Is No New File And Continiou With Same File And Waiting For a New Record " + FileName);
                                            break;
                                        }
                                        else
                                        {
                                            FileName = Dir + dFileNameList[(i + 1)].ToString();
                                            Position = 0;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " TinyProxyRecorder In ParseFileNameRemote() -->> Finished Reading The File And Continiou With New File " + FileName);
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

                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[dFileNameList.Length - 1].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                "  TinyProxyRecorder In ParseFileNameRemote() -->>  Last File Is Null And Setted The File To " + FileName);

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
                Log.Log(LogType.FILE, LogLevel.ERROR, "  TinyProxyRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  TinyProxyRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, " TinyProxyRecorder In ParseFileNameRemote() -->>  Exit The Function");
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
            catch (Exception ex)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  TinyProxyRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  TinyProxyRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  TinyProxyRecorder In GetFiles() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  TinyProxyRecorder In GetFiles() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }

        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];
            UInt64 indexnumber = Convert.ToUInt64(arrFileNames[arrFileNames.Count - 1].ToString().Split('.')[1]); ;

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                if (arrFileNames[i].ToString().Contains("."))
                {
                    dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[1]);
                    dFileNameList[i] = arrFileNames[i].ToString();
                }
                else
                {
                    dFileNumberList[i] = indexnumber;
                    dFileNameList[i] = arrFileNames[i].ToString();
                }
            }
            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
        }

        private void SetNextFile(String[] dFileNameList, string sFunction)
        {
            try
            {
                for (int i = 0; i < dFileNameList.Length; i++)
                {
                    UInt64 lFileNumber = Convert.ToUInt64(dFileNameList[i].ToString().Split('.')[1]);
                    UInt64 lLastFileNumber = Convert.ToUInt64(Path.GetFileName(lastFile).Split('.')[1]);

                    if (lFileNumber > lLastFileNumber)
                    {
                        FileName = Dir + dFileNameList[i].ToString();
                        Position = 0;
                        lastFile = FileName;

                        Log.Log(LogType.FILE, LogLevel.DEBUG, sFunction + " | LastFile Silinmis , Dosya Bulunamadı  Yeni File : " + FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ApacheAccessRecorder In SetNextFile() Exception Message -->> " + ex.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "  ApacheAccessRecorder In SetNextFile() Exception StackTrace -->> " + ex.StackTrace);
            }
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
