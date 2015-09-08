using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.IO.Compression;
using Parser;
using Log;
using CustomTools;
using Microsoft.Win32;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Threading;

namespace Parser
{
    public class AccessControlRecorder : Parser
    {   
        public AccessControlRecorder()
            : base()
        {
            LogName = "AccessControlRecorder";
            enc = Encoding.Unicode;
        }

        public override void Init()
        {
            GetFiles();
        }

        public AccessControlRecorder(String fileName)
            : base(fileName)
        {
        }

        protected override void dayChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dayChangeTimer.Enabled = false;
            if (remoteHost == "")
            {
                String fileLast = FileName;
                ParseFileName();
                if (FileName != fileLast)
                {
                    Stop();
                    Position = 0;
                    lastLine = "";
                    lastFile = FileName;
                    Start();
                    Log.Log(LogType.FILE, LogLevel.INFORM, " AccessControlRecorder In dayChangeTimer_Elapsed() -->> File Changed, New File Is, " + FileName);
                }
                else
                {
                    FileInfo fi = new FileInfo(FileName);
                    if (fi.Length - 1 > Position)
                    {
                        Stop();
                        Start();
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In dayChangeTimer_Elapsed() -->> Day Change Timer File Is: " + FileName);
                }
                
                dayChangeTimer.Enabled = true;
            }
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {
            base.SetConfigData(Identity, Location, LastLine, LastPosition, LastFile, LastKeywords, FromEndOnLoss
            , MaxLineToWait, User, Password, RemoteHost, SleepTime, TraceLevel, CustomVar1, CustomVar2, virtualhost
            , dal, Zone);
            FileName = LastFile;
        }

        public override void GetFiles()
        {
            try
            {   
                GetRegistry();
                Dir = GetLocation();
                Today = DateTime.Now;
                ParseFileName();
            }
            catch (Exception e)
            {
                if (reg == null)
                {
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, "Using Registry is " + usingRegistry.ToString());
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Error while getting files, Exception: reg is not null " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        }

        public override String GetLocation()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Enter The Function");
            
            string templocationregistry = "";
            string templocationspecialfolder = "";
            string templocation = "";
            
            if (usingRegistry)
            {
                
                try
                {
                    if (reg == null)
                    {
                        reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\" + LogName, true);
                        //reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\NATEK\\Security Manager\\Recorder\\AccessControlRecorder", true);
                    }
                    templocationregistry = reg.GetValue("Location").ToString();
                    templocationspecialfolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    templocation = templocationspecialfolder + "\\" + templocationregistry;
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  Catch In GetLocation() : " + ex.Message + " templocation  is : " + templocation);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Location is : " + templocation);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Exit The Function");
                return templocation;
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Location is : " + Dir);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In GetLocation() -->> Exit The Function");
                return Dir;
            }
        }

        public override void Start()
        {
            base.Start();
        }

        public override bool ParseSpecific(String line, bool dontSend) // string parçalama
        {
            line = line.Trim();
            line = line.Replace("\0", "");

            if (line == "" || line == " ")
                return true;

            if (!dontSend)
            {
                try
                {
                    Rec rRec = new Rec();
                    rRec.LogName = LogName;
                    rRec = stringdevideProcess(line, rRec);
                    SetRecordData(rRec);
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  AccessControlRecorder In ParseSpecific In catch -->>" + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  AccessControlRecorder In ParseSpecific In catch -->>" + e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "  AccessControlRecorder In ParseSpecific In catch -->>" + "Line : " + line);
                    return true;
                }
            }

            return true;
        }

        public Rec stringdevideProcess(string sLine, Rec rRec)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " AccessControlRecorder In stringdevideProcess() -->> Entered The Function");
            Log.Log(LogType.FILE, LogLevel.DEBUG, " AccessControlRecorder In stringdevideProcess() -->> Last File Is" + lastFile.ToString());
            Log.Log(LogType.FILE, LogLevel.DEBUG, " AccessControlRecorder In stringdevideProcess() -->> Line Is : " + sLine);
            
            string filenameItem = Path.GetFileName(lastFile).ToString().Split('.')[0].Replace('-', '.');

