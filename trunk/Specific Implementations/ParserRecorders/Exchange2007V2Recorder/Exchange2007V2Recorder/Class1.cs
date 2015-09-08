
/* 
 * Developer    : Onur Sarıkaya
 * Date         : 03.07.2012
 * 
 * KOM'da mevcut olan Exchange 2010 için 2007 dll deki parse yapısı daha uygun olduğu için 2007 dll'i kullanılmış ancak dosya atlama gibi bir sıkıntı meydana gelmiştir.
 * Mevcut dll'in setlastfile, sortfile, isfilefinished fonksiyonları yeniden yazılıp dll V2 adıyla yeniden çıkarılmıştır.
 * Exchange2007V2 dll şuan sorunsuz çalışmaktadır.
 * 
 * İlgili kişi : Nezih Ünal 
 */


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;
using System.Globalization;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        public bool IsFolderFinished;
        public string fileName;
    }

    public class Exchange2007V2Recorder : Parser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter
        private int num;
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;


        //MSGTRKM20120105-1.LOG
        public Exchange2007V2Recorder()//recorderName
            : base()
        {
            LogName = "Exchange2007V2Recorder";//recorderName***
            RecordFields = new Fields();
            usingKeywords = false;
        } // Exchange2010Recorder

        public override void Init()
        {
            GetFiles();
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
        } // Init

        public override void Start()
        {
            base.Start();
        } // Start

        // dsfd
        public Exchange2007V2Recorder(String fileName)//recorderName***
            : base(fileName)
        {

        } // ILMRecorder

        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
            try
            {
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnLocal();
                    fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is successfully FINISHED");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
            }
        } // ParseFileNameLocal

        protected override void ParseFileNameRemote()
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseFileNameRemote() -->> is STARTED");
            try
            {
                se = new SshExec(remoteHost, user);
                se.Password = password;
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameRemote() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnRemote();
                    fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);
                }
                else
                {
                    FileName = Dir;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameRemote() -->> An eror occurred : " + ex.ToString());
            }
        } // ParseFileNameRemote

        public override bool ParseSpecific(string line, bool dontSend)
        {
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
                    String[] fields = line.Split(',');
                    Int32 count = 0;
                    foreach (String field in fields)
                    {
                        if (field.StartsWith("#Fields:"))
                        {
                            String[] arr = field.Split(' ');
                            if (arr.Length == 2)
                            {
                                dictHash.Add(arr[1], count);
                                count++;
                            }
                            continue;
                        }
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
                return true;
            }
            if (!dontSend)
            {
                String[] arr = SpaceSplit(line, false, '"', true, ',');

                if (arr.Length < 2)
                    return true;
                else if (arr.Length > 19)
                {
                    try
                    {
                        Rec r = new Rec();
                        Int32 dateIndex = dictHash["date-time"];
                        arr[dateIndex] = arr[dateIndex].TrimEnd('Z');
                        String[] arrDate = arr[dateIndex].Split('T');

                        arrDate[0] = arrDate[0].Replace('-', '/');

                        r.Datetime = arrDate[0] + " " + arrDate[1];

                        r.EventCategory = arr[dictHash["event-id"]];
                        try
                        {
                            r.CustomInt6 = Convert.ToInt64(arr[dictHash["total-bytes"]]);
                        }
                        catch
                        {
                            r.CustomInt6 = -1;
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["recipient-count"]]);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }

                        try
                        {
                            r.EventId = Convert.ToInt64(arr[dictHash["internal-message-id"]]);
                        }
                        catch
                        {
                            r.EventId = -1;
                        }
                        //Recipient Status
                        String[] arrRecpStatus = arr[dictHash["recipient-status"]].Split(' ');
                        if (arrRecpStatus.Length > 2)
                        {
                            try
                            {
                                r.CustomInt1 = Convert.ToInt32(arrRecpStatus[0]);
                            }
                            catch
                            {
                                r.CustomInt1 = -1;
                            }
                        }
                        try//if log format is journal.report than sender-address may not be exist..
                        {
                            r.CustomStr3 = arr[dictHash["sender-address"]];
                            r.CustomStr10 = arr[dictHash["return-path"]];
                        }
                        catch { }
                        r.CustomStr2 = (arr[dictHash["message-subject"]]).ToString(CultureInfo.InvariantCulture);
                        try
                        {
                            if (arr[dictHash["recipient-address"]].Length > 890)
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, "recipient-address is too big for 1 column, CustomStr8 and CustomStr5 will be used for this recipient-address");
                                StringBuilder sb = new StringBuilder();
                                String temp = arr[dictHash["recipient-address"]];
                                Char[] tempArr = temp.ToCharArray();
                                Int32 i = 0;
                                Int32 val = 0;
                                for (i = 0; i < tempArr.Length; i++)
                                {
                                    sb.Append(tempArr[i]);
                                    if (i > 890 && val == 0)
                                    {
                                        r.CustomStr1 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                    else if ((i > 8880 && val == 1) || (val == 1 && i == tempArr.Length - 1))
                                    {
                                        r.CustomStr8 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                    else if ((i > 16870 && val == 2) || (val == 2 && i == tempArr.Length - 1))
                                    {
                                        r.CustomStr5 = sb.ToString();
                                        sb.Remove(0, sb.Length);
                                        val++;
                                    }
                                }
                            }
                            else
                                r.CustomStr1 = arr[dictHash["recipient-address"]];

                        }
                        catch (Exception e)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Error at parsing recipient-address");
                        }
                        r.CustomStr4 = arr[dictHash["client-ip"]];
                        //r.CustomStr5 = arr[dictHash["client-hostname"]];
                        r.CustomStr6 = arr[dictHash["message-id"]];
                        r.CustomStr7 = arr[dictHash["related-recipient-address"]];
                        r.CustomStr9 = arr[dictHash["server-ip"]];
                        r.SourceName = arr[dictHash["source"]];
                        r.EventType = arr[dictHash["connector-id"]];
                        r.Description = arr[dictHash["source-context"]] + " - " + arr[dictHash["reference"]];
                        r.LogName = "MsExchange2007Recorder";
                        r.ComputerName = arr[dictHash["server-hostname"]];
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
                else
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Log is in wrong format");
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Line : " + line);
                }
            }
            return true;
        }

        /// <summary>
        /// Check the given file is finished or not 
        /// </summary>
        /// <param name="file">File full path which will check</param>
        /// <returns>if file finished return true</returns>
        private Boolean IsFileFinished(String file)
        {
            int lineCount = 0;
            String stdOut = "";
            String stdErr = "";
            String commandRead;
            StringReader stReader;
            String line = "";

            try
            {
                if (remoteHost != "")
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> remote host is " + remoteHost);

                    if (readMethod == "nread")
                    {
                        commandRead = "nread" + " -n " + Position + "," + 3 + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> CommandRead For nread Is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> CommandRead returned strOut : " + stdOut);

                        stReader = new StringReader(stdOut);
                        while ((line = stReader.ReadLine()) != null)
                        {
                            if (line.StartsWith("~?`Position"))
                            {
                                continue;
                            }
                            lineCount++;
                        }
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> Will read line count is : " + lineCount);
                    }
                    else
                    {
                        commandRead = "sed" + " -n " + Position + "," + (Position + 2) + "p " + file;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> CommandRead For nread is : " + commandRead);

                        se.Connect();
                        se.RunCommand(commandRead, ref stdOut, ref stdErr);
                        se.Close();

                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> CommandRead returned strOut : " + stdOut);

                        stReader = new StringReader(stdOut);

                        while ((line = stReader.ReadLine()) != null)
                        {
                            lineCount++;
                        }
                    }

                    if (lineCount > 1)
                        return false;
                    else
                        return true;
                }
                else
                {
                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                " IsFileFinished() -->> reading local file! " + file);
                        fileStream.Seek(Position, SeekOrigin.Begin);
                        FileInfo fileInfo = new FileInfo(file);
                        Int64 fileLength = fileInfo.Length;
                        Byte[] byteArray = new Byte[3];
                        fileStream.Read(byteArray, 0, 3);
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> fileLength : " + fileLength);
                        Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> Position : " + Position);

                        if (Position + 1 == fileLength)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                            return true;
                        }

                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return false. ");
                            return false;
                        }

                        //if (byteArray[2] == 0)
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return true.");
                        //    RecordFields.fileName = file;
                        //    return true;
                        //}
                        //else
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> return false. ");
                        //    return false;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
                return false;
            }
        }

        private bool IsFolderComplete(string fileName)
        {
            Log.Log(LogType.FILE, LogLevel.ERROR, "IsFolderComplete is starting.");

            for (int i = 0; i < fileName.Length; i++)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFolderComplete() -->" + fileName[i]);
            }

            bool returnStatement = false;
            try
            {
                List<String> fileNameList = GetFileNamesOnLocal();
                fileNameList = SortFileNames(fileNameList);
                Dictionary<string, int> dict = new Dictionary<string, int>();
                for (int i = 0; i < fileNameList.Count; i++)
                {
                    dict.Add(fileNameList[i], i);
                }

                if (dict.ContainsKey(fileName))
                {
                    int value = dict[fileName];
                    RecordFields.num3 = value;
                    if (RecordFields.num3 == fileNameList.Count - 1)
                    {
                        returnStatement = true;
                    }
                    else
                    {
                        returnStatement = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFolderComplete() -->> An error occurred." + ex.ToString());
                returnStatement = false;
            }
            return returnStatement;
        } // IsFileFinished

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
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Mesaj: " + e.StackTrace);
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
                }
            }
        } // GetFiles

        protected override void dayChangeTimer_Elapsed(Object sender, ElapsedEventArgs e)
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
                    Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> File changed, new file is, " + FileName);
                }
                base.Start();
            }
            dayChangeTimer.Start();
        } // dayChangeTimer_Elapsed

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                List<String> fileNameList = new List<String>();
                foreach (String fileName in Directory.GetFiles(Dir))
                {
                    String fileShortName = Path.GetFileName(fileName).ToString();
                    fileNameList.Add(fileShortName);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnLocal

        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnRemote()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is STARTED ");

                String line = "";
                String stdOut = "";
                String stdErr = "";

                String command = "ls -lt " + Dir;//FileNames contains what.*** fileNameFilter
                Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> SSH command : " + command);

                se.Connect();
                se.RunCommand(command, ref stdOut, ref stdErr);
                se.Close();

                StringReader stringReader = new StringReader(stdOut);
                List<String> fileNameList = new List<String>();
                Boolean foundAnyFile = false;

                while ((line = stringReader.ReadLine()) != null)
                {
                    String[] arr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameList.Add(arr[arr.Length - 1]);
                    Log.Log(LogType.FILE, LogLevel.DEBUG, "GetFileNamesOnRemote() -->> File name is read: " + arr[arr.Length - 1]);
                    foundAnyFile = true;
                }

                if (!foundAnyFile)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetFileNamesOnRemote() -->> There is no proper file in directory");
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnRemote() -->> is successfully FINISHED");
                return fileNameList;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnRemote() -->> An error occurred :" + ex.ToString());
                return null;
            }
        } // GetFileNamesOnRemote
        /// <summary>
        /// Select the suitable last file in the file name list
        /// </summary>
        /// <param name="fileNameList">The file names are in the stored directory</param>
        private void SetLastFile(List<string> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);

                Dictionary<string, int> dictionary = new Dictionary<string, int>();
                ArrayList list = new ArrayList();

                for (int num = 0; num < fileNameList.Count; num++)
                {
                    dictionary.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture), num);
                    list.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture));
                }

                if (list.Count > 0)
                {
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile, new int[0]);
                        string item = lastFile.Replace(Dir, "");
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> item is " + item);
                        if (fileNameList.Contains(item))
                        {
                            if (this.IsFileFinished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
                                //int num2 = fileNameList.BinarySearch(lastFile);
                                //for (num = 0; num < list.Count; num++)
                                //{
                                //    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> FileName : " + list[num].ToString(), new int[0]);
                                //}

                                string key = item;
                                if (dictionary.ContainsKey(key))
                                {

                                    RecordFields.num3 = dictionary[key];
                                    //FileName = lastFile;
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                            FileName, new int[0]);
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                            RecordFields.num3.ToString(CultureInfo.InvariantCulture));

                                    if ((RecordFields.num3 + 1) != list.Count)
                                    {
                                        FileName = Dir + list[RecordFields.num3 + 1].ToString();
                                        Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.num3 + 1].ToString(), " Value ", RecordFields.num3 }), new int[0]);
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName, new int[0]);
                                        RecordFields.num3 = RecordFields.num3 + 1;
                                    }
                                    else
                                    {
                                        //FileName = lastFile;
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
                                    }
                                }
                            }
                            else
                            {
                                FileName = lastFile;
                            }
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
                            //FileName = Dir + fileNameList[fileNameList.Count - 1];
                            FileName = Dir + fileNameList[fileNameList.Count + 1];
                            lastFile = FileName;
                            //Position = 0;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
                        }
                    }
                    else
                    {
                        FileName = Dir + fileNameList[0];
                        lastFile = FileName;
                        //Position = 0;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
                }
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile

        private int CoderParse(String line, ref CustomTools.CustomBase.Rec rec)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Line is : " + line + "");
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Arraylist fill started.");
                try
                {
                    string[] arr;
                    if (!line.StartsWith("#"))
                    {
                        //if (line.Length > 899)
                        {
                            arr = line.Split('-');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i].StartsWith("{"))
                                {
                                    rec.EventCategory = arr[i].Replace("{", " ").Replace("}", " ");
                                }
                                if (arr[i].StartsWith("<"))
                                {
                                    rec.CustomStr2 = arr[i].Replace("<", " ").Replace(">", " ");
                                }

                                if (arr[i].Contains("Sicil:"))
                                {
                                    try
                                    {
                                        rec.CustomInt6 = Convert.ToInt32(arr[i].Split(':')[1]);
                                    }
                                    catch (Exception)
                                    {
                                        rec.CustomInt6 = 0;
                                    }
                                }
                            }

                            //DateTime dt = Convert.ToDateTime(arr[4]);
                            rec.Datetime = lastFile.Split(' ')[2].Split('.')[0]; ;//DateTime.Now.ToString();//dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> datetime" + rec.Datetime);
                            rec.CustomStr1 = arr[6];
                            rec.CustomStr7 = arr[4];
                            rec.EventType = Between(line, "->", "Sicil");
                            rec.Description = line;

                            //Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime" + dt.ToString("yyyy-MM-dd HH:mm:ss"));

                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime" + rec.Datetime);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr1" + rec.CustomStr1);

                            //Date time 14.05.2012 10.05.255 şeklinde geldiği ve bu bizim formatımıza convert edilirken hata verdiğinden 
                            // orj date time customstr7'ye basılıp dosya adındaki date datetime'e basıldı.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> line is unrecognized, unable to parse!" + ex.StackTrace);
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
                return 1;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> " + ex.StackTrace);
                return 0;
            }
        } // CoderParse

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

        /// <summary>
        /// Control giving line will reading or skipping
        /// </summary>
        /// <param name="line">Will checking line</param>
        /// <returns>Returns line will reading or skipping</returns>
        private Boolean IsSkipKeyWord(String line)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is STARTED ");
                if (null != _skipKeyWords && _skipKeyWords.Length > 0)
                {
                    foreach (String item in _skipKeyWords)
                    {
                        if (line.StartsWith(item))
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned True ");
                            return true;
                        }
                    }
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned False");
                return false;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " IsSkipKeyWord() -->> An error occured" + ex.ToString());
                return false;
            }
        } // IsSkipKeyWord

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        private List<String> SortFileNames(List<String> fileNameList)
        {
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            try
            {

                List<String> _fileNameList = new List<String>();
                List<String> _fileNameListNew = new List<String>();
                FileNameProp fileNameProp = new FileNameProp();
                FileNameProp[] fileNamePropArray;
                long[] fileNameindexes = new long[fileNameList.Count];


                if (_fileNameFilters != null)
                {
                    foreach (String fileName in fileNameList)
                    {
                        foreach (String fileNamefilter in _fileNameFilters)
                        {
                            fileName.Contains(fileNamefilter);
                            _fileNameList.Add(fileName);
                            break;
                        }
                    }
                }
                else
                {
                    _fileNameList = fileNameList;
                }
                fileNamePropArray = new FileNameProp[_fileNameList.Count];

                int k = 0;
                foreach (String line in _fileNameList)
                {
                    fileNameProp = new FileNameProp();
                    fileNameProp.FileName = line;

                    String[] subLine0 = line.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    fileNameProp.FNGenericPart = subLine0[0];

                    int splitIndex = subLine0[0].IndexOf("20");//valid for about 90 years

                    fileNameProp.FNYearPart = subLine0[0].Substring(splitIndex, 4);
                    fileNameProp.FNMonthPart = subLine0[0].Substring(splitIndex + 4, 2);
                    fileNameProp.FNDayPart = subLine0[0].Substring(splitIndex + 6, 2);

                    fileNameProp.FNNumberPart = subLine0[1];


                    SetFileNumber(ref fileNameProp);
                    fileNamePropArray[k] = fileNameProp;
                    fileNameindexes[k] = fileNameProp.FileNumber;
                    k++;
                }

                if (fileNamePropArray[0].FileNumber == 0)
                {
                    Array.Sort(fileNamePropArray, new FileNamePropComparerByGenericPart());
                }
                else
                {
                    Array.Sort(fileNameindexes, fileNamePropArray);
                }

                _fileNameList.Clear();
                foreach (FileNameProp item in fileNamePropArray)
                {
                    string fileName = item.FileName;
                    if (string.IsNullOrEmpty(tempCustomVar1) || tempCustomVar1 == "MSGTRK")
                    {
                        if (item.FileName.StartsWith("MSGTRK") && !item.FileName.StartsWith("MSGTRKM"))
                        {
                            _fileNameList.Add(fileName);
                            _fileNameListNew.Add(fileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
                        }
                    }
                    else if (tempCustomVar1 == "MSGTRKM")
                    {
                        if (item.FileName.StartsWith("MSGTRKM"))
                        {
                            _fileNameList.Add(fileName);
                            _fileNameListNew.Add(fileName);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
                        }
                    }
                    //Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
                }

                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is successfully FINISHED.");
                for (int i = 0; i < _fileNameList.Count; i++)
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> FileNames : " + _fileNameList[i].ToString());
                }
                return _fileNameListNew;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SortFileNames() -->> An error occurred" + ex.ToString());
                return null;
            }

            //  foreach (FileNameProp item in fileNamePropArray)
            //  {
            //    _fileNameList.Add(item.FileName);
            //    Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Sorting file name is " + item.FileName);
            //  }

            //  Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is successfully FINISHED.");
            //  return _fileNameList;
            //}
            //catch (Exception ex)
            //{
            //  Log.Log(LogType.FILE, LogLevel.ERROR, " SortFileNames() -->> An error occurred" + ex.ToString());
            //  return null;
            //}
        } // SortFileNames

        /// <summary>
        /// Create file number given file properties
        /// </summary>
        /// <param name="fileNameProp">File date properties</param>
        private void SetFileNumber(ref FileNameProp fileNameProp)
        {
            String _fileNumber = "";
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is STARTED");

                _fileNumber = fileNameProp.FNYearPart +
                                        fileNameProp.FNMonthPart +
                                        fileNameProp.FNDayPart +
                                        fileNameProp.FileNumber;

                fileNameProp.FileNumber = Convert.ToInt64(_fileNumber);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is successfully FINISHED");
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetFileNumber() -->> An error occurred" + ex.ToString());
            }

        } // SetFileNumber

        /*
                /// <summary>
                /// Create file number given file properties
                /// </summary>
                /// <param name="fileNameProp">File date properties</param>
                private void SetFileNumber(ref FileNameProp fileNameProp)
                {
                    String _fileNumber = "";
                    try
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is STARTED");

                        _fileNumber = fileNameProp.FNYearPart +
                                                fileNameProp.FNMonthPart +
                                                fileNameProp.FNDayPart +
                                                fileNameProp.FileNumber;

                        fileNameProp.FileNumber = Convert.ToInt64(_fileNumber);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetFileNumber() -->> is successfully FINISHED");
                    }
                    catch (Exception ex)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " SetFileNumber() -->> An error occurred" + ex.ToString());
                    }

                } */
        // SetFileNumber

        class FileNameProp : IComparable
        {
            public String FileName = "0";
            public Int64 FileNumber = 0;
            public String FNYearPart = "0";
            public String FNMonthPart = "0";
            public String FNDayPart = "0";
            public String FNNumberPart = "0";
            public String FNGenericPart = "0";

            public int CompareTo(object obj)
            {
                FileNameProp sampleFNNo = (FileNameProp)obj;
                return Convert.ToInt32(this.FileNumber - sampleFNNo.FileNumber);
            }
        }

        class FileNamePropComparerByGenericPart : IComparer
        {
            public int Compare(object x, object y)
            {
                FileNameProp fnp1 = (FileNameProp)x;
                FileNameProp fnp2 = (FileNameProp)y;
                return String.Compare(fnp1.FNGenericPart, fnp2.FNGenericPart);
            }
        }

        public struct newFields
        {
            public string datetime;
            public string clientIp;
            public string clientHostname;
            public string serverIp;
            public string serverHostname;
            public string sourceContext;
            public string connectorId;
            public string source;
            public string eventId;
            public string internalMessageId;
            public string messageId;
            public string recipientAddress;
            public string recipientStatus;
            public string totalBytes;
            public string recipientCount;
            public string relatedRecipientAddress;
            public string reference;
            public string messageSubject;
            public string senderAddress;
            public string returnPath;
            public string messageInfo;
            public string directionality;
            public string tenantId;
            public string originalClientIp;
            public string originalServerIp;
            public string customData;

            public void newFunc(ref Parser.Rec Rec)
            {

            } // newFunc
        }
    }
}

//
