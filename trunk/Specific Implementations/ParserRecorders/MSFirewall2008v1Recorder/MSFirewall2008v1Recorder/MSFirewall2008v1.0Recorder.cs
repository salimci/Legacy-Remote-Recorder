//Name: MSFirewall2008v1Recorder
//Writer: Ali Yıldırım
//Date: 22/07/2010

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Parser;
using CustomTools;
using Log;
using System.Collections;
using System.Globalization;
using System.Timers;
using System.Threading;


namespace Parser
{
    public class MSFirewall2008v1Recorder : Parser
    {
        Dictionary<String, Int32> dictHash;

        public MSFirewall2008v1Recorder()
            : base()
        {
            LogName = "MSFirewall2008v1Recorder";
            usingKeywords = false;
            lineLimit = 50;
        }

        public override void Init()
        {
            GetFiles();
        }

        public MSFirewall2008v1Recorder(String fileName)
            : base(fileName)
        {
        }

        public override bool ParseSpecific(String line, bool dontSend)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() | line : " + line);

            if (string.IsNullOrEmpty(line.Trim()) == true)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is null or Empty ");
                return true;
            }

            line = line.Trim();
            //#Fields: date time action protocol src-ip dst-ip src-port dst-port size tcpflags tcpsyn tcpack tcpwin icmptype icmpcode info path
            