            //Line : 13:41:23.351||Allow||OpenRead||System||||ULASTIRMA\natekservice||Disk||C:\||bdb1168b-95c1-49aa-b94d-3148eed7f85c.UserName
            char[] delimiters = new char[] { '|', '|' };
            string[] tempfields = sLine.Split(delimiters, StringSplitOptions.None);
            string[] fields = new string[11];
            int j = 0;

            for (int k = 0; k < tempfields.Length; k = k + 2)
            {
                fields[j] = tempfields[k];
                j++;
            }

            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            // Convert the string into a byte[].
            byte[] unicodeBytes = unicode.GetBytes(fields[0]);

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            // This is a slightly different approach to converting to illustrate
            // the use of GetCharCount/GetChars.
            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);
            fields[0] = asciiString;

            
            for (int y = 0; y < fields.Length; y++)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, fields[y]);
            }

            if (fields[0].Contains("?"))
            {
                fields[0] = fields[0].Replace("?", "");
            }

            string fileDate = "";

            try
            {
                filenameItem += " " + fields[0];
                fileDate = filenameItem;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " AccessControlRecorder In stringdevideProcess() -->> An error accured while parsing date time " + ex.Message);
            }

            try
            {
                rRec.Datetime = fileDate;
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " AccessControlRecorder In stringdevideProcess() -->> An Error Accured While Creating Date Time" + e.Message);
            }

            rRec.EventType = fields[1];
            rRec.EventCategory = fields[2];
            //rRec.CustomStr1 = fields[3];
            rRec.SourceName = fields[3];

            
            string exename = "";
            string exepath = "";

            try
            {
                string[] exeinformation = fields[4].Split('\\');
                exename = exeinformation[exeinformation.Length - 1];
                for (int i = 0; i < exeinformation.Length - 1; i++)
                {
                    exepath += exeinformation[i] + "\\";
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " AccessControlRecorder In stringdevideProcess() -->> An Error Accured While Creatind Exe Information" + e.Message);
            }

            //rRec.CustomStr2 = exepath;
            //rRec.CustomStr3 = exename;

            string domain = "";
            string username = "";

            try
            {
                if (fields[5].Contains("\\"))
                {
                    string[] domainandusername = fields[5].Split('\\');
                    domain = domainandusername[0];
                    username = domainandusername[1];
                }
                else
                {
                    username = fields[5];
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " AccessControlRecorder In stringdevideProcess() -->> An Error Accured While Creatind User and Domain Name" + e.Message);
            }

            rRec.UserName = domain + "\\" + username;
            rRec.CustomStr1 = domain + "\\" + username;
            rRec.CustomStr2 = username;
            
            //rRec.CustomStr4 = domain;
            rRec.CustomStr5 = exename;
            rRec.CustomStr6 = exepath + "\\" + exename;

            

            string filename = "";
            string filepath = "";

            try
            {
                string[] fileinformation = fields[7].Split('\\');
                filename = fileinformation[fileinformation.Length - 1];
                for (int i = 0; i < fileinformation.Length - 1; i++)
                {
                    filepath += fileinformation[i] + "\\";
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " AccessControlRecorder In stringdevideProcess() -->> An Error Accured While Creating File Name And Path" + e.Message);
            }


            rRec.CustomStr3 = filepath;
            rRec.CustomStr4 = filename;
            rRec.CustomStr7 = fields[2];
            rRec.CustomStr8 = filepath + "\\" + filename;
            rRec.CustomStr9 = fields[6];
            
            //rRec.LogName = LogName;
            rRec.LogName = "NATEKAccessControlOS";

            //string tempcomputerName = "" ;
            //tempcomputerName = GetLocation();

            //tempcomputerName = tempcomputerName.TrimStart('\\');
            //int indexslash = 0;
            //indexslash = tempcomputerName.IndexOf('\\', 0);
            //string computerName = tempcomputerName.Substring(0,indexslash);

            //rRec.ComputerName = computerName;

            rRec.ComputerName = Environment.MachineName;

            Log.Log(LogType.FILE, LogLevel.DEBUG, " AccessControlRecorder In stringdevideProcess() -->> Exit The Function");
            return rRec;
        }
        
        protected override void ParseFileNameLocal() // last file ve position
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " AccessControlRecorder In ParseFileNameLocal() -->> Enter The Function");
            
            FileStream fs = null;
            BinaryReader br = null;
            Int64 currentPosition = Position;

            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Searching In Directory : " + Dir);

                ArrayList filenameList = new ArrayList();

                foreach (String file in Directory.GetFiles(Dir))
                {
                  string fName = Path.GetFileName(file);
                  if (fName.Contains("search-"))
                  {
                    continue;
                  }
                    filenameList.Add(fName);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> " + filenameList.Count.ToString() + " File Found ");

                string[] fileName = new string[filenameList.Count];
                string[] fileName2 = new string[3];
                long[] permanentfileName = new long[filenameList.Count];
                string[] fullfileName = new string[filenameList.Count];

                for (int i = 0; i < filenameList.Count; i++)
                {
                    fileName[i] = filenameList[i].ToString().Split('.')[0];
                    fileName2 = fileName[i].Split('-');
                    permanentfileName[i] = Convert.ToInt64(fileName2[0] + fileName2[1] + fileName2[2]);
                    fullfileName[i] = filenameList[i].ToString();
                }

                Array.Sort(permanentfileName, fullfileName);
                
                if (String.IsNullOrEmpty(lastFile))
                {
                    if (fullfileName.Length > 0)
                    {
                        FileName = Dir + fullfileName[0].ToString();
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " AccessControlRecorder In ParseFileNameLocal() -->> Last File Is Null So Last File Setted The First File " + lastFile);
                    }
                    else 
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " AccessControlRecorder In ParseFileNameLocal() -->> Last File Is Null And There Is No File To Set");
                    }
                }
                else
                {
                    if (File.Exists(lastFile))
                    {   
                        fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        br = new BinaryReader(fs, enc);
                        br.BaseStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fi = new FileInfo(lastFile);
                        Int64 fileLength = fi.Length;
                            Char c = ' ';
                            while (!Environment.NewLine.Contains(c.ToString()) && (br.BaseStream.Position < fileLength))
                            {
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is1 " + br.BaseStream.Position);
                                c = br.ReadChar();
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is2 " + br.BaseStream.Position);

                                
                                if (Environment.NewLine.Contains(c.ToString()) || br.BaseStream.Position == fileLength)
                                {
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : Position Is " + br.BaseStream.Position);
                                    Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Setted To Next End of Line : FileLength Is " + fileLength);
                                }
                            }


                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Position Is : " + br.BaseStream.Position.ToString());
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Length Is : " + br.BaseStream.Length.ToString());

                        if (br.BaseStream.Position == br.BaseStream.Length || br.BaseStream.Position == br.BaseStream.Length - 1)
                        {
                            for (int i = 0; i < fullfileName.Length; i++)
                            {   
                                if (Dir + fullfileName[i].ToString() == lastFile)
                                {
                                    if (i + 1 == fullfileName.Length)
                                    {
                                        FileName = lastFile;
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " AccessControlRecorder In ParseFileNameLocal() -->> There Is No New Fýle and Waiting For New Record");
                                        break;
                                    }
                                    else
                                    {
                                        FileName = Dir + fullfileName[(i + 1)].ToString();
                                        lastFile = FileName;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " AccessControlRecorder In ParseFileNameLocal() -->> Reading Of The File " + fullfileName[i] + " Finished And Continiu With New File " + fullfileName[i + 1]);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            FileName = lastFile;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Continiu reading the last file : " + FileName);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Last File Not Found : " + lastFile);
                        string[] _fileName = Path.GetFileName(lastFile).ToString().Split('.')[0].Split('-');
                        long lastfileDate = Convert.ToInt64(_fileName[0] + _fileName[1] + _fileName[2]);
                        int _index = 0;
                        bool controlofexistnextfile = false;
                        for (int i = 0; i < permanentfileName.Length; i++)
                        {
                            if (permanentfileName[i] > lastfileDate)
                            {
                                _index = i;
                                controlofexistnextfile = true;
                                break;
                            }
                        }
                        if (controlofexistnextfile)
                        {
                            FileName = Dir + fullfileName[_index];
                        }
                        else 
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> There Is No New File ; Waiting For a New File");
                        }
                    }
                }
            }
            else
                FileName = Dir;

            if (br != null && fs != null)
            {
                br.Close();
                fs.Close();
            }
            
            lastFile = FileName;

            Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Filename is: " + FileName);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "  AccessControlRecorder In ParseFileNameLocal() -->> Exit The Function");
        }
    }
}

