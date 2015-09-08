using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Exchange2010SP2V1_0_2Recorder;
using Microsoft.Win32;
using System.IO;
using System.Timers;
using CustomTools;
using Log;
using SharpSSH.SharpSsh;
using System.Collections;
using System.Globalization;
using System.Threading;

namespace Parser
{
    public struct Fields
    {
        public int num3;
        //public long lineNumber;
        public string currentFile;
    }

    public class Exchange2010SP2V1_0_2Recorder : Parser
    {
        private String[] _skipKeyWords = null;
        private String[] _fileNameFilters = null;
        private int num;
        private Fields RecordFields;
        Dictionary<String, Int32> dictHash;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";
        object syncRoot = new object();

        public Exchange2010SP2V1_0_2Recorder()
            : base()
        {
            LogName = "Exchange2010SP2V1_0_2Recorder";
            RecordFields = new Fields();
            usingKeywords = true;
        } // Exchange2010SP2V1_0_2Recorder
        //
        public override void Init()
        {//
            Log.SetLogFileSize(100000000);

            try
            {
                GetFiles();
                Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");//
            }
            catch (Exception exception)
            {

            }
        }

        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " fileSystemWatcher_Changed() -->> ");
        }

        public override void Start()
        {
            base.Start();
        } // Start


        public Exchange2010SP2V1_0_2Recorder(String fileName)
            : base(fileName)
        {

        } // Exchange2010SP2V1_0_2Recorder

        protected override void ParseFileNameLocal()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                try
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
                    if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                        List<String> fileNameList = GetFileNamesOnLocal();
                        fileNameList = SortFileNames(fileNameList);
                        for (int i = 0; i < fileNameList.Count; i++)
                        {
                            Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> Sorting Files. " + fileNameList[i]);
                        }
                        SetLastFile(fileNameList);
                    }
                    else
                    {
                        FileName = Dir;
                    }
                    Log.Log(LogType.FILE, LogLevel.DEBUG, " ParseFileNameLocal() -->> is successfully FINISHED");
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
                }

                finally
                {
                    Monitor.Exit(syncRoot);
                }
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

        //public override bool ParseSpecific(String line, bool dontSend)
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

        //        if (!string.IsNullOrEmpty(RecordFields.currentFile))
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG,
        //                    "ParseSpecific | currentFile : " + RecordFields.currentFile);
        //        }
        //        else
        //        {
        //            RecordFields.currentFile = lastFile;
        //        }
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | dontSend : " + dontSend);
        //    }
        //    catch (Exception exception)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Log error : " + exception.ToString());
        //        WriteMessage("ParseSpecific | Log error : " + exception.ToString());
        //    }

        //    if (string.IsNullOrEmpty(line))
        //    {
        //        return true;
        //    }

        //    Rec r = new Rec();

        //    //try
        //    //{
        //    try
        //    {
        //        if (line.StartsWith("#"))
        //        {
        //            if (line.StartsWith("#Fields:"))
        //            {
        //                if (dictHash != null)
        //                    dictHash.Clear();
        //                dictHash = new Dictionary<String, Int32>();
        //                String[] fields = line.Split(',');
        //                String[] first = fields[0].Split(' ');
        //                fields[0] = first[1];
        //                Int32 count = 0;
        //                foreach (String field in fields)
        //                {
        //                    dictHash.Add(field, count);
        //                    count++;
        //                }
        //                String add = "";

        //                foreach (KeyValuePair<String, Int32> kvp in dictHash)
        //                {
        //                    add += kvp.Key + ",";
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Keys : " + kvp.Key);
        //                }
        //                SetLastKeywords(add);
        //                keywordsFound = true;
        //            }
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + ex.Message);
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + line);
        //    }

        //    try
        //    {
        //        if (!line.StartsWith("#"))
        //        {
        //            String[] arr = line.Split(',');
        //            //for (int i = 0; i < arr.Length; i++)
        //            //{
        //            //    Log.Log(LogType.FILE, LogLevel.DEBUG, "arr[i]:  " + arr[i]);
        //            //}
        //            try
        //            {
        //                string dateString = line.Split(',')[0];
        //                string date12 = Before(dateString, "T");
        //                string time1 = Between(dateString, "T", "Z");
        //                string dateString2 = date12 + " " + time1;
        //                DateTime dt;
        //                dt = Convert.ToDateTime(dateString2);
        //                r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "Datetime : " + ex.Message);
        //            }
        //            r.LogName = LogName;

        //            string add = "";

        //            try
        //            {
        //                foreach (KeyValuePair<String, Int32> kvp in dictHash)
        //                {
        //                    add += kvp.Key + ",";
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "Values : " + kvp.Key);
        //                }
        //            }
        //            catch (Exception exception)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "foreach: " + exception.Message);
        //            }

        //            try
        //            {
        //                //if (arr.Length > dictHash["event-id"])
        //                {
        //                    r.EventCategory = arr[dictHash["event-id"]];
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
        //                }
        //                //else
        //                //{
        //                //    Position = Position - line.Length;
        //                //    return false;
        //                //}
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "EventCategory : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.EventId = Convert.ToInt64(arr[dictHash["internal-message-id"]]);
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.WARN, "EventId: " + ex.Message);
        //                Log.Log(LogType.FILE, LogLevel.WARN, "EventId Real Value: " + arr[dictHash["internal-message-id"]]);
        //            }

        //            try
        //            {
        //                r.EventType = arr[dictHash["connector-id"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "EventType : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.SourceName = arr[dictHash["source"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "SourceName : " + r.SourceName);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "SourceName : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.LogName = LogName;
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "LogName : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.ComputerName = arr[dictHash["server-hostname"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
        //            }

        //            try
        //            {
        //                string str1 = arr[dictHash["recipient-address"]];
        //                if (str1.Length > 899)
        //                {
        //                    r.CustomStr1 = str1.Substring(0, 899);
        //                }
        //                else
        //                {
        //                    r.CustomStr1 = str1;
        //                }

        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
        //            }

        //            try
        //            {

        //                r.CustomStr2 = arr[dictHash["message-subject"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 : " + ex.Message);
        //            }

        //            try
        //            {

        //                r.CustomStr3 = arr[dictHash["sender-address"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 : " + ex.Message);
        //            }

        //            try
        //            {

        //                r.CustomStr4 = arr[dictHash["client-ip"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.CustomStr5 = arr[dictHash["connector-id"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 : " + ex.Message);

        //            }

        //            try
        //            {
        //                r.CustomStr6 = arr[dictHash["message-id"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 : " + ex.Message);

        //            }

        //            try
        //            {
        //                r.CustomStr7 = arr[dictHash["recipient-address"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 : " + ex.Message);

        //            }

        //            try
        //            {
        //                if (string.IsNullOrEmpty(arr[dictHash["recipient-status"]]))
        //                {
        //                    r.CustomStr8 = arr[dictHash["recipient-status"]];
        //                    Log.Log(LogType.FILE, LogLevel.DEBUG,
        //                            "CustomInt1 : " + arr[dictHash["recipient-status"]]);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 : " + ex.Message);
        //            }

        //            try
        //            {

        //                r.CustomStr9 = arr[dictHash["server-ip"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.CustomStr10 = arr[dictHash["return-path"]];
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10 : " + r.CustomStr10);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.CustomInt2 = Convert.ToInt32(arr[dictHash["recipient-count"]]);
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + r.CustomInt2);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 : " + ex.Message);
        //                Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 Real Value : " + arr[dictHash["recipient-count"]]);
        //                r.CustomInt2 = 0;
        //            }

        //            try
        //            {
        //                r.CustomInt6 = Convert.ToInt32(arr[dictHash["total-bytes"]]);
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 : " + r.CustomInt6);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt6 : " + ex.Message);
        //                Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt6 Real Value : " + arr[dictHash["total-bytes"]]);
        //                r.CustomInt6 = 0;
        //            }

        //            try
        //            {
        //                if (line.Length > 899)
        //                {
        //                    r.Description = line.Substring(0, 899);
        //                }

        //                else
        //                {
        //                    r.Description = line;
        //                }
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + line);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "Description : " + ex.Message);
        //            }

        //            try
        //            {
        //                r.CustomStr8 = FileName;
        //            }
        //            catch (Exception exception)
        //            {
        //                Log.Log(LogType.FILE, LogLevel.ERROR, "FileName Mapping Error: " + exception.Message);
        //            }
        //        }
        //        else
        //        {
        //            Log.Log(LogType.FILE, LogLevel.DEBUG, "Line startswith #  ");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "ikinci kısım | " + ex.Message);
        //    }

        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, "Start sending data, CurrentFile is: " + FileName + " Position: " + Position);
        //        SetRecordData(r);
        //        Log.Log(LogType.FILE, LogLevel.INFORM, "Finished sending data.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, "SetRecordData : " + ex.Message);
        //        Position = Position - line.Length;
        //        return false;
        //    }

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.Message);
        //    //    Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.StackTrace);
        //    //    Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | Line : " + line);

        //    //    WriteMessage("ParseSpecific | Parsing Error : " + e.ToString());
        //    //    WriteMessage("ParseSpecific | line: " + line);
        //    //    return false;
        //    //}



        //    return true;
        //}

        public override bool ParseSpecific(String line, bool dontSend)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | Line : " + line);

                if (!string.IsNullOrEmpty(RecordFields.currentFile))
                {
                    Log.Log(LogType.FILE, LogLevel.DEBUG,
                            "ParseSpecific | currentFile : " + RecordFields.currentFile);
                }
                else
                {
                    RecordFields.currentFile = lastFile;
                }
                Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific | dontSend : " + dontSend);
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific | Log error : " + exception.ToString());
                WriteMessage("ParseSpecific | Log error : " + exception.ToString());
            }

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            Rec r = new Rec();

            try
            {
                try
                {
                    if (line.StartsWith("#"))
                    {
                        if (line.StartsWith("#Fields:"))
                        {
                            if (dictHash != null)
                                dictHash.Clear();
                            dictHash = new Dictionary<String, Int32>();
                            String[] fields = line.Split(',');
                            String[] first = fields[0].Split(' ');
                            fields[0] = first[1];
                            Int32 count = 0;
                            foreach (String field in fields)
                            {
                                dictHash.Add(field, count);
                                count++;
                            }
                            String add = "";

                            foreach (KeyValuePair<String, Int32> kvp in dictHash)
                            {
                                add += kvp.Key + ",";
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Keys : " + kvp.Key);
                            }
                            SetLastKeywords(add);
                            keywordsFound = true;
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + ex.Message);
                    Log.Log(LogType.FILE, LogLevel.ERROR, "birinci kısım : " + line);
                }

                try
                {
                    if (!line.StartsWith("#"))
                    {
                        String[] arr = line.Split(',');
                        //for (int i = 0; i < arr.Length; i++)
                        //{
                        //    Log.Log(LogType.FILE, LogLevel.DEBUG, "arr[i]:  " + arr[i]);
                        //}
                        try
                        {
                            string dateString = line.Split(',')[0];
                            string date12 = Before(dateString, "T");
                            string time1 = Between(dateString, "T", "Z");
                            string dateString2 = date12 + " " + time1;
                            DateTime dt;
                            dt = Convert.ToDateTime(dateString2);
                            r.Datetime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Datetime : " + r.Datetime);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Datetime : " + ex.Message);
                        }
                        r.LogName = LogName;

                        string add = "";

                        try
                        {
                            foreach (KeyValuePair<String, Int32> kvp in dictHash)
                            {
                                add += kvp.Key + ",";
                                Log.Log(LogType.FILE, LogLevel.DEBUG, "Values : " + kvp.Key);
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "foreach: " + exception.Message);
                        }

                        try
                        {
                            r.EventCategory = arr[dictHash["event-id"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "EventCategory : " + ex.Message);
                        }

                        try
                        {
                            r.EventId = Convert.ToInt64(arr[dictHash["internal-message-id"]]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory : " + r.EventCategory);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.WARN, "EventId: " + ex.Message);
                            Log.Log(LogType.FILE, LogLevel.WARN,
                                    "EventId Real Value: " + arr[dictHash["internal-message-id"]]);
                        }

                        try
                        {
                            r.EventType = arr[dictHash["connector-id"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "EventType : " + r.EventType);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "EventType : " + ex.Message);
                        }

                        try
                        {
                            r.SourceName = arr[dictHash["source"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "SourceName : " + r.SourceName);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "SourceName : " + ex.Message);
                        }

                        try
                        {
                            r.LogName = LogName;
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "LogName : " + r.LogName);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "LogName : " + ex.Message);
                        }

                        try
                        {
                            r.ComputerName = arr[dictHash["server-hostname"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName : " + r.ComputerName);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                        }

                        try
                        {
                            string str1 = arr[dictHash["recipient-address"]];
                            if (str1.Length > 899)
                            {
                                r.CustomStr1 = str1.Substring(0, 899);
                            }
                            else
                            {
                                r.CustomStr1 = str1;
                            }

                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1 : " + r.CustomStr1);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "ComputerName : " + ex.Message);
                        }

                        try
                        {

                            r.CustomStr2 = arr[dictHash["message-subject"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2 : " + r.CustomStr2);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2 : " + ex.Message);
                        }

                        try
                        {

                            r.CustomStr3 = arr[dictHash["sender-address"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3 : " + r.CustomStr3);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3 : " + ex.Message);
                        }

                        try
                        {

                            r.CustomStr4 = arr[dictHash["client-ip"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4 : " + r.CustomStr4);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr5 = arr[dictHash["connector-id"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5 : " + r.CustomStr5);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5 : " + ex.Message);

                        }

                        try
                        {
                            r.CustomStr6 = arr[dictHash["message-id"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6 : " + r.CustomStr6);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6 : " + ex.Message);

                        }

                        try
                        {
                            r.CustomStr7 = arr[dictHash["recipient-address"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7 : " + r.CustomStr7);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7 : " + ex.Message);

                        }

                        try
                        {
                            if (string.IsNullOrEmpty(arr[dictHash["recipient-status"]]))
                            {
                                r.CustomStr8 = arr[dictHash["recipient-status"]];
                                Log.Log(LogType.FILE, LogLevel.DEBUG,
                                        "CustomInt1 : " + arr[dictHash["recipient-status"]]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8 : " + ex.Message);
                        }

                        try
                        {

                            r.CustomStr9 = arr[dictHash["server-ip"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr9 : " + r.CustomStr9);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr9 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr10 = arr[dictHash["return-path"]];
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10 : " + r.CustomStr10);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10 : " + ex.Message);
                        }

                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(arr[dictHash["recipient-count"]]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2 : " + r.CustomInt2);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt2 : " + ex.Message);
                            Log.Log(LogType.FILE, LogLevel.WARN,
                                    "CustomInt2 Real Value : " + arr[dictHash["recipient-count"]]);
                            r.CustomInt2 = 0;
                        }

                        try
                        {
                            r.CustomInt6 = Convert.ToInt32(arr[dictHash["total-bytes"]]);
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6 : " + r.CustomInt6);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.WARN, "CustomInt6 : " + ex.Message);
                            Log.Log(LogType.FILE, LogLevel.WARN,
                                    "CustomInt6 Real Value : " + arr[dictHash["total-bytes"]]);
                            r.CustomInt6 = 0;
                        }

                        try
                        {
                            if (line.Length > 899)
                            {
                                r.Description = line.Substring(0, 899);
                            }

                            else
                            {
                                r.Description = line;
                            }
                            Log.Log(LogType.FILE, LogLevel.DEBUG, "Description : " + line);
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "Description : " + ex.Message);
                        }

                        try
                        {
                            r.CustomStr8 = FileName;
                        }
                        catch (Exception exception)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "FileName Mapping Error: " + exception.Message);
                        }

                        try
                        {
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                        "Start sending data, CurrentFile is: " + FileName + " Position: " + Position);
                                SetRecordData(r);
                                Log.Log(LogType.FILE, LogLevel.INFORM, "Finished sending data.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Log(LogType.FILE, LogLevel.ERROR, "SetRecordData : " + ex.Message);
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "Line startswith #  ");
                    }
                }
                catch (Exception ex)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "ikinci kısım | " + ex.Message);
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | " + e.StackTrace);
                Log.Log(LogType.FILE, LogLevel.ERROR, "!StartsWith(#) | Line : " + line);

                //WriteMessage("ParseSpecific | Parsing Error : " + e.ToString());
                //WriteMessage("ParseSpecific | line: " + line);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get string value before b.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Before(string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public static string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

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
                using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                            " IsFileFinished() -->> reading local file! " + file);

                    fileStream.Seek(Position, SeekOrigin.Begin);
                    FileInfo fileInfo = new FileInfo(file);
                    Int64 fileLength = fileInfo.Length;
                    Byte[] byteArray = new Byte[3];
                    fileStream.Read(byteArray, 0, 3);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> filelength : " + fileLength);
                    Log.Log(LogType.FILE, LogLevel.INFORM, " IsFileFinished() -->> Position : " + Position);

                    if (Position + 1 == fileLength || Position == fileLength)
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, "IsFileFinished() -->> " + file + " finished.");
                        WriteMessage("IsFileFinished() -->> " + file + " Position: " + Position);
                        WriteMessage("IsFileFinished() -->> " + file + " fileLength: " + fileLength);
                        WriteMessage("IsFileFinished() -->> " + file + " finished.");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    //using (StreamReader sr = new StreamReader(file))
                    //{
                    //    while ((line = sr.ReadLine()) != null)
                    //    {
                    //        return true;
                    //    }
                    //    return false;
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
                WriteMessage("IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
                WriteMessage("IsFileFinished() -->> " + ex.StackTrace);
                return false;
            }
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

        ///// <summary>
        ///// Select the suitable last file in the file name list
        ///// </summary>
        ///// <param name="fileNameList">The file names are in the stored directory</param>
        //private void SetLastFile(List<string> fileNameList)
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
        //        Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);

        //        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        //        ArrayList list = new ArrayList();

        //        for (int num = 0; num < fileNameList.Count; num++)
        //        {
        //            dictionary.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture), num);
        //            list.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture));
        //        }

        //        if (list.Count > 0)
        //        {
        //            if (!string.IsNullOrEmpty(lastFile))
        //            {
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile);
        //                string item = lastFile.Replace(Dir, "");
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> item is " + item);

        //                if (fileNameList.Contains(item))
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> num3: " + RecordFields.num3 + "list.count: " + list.Count);
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> fName: " + list[RecordFields.num3]);

        //                    if ((RecordFields.num3 + 1) != list.Count)
        //                    {
        //                        if (IsFileFinished(lastFile))
        //                        {
        //                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
        //                            string key = item;
        //                            if (dictionary.ContainsKey(key))
        //                            {
        //                                RecordFields.num3 = dictionary[key];
        //                                Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
        //                                        FileName);
        //                                Log.Log(LogType.FILE, LogLevel.INFORM,
        //                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
        //                                        RecordFields.num3.ToString(CultureInfo.InvariantCulture));

        //                                if ((RecordFields.num3 + 1) != list.Count)
        //                                {
        //                                    FileName = Dir + list[RecordFields.num3 + 1].ToString();
        //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.num3 + 1].ToString(), " Value ", RecordFields.num3 }));
        //                                    lastFile = FileName;
        //                                    Position = 0;
        //                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName);
        //                                    RecordFields.num3 = RecordFields.num3 + 1;
        //                                    RecordFields.currentFile = lastFile;
        //                                }
        //                                else
        //                                {
        //                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            FileName = lastFile;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile);
        //                    //lastFile = "";
        //                    FileName = Dir + fileNameList[fileNameList.Count + 1];
        //                    lastFile = FileName;
        //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName);
        //                }//
        //            }
        //            else
        //            {
        //                FileName = Dir + fileNameList[0];
        //                lastFile = FileName;
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile);
        //                RecordFields.currentFile = lastFile;
        //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile);
        //            }
        //        }
        //        else
        //        {
        //            Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir);
        //            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
        //        WriteMessage(" SetLastFile() -->> An error occurred : " + exception.ToString());
        //    }
        //} // SetLastFile

        private void SetLastFile(List<string> fileNameList)
        {

            for (int i = 0; i < fileNameList.Count; i++)
            {
                //Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> " + fileNameList[i].ToString());
                fileNameList[i] = Dir + fileNameList[i];
            }
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> LastFile: ");
            try
            {
                if (fileNameList != null && fileNameList.Count > 0)
                {
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        FileInfo inf = new FileInfo(lastFile);
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                            " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile);
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> item is " + inf.FullName);

                        int idx;


                        if ((idx = fileNameList.IndexOf(inf.FullName)) >= 0)
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> idx is " + idx.ToString());
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> fileNameList.Count is " + fileNameList.Count.ToString());

                            if (idx + 1 != fileNameList.Count)
                            {
                                if (IsFileFinished(lastFile))
                                {
                                    idx++;
                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName);

                                    if (idx < fileNameList.Count)
                                    {
                                        FileName = fileNameList[idx];
                                        Log.Log(LogType.FILE, LogLevel.DEBUG,
                                                " SetLastFile() -->> Onur--- : " + FileName + " Value " + idx);
                                        lastFile = FileName;
                                        Position = 0;
                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " SetLastFile() -->> Last file is finished. New file is  : " + FileName);

                                        if (tempCustomVar1.ToLower() == "[delete]")
                                        {
                                            for (int i = 0; i < idx; i++)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Deleting file " + fileNameList[i]);
                                                File.Delete(fileNameList[i]);
                                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Deleted file " + fileNameList[i]);
                                            }
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
                                FileName = lastFile;
                            }
                        }
                        else
                        {
                            FileName = lastFile;
                        }
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile);
                        FileName = fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->> No input file in directory:" + Dir);
                    FileName = null;
                    lastFile = FileName;
                }

            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
            }
        } // SetLastFile

        public bool FileExistControl(List<string> fileNameList, string fileName)
        {
            try
            {
                return true;
            }
            catch (Exception exception)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " FileExistControl() -->> An error occurred : " + exception.ToString());
                return false;
            }
        } // FileExistControl

        public bool FileFinishControl(long position, long fileLength)
        {
            if (position == fileLength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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

        protected FileNameComperator filenameComperator = new FileNameComperator();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        /// 

        //private List<string> SortFileNames(List<string> fileNameList)
        //{
        //    Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> is STARTED. ");
        //    List<string> filteredFileNameList = new List<string>();
        //    foreach (var fileName in fileNameList)
        //    {
        //        if (string.IsNullOrEmpty(tempCustomVar1) || tempCustomVar1 == "MSGTRK")
        //        {
        //            if (fileName.StartsWith("MSGTRK") && !fileName.StartsWith("MSGTRKM"))
        //            {
        //                filteredFileNameList.Add(fileName);
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Adding file name is " + fileName);
        //            }
        //        }
        //        else if (tempCustomVar1 == "MSGTRKM")
        //        {
        //            if (fileName.StartsWith("MSGTRKM"))
        //            {
        //                filteredFileNameList.Add(fileName);
        //                Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> Adding file name is " + fileName);
        //            }//
        //        }
        //    }
        //    //MSGTRK20130610-1.LOG
        //    filteredFileNameList.Sort(filenameComperator);
        //    foreach (var item in filteredFileNameList)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> item is: " + item);
        //    }
        //    Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() -->> is Finished. ");
        //    return fileNameList;
        //} // SortFileNames

        private List<String> SortFileNames(List<String> fileNameList)
        {
            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                FileInfo f = new FileInfo(fileNameList[i]);
                string[] fn = f.Name.Split('.');
                if (fn.Length == 2 && fn[1] == "LOG")
                {
                    if (string.IsNullOrEmpty(tempCustomVar1) || tempCustomVar1 == "MSGTRK")
                    {
                        if (fn[0].StartsWith("MSGTRK") && !fn[0].StartsWith("MSGTRKM"))
                        {
                            _fileNameList.Add(fileNameList[i]);
                        }
                    }
                    else if (tempCustomVar1 == "MSGTRKM")
                    {
                        if (fn[0].StartsWith("MSGTRKM"))
                        {
                            _fileNameList.Add(fileNameList[i]);
                        }//
                    }
                }
            }
            _fileNameList.Sort(filenameComperator);
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            foreach (string t in _fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames

        public void WriteMessage(string line)
        {
            string path = @"C:\ExchangeTest\\ExchangeLog.log";

            string directoryPath = Before(path, "\\");
            if (!Directory.Exists("C:\\ExchangeTest"))
            {
                Directory.CreateDirectory("C:\\ExchangeTest");
            }

            if (!File.Exists(path))
            {
                File.Create(path);
            }

            File.AppendAllText(path, DateTime.Now.ToString(dateFormat) + (" - " + line));
            File.AppendAllText(path, "\r\n");
            File.AppendAllText(path, "***********************************************");
            File.AppendAllText(path, "\r\n");
        }
    }
}

//
