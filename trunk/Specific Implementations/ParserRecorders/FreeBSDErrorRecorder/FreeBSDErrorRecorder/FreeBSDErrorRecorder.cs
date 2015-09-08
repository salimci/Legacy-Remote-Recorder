//Name: FreeBSDErrorRecorder
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
    public class FreeBSDErrorRecorder : Parser
    {
        public FreeBSDErrorRecorder()
            : base()
        {
            LogName = "FreeBSDErrorRecorder";
            usingKeywords = false;
            lineLimit = 1000;
        }

        public FreeBSDErrorRecorder(string fileName)
            : base(fileName)
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
                //[Tue May 25 17:31:23 2010] [notice] Apache/1.3.37 (Unix) configured -- resuming normal operations
                //[Tue May 25 17:34:09 2010] [notice] SIGHUP received.  Attempting to restart
                //[Tue May 25 17:31:23 2010] [notice] Accept mutex: flock (Default: flock)
                //[Tue May 25 17:42:15 2010] [error] [client 67.218.116.132] File does not exist: /usr/local/www/data/aveiro/robots.txt

                char[] ayrac = { ' ' };
                String[] fields = line.Split(ayrac, StringSplitOptions.RemoveEmptyEntries);
                Rec r = new Rec();

                try
                {
                    DateTime date = Convert.ToDateTime(fields[2] + "/" + fields[1] + "/" + fields[4].Trim(']'));
                    r.Datetime = (date.Day + "/" + date.Month + "/" + date.Year + " " + fields[3]).ToString();
                    r.CustomStr1 = fields[5].TrimStart('[').TrimEnd(']');

                    if (fields.Length >= 6)
                    {
                        if (r.CustomStr1 == "notice")
                        {
                            r.Description = "";
                            for (int i = 6; i < fields.Length; i++)
                            {
                                r.Description += fields[i].Trim();
                            }
                            r.CustomStr4 = "";
                            r.ComputerName = "";
                        }
                        else if (r.CustomStr1 == "error")
                        {
                            r.ComputerName = fields[7].TrimEnd(']');
                            r.Description = "";
                            if (fields.Length >= 8)
                            {
                                for (int i = 8; i < fields.Length; i++)
                                {
                                    if (!fields[i].StartsWith("/"))
                                        r.Description += fields[i] + " ";
                                    r.Description.Trim();
                                    if (fields[i].StartsWith("/"))
                                        r.CustomStr4 = fields[i];
                                }
                            }
                        }
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
                Log.Log(LogType.FILE, LogLevel.INFORM, " FreeBSDErrorRecorder In ParseFileNameRemote() -->> Enter The Function ");

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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDErrorRecorder In ParseFileNameRemote() -->> SSH command : " + command);
                    se.RunCommand(command, ref stdOut, ref stdErr);
                    se.Close();

                    StringReader sr = new StringReader(stdOut);
                    ArrayList arrFileNameList = new ArrayList();

                    while ((line = sr.ReadLine()) != null)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDErrorRecorder In ParseFileNameRemote() -->> Dosya ismi okundu: " + line);
                        String[] arr = line.Split(' ');
                        if (arr.Length >= 9 && arr[9].StartsWith("administrative") == true)
                        {
                            arrFileNameList.Add(arr);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDErrorRecorder In ParseFileNameRemote() -->> Okunan Dosya ismi arrayFileNameList'e atıldı. ");
                        }
                    }

                    String[] dFileNameList = SortFiles(arrFileNameList);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDErrorRecorder In ParseFileNameRemote() -->> arrayFileNameList'e atılan dosya isimleri sıralandı. ");

                    if (!String.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDErrorRecorder In ParseFileNameRemote() -->> LastFile is not null: " + lastFile);

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
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "FreeBSDErrorRecorder In ParseFileNameRemote() -->> LastName Is Null and FileName Is Setted To : " + FileName);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  FreeBSDErrorRecorder In ParseFileNameRemote() -->> In The Log Location There Is No Log File");
                        }
                    }
                }
                else
                {
                    FileName = Dir;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  FreeBSDErrorRecorder In ParseFileNameRemote() -->> Directory file olarak gösterildi.: " + FileName);
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "FreeBSDErrorRecorder In ParseFileNameRemote() In Catch -->>" + exp.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "FreeBSDErrorRecorder In ParseFileNameRemote() In Catch -->>" + exp.StackTrace);
                return;
            }

            Log.Log(LogType.FILE, LogLevel.INFORM, "FreeBSDErrorRecorder In ParseFileNameRemote() -->>  Exit The Function");
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeBSDErrorRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "  FreeBSDErrorRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeBSDErrorRecorder In ParseFileNameRemote() Exception Message -->> " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  FreeBSDErrorRecorder In ParseFileNameRemote() Exception StackTrace -->> " + ex.StackTrace);
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
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDErrorRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);

                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDErrorRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

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
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDErrorRecorder In CheckPosition() -->> commandRead For nread Is : " + commandRead);

                se.Connect();
                se.RunCommand(commandRead, ref stdOut, ref stdErr);
                se.Close();
                Log.Log(LogType.FILE, LogLevel.DEBUG, " FreeBSDErrorRecorder In CheckPosition() -->> commandRead'den dönen strOut : " + stdOut);

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
