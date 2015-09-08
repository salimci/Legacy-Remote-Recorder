/*
 Onur sarıkaya
 * 
 * 28.05.2013 tarihinde EventCategory, EventType ve LogName kısmında bir güncelleme yapılmıştır.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Timers;
using CustomTools;
using DAL;
using Log;
using NT2008EventLogFileRecorder;
using SharpSSH.SharpSsh;
using System.Diagnostics;

namespace Parser
{
    public class NT2008EventLogFileRecorder : EvtxParser//recorderName***
    {
        private String[] _skipKeyWords = null;//SkipKeyWords
        private String[] _fileNameFilters = null; //FileNameFilter

        //object cSync = new object();
        //private int cnt = 0;
        //private System.Threading.Timer printTimer;
        

        //static NT2008EventLogFileRecorder()
        //{
        //    using (StreamWriter streamWriter = new StreamWriter(@"C:\Program Files\Natek\Security Manager\RemoteRecorder\log\stat.txt", true))
        //    {
        //        streamWriter.WriteLine("============================ START {0} =======================",
        //                               DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture));
        //    }
        //}

        //void PrintCount(object state)
        //{
        //    using (StreamWriter streamWriter = new StreamWriter(@"C:\Program Files\Natek\Security Manager\RemoteRecorder\log\stat.txt", true))
        //    {
        //        lock (cSync)
        //        {
        //            streamWriter.WriteLine("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff", CultureInfo.InvariantCulture), cnt);
        //        }
        //    }
        //}

        public NT2008EventLogFileRecorder()//recorderName
            : base()
        {
            LogName = "NT2008EventLogFile Recorder";//recorderName***

        } // Exchange2010Recorder

        public override void Init()
        {
            //printTimer = new System.Threading.Timer(new TimerCallback(PrintCount), null, 10000, 10000);
            GetFiles();
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init - LogName()" + LogName);
            Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
            //
        } // Init

        public NT2008EventLogFileRecorder(String fileName)//recorderName***
            : base(fileName)
        {

        } // Exchange2010Recorder

        //29.11.2012 
        protected override void ParseFileNameLocal()
        {
            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
            try
            {
                if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
                    List<String> fileNameList = GetFileNamesOnLocal();

                    //for (int i = 0; i < fileNameList.Count; i++)
                    //{
                    //    if (!RecordFields.NewEvtxFileNameList.ContainsKey(fileNameList[i]) && !RecordFields.OldEvtxFileNameList.ContainsKey(fileNameList[i]))
                    //    {
                    //        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> if içi ");
                    //        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> RecordFields.NewEvtxFileNameList.Count : " + RecordFields.NewEvtxFileNameList.Count.ToString());
                    //        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> RecordFields.OldEvtxFileNameList.Count : " + RecordFields.OldEvtxFileNameList.Count.ToString());
                    //        RecordFields.NewEvtxFileNameList.Add(fileNameList[i], i);
                    //    }
                    //}
                    fileNameList = SortFileNames(fileNameList);
                    SetLastFile(fileNameList);

                    //foreach (var item in RecordFields.NewEvtxFileNameList)
                    //{
                    //    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> NewEvtxFileNameList: " + item.Key + "Value. " + item.Value);
                    //    RecordFields._fileNames.Add(item.Key.ToString());
                    //}
                    //RecordFields._fileNames = SortFileNames(RecordFields._fileNames);
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
                //se. = password;
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

        public override bool ParseSpecific(EventLogReader logReader, EventRecord eventInstance, bool dontSend, string currentFile)
        {
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "Parsing Specific");
            Rec r = new Rec();

            //Log.Log(LogType.FILE, LogLevel.INFORM, "Parse Specific, " + eventInstance.Bookmark.ToString());
            r.EventId = eventInstance.Id;
            //Log.Log(LogType.FILE, LogLevel.INFORM, "ParseSpecific() -->> EventId: " + r.EventId);
            r.EventType = eventInstance.TaskDisplayName;
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> EventType: " + r.EventType);

            r.Description = eventInstance.FormatDescription();


            r.EventCategory = eventInstance.LevelDisplayName;//
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> EventCategory: " + r.EventCategory);
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> Description: " + eventInstance.FormatDescription());
            r.ComputerName = eventInstance.MachineName;
            DateTime dtCreate = Convert.ToDateTime(eventInstance.TimeCreated);
            r.Datetime = dtCreate.ToString("yyyy-MM-dd HH:mm:ss");
            //Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> DateTime: " + r.Datetime);

            try
            {
                #region NtEventLogRecorder 2008 Parser

                string[] descArr = r.Description.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                bool subjectMode = false;
                bool objectMode = false;
                bool targetMode = false;
                bool accessMode = false;
                bool processMode = false;
                bool applMode = false;
                bool networkMode = false;
                bool authenMode = false;
                bool dummyAccessControl = false;
                bool newAccountMode = false;

                for (int i = 0; i < descArr.Length; i++)
                {

                    if (!descArr[i].Contains(":"))
                    {
                        if (accessMode)
                        {
                            r.CustomStr7 += " " + descArr[i].Trim();
                            if (r.CustomStr7.Length > 900)
                            {
                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                            }
                        }
                    }
                    else
                    {
                        string[] lineArr = descArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //L.Log(LogType.FILE, LogLeve//L.DEBUG, "DescArr[" + i + "]:" + DescArr[i]);


                        if (descArr[i].Contains("Logon Type"))
                        {
                            //L.Log(LogType.FILE, LogLeve//L.DEBUG, "Logon Type Bulundu:" + DescArr[i]);
                            string logontypestr = descArr[i].Split(':')[1].Trim();
                            //L.Log(LogType.FILE, LogLeve//L.DEBUG, "Logon Type Değeri:" + logontypestr);
                            if (logontypestr != "")
                            {
                                r.CustomInt3 = Convert.ToInt32(logontypestr);
                            }
                        }

                        if (lineArr[lineArr.Length - 1].Trim() == "")
                        {
                            #region Mode
                            if (lineArr[0].Trim() == "Application Information")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = true;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Network Information")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = true;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Subject"
                                  || lineArr[0].Trim() == "New Logon"
                                  || lineArr[0].Trim() == "Account Whose Credentials Were Used"
                                  || lineArr[0].Trim() == "Credentials Which Were Replayed"
                                  || lineArr[0].Trim() == "Account That Was Locked Out"
                                  || lineArr[0].Trim() == "New Computer Account"
                                  || lineArr[0].Trim() == "Computer Account That Was Changed"
                                  || lineArr[0].Trim() == "Source Account")
                            {
                                subjectMode = true;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Target"
                                || lineArr[0].Trim() == "Target Account"
                                || lineArr[0].Trim() == "Target Computer"
                                || lineArr[0].Trim() == "Target Server")
                            {
                                subjectMode = true;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Object")
                            {
                                subjectMode = false;
                                objectMode = true;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Process Information" || lineArr[0].Trim() == "Process")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = true;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Access Request Information")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = true;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "Detailed Authentication Information")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = true;
                                newAccountMode = false;
                            }
                            else if (lineArr[0].Trim() == "New Account")
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = true;
                            }
                            else
                            {
                                subjectMode = false;
                                objectMode = false;
                                targetMode = false;
                                accessMode = false;
                                processMode = false;
                                applMode = false;
                                networkMode = false;
                                authenMode = false;
                                newAccountMode = false;
                            }
                            #endregion
                        }
                        else
                        {
                            if (subjectMode)
                            {
                                #region SubjectMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "User Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;

                                    //case "Security ID":
                                    //    if ( CustomStr2 == null)
                                    //    {
                                    //         CustomStr2 = appendArrayElements(lineArr);
                                    //    }
                                    //    break;
                                    case "Logon ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt6 = 0;
                                        }
                                        break;
                                    case "Client Context ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt6 = 0;
                                        }
                                        break;
                                    case "Account Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (targetMode)
                            {

                                #region TargetMode==true

                                switch (lineArr[0].Trim())
                                {
                                    case "User Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    //case "Target Server Name":
                                    //     CustomStr2 = appendArrayElements(lineArr);
                                    //    break;
                                    case "Account Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Old Account Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "New Account Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;


                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (objectMode)
                            {
                                #region ObjectMode=True
                                switch (lineArr[0].Trim())
                                {

                                    case "Object Name":
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Object Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Operation Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Handle ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt7 = 0;
                                        }
                                        break;
                                    case "Primary User Name":
                                        if (r.CustomStr1 == null)
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client User Name":
                                        if (r.CustomStr2 == null)
                                        {
                                            r.CustomStr2 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (accessMode)
                            {
                                #region AccessMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Accesses":
                                        if (r.CustomStr7 == null)
                                        {
                                            r.CustomStr7 = appendArrayElements(lineArr);
                                            if (r.CustomStr7.Length > 900)
                                            {
                                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                                            }
                                            dummyAccessControl = true;
                                        }
                                        break;
                                    case "Access Mask":
                                        if (dummyAccessControl)
                                        {
                                            r.CustomStr7 += " " + appendArrayElements(lineArr);
                                            if (r.CustomStr7.Length > 900)
                                            {
                                                r.CustomStr7 = r.CustomStr7.Substring(0, 900);
                                            }
                                        }
                                        break;
                                    case "Operation Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (processMode)
                            {
                                #region ProcessMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Duration":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "PID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (applMode)
                            {
                                #region ApplMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Logon Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Duration":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Application Instance ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Application Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (networkMode)
                            {

                                ////L.Log(LogType.FILE, LogLeve//L.DEBUG, "lineArr[0]:" + lineArr[0]);

                                #region NetworkMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Client Address":
                                        r.CustomStr3 = lineArr[lineArr.Length - 1];
                                        break;
                                    case "Source Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Workstation Name":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    //case "ffff":
                                    //     CustomStr3 = appendArrayElements(lineArr);
                                    //    break;

                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (authenMode)
                            {
                                #region AuthenMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Authentication Package":
                                        string authenPack = appendArrayElements(lineArr);
                                        if (authenPack.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Pre-Authentication Type":
                                        string authenPack3 = appendArrayElements(lineArr);
                                        if (authenPack3.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack3.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack3.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Logon Process":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Account":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else if (newAccountMode)
                            {
                                #region NewAccountMode==True
                                switch (lineArr[0].Trim())
                                {
                                    case "Account Name":
                                        if (r.CustomStr1 != null)
                                        {
                                            r.CustomStr2 = r.CustomStr1;
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        else
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region Other

                                switch (lineArr[0].Trim())
                                {
                                    case "Logon Type":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt3 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt3 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt3 = 0;
                                        }
                                        break;
                                    case "Error Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Status Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Failure Code":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt1 = 0;
                                        }
                                        break;
                                    case "Caller Workstation":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Workstation Name":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Workstation":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "User Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Name":
                                        if (r.CustomStr1 != null)
                                        {
                                            r.CustomStr2 = r.CustomStr1;
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        else
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client Name":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Account":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Caller User Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Account Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Name":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Group Domain":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Caller Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Target Domain":
                                        r.CustomStr7 = appendArrayElements(lineArr);
                                        break;
                                    case "Target User Name":
                                        r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    case "Source Network Address":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Client Address":
                                        r.CustomStr3 = lineArr[lineArr.Length - 1];
                                        // CustomStr3 = appendArrayElements(lineArr);dali
                                        break;
                                    case "Source Port":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Authentication Package":
                                        string authenPack = appendArrayElements(lineArr);
                                        if (authenPack.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack.Contains("Kerberos") || authenPack.Contains("KDS"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Pre-Authentication Type":
                                        string authenPack2 = appendArrayElements(lineArr);
                                        if (authenPack2.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack2.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack2.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Caller Process ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "PID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "Logon Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Logon Process":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Process Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Image File Name":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Duration":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt2 = 0;
                                        }
                                        break;
                                    case "Object Name":
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Object Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Operation Type":
                                        r.CustomStr9 = appendArrayElements(lineArr);
                                        break;
                                    case "Handle ID":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt7 = 0;
                                        }
                                        break;
                                    case "Primary User Name":
                                        if (r.CustomStr1 == null)
                                        {
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    case "Client User Name":
                                        if (r.CustomStr2 == null)
                                        {
                                            r.CustomStr2 = appendArrayElements(lineArr);
                                        }
                                        break;
                                    //case "ffff":
                                    //     CustomStr3 = appendArrayElements(lineArr);
                                    //    break;


                                    //D.Ali Türkce Gelen Loglar İçin
                                    case "Kullanıcı Adı":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "İş İstasyonu Adı":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma işlemi":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma Türü":
                                        if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                            r.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                        else
                                            r.CustomInt5 = -1;
                                        break;
                                    case "Etki Alanı":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak Ağ Adresi":
                                        r.CustomStr3 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Hesabı":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak İş İstasyonu":
                                        r.CustomStr4 = appendArrayElements(lineArr);
                                        break;
                                    case "Share Name":
                                        r.CustomStr8 = appendArrayElements(lineArr);
                                        break;
                                    case "Hesap Adı":
                                        if (string.IsNullOrEmpty(r.CustomStr1))
                                            r.CustomStr1 = appendArrayElements(lineArr);
                                        else
                                            r.CustomStr2 = appendArrayElements(lineArr);
                                        break;
                                    /////////

                                    case "Güvenlik Kimliği":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Hesap Etki Alanı":
                                        r.CustomStr5 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Açma Kimliği":
                                        r.CustomStr1 = appendArrayElements(lineArr);
                                        break;
                                    case "Oturum Türü":
                                        if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                            r.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                        else
                                            r.CustomInt5 = -1;
                                        break;

                                    case "İşlem Kimliği":
                                        if (!lineArr[1].Contains("-"))
                                        {
                                            if (lineArr[1].Contains("0x"))
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                            }
                                            else
                                            {
                                                r.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                            }
                                        }
                                        else
                                        {
                                            r.CustomInt8 = 0;
                                        }
                                        break;
                                    case "İşlem Adı":
                                        r.CustomStr6 = appendArrayElements(lineArr);
                                        break;
                                    case "Kaynak Bağlantı Noktası":
                                        try
                                        {
                                            r.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                        }
                                        catch (Exception)
                                        {
                                            r.CustomInt4 = 0;
                                        }
                                        break;
                                    case "Kimlik Doğrulama Paketi":
                                        string authenPack4 = appendArrayElements(lineArr);
                                        if (authenPack4.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack4.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack4.Contains("Kerberos"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;
                                    case "Paket Adı (yalnızca NTLM)":
                                        string authenPack3 = appendArrayElements(lineArr);
                                        if (authenPack3.Contains("Negotiate"))
                                        {
                                            r.CustomInt5 = 0;
                                        }
                                        else if (authenPack3.Contains("NTLM"))
                                        {
                                            r.CustomInt5 = 1;
                                        }
                                        else if (authenPack3.Contains("Kerberos") || authenPack3.Contains("KDS"))
                                        {
                                            r.CustomInt5 = 2;
                                        }
                                        else
                                        {
                                            r.CustomInt5 = 3;
                                        }
                                        break;



                                    default:
                                        break;
                                }
                                #endregion
                            }
                        }
                    }
                }

                //Encoding.ASCII.GetByteCount(r.Description)>4000

                if (r.Description.Length > 900)
                {
                    if (r.Description.Length > 1800)
                    {
                        r.CustomStr10 = r.Description.Substring(900, 900);
                    }
                    else
                    {
                        r.CustomStr10 = r.Description.Substring(900, r.Description.Length - 900);
                    }
                    r.Description = r.Description.Substring(0, 900);
                }
                #endregion
            }
            catch (Exception ex)
            {
                //Log.Log(LogType.FILE, LogLevel.ERROR, "ParseSpecific, Error: " + ex.Message);
            }

            r.CustomStr9 = currentFile;
            lastFile = currentFile;

            ArrayList array = new ArrayList((ICollection)eventInstance.KeywordsDisplayNames);
            r.EventCategory = r.EventType;
            r.EventType = array[0].ToString();

            if (!string.IsNullOrEmpty(r.EventType))
            {
                if (r.EventType.Contains(" "))
                {
                    r.EventType = r.EventType.Split(' ')[1];
                }
            }

            r.LogName = "NT-" + eventInstance.LogName;//

            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> lastFile: " + lastFile);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "ParseSpecific() -->> lastLine: " + lastLine);
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Record is sending now.");
            SetRecordData(r);
            //lock (cSync)
            //{
            //    ++cnt;
            //}
            Log.Log(LogType.FILE, LogLevel.DEBUG, "Record sended.");
            return true;
        }
        //

        // ParseSpecific

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

        //Utils
        /// <summary>
        /// line space split function
        /// </summary>
        /// <param name="line"></param>
        /// gelen line 
        /// <param name="useTabs"></param>
        /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
        /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
        /// <returns></returns>
        public virtual String[] SpaceSplit(String line, bool useTabs)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ' && (!useTabs || c != '\t'))
                {
                    if (space)
                    {
                        if (sb.ToString() != "")
                        {
                            lst.Add(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                        space = false;
                    }
                    sb.Append(c);
                }
                else if (!space)
                {
                    space = true;
                }
            }

            if (sb.ToString() != "")
                lst.Add(sb.ToString());

            return lst.ToArray();
        } // SpaceSplit

        private string appendArrayElements(string[] arr)
        {
            string totalString = "";
            for (int i = 1; i < arr.Length; i++)
            {
                totalString += ":" + arr[i].Trim();
            }

            //return totalString.TrimStart(":".ToCharArray()).TrimEnd(":".ToCharArray());
            return totalString.Trim(':').Trim('f').Trim(':').Trim('f');

        }

        /// <summary>
        /// Check the given file is finished or not 
        /// </summary>
        /// <param name="file">File full path which will check</param>
        /// <returns>if file finished return true</returns>


        static long CountLinesInFile(string f)
        {
            long count = 0;
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        } // CountLinesInFile

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
                try
                {
                    Stop();
                    try
                    {
                        ParseFileName();
                    }
                    finally
                    {
                        Start();
                    }
                }
                finally
                {
                    dayChangeTimer.Start();
                }
            }
        } // dayChangeTimer_Elapsed

        //29.11.2012
        /// <summary>
        /// Gets the file names on the given directory
        /// </summary>
        /// <returns>Returned file names</returns>
        private List<String> GetFileNamesOnLocal()
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
                List<String> fileNameList = new List<String>(Directory.GetFiles(Dir));
                /*
                    List<String> fileNameList = new List<String>();
                    foreach (String fileName in Directory.GetFiles(Dir))
                    {
                        String fileShortName = Path.GetFileName(fileName).ToString();
                        fileNameList.Add(fileShortName);
                    }
                 */
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

                //se.Connect();
                //se.RunCommand(command, ref stdOut, ref stdErr);
                //se.Close();

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

        private void SetLastFile(List<string> fileNameList)
        {//
            try
            {
                if (fileNameList != null && fileNameList.Count > 0)
                {
                    if (!string.IsNullOrEmpty(lastFile))
                    {
                        FileInfo inf = new FileInfo(lastFile);
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                            " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile + ", LineLimit=" + lineLimit);
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> item is " + inf.FullName);

                        int idx;

                        if ((idx = fileNameList.IndexOf(inf.FullName)) >= 0)
                        {
                            if (IsFileFinished(lastFile))
                            {
                                idx++;
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                        FileName);
                                Log.Log(LogType.FILE, LogLevel.INFORM,
                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
                                        idx);

                                if (idx < fileNameList.Count)
                                {
                                    FileName = fileNameList[idx];
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Onur--- : " + FileName + " Value " + idx);
                                    lastFile = FileName;
                                    Position = 0;
                                    Log.Log(LogType.FILE, LogLevel.INFORM,
                                            " SetLastFile() -->> Last file is finished. New file is  : " + FileName);

                                    if (tempCustomVar1 == "[delete]")
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
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" +
                                lastFile);
                        FileName = fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                " SetLastFile() -->>  Last file is assign on database : " + FileName);
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
                WriteLogFile("SetlastFile, ", exception.ToString());
            }
        } // SetLastFile

        /*
        private void SetLastFileOld(List<string> fileNameList)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);

                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);
                Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> lastFile : " + lastFile);
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
                            if (IsFileFinished(lastFile))
                            {
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  File Finished True.");
                                string key = item;
                                if (dictionary.ContainsKey(key))
                                {
                                    RecordFields.num3 = dictionary[key];
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
                                        RecordFields.lineNumber = 0;
                                        RecordFields.currentFile = lastFile;

                                        string isDelete = "[delete]";

                                        if (tempCustomVar1.Contains(","))
                                        {
                                            if (RecordFields.tempArray.Length != 0)
                                            {
                                                for (int i = 0; i < RecordFields.tempArray.Length; i++)
                                                {
                                                    if (RecordFields.tempArray[i].ToLower().Trim() == isDelete)
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Last file is finished. num3 is :" +
                                                        RecordFields.num3);
                                                        if (RecordFields.num3 == 0)
                                                        {
                                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                        }
                                                        else
                                                        {
                                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3 - 1]);
                                                        }
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                                        File.Delete(Dir + list[RecordFields.num3 - 1]);
                                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                                " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
                                                                list[RecordFields.num3 - 1]);
                                                    }
                                                }
                                            }
                                        }
                                        else if (tempCustomVar1 == isDelete)
                                        {
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Last file is finished. num3 is :" +
                                                        RecordFields.num3);
                                            if (RecordFields.num3 == 0)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                            }
                                            else
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3 - 1]);
                                            }
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                            File.Delete(Dir + list[RecordFields.num3 - 1]);
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                                " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
                                                                list[RecordFields.num3 - 1]);
                                        }
                                    }
                                    else
                                    {
                                        //Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. ");

                                        string isDelete = "[delete]";

                                        if (tempCustomVar1.Contains(","))
                                        {
                                            if (RecordFields.tempArray.Length != 0)
                                            {
                                                for (int i = 0; i < RecordFields.tempArray.Length; i++)
                                                {
                                                    if (RecordFields.tempArray[i].ToLower().Trim() == isDelete)
                                                    {
                                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Last file is finished. num3 is :" +
                                                        RecordFields.num3);
                                                        if (RecordFields.num3 == 0)
                                                        {
                                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                        }
                                                        else
                                                        {
                                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                                        }
                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                                        File.Delete(Dir + list[RecordFields.num3]);
                                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                                " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
                                                                list[RecordFields.num3]);
                                                    }
                                                }
                                            }
                                        }
                                        else if (tempCustomVar1 == isDelete)
                                        {
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                        " SetLastFile() -->> Last file is finished. num3 is :" +
                                                        RecordFields.num3);
                                            if (RecordFields.num3 == 0)
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                            }
                                            else
                                            {
                                                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
                                            }
                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
                                            File.Delete(Dir + list[RecordFields.num3]);
                                            Log.Log(LogType.FILE, LogLevel.INFORM,
                                                                " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
                                                                list[RecordFields.num3]);
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
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
                            FileName = Dir + fileNameList[fileNameList.Count + 1];
                            lastFile = FileName;
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
                        }
                    }
                    else
                    {
                        FileName = Dir + fileNameList[0];
                        lastFile = FileName;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
                        RecordFields.currentFile = lastFile;
                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile, new int[0]);
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
                WriteLogFile("SetlastFile, ", exception.ToString());
            }
        } // SetLastFileOld
        */
        private void WriteLogFile(String functionName, string ex)
        {
            StreamWriter LogFile = File.AppendText("C:" + "\\Natek Event Log Parser.log");
            LogFile.WriteLine(DateTime.Now + " " + functionName + "  : " + ex);
            LogFile.WriteLine();
            LogFile.Close();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        private List<String> SortFileNames(List<String> fileNameList)
        {
            List<string> _fileNameList = new List<string>();
            for (int i = 0; i < fileNameList.Count; i++)
            {
                FileInfo f = new FileInfo(fileNameList[i]);
                string[] fn = f.Name.Split('.');
                if (fn.Length == 2 && fn[1] == "evtx" && fn[0].Contains("Archive"))
                {
                    _fileNameList.Add(fileNameList[i]);
                }
            }
            _fileNameList.Sort();
            Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
            foreach (string t in _fileNameList)
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
            }
            return _fileNameList;
        } // SortFileNames
    }

    //public class NT2008EventLogFileRecorder : EvtxParser//recorderName***
    //{
    //    private String[] _skipKeyWords = null;//SkipKeyWords
    //    private String[] _fileNameFilters = null; //FileNameFilter
    //    private Fields RecordFields;
    //    string _description = "";
    //    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    //    public NT2008EventLogFileRecorder()//recorderName
    //        : base()
    //    {
    //        LogName = "NT2008EventLogFile Recorder";
    //        RecordFields = new Fields();
    //    } // Exchange2010Recorder

    //    public override void Init()
    //    {
    //        GetFiles();
    //        Log.Log(LogType.FILE, LogLevel.INFORM, "Init - LogName()" + LogName);
    //        Log.Log(LogType.FILE, LogLevel.INFORM, "Init()");
    //    } // Init

    //    public override void Start()
    //    {
    //        base.Start();
    //    } // Start

    //    public NT2008EventLogFileRecorder(String fileName)//recorderName***
    //        : base(fileName)
    //    {

    //    } // NT2008EventLogFileRecorder

    //    /// <summary>
    //    /// string between function
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// gelen tüm string
    //    /// <param name="a"></param>
    //    /// başlangıç string
    //    /// <param name="b"></param>
    //    /// bitiş string
    //    /// <returns></returns>
    //    public static string Between(string value, string a, string b)
    //    {
    //        int posA = value.IndexOf(a);
    //        int posB = value.LastIndexOf(b);


    //        if (posA == -1)
    //        {
    //            return "";
    //        }
    //        if (posB == -1)
    //        {
    //            return "";
    //        }
    //        int adjustedPosA = posA + a.Length;
    //        if (adjustedPosA >= posB)
    //        {
    //            return "";
    //        }
    //        return value.Substring(adjustedPosA, posB - adjustedPosA);
    //    } // Between

    //    //Utils
    //    /// <summary>
    //    /// line space split function
    //    /// </summary>
    //    /// <param name="line"></param>
    //    /// gelen line 
    //    /// <param name="useTabs"></param>
    //    /// eğer line içinde tab boşluk var ise ve buna göre de split yapılmak isteniyorsa true
    //    /// eğer line içinde tab boşluk var ise ve buna göre  split yapılmak istenmiyorsa false
    //    /// <returns></returns>
    //    public virtual String[] SpaceSplit(String line, bool useTabs)
    //    {
    //        List<String> lst = new List<String>();
    //        StringBuilder sb = new StringBuilder();
    //        bool space = false;
    //        foreach (Char c in line.ToCharArray())
    //        {
    //            if (c != ' ' && (!useTabs || c != '\t'))
    //            {
    //                if (space)
    //                {
    //                    if (sb.ToString() != "")
    //                    {
    //                        lst.Add(sb.ToString());
    //                        sb.Remove(0, sb.Length);
    //                    }
    //                    space = false;
    //                }
    //                sb.Append(c);
    //            }
    //            else if (!space)
    //            {
    //                space = true;
    //            }
    //        }

    //        if (sb.ToString() != "")
    //            lst.Add(sb.ToString());

    //        return lst.ToArray();
    //    } // SpaceSplit

    //    public override void GetFiles()
    //    {
    //        try
    //        {
    //            Dir = GetLocation();
    //            GetRegistry();
    //            Today = DateTime.Now;
    //            ParseFileName();
    //        }
    //        catch (Exception e)
    //        {
    //            if (reg == null)
    //            {
    //                Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
    //                Log.Log(LogType.EVENTLOG, LogLevel.ERROR, " GetFiles() -->> Mesaj: " + e.StackTrace);
    //            }
    //            else
    //            {
    //                Log.Log(LogType.FILE, LogLevel.ERROR, " GetFiles() -->> Error while getting files, Exception: " + e.Message);
    //                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
    //            }
    //        }
    //    } // GetFiles

    //    protected override void dayChangeTimer_Elapsed(Object sender, ElapsedEventArgs e)
    //    {
    //        dayChangeTimer.Stop();
    //        if (remoteHost == "")
    //        {
    //            String fileLast = FileName;
    //            Stop();
    //            ParseFileName();
    //            if (FileName != fileLast)
    //            {
    //                Position = 0;
    //                lastLine = "";
    //                lastFile = FileName;
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " dayChangeTimer_Elapsed() -->> File changed, new file is, " + FileName);
    //            }
    //            base.Start();
    //        }
    //        dayChangeTimer.Start();
    //    } // dayChangeTimer_Elapsed

    //    private string appendArrayElements(string[] arr)
    //    {
    //        string totalString = "";
    //        for (int i = 1; i < arr.Length; i++)
    //        {
    //            totalString += ":" + arr[i].Trim();
    //        }
    //        return totalString.Trim(':').Trim('f').Trim(':').Trim('f');
    //    }
    //
    //    protected override void ParseFileNameLocal()
    //    {
    //        Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is STARTED ");
    //        try
    //        {
    //            if (Dir.EndsWith("/") || Dir.EndsWith("\\"))
    //            {
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Searching files in directory : " + Dir);
    //                List<String> fileNameList = GetFileNamesOnLocal();
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> fileName'ler Geldi");
    //                fileNameList = SortFileNames(fileNameList);
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> Sort edildi.");
    //                RecordFields.fileNameList1 = fileNameList;
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> List'e atıldı.");
    //                for (int i = 0; i < RecordFields.fileNameList1.Count; i++)
    //                {
    //                    Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> List yazdırılıyor. " + RecordFields.fileNameList1[i]);
    //                }
    //                SetLastFile(RecordFields.fileNameList1);
    //            }
    //            else
    //            {
    //                FileName = Dir;
    //            }
    //            Log.Log(LogType.FILE, LogLevel.INFORM, " ParseFileNameLocal() -->> is successfully FINISHED");
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Log(LogType.FILE, LogLevel.ERROR, " ParseFileNameLocal() -->> An error occurred : " + ex.ToString());
    //        }
    //    } // ParseFileNameLocal

    //    /// <summary>
    //    /// Gets the file names on the given directory
    //    /// </summary>
    //    /// <returns>Returned file names</returns>
    //    /// 2.
    //    private List<String> GetFileNamesOnLocal()
    //    {
    //        try
    //        {
    //            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is STARTED ");
    //            List<String> fileNameList = new List<String>();
    //            foreach (String fileName in Directory.GetFiles(Dir))
    //            {
    //                String fileShortName = Path.GetFileName(fileName).ToString();
    //                if (!fileNameList.Contains(fileShortName))
    //                {
    //                    fileNameList.Add(fileShortName);
    //                }
    //            }

    //            for (int i = 0; i < fileNameList.Count; i++)
    //            {
    //                Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> FileNameList: " + fileNameList[i]);
    //            }
    //            Log.Log(LogType.FILE, LogLevel.DEBUG, " GetFileNamesOnLocal() -->> is successfully FINISHED");
    //            return fileNameList;
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Log(LogType.FILE, LogLevel.ERROR, " GetFileNamesOnLocal() -->> An error occurred :" + ex.ToString());
    //            return null;
    //        }
    //    } // GetFileNamesOnLocal

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="fileNameList"></param>
    //    /// <returns></returns>
    //    /// 3.
    //    private List<String> SortFileNames(List<String> fileNameList)
    //    {
    //        Log.Log(LogType.FILE, LogLevel.DEBUG, " SortFileNames() -->> is STARTED ");
    //        List<string> _fileNameList = new List<string>();
    //        for (int i = 0; i < fileNameList.Count; i++)
    //        {
    //            if (fileNameList[i].Split('.')[1] == "log" && fileNameList[i].Split('.')[0].Contains("Archive") && fileNameList[i].Split('.').Length == 2)
    //            {
    //                _fileNameList.Add(fileNameList[i]);
    //                _fileNameList.Sort();
    //            }
    //        }
    //        foreach (string t in _fileNameList)
    //        {
    //            Log.Log(LogType.FILE, LogLevel.INFORM, " SortFileNames() " + t);
    //        }
    //        return _fileNameList;
    //    } // SortFileNames

    //    private void SetLastFile(List<string> fileNameList)
    //    {
    //        try
    //        {
    //            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir, new int[0]);
    //            Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> num3 is : " + RecordFields.num3);
    //            Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> lastFile : " + lastFile);
    //            Dictionary<string, int> dictionary = new Dictionary<string, int>();
    //            ArrayList list = new ArrayList();

    //            for (int num = 0; num < fileNameList.Count; num++)
    //            {
    //                dictionary.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture), num);
    //                list.Add(fileNameList[num].ToString(CultureInfo.InvariantCulture));
    //            }

    //            if (list.Count > 0)
    //            {
    //                if (!string.IsNullOrEmpty(lastFile))
    //                {
    //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is not null. LasFile is " + lastFile, new int[0]);
    //                    string item = lastFile.Replace(Dir, "");
    //                    Log.Log(LogType.FILE, LogLevel.DEBUG, " SetLastFile() -->> item is " + item);

    //                    if (fileNameList.Contains(item))
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
    //                                        FileName, new int[0]);
    //                                Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                        " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " +
    //                                        RecordFields.num3.ToString(CultureInfo.InvariantCulture));

    //                                if ((RecordFields.num3 + 1) != list.Count)
    //                                {
    //                                    FileName = Dir + list[RecordFields.num3 + 1].ToString();
    //                                    Log.Log(LogType.FILE, LogLevel.DEBUG, string.Concat(new object[] { " SetLastFile() -->> Onur--- : ", list[RecordFields.num3 + 1].ToString(), " Value ", RecordFields.num3 }), new int[0]);
    //                                    lastFile = FileName;
    //                                    Position = 0;
    //                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. New file is  : " + FileName, new int[0]);
    //                                    RecordFields.num3 = RecordFields.num3 + 1;
    //                                    RecordFields.lineNumber = 0;
    //                                    RecordFields.currentFile = lastFile;

    //                                    string isDelete = "[delete]";

    //                                    if (tempCustomVar1.Contains(","))
    //                                    {
    //                                        if (RecordFields.tempArray.Length != 0)
    //                                        {
    //                                            for (int i = 0; i < RecordFields.tempArray.Length; i++)
    //                                            {
    //                                                if (RecordFields.tempArray[i].ToLower().Trim() == isDelete)
    //                                                {
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
    //                                                    RecordFields.num3);
    //                                                    if (RecordFields.num3 == 0)
    //                                                    {
    //                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3 - 1]);
    //                                                    }
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
    //                                                    File.Delete(Dir + list[RecordFields.num3 - 1]);
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                            " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
    //                                                            list[RecordFields.num3 - 1]);
    //                                                }
    //                                            }
    //                                        }
    //                                    }
    //                                    else if (tempCustomVar1 == isDelete)
    //                                    {
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
    //                                                    RecordFields.num3);
    //                                        if (RecordFields.num3 == 0)
    //                                        {
    //                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                        }
    //                                        else
    //                                        {
    //                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3 - 1]);
    //                                        }
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
    //                                        File.Delete(Dir + list[RecordFields.num3 - 1]);
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                            " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
    //                                                            list[RecordFields.num3 - 1]);
    //                                    }
    //                                }
    //                                else
    //                                {
    //                                    //Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. Continue same file : " + FileName, new int[0]);
    //                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished. But not any file for reading. ");

    //                                    string isDelete = "[delete]";

    //                                    if (tempCustomVar1.Contains(","))
    //                                    {
    //                                        if (RecordFields.tempArray.Length != 0)
    //                                        {
    //                                            for (int i = 0; i < RecordFields.tempArray.Length; i++)
    //                                            {
    //                                                if (RecordFields.tempArray[i].ToLower().Trim() == isDelete)
    //                                                {
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
    //                                                    RecordFields.num3);
    //                                                    if (RecordFields.num3 == 0)
    //                                                    {
    //                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                                    }
    //                                                    else
    //                                                    {
    //                                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                                    }
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
    //                                                    File.Delete(Dir + list[RecordFields.num3]);
    //                                                    Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                            " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
    //                                                            list[RecordFields.num3]);
    //                                                }
    //                                            }
    //                                        }
    //                                    }
    //                                    else if (tempCustomVar1 == isDelete)
    //                                    {
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                    " SetLastFile() -->> Last file is finished. num3 is :" +
    //                                                    RecordFields.num3);
    //                                        if (RecordFields.num3 == 0)
    //                                        {
    //                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                        }
    //                                        else
    //                                        {
    //                                            Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Okunan dosya " + list[RecordFields.num3]);
    //                                        }
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is finished.");
    //                                        File.Delete(Dir + list[RecordFields.num3]);
    //                                        Log.Log(LogType.FILE, LogLevel.INFORM,
    //                                                            " SetLastFile() -->> Last file is finished. Last file is deleted. Deleted file is : " +
    //                                                            list[RecordFields.num3]);
    //                                    }
    //                                }
    //                            }
    //                            //RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
    //                        }
    //                        else
    //                        {
    //                            FileName = lastFile;
    //                            //RecordFields.totalLineCountinFile = CountLinesInFile(lastFile);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Last file is not found in directory : " + Dir + " LastFile :" + lastFile, new int[0]);
    //                        FileName = Dir + fileNameList[fileNameList.Count + 1];
    //                        lastFile = FileName;
    //                        Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->>  Last file is assign on database : " + FileName, new int[0]);
    //                    }
    //                }
    //                else
    //                {
    //                    FileName = Dir + fileNameList[0];
    //                    lastFile = FileName;
    //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> LastFile is null. Setted lastfile is : " + lastFile, new int[0]);
    //                    RecordFields.currentFile = lastFile;
    //                    Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Current file is : " + lastFile, new int[0]);
    //                }
    //            }
    //            else
    //            {
    //                Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> There is NO Log File exists in Dir :" + Dir, new int[0]);
    //                Log.Log(LogType.FILE, LogLevel.INFORM, " SetLastFile() -->> Searching files in directory : " + Dir);
    //            }
    //        }
    //        catch (Exception exception)
    //        {
    //            Log.Log(LogType.FILE, LogLevel.ERROR, " SetLastFile() -->> An error occurred : " + exception.ToString());
    //        }
    //    } // SetLastFile

    //    /// <summary>
    //    /// Check the given file is finished or not 
    //    /// </summary>
    //    /// <param name="file">File full path which will check</param>
    //    /// <returns>if file finished return true</returns>
    //    private Boolean IsFileFinished(String file)
    //    {
    //        String line = "";
    //        RecordFields.currentFile = file;
    //        try
    //        {
    //            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    //            {
    //                Log.Log(LogType.FILE, LogLevel.INFORM,
    //                        " IsFileFinished() -->> reading local file! " + file);

    //                using (StreamReader sr = new StreamReader(file))
    //                {
    //                    while ((line = sr.ReadLine()) != null)
    //                    {
    //                        //MessageBox.Show(line);
    //                    }

    //                    if ((line == sr.ReadLine()) != null)
    //                    {
    //                        return true;
    //                    }
    //                    else
    //                    {
    //                        return false;
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> An error occurred is file : " + lastFile + "  : " + ex.ToString());
    //            Log.Log(LogType.FILE, LogLevel.ERROR, "IsFileFinished() -->> " + ex.StackTrace);
    //            return false;
    //        }
    //    } // IsFileFinished

}




