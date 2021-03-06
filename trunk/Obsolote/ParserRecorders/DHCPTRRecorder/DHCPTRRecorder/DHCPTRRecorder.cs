using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;
using Microsoft.Win32;
using CustomTools;
using SharpSSH.SharpSsh;
using Log;
using System.Collections;

namespace Parser
{
    public class DHCPTRRecorder : Parser
    {
	
        Dictionary<String, Int32> dictHash;

        public DHCPTRRecorder()
            : base()
        {
            LogName = "DHCPTRRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public override bool ParseSpecific(string line, bool dontSend)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific line");
            if (line == "")
                return true;

            String[] arr = line.Split(',');
            if (arr.Length < 2)
                return true;

            //Windows 2000 fix
            if (arr[0] == "Kimlik Tarih")
            {
                List<String> lst = new List<String>();
                String[] temp = arr[0].Split(' ');

                lst.Add(temp[0]);
                lst.Add(temp[1]);

                for (Int32 i = 1; i < arr.Length; i++)
                {
                    lst.Add(arr[i]);
                }

                arr = lst.ToArray();
            }

            if (dictHash.Count > 0)
            {
                if (dictHash.Count > arr.Length)
                {
                    Log.Log(LogType.FILE, LogLevel.WARN, "Wrong format on parse, expected parse count " + dictHash.Count + ", found " + arr.Length + ", line: " + line + "!");
                    Log.Log(LogType.FILE, LogLevel.WARN, "Please fix your DHCP Logger before messing with developer! Parsing will continue...");
                    return true;
                }
            }

            try
            {
                if (arr[0] == "Kimlik")
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    Int32 count = 0;
                    foreach (String field in arr)
                    {
                        dictHash.Add(field.Trim(), count);
                        count++;
                    }
                    SetLastKeywords(line);
                    keywordsFound = true;
                }
                else
                {
                    if (!dontSend)
                    {
                        Rec r = new Rec();

                        Int32 dateIndex = dictHash["Tarih"];

                        String[] temp = arr[dateIndex].Split('/');

                        r.Datetime = "20" + temp[2] + "/" + temp[0] + "/" + temp[1] + " " + arr[dictHash["Saat"]];
                        try
                        {
                            r.EventId = Convert.ToInt64(arr[dictHash["Kimlik"]]);
                        }
                        catch
                        {
                            r.EventId = -1;
                        }

                        try
                        {
                            r.Description = arr[dictHash["Açıklama"]].Normalize();
                        }
                        catch
                        {
                            try
                            {
                                r.Description = arr[dictHash["Açiklama"]].Normalize();
                            }
                            catch
                            {
                                try
                                {
                                    r.Description = arr[dictHash["Açýklama"]].Normalize();
                                }
                                catch
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find description.");
                                    r.Description = "";
                                }
                            }
                        }
                        r.UserName = arr[dictHash["IP Adresi"]];
                        r.EventCategory = arr[dictHash["MAC Adresi"]];

                        r.LogName = LogName;
                        try
                        {
                            r.ComputerName = arr[dictHash["Ana Bilgisayar Adı"]].Normalize();
                        }
                        catch
                        {
                            try
                            {
                                r.ComputerName = arr[dictHash["Ana Bilgisayar Adi"]].Normalize();
                            }
                            catch
                            {
                                try
                                {
                                    r.ComputerName = arr[dictHash["Ana Bilgisayar Adý"]].Normalize();
                                }
                                catch
                                {
                                    Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot find computer name.");
                                    r.ComputerName = "";
                                }
                            }
                        }

                        r.CustomStr1 = Dir;

                        SetRecordData(r);
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
            return true;
        }

        public override void Start()
        {
            try
            {
                String keywords = GetLastKeywords();
                String[] arr = keywords.Split(',');

                //Windows 2000 fix
                if (arr[0] == "Kimlik Tarih")
                {
                    List<String> lst = new List<String>();
                    String[] temp = arr[0].Split(' ');

                    lst.Add(temp[0]);
                    lst.Add(temp[1]);

                    for (Int32 i = 1; i < arr.Length; i++)
                    {
                        lst.Add(arr[i]);
                    }

                    arr = lst.ToArray();
                }

                if (dictHash == null)
                    dictHash = new Dictionary<String, Int32>();
                if (arr.Length > 2)
                    dictHash.Clear();
                Int32 count = 0;
                foreach (String keyword in arr)
                {
                    if (keyword == "")
                        continue;
                    dictHash.Add(keyword.Trim(), count);
                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            }
            base.Start();
        }

        private String GetDayString()
        {
            DayOfWeek dow = DateTime.Now.DayOfWeek;

            switch (dow)
            {
                case DayOfWeek.Monday:
                    return "Pzt";
                case DayOfWeek.Tuesday:
                    return "Sal";
                case DayOfWeek.Wednesday:
                    return "Çar";
                case DayOfWeek.Thursday:
                    return "Per";
                case DayOfWeek.Friday:
                    return "Cum";
                case DayOfWeek.Saturday:
                    return "Cmt";
                case DayOfWeek.Sunday:
                    return "Paz";
            }

            return "Pzt";
        }

        protected override void ParseFileNameLocal()
        {
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                if (!string.IsNullOrEmpty(lastFile))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> ConvertLastFileName() function Will Start.  lastFile : " + lastFile);
                    lastFile = ConvertLastFileName(lastFile);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> ConvertLastFileName() function is successfully finished  lastFile : " + lastFile);
                    if (File.Exists(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> lastFile is not null  : " + lastFile);

                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, enc);

                        br.BaseStream.Seek(Position, SeekOrigin.Begin);

                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Position is: " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Length is: " + br.BaseStream.Length.ToString());

                        #region yeni eklendi
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;
                        Char c = ' ';
                        while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                        {
                            c = br.ReadChar();

                            if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                            }
                        }
                        #endregion

                        if (br.BaseStream.Position == br.BaseStream.Length - 1 || br.BaseStream.Position == br.BaseStream.Length)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Position is at the end of the file");

                            
                            if (lastFile.Contains(GetDayString()))
                            {

                                FileName = lastFile;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> There is no new file So continuing with same file : " + FileName);
                            }
                            else
                            {
                                setDailyFile();
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal -->> Not end of the file , continui reading the file");
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " DHCPRecorder In ParseFileNameLocal() -->> FileName = LastFile " + FileName);
                        }
                        
