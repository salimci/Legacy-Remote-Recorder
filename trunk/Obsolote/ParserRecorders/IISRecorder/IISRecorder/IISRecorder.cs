using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using System.Globalization;

namespace Parser
{
    public class IISRecorder : Parser
    {
        Dictionary<String, Int32> dictHash;
        string iisType = "";
        string fileNameType = ""; // ex for ex1231 , extend for extend1
        string oldFileNameType = "";
        string machineName = "";

        public IISRecorder()
            : base()
        {
            LogName = "IISRecorder";
        }

        public override void Init()
        {
            GetFiles();
        }

        public IISRecorder(String fileName)
            : base(fileName)
        {
        } // IISRecorder

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
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        } // dayChangeTimer_Elapsed

        public override bool ParseSpecific(String line, bool dontSend)
        {
            bool sqlInjection = false;

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Starts");
            if (line == "")
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is empty");
                return true;
            }
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
                return true;
            }

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line Is : --->> " + line.ToString());

            if (!dontSend)
            {
                StringBuilder csVer_cUserAgentBuilder = new StringBuilder();
                String[] arr = line.Split(' ');

                for (int i = 0; i < arr.Length; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "arr : " + arr[i]);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.5");
                try
                {
                    Rec r = new Rec();
                    Int32 dateIndex = dictHash["date"];
                    arr[dateIndex] = arr[dateIndex].Replace('-', '/');
                    r.Datetime = arr[dateIndex] + " " + arr[dictHash["time"]];
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.6");
                    try
                    {
                        r.SourceName = arr[dictHash["s-sitename"]];
                    }
                    catch (Exception e) { Log.Log(LogType.FILE, LogLevel.ERROR, "1" + e.Message); }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.7");

                    try
                    {
                        r.EventType = arr[dictHash["cs-method"]];
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "2" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.8");

                    try
                    {
                        if (arr[dictHash["cs-uri-stem"]].Length > 900)
                        {
                            string tempdata = arr[dictHash["cs-uri-stem"]];
                            r.Description = tempdata.Substring(0, 900);
                            // Onur
                            if (r.Description.Contains(";"))
                            {
                                string[] sqlInjectionStringArray = r.Description.Split(';');
                                r.Description = sqlInjectionStringArray[0];
                                //r.CustomStr1 = sqlInjectionStringArray[1];
                                r.CustomStr1 = SqlInjectionStringConcat(sqlInjectionStringArray);
                            }
                        }
                        else
                        {
                            r.Description = arr[dictHash["cs-uri-stem"]];
                            // Onur
                            if (r.Description.Contains(";"))
                            {
                                string[] sqlInjectionStringArray = r.Description.Split(';');
                                r.Description = sqlInjectionStringArray[0];
                                //r.CustomStr1 = sqlInjectionStringArray[1];
                                r.CustomStr1 = SqlInjectionStringConcat(sqlInjectionStringArray);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "3" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 2.9");

                    try
                    {
                        if (arr[dictHash["cs-uri-query"]].Length > 900)
                        {
                            string tempdata = arr[dictHash["cs-uri-query"]];

                            if (tempdata.Length >= 1800)
                            {
                                tempdata = tempdata.Substring(0, 1800);
                                if (string.IsNullOrEmpty(r.CustomStr1))
                                {
                                    r.CustomStr1 = tempdata.Substring(0, 900);
                                    r.CustomStr10 = tempdata.Substring(901, tempdata.Length - 900);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(r.CustomStr1))
                                {
                                    r.CustomStr1 = tempdata.Substring(0, 900);
                                    r.CustomStr10 = tempdata.Substring(901, tempdata.Length - 900);
                                }
                            }
                        }
                        else
                        {
                            r.CustomStr1 = arr[dictHash["cs-uri-query"]];
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.0");

                    try
                    {
                        r.UserName = arr[dictHash["cs-username"]];
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "4" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.1");

                    try
                    {
                        r.CustomStr3 = arr[dictHash["c-ip"]];
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "5" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.2");

                    try
                    {
                        r.CustomInt1 = Convert.ToInt32(arr[dictHash["sc-status"]]);
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "6" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.3");

                    try
                    {
                        r.CustomInt2 = Convert.ToInt32(arr[dictHash["sc-substatus"]]);
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "7" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.4");

                    try
                    {
                        r.CustomInt4 = Convert.ToInt32(arr[dictHash["sc-win32-status"]]);
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "8" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.5");

                    try
                    {
                        r.CustomStr4 = arr[dictHash["s-ip"]];
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "9" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.6");

                    try
                    {
                        r.CustomStr2 = arr[dictHash["s-port"]];
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "10" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.7");
                    if (dictHash.ContainsKey("cs-version"))
                    {
                        try
                        {
                            csVer_cUserAgentBuilder.Append((arr[dictHash["cs-version"]])).Append(" ");
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "11" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.8");

                    try
                    {
                        csVer_cUserAgentBuilder.Append((arr[dictHash["cs(User-Agent)"]]));
                    }
                    catch (Exception e)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, "12" + e.Message);
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 3.9");

                    r.CustomStr6 = csVer_cUserAgentBuilder.ToString();

                    csVer_cUserAgentBuilder.Remove(0, csVer_cUserAgentBuilder.Length);

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4");

                    if (dictHash.ContainsKey("cs(Referer)"))
                    {
                        try
                        {
                            if (arr[dictHash["cs(Referer)"]].Length > 900)
                                r.CustomStr5 = arr[dictHash["cs(Referer)"]].Substring(0, 899);
                            else
                                r.CustomStr5 = arr[dictHash["cs(Referer)"]];
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "13" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.3");

                    if (dictHash.ContainsKey("sc-bytes"))
                    {
                        try
                        {
                            r.CustomStr7 = arr[dictHash["sc-bytes"]];
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "14" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.4");

                    if (dictHash.ContainsKey("cs(Cookie)"))
                    {
                        try
                        {
                            r.CustomStr8 = arr[dictHash["cs(Cookie)"]];
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "15" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.5");

                    if (dictHash.ContainsKey("cs-host"))
                    {
                        try
                        {
                            r.CustomStr9 = arr[dictHash["cs-host"]];
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "16" + e.Message);
                        }
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.6");

                    if (dictHash.ContainsKey("cs-bytes"))
                    {
                        try
                        {
                            r.CustomInt6 = Convert.ToInt64(arr[dictHash["cs-bytes"]]);
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "17" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.7");

                    try
                    {
                        if (r.CustomStr10 == null)
                        {
                            r.CustomStr10 = iisType;
                        }
                    }
                    catch (Exception e) { Log.Log(LogType.FILE, LogLevel.ERROR, "18" + e.Message); }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.8");

                    if (dictHash.ContainsKey("time-taken"))
                    {
                        try
                        {
                            r.CustomInt5 = Convert.ToInt32(arr[dictHash["time-taken"]]);
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "19" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 4.9");

                    if (dictHash.ContainsKey("s-computername"))
                    {
                        try
                        {
                            r.ComputerName = arr[dictHash["s-computername"]];
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "20" + e.Message);
                        }
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 5.1");

                    r.LogName = LogName;

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Splitting 5.2");

                    if (usingRegistry)
                    {
                        r.ComputerName = r.CustomStr4;
                    }
                    else
                    {
                        r.ComputerName = machineName.Split('\\')[2];
                    }

                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Setting Record Data");
                    SetRecordData(r);
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Finish Record Data");
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "Line : " + line);
                    return false;
                }
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Ends");
            return true;
        } // ParseSpecific

        public string SqlInjectionStringConcat(string[] array)
        {
            string fullString = "";
            try
            {
                if (array.Length > 1)
                {
                    for (int i = 1; i < array.Length; i++)
                    {
                        fullString += array[i];
                    }
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        fullString = array[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "SqlInjectionStringConcat() ERROR: " + ex.Message);
            }
            return fullString;
        } // SqlInjectionStringConcat

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {
            base.SetConfigData(Identity, Location, LastLine, LastPosition, LastFile, LastKeywords, FromEndOnLoss
            , MaxLineToWait, User, Password, RemoteHost, SleepTime, TraceLevel, CustomVar1, CustomVar2, virtualhost
            , dal, Zone);
            iisType = CustomVar1;
            FileName = LastFile;
            machineName = Location;
        } // SetConfigData

        protected override void ParseFileNameLocal()
        {
            bool isFileNameChange = false;
            FileStream fs = null;
            BinaryReader br = null;
            bool fileIsFinished = false;
            Int64 currentPosition = Position;
            FileName = lastFile;

            #region endOfFile
            if (lastFile != null && lastFile != "" && lastFile != " ")
            {
                if (File.Exists(lastFile) == true)
                {
                    if (lastFile.EndsWith("zip"))
                    {
                        fileIsFinished = true;
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Last File .zip ile bittiginden deðiþtirilecektir");
                    }

                    Int64 positionOfParser = 0;
                    Int64 lengthOfFile = 26;
                    try
                    {
                        fs = new FileStream(lastFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    catch (Exception exc)
                    {
                        Log.Log(LogType.FILE, LogLevel.WARN, "File deleted or rotated..");
                        Log.Log(LogType.FILE, LogLevel.WARN, "Last file not found, looking for next file");
                        Log.Log(LogType.FILE, LogLevel.ERROR, exc.Message);
                        lastFile = "";
                        Position = 0;
                    }

                    br = new BinaryReader(fs, enc);
                    br.BaseStream.Seek(Position, SeekOrigin.Begin);

                    #region yeni eklendi
                    FileInfo finfo = new FileInfo(lastFile);
                    Int64 fileLength = finfo.Length;
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
                    #endregion

                    positionOfParser = br.BaseStream.Position;
                    br.Close();
                    fs.Close();
                    FileInfo fi = new FileInfo(lastFile);
                    lengthOfFile = fi.Length;
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Position is: " + positionOfParser);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Length is: " + lengthOfFile);

                    if (positionOfParser > lengthOfFile - 2 || positionOfParser > lengthOfFile - 1 || positionOfParser == lengthOfFile)
                        fileIsFinished = true;
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Not at the end of file, parsing continues");
                        return;
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Dosya Silinmiþ Bulunamadý. Sýradaki Dosya Set Edilecek!!  Silinen Dosya : " + lastFile);

                    fileIsFinished = true;
                }
            }
            #endregion

            if (Dir.EndsWith("/") || Dir.EndsWith("\\") || fileIsFinished)
            {
                bool fileFoundToParse = false;

                #region findFileTypeofIIS
                if (fileNameType == "")
                {
                    FileInfo fi = null;
                    DateTime dt = DateTime.MinValue;
                    if (lastFile != null && lastFile != "" && lastFile != " ")
                    {
                        fi = new FileInfo(lastFile);
                        dt = fi.CreationTime;
                    }
                    String fileType = "";

                    foreach (String file in Directory.GetFiles(Dir))
                    {
                        fi = new FileInfo(file);
                        if (fi.CreationTime > dt && file.EndsWith(".log"))
                        {
                            dt = fi.CreationTime;
                            fileType = file;
                            break;
                        }
                    }//ex09121014.log
                    if (fileType.Contains("extend"))
                    {
                        fileNameType = "extend";
                    }
                    else if (fileType.Contains("ex"))
                    {
                        if (fileType.Substring(fileType.Length - 14, 2) == "ex")
                        {
                            fileNameType = "exHour";
                        }
                        else if (fileType.Substring(fileType.Length - 10, 2) == "ex")
                        {
                            fileNameType = "exMonth";
                        }
                        else
                            fileNameType = "ex";
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Filename type is :" + fileNameType);
                    oldFileNameType = fileNameType;
                }
                #endregion

                #region isThereFileToParse
                try
                {
                    if (lastFile != "" && lastFile != null)
                    {
                        FileInfo fi = new FileInfo(lastFile);
                        FileInfo fiOld = new FileInfo(lastFile);
                        foreach (String file in Directory.GetFiles(Dir))
                        {
                            fi = new FileInfo(file);
                            if (fiOld.CreationTime < fi.CreationTime && file.EndsWith(".log"))
                            {
                                if (file.Contains("extend"))
                                {
                                    fileNameType = "extend";
                                }
                                else if (file.Contains("ex"))
                                {
                                    if (file.Substring(file.Length - 14, 2) == "ex")
                                    {
                                        fileNameType = "exHour";
                                    }
                                    else if (file.Substring(file.Length - 10, 2) == "ex")
                                    {
                                        fileNameType = "exMonth";
                                    }
                                    else
                                        fileNameType = "ex";
                                }
                                fileFoundToParse = true;
                                break;
                            }
                        }

                        if (fileNameType == oldFileNameType)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "New log files found, recorder will parse these files..");
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "IIS Log Format is changed, looking for new file..");
                            lastFile = "";
                            Position = 0;
                            oldFileNameType = fileNameType;
                            fileFoundToParse = false;
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, e.Message);
                    Log.Log(LogType.FILE, LogLevel.INFORM, e.StackTrace);
                }
                #endregion

                if (fileNameType == "ex")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Looking for ex files");
                    #region searchingExFileType
                    if (lastFile == "" || lastFile == null)
                    {
                        FileInfo fi = null;
                        DateTime lastCreationDate = DateTime.MinValue;
                        foreach (String file in Directory.GetFiles(Dir))
                        {
                            fi = new FileInfo(file);
                            if (fi.CreationTime > lastCreationDate && file.EndsWith(".log"))
                            {
                                FileName = file;
                                isFileNameChange = true;
                                lastCreationDate = fi.CreationTime;
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.INFORM, "File found : " + FileName);
                    }
                    else if (fileFoundToParse)
                    {//ex091121   
                        if (fileIsFinished)
                        {
                            int escape = 0;
                            while (true)
                            {
                                DateTime nextDay = DateTime.Now;
                                String calculateDate = "";
                                String year = "";
                                String month = "";
                                String day = "";
                                String modifiedLastFile = "";
                                modifiedLastFile = lastFile.Substring(lastFile.Length - 12);
                                calculateDate = modifiedLastFile.Remove(0, 2);
                                year = DateTime.Now.Year.ToString().Substring(0, 2) + calculateDate.Substring(0, 2);
                                month = calculateDate.Substring(2, 2);
                                day = calculateDate.Substring(4, 2);
                                nextDay = Convert.ToDateTime(month + "/" + day + "/" + year, CultureInfo.InvariantCulture);
                                nextDay = nextDay.AddDays(1);
                                year = nextDay.Year.ToString();
                                String yearShort = year[year.Length - 2].ToString() + year[year.Length - 1].ToString();
                                day = "ex" + yearShort + ((nextDay.Month < 10) ? ("0" + nextDay.Month.ToString()) : nextDay.Month.ToString()) + ((nextDay.Day < 10) ? ("0" + nextDay.Day.ToString()) : nextDay.Day.ToString());
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + day + " , in directory: " + Dir);
                                foreach (String file in Directory.GetFiles(Dir))
                                {
                                    if (file.Contains(day) && !file.Contains("zip"))
                                    {
                                        FileName = file;
                                        isFileNameChange = true;
                                        break;
                                    }
                                }
                                lastFile = day + ".log";
                                if (escape++ > 100)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Can not find a file to parse in directory : " + Dir);
                                    break;
                                }
                                else if (isFileNameChange)
                                    break;
                            }
                        }
                    }
                    #endregion
                }
                else if (fileNameType == "extend")
                {
                    #region searchingExtendFileType
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Looking for extend files");
                    String[] fileNameArr = null;
                    String[] fileNameArr2 = null;
                    String[] oldFileArr = null;
                    Int32 lastFileNumber = 0;
                    Int32 biggestFileNumber = 9999999;
                    Int32 oldFileNumber = 0;
                    if (lastFile == "")
                    {
                        oldFileNumber = 0;
                    }
                    else
                    {
                        oldFileArr = lastFile.Split('\\');
                        oldFileNumber = Convert.ToInt32(oldFileArr[oldFileArr.Length - 1].Split('.')[0].Remove(0, 6));
                    }
                    if (oldFileNumber > 0)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "Searching next extend file after extend" + oldFileNumber.ToString());
                        try
                        {
                            if (fileIsFinished)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "Position is at the end of file changing the file..");
                                foreach (String file in Directory.GetFiles(Dir))
                                {
                                    if (file.Contains("extend") && !file.Contains("zip"))
                                    {
                                        fileNameArr = file.Split('\\');
                                        fileNameArr2 = fileNameArr[fileNameArr.Length - 1].Split('.');
                                        lastFileNumber = Convert.ToInt32(fileNameArr2[0].Remove(0, 6));
                                        if (biggestFileNumber > lastFileNumber && lastFileNumber > oldFileNumber)
                                        {
                                            FileName = file;
                                            biggestFileNumber = lastFileNumber;
                                            isFileNameChange = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "Position is not at the end of file parsing continues..");
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Error on file changing");
                        }
                        finally { }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "No lastfile found so getting last file");
                        biggestFileNumber = 0;
                        try
                        {
                            foreach (String file in Directory.GetFiles(Dir))
                            {
                                if (file.Contains("extend") && !file.Contains("zip"))
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "fileName is : " + file);
                                    fileNameArr = file.Split('\\');
                                    fileNameArr2 = fileNameArr[fileNameArr.Length - 1].Split('.');
                                    lastFileNumber = Convert.ToInt32(fileNameArr2[0].Remove(0, 6));
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "FileNumber: " + lastFileNumber);
                                    if (lastFileNumber > biggestFileNumber)
                                    {
                                        FileName = file;
                                        isFileNameChange = true;
                                        biggestFileNumber = lastFileNumber;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                        }
                    }
                    #endregion
                }
                else if (fileNameType == "exMonth")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Looking for exMonth files");
                    #region searchingExMonthFileType
                    if (lastFile == "" || lastFile == null)
                    {
                        FileInfo fi = null;
                        DateTime lastCreationDate = DateTime.MinValue;
                        foreach (String file in Directory.GetFiles(Dir))
                        {
                            fi = new FileInfo(file);
                            if (fi.CreationTime > lastCreationDate && file.EndsWith(".log") && !file.Contains("zip"))
                            {
                                FileName = file;
                                isFileNameChange = true;
                                lastCreationDate = fi.CreationTime;
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "File found : " + FileName);
                    }
                    else if (fileFoundToParse)
                    {
                        if (fileIsFinished)
                        {
                            int escape = 0;
                            while (true)
                            {
                                String nextMonth = "";
                                String calculateDate = "";
                                Int32 year = 0;
                                Int32 month = 0;
                                //String day = "";
                                String modifiedLastFile = "";
                                modifiedLastFile = lastFile.Substring(lastFile.Length - 10);
                                calculateDate = modifiedLastFile.Remove(0, 2);
                                year = Convert.ToInt32(calculateDate.Substring(0, 2));
                                month = Convert.ToInt32(calculateDate.Substring(2, 2));
                                if (month == 12)
                                {
                                    year++;
                                    month = 1;
                                }
                                else
                                {
                                    month++;
                                }
                                nextMonth = "ex" + ((year < 10) ? ("0" + year.ToString()) : year.ToString()) + ((month < 10) ? ("0" + month.ToString()) : month.ToString());
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + nextMonth + " , in directory: " + Dir);
                                foreach (String file in Directory.GetFiles(Dir))
                                {
                                    if (file.Contains(nextMonth) && !file.Contains("zip"))
                                    {
                                        FileName = file;
                                        isFileNameChange = true;
                                        break;
                                    }
                                }
                                lastFile = nextMonth + ".log";
                                if (escape++ > 100)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Can not find a file to parse in directory : " + Dir);
                                    break;
                                }
                                else if (isFileNameChange)
                                    break;
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "There are logs in file that are not parsed.");
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Parsing continues in the same file");
                        }
                    }
                    #endregion
                }
                else if (fileNameType == "exHour")
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Looking for exHour files");
                    #region searchingExHourFileType
                    if (lastFile == "" || lastFile == null)
                    {
                        FileInfo fi = null;
                        DateTime lastCreationDate = DateTime.MinValue;
                        foreach (String file in Directory.GetFiles(Dir))
                        {
                            fi = new FileInfo(file);
                            if (fi.CreationTime > lastCreationDate && file.EndsWith(".log") && file.Substring(file.Length - 14, 2) == "ex" && !file.Contains("zip"))
                            {
                                FileName = file;
                                isFileNameChange = true;
                                lastCreationDate = fi.CreationTime;
                            }
                        }
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "File found : " + FileName);
                    }
                    else if (fileFoundToParse)
                    {
                        if (fileIsFinished)
                        {
                            int escape = 0;
                            while (true)
                            {
                                String newFile = "";
                                Int32 year = 0;
                                Int32 month = 0;
                                Int32 hour = 0;
                                Int32 day = 0;
                                String modifiedLastFile = "";
                                modifiedLastFile = lastFile.Substring(lastFile.Length - 12);
                                year = Convert.ToInt32(modifiedLastFile.Substring(0, 2));
                                month = Convert.ToInt32(modifiedLastFile.Substring(2, 2));
                                day = Convert.ToInt32(modifiedLastFile.Substring(4, 2));
                                hour = Convert.ToInt32(modifiedLastFile.Substring(6, 2));
                                DateTime newDate = DateTime.MinValue;
                                newDate = newDate.AddYears(2000 + year - 1);
                                newDate = newDate.AddMonths(month - 1);
                                newDate = newDate.AddDays(day - 1);
                                newDate = newDate.AddHours(hour);
                                newDate = newDate.AddHours(1);
                                newFile = ("ex" + newDate.Year.ToString().Substring(2, 2) + ((newDate.Month < 10) ? ("0" + newDate.Month.ToString()) : newDate.Month.ToString()) + ((newDate.Day < 10) ? ("0" + newDate.Day.ToString()) : newDate.Day.ToString()) + ((newDate.Hour < 10) ? ("0" + newDate.Hour.ToString()) : newDate.Hour.ToString()));
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Searching for file: " + newFile + " , in directory: " + Dir);
                                foreach (String file in Directory.GetFiles(Dir))
                                {
                                    if (file.Contains(newFile) && !file.Contains("zip"))
                                    {
                                        FileName = file;
                                        isFileNameChange = true;
                                        break;
                                    }
                                }
                                lastFile = newFile + ".log";
                                if (escape++ > 100)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Can not find a file to parse in directory : " + Dir);
                                    break;
                                }

                                if (isFileNameChange)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, "There are logs in file that are not parsed.");
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Parsing continues in the same file");
                        }
                    }
                    #endregion
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, "Unknown IIS file format (not like ex file or extend files)");
                }
            }
            else
                FileName = Dir;

            if (isFileNameChange)
            {
                lastFile = FileName;
                Position = 0;
                isFileNameChange = false;
                Log.Log(LogType.FILE, LogLevel.INFORM, "FileName Changed new file" + lastFile + " " + FileName);
            }
            else
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, "No new file found to parse");
            }
        } // ParseFileNameLocal

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
        } // GetFiles

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
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Cannot read keywords, but parsing will continue");
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
            }
            base.Start();
        } // Start
    }
}