            if (line.StartsWith("#"))
            {
                if (line.StartsWith("#Fields:"))
                {
                    if (dictHash != null)
                        dictHash.Clear();
                    dictHash = new Dictionary<String, Int32>();
                    String[] fields = line.Split(' ');
                    Int32 count = 0;
                    foreach (String field in fields)
                    {
                        if (field == "#Fields:")
                            continue;
                        dictHash.Add(field, count);
                        count++;
                    }
                    String add = "";
                    foreach (KeyValuePair<String, Int32> kvp in dictHash)
                    {
                        add += kvp.Key + ",";
                    }
                    SetLastKeywords(add);
                    keywordsFound = true;
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line starts with # char");
                return true;  // neden metoddan çıkıyor???

            }
            if (!dontSend)
            {
                String[] arr = line.Split(' ');

                try
                {
                    Rec r = new Rec();

                    DateTime date = Convert.ToDateTime(arr[dictHash["date"]].Replace('-','/'));
                    r.Datetime = date.Day + "/" + date.Month + "/" +date.Year + " " + arr[dictHash["time"]];
                    r.EventCategory = arr[dictHash["action"]];
                    r.CustomStr7 = arr[dictHash["protocol"]];
                    r.CustomStr3 = arr[dictHash["src-ip"]];
                    r.CustomStr4 = arr[dictHash["dst-ip"]];
                    r.CustomInt1 = ObjectToInt32(arr[dictHash["src-port"]],0);
                    r.CustomInt2 = ObjectToInt32(arr[dictHash["dst-port"]],0);
                    r.CustomInt5 = ObjectToInt32(arr[dictHash["size"]],0);
                    r.CustomStr8 = arr[dictHash["tcpflags"]];
                    r.CustomStr9 = arr[dictHash["tcpsyn"]];
                    r.CustomStr10 = arr[dictHash["tcpwin"]];
                    r.CustomStr9 += " " + arr[dictHash["icmptype"]];
                    r.CustomStr10 += " " + arr[dictHash["icmpcode"]];
                    r.Description = arr[dictHash["info"]];
                    r.SourceName = arr[dictHash["path"]];

                    r.LogName = LogName;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Finish Record Data");

                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseSpecific() | Line : " + line);
                    return false;
                }

            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParsingEnds");

            return true;
        }

        //pfirewall
        //pfirewall.log.old
        protected override void ParseFileNameLocal()
        {
            string newFile = "";
            string oldFile = "";
            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Searching for file in directory: " + Dir);
                ArrayList arrFileNames = new ArrayList();
                foreach (String file in Directory.GetFiles(Dir))
                {
                    string sFile = Path.GetFileName(file).ToString();

                    if (sFile.Equals("pfirewall.log") == true)
                    {
                        newFile = sFile;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() |  New File found : " + newFile);
                    }
                    else if (sFile.Equals("pfirewall.log.old") == true)
                    {
                        oldFile = sFile;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() |  Old File found : " + oldFile);
                    }
                }

                if (string.IsNullOrEmpty(lastFile) == false)
                {

                    if (File.Exists(lastFile) == true)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile is not null  : " + lastFile);
                        
                        FileInfo fi = null;
                        FileStream fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile is not null  : " + lastFile);

                            fi = new FileInfo(lastFile);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile okunmaya başlanacak, Dosya ismi  : " + lastFile);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile okunmaya başlanacak, Dosya boyutu  : " + fi.Length);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile okunmaya başlanacak, Pozisyon  : " + Position);
                            if (fi.Length - 1 == Position || fi.Length == Position)
                            {
                                if (lastFile == Dir + oldFile)
                                {
                                    FileName = Dir + newFile;
                                    lastFile = FileName;
                                    Position = 0;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Old Dosyasının sonuna ulaşıldı yeni dosya  : " + lastFile);
                                }
                                else
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | New Dosyasının sonuna ulaşıldı, yeni veri beklencek  : " + lastFile);
                                }
                            }
                            else
                            {

                                if (LineIsHere(fs, fi) == false)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine is not found in LastFile: " + lastFile);
                                    if (lastFile == Dir + oldFile)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile is oldFile, now seach for new file.");
                                        //last line new file da aracancak.
                                        fs = new FileStream(Dir + newFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                        fi = new FileInfo(Dir + newFile);

                                        if (LineIsHere(fs, fi) == false)
                                        {
                                            //Last line iki dosyadada bulunamadı.
                                            FileName = Dir + newFile;
                                            lastFile = FileName;
                                            Position = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine could not find any files, FileName: " + lastFile);
                                        }
                                        else
                                        {
                                            //last line new file da
                                            FileName = Dir + newFile;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in newFile, FileName: " + lastFile);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in newFile, Dosya boyutu  : " + fi.Length);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in newFile, Pozisyon  : " + Position);
                                        }
                                    }
                                    else
                                    {
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile is newFile, now seach for old file.");
                                        //last line old file da aranacak.

                                        fs = new FileStream(Dir + oldFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                        fi = new FileInfo(Dir + oldFile);

                                        if (LineIsHere(fs, fi) == false)
                                        {
                                            //Last line iki dosyadada bulunamadı.
                                            FileName = Dir + newFile;
                                            lastFile = FileName;
                                            Position = 0;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine could not find any files, FileName: " + lastFile);
                                        }
                                        else
                                        {
                                            //lastline old file da
                                            FileName = Dir + oldFile;
                                            lastFile = FileName;
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in oldFile, FileName: " + lastFile);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in oldFile, Dosya boyutu  : " + fi.Length);
                                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in oldFile, Pozisyon  : " + Position);
                                        }
                                    }
                                }
                                else
                                {
                                    FileName = lastFile;
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastLine found in lastFile, continue to read lastFile: " + lastFile);
                                }
                            }

                           fs.Close();
                    }
                    else 
                    {
                        FileName = Dir + newFile;
                        lastFile = FileName;
                        Position = 0;
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | lastFile doesn't exist.");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Assign new file. Filename: " + FileName);
                    }
                }
                else
                {
                    FileName = Dir + newFile;
                    lastFile = FileName;
                    Position = 0;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | LastFile is null or empty.");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameLocal() | Assign new file. Filename: " + FileName);
                }
            }
            else
                FileName = Dir;
        }
        
        private bool LineIsHere(FileStream fs, FileInfo fi)
        {

            Log.Log(LogType.FILE, LogLevel.DEBUG, "LineIsHere() | Searching for last line in file: " + fi.Name);

            Int64 currentPosition;
            string line = "";

            BinaryReader br = null;
            br = new BinaryReader(fs);
            br.BaseStream.Seek(Position, SeekOrigin.Begin);

            Log.Log(LogType.FILE, LogLevel.DEBUG, "LineIsHere() | Position is: " + br.BaseStream.Position.ToString());
            Log.Log(LogType.FILE, LogLevel.DEBUG, "LineIsHere() | Length is: " + br.BaseStream.Length.ToString());

            Int64 fileLength = fi.Length;
            StringBuilder lineSb = new StringBuilder();
            if (Position > fileLength)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "LineIsHere() | Position is gerater than file name. File: " + fi.Name);
                return false;
            }

            while (br.BaseStream.Position != fileLength)
            {
                currentPosition = br.BaseStream.Position;
                Char c = ' ';
                while (!Environment.NewLine.Contains(c.ToString()))
                {
                    if (br.BaseStream.Position == fileLength)
                        break;
                    c = br.ReadChar();
                    if (Environment.NewLine.Contains(c.ToString()))
                    {
                        line = lineSb.ToString();

                            if (line == lastLine)
                            {
                                //Position = currentPosition;
                                lastLine = line;
                                br.Close();
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "LineIsHere() | Last File is found. File: " + fi.Name);
                                return true;
                            }

                        lineSb.Remove(0, lineSb.Length);
                    }
                    else
                        lineSb.Append(c);
                    
                }
            }

                lineSb.Remove(0, lineSb.Length);
                br.Close();
                return false;
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Stop();
            if (remoteHost == "")
            {
                String fileLast = FileName;
                Stop();
                ParseFileName();
                if (FileName != fileLast)
                {
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Log.Log(LogType.FILE, LogLevel.INFORM, "File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Error while getting files, Exception: " + ex.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, ex.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                }
            }
        }
        
        public override void Start()
        {
            try
            {
                String keywords = GetLastKeywords();
                String[] arr = keywords.Split(',');
                if (dictHash == null)
                    dictHash = new Dictionary<String, Int32>();
                if (arr.Length > 2)
                    dictHash.Clear();
                Int32 count = 0;
                foreach (String keyword in arr)
                {
                    if (keyword == "")
                        continue;
                    dictHash.Add(keyword, count);
                    count++;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
            }
            base.Start();
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
