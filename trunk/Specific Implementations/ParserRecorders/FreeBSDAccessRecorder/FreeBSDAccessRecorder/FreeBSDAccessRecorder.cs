//Name: FreeBSDAccessRecorder
//Writer: Ali Yıldırım
//Date: 07/08/2010

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
    public class FreeBSDAccessRecorder : Parser
    {
        public FreeBSDAccessRecorder() : base()
        {
            LogName = "FreeBSDAccessRecorder";
            usingKeywords = false;
            lineLimit = 1000;
        }

        public FreeBSDAccessRecorder(string fileName) : base(fileName)
        { }
        
        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> Parsing Specific line: " + line);
            if (string.IsNullOrEmpty(line))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is Null Or Empty");
                return true;
            }

            if (!dontSend)
            {
                //78.161.53.88 - - [28/Jun/2010:22:50:52 +0300] "GET /assets/images/hattusas1.jpg HTTP/1.1" 200 40410 "http://twinningroadtransport.ubak.gov.tr/index.php?id=11" "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; GTB6.5; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C)"
                //94.54.59.204 - - [25/May/2010:22:14:12 +0300] "GET /assets/images/ataturk3.gif HTTP/1.1" 200 36005 "http://twinningroadtransport.ubak.gov.tr/index.php?id=15" "Mozilla/5.0 (Windows; U; Windows NT 6.1; tr; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3"
                //207.46.13.93 - - [17/Jul/2010:07:59:28 +0300] "GET /robots.txt HTTP/1.1" 404 306 "-" "msnbot/2.0b (+http://search.msn.com/msnbot.htm)"
                //95.66.0.12 - - [17/Jul/2010:06:13:57 +0300] "GET /index.php?id='7 HTTP/1.1" 302 5 "-" "-"

                char[] ayrac = {' '};
                String[] fields = line.Split(ayrac, StringSplitOptions.RemoveEmptyEntries);
                
                try
                {
                    Rec r = new Rec();

                        r.ComputerName = fields[0];
                        string[] arrDate = fields[3].TrimStart('[').Split(':', '/');
                        DateTime date = Convert.ToDateTime(arrDate[0] + "/" + arrDate[1] + "/" + arrDate[2]);
                        r.Datetime = (date.Day + "/" + date.Month + "/" + date.Year + " " + arrDate[3] + ":" + arrDate[4] + ":" + arrDate[5]).ToString();
                        r.EventType = fields[5].TrimStart('"');

                        if (fields.Length >= 6)
                        {
                            r.CustomStr4 = fields[6];
                            r.CustomStr1 = fields[7].TrimEnd('"');
                            r.CustomInt1 = ObjectToInt32(fields[8], 0);
                            r.CustomInt2 = ObjectToInt32(fields[9], 0);
                            r.CustomStr2 = fields[10].Trim('"');

                            r.Description = "";
                            if (fields.Length > 11)
                            {
                                r.CustomStr3 = fields[11].Trim('"');

                                for (int i = 12; i < fields.Length; i++)
                                {
                                    r.Description += fields[i].Trim('"', '(', ')') + " ";
                                }
                                r.Description.Trim();
                            }
                            else
                            {

                                r.CustomStr3 = fields[11].Trim('"');
                            }
                        }
                        else
                        {
                            r.Description = "";

                            for (int i = 0; i < fields.Length; i++)
                            {
                                r.Description += " " + fields[i];
                            }
                            r.Description.Trim();
                        }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Finish Record Data");
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return true;
                }
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> ParsingSpesific Ends");

            return true;
        }

        protected override void ParseFileNameRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " FreeBSDAccessRecorder In ParseFileNameRemote() -->> Enter The Function ");

                string stdOut = "";
                string stdErr = "";
                String line = "";

                se = new SshExec(remoteHost, user);
                se.Password = password;

                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Home Directory | " + Dir);

                    se.Connect();
                    se.SetTimeout(Int32.MaxValue);
                    String command = "ls -lt " + Dir + " | grep administrative";
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDAccessRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDAccessRecorder In ParseFileNameRemote() -->> Dosya ismi okundu: " + line);
                        String[] arr = line.Split(' ');
                        if (arr.Length >= 9 && arr[9].StartsWith("administrative") == true)
                        {
                            arrFileNameList.Add(arr);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDAccessRecorder In ParseFileNameRemote() -->> Okunan Dosya ismi arrayFileNameList'e atıldı. ");
                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDAccessRecorder In ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı. ");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDAccessRecorder In ParseFileNameRemote() -->> LastFile is not null: " + lastFile);

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
                            //Son okunan dosya o directory'de bulundu.
                        }
                        else
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            Position = 0;
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " | LastFile Silinmis , Dosya Bulunamadı  Yeni File : " + FileName);
                        }
                    }
                    else
                    {
                        if (dFileNameList.Length > 0)
                        {
                            FileName = Dir + dFileNameList[0].ToString();
                            lastFile = FileName;
                            Position = 0;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDAccessRecorder In ParseFileNameRemote() -->> LastName Is Null and FileName Is Setted To : " + FileName);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  FreeBSDAccessRecorder In ParseFileNameRemote() -->> In The Log Location There Is No Log File");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  FreeBSDAccessRecorder In ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "FreeBSDAccessRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "FreeBSDAccessRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, "FreeBSDAccessRecorder In ParseFileNameRemote() -->>  Exit The Function");
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeBSDAccessRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeBSDAccessRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeBSDAccessRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeBSDAccessRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
            }
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (Today.Day != DateTime.Now.Day || FileName == null)
            {
                String oldFile = FileName;
                DateTime oldTime = Today;
                Today = DateTime.Now;
                Stop();
                Position = 0;
                lastLine = "";
                ParseFileName();
                if (FileName == null)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find file to parse, please check your path!");
                    Today = oldTime;
                }
                else
                {
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Day changed, new file is, " + FileName);
                }
            }
            dayChangeTimer.Start();
        }

        public override void Start()
        {
            base.Start();
        }
        
        private string[] SortFiles(ArrayList arrFileNames)
        {
            UInt64[] dFileNumberList = new UInt64[arrFileNames.Count];
            String[] dFileNameList = new String[arrFileNames.Count];

            for (int i = 0; i < arrFileNames.Count; i++)
            {
                if (arrFileNames[i].ToString().Split('.').Length <= 3)
                {
                    dFileNumberList[0] = 0;
                    dFileNameList[0] = arrFileNames[i].ToString();
                    arrFileNames.RemoveAt(i);
                    break;
                }
            }

            for (int i = 1; i < arrFileNames.Count; i++)
            {
                dFileNumberList[i] = Convert.ToUInt64(arrFileNames[i].ToString().Split('.')[1]);
                dFileNameList[i] = arrFileNames[i].ToString();
            }

            Array.Sort(dFileNumberList, dFileNameList);
            return dFileNameList;
        }

        public bool lineBulundu = false;
        public int lineCount = 0;
        /// <summary>
        /// Aldığı file içerisinde lastLine'ı ve varsa okunacak line'ı bulur.
        /// </summary>
        /// <param name="lastFile"> Belirli Pozisyondaki LastLine'ı Aradığımız Dosya İsmi.</param>
        /// <returns></returns>
        private bool CheckPositionInOldFiles(string lastFile)
        {
            string stdOut = "";
            string stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            if (readMethod == "nread")
            {
                commandRead = "nread" + " -n " + Position + "," + 2 + "p " + lastFile;
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDAccessRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);

                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDAccessRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

                stReader = new StringReader(stdOut);

                //lastFile'dan line ve posizton okundu ve şimdi test ediliyor. 
                while ((line = stReader.ReadLine()) != null)
                {
                    if (lastLine == line)
                    {
                        lineBulundu = true;
                        continue;
                    }

                    if (line.StartsWith("~?`Position"))
                    {
                        continue;
                    }

                    lineCount++;
                }
            }
            else
            {
                commandRead = "sed" + " -n " + Position + "," + (Position + 1) + "p " + lastFile;
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDAccessRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);

                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDAccessRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

                stReader = new StringReader(stdOut);

                while ((line = stReader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    if (line == lastLine)
                    {
                        lineBulundu = true;
                        continue;
                    }
                    lineCount++;
                }
            }

            if (lineBulundu == true)
                return true;
            else
                return false;
        }

        private int ObjectToInt32(string sObject, int iReturn)
        {
            try
            {
                return Convert.ToInt32(sObject);
            }
            catch
            {
                return iReturn;
            }
        }
    }
}