                        br.Close();
                        fs.Close();
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> Last File is not found  : " + lastFile);
                        setDailyFile();
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In ParseFileNameLocal() -->> lastFile is Null or Empty");
                    setDailyFile();
                }
            }
            else
                FileName = Dir;
        }

        private String ConvertLastFileName(String lastFile)
        {
            try
            {
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Mon"))
                {
                    lastFile = lastFile.Replace("Mon", "Pzt");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Tue"))
                {
                    lastFile = lastFile.Replace("Tue", "Sal");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Wed"))
                {
                    lastFile = lastFile.Replace("Wed", "Çar");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Thu"))
                {
                    lastFile = lastFile.Replace("Thu", "Per");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Fri"))
                {
                    lastFile = lastFile.Replace("Fri", "Cum");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Sat"))
                {
                    lastFile = lastFile.Replace("Sat", "Cmt");
                }
                if (lastFile.Substring(lastFile.Length - 7, 7).Contains("Sun"))
                {
                    lastFile = lastFile.Replace("Sun", "Paz");
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder -->> ConvertLastFileName() -->> lastFile :" + lastFile);
                return lastFile;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }

        private void setDailyFile()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> Start");

                string oldLastFile = lastFile;

                ArrayList aryFiles = getFileList();

                if (aryFiles != null)
                {
                    if (aryFiles.Count > 0)
                    {
                        bool controlDhcpFile = false;

                        for (int i = 0; i < aryFiles.Count; i++)
                        {
                            if (aryFiles[i].ToString().Contains(GetDayString()))
                            {
                                FileName = Dir + aryFiles[i];
                                lastFile = FileName;
                                Position = 0;
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> New File Is  : " + FileName);
                                controlDhcpFile = true;
                                break;
                            }
                        }

                        if (controlDhcpFile)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> FileName is : " + FileName);
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> File Not Foud to read Which contains " + GetDayString());
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "  DHCPRecorder In setDailyFile() -->> There is any file returned from Dir.");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> aryFiles.Count == 0");
                        FileName = "";
                        lastFile = FileName;
                        Position = 0;
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> aryFiles is null");
                    FileName = "";
                    lastFile = FileName;
                    Position = 0;
                }

                checkForPreviousFile(oldLastFile);
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  DHCPRecorder In setDailyFile() -->>  : " + exp.Message);
            }
        }

        private ArrayList getFileList()
        {
            ArrayList arrFileNames = new ArrayList();

            try
            {
                //Dir yokmuş gibi yanıt dönüyor!!!
                if (Directory.Exists(Dir) == true)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                         " DHCPRecorder In  getFileList() | Directory is found  : " + Dir);

                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        string sFile = Path.GetFileName(file).ToString();
                        if (sFile.StartsWith("DhcpSrvLog"))
                            arrFileNames.Add(sFile);
                    }
                    return arrFileNames;
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                          " DHCPRecorder In  getFileList() | Directory is not found  : " + Dir);
                    return arrFileNames;
                }
            }
            catch (Exception exp)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "  DHCPRecorder In getFileList() |   : " + exp.Message);
                return arrFileNames;
            }
        }

        private void checkForPreviousFile(string oldLastFile)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> oldLastFile  : " + oldLastFile);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> lastFile     : " + lastFile);

            if (string.IsNullOrEmpty(oldLastFile) || string.IsNullOrEmpty(lastFile))
            {
                return;
            }

            //string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            string[] days = { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };

            string oFile = Path.GetFileName(oldLastFile).ToString();

            string fieldoldfile = oFile.Split('-')[1].Trim().Split('.')[0].ToString();

            int indexofoldfileindays = 0;

            for (int i = 0; i < days.Length; i++)
            {
                if (days[i] == fieldoldfile)
                {
                    indexofoldfileindays = i;
                }
            }

            string nFile = Path.GetFileName(lastFile).ToString();

            string fieldnewfile = nFile.Split('-')[1].Trim().Split('.')[0].ToString();

            int indexofnewfileindays = 0;

            for (int j = 0; j < days.Length; j++)
            {
                if (days[j] == fieldnewfile)
                {
                    indexofnewfileindays = j;
                }
            }

            int difference = indexofnewfileindays - indexofoldfileindays;

            if (difference < 0)
            {
                difference += 7;
            }

            if (difference != 1)
            {
                int arrayindex = indexofoldfileindays % 6;

                if (arrayindex != 0)
                {
                    arrayindex++;
                }
                else
                {
                    arrayindex = 0;
                }

                FileName = Dir + "DhcpSrvLog-" + days[arrayindex] + ".log";
                lastFile = FileName;
                Position = 0;
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  DHCPRecorder In setDailyFile() -->> New File Is  : " + FileName);
            }
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
