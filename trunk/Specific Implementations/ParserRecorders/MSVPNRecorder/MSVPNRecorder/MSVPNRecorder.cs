//Name: MS VPN Recorder
//Writer: Selahattin ÜNALAN
//Date: 18.10.2011

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Parser;
using Log;
using Microsoft.Win32;
using System.Globalization;

namespace Parser
{
    public partial class MSVPNRecorder : AppParser
    {
        public MSVPNRecorder()
            : base()
        {
            usingRegistry = true;
            LogName = "MSVPNRecorder";
        }

        public override void Init()
        {
            Dir = GetLocation();
            GetRegistry();
        }

        public String ReadEventSourceInfo(String source, String type)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog");
                bool found = false;
                String ret = "";
                foreach (String keyInName in key.GetSubKeyNames())
                {
                    RegistryKey keyIn = key.OpenSubKey(keyInName);
                    foreach (String keyInName2 in keyIn.GetSubKeyNames())
                    {
                        if (keyInName2.Contains(source))
                        {
                            RegistryKey keyToLook = keyIn.OpenSubKey(keyInName2);
                            ret = (String)keyToLook.GetValue(type);
                            if (ret != null)
                                found = true;
                            else
                            {
                                type = "EventMessageFile";
                                ret = (String)keyToLook.GetValue(type);
                                if (ret != null)
                                    found = true;
                            }
                            keyToLook.Close();
                            break;
                        }
                    }
                    keyIn.Close();
                    if (found)
                        break;
                }
                key.Close();

                /*if (!found && Dir == "Security")
                {
                    RegistryKey keySec = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\EventLog\\Security\\Security");
                    ret = (String)keySec.GetValue(type);
                }*/
                return ret;
            }
            catch
            {
                return "";
            }
        }

        public String GetEventMessage(
            IntPtr hDll, /* Handle to the event message file */
            UInt32 dwEventIndex, /* Index of the event description message */
            UInt32 dwLanguageID, /* Language ID of the message to retrieve */
            String[] lpInserts) /* Array of insertion strings */
        {
            UInt32 dwReturn;
            IntPtr lpMsgBuf = IntPtr.Zero;
            UInt32 dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER |
            FORMAT_MESSAGE_FROM_HMODULE |
            FORMAT_MESSAGE_FROM_SYSTEM;

            if (lpInserts.Length != 0)
            {
                dwFlags |= FORMAT_MESSAGE_ARGUMENT_ARRAY;

                try
                {
                    dwReturn = FormatMessage(dwFlags, hDll, dwEventIndex, dwLanguageID, ref lpMsgBuf, 0, lpInserts);
                }
                catch (Exception exp)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetEventMessage:FormatMessage:" + exp.Message);
                }
            }
            else
            {
                dwReturn = FormatMessage(dwFlags, hDll, dwEventIndex, dwLanguageID, ref lpMsgBuf, 0, null);
            }

            String ret = "";
            if (lpMsgBuf != IntPtr.Zero)
                ret = Marshal.PtrToStringAnsi(lpMsgBuf);

            return ret;
        }

        public String GetString(UInt32 id, String sourceName, String type, List<String> lst)
        {
            String str = ReadEventSourceInfo(sourceName, type);
            IntPtr hEvt = LoadLibraryEx(str, IntPtr.Zero, DONT_RESOLVE_DLL_REFERENCES);
            String ret = "";
            // Load the event message file DLL 
            if (hEvt != IntPtr.Zero)
            {
                // Get the event message with the paramater strings inserted 
                ret = GetEventMessage(hEvt, id,
                MAKELANGID(LANG_ENGLISH, SUBLANG_DEFAULT), lst.ToArray());
                FreeLibrary(hEvt);
            }
            return ret;
        }

        public override void Parse()
        {
            IntPtr handle = OpenEventLog(remoteHost, Dir);
            Byte[] output = new byte[65536];
            Int32 bytesRead = 0;
            Int32 minNumberOfBytesNeeded = 0;
            try
            {
                Int32 flags = 0;
                if (Position == 0)
                    flags = (Int32)ReadFlags.EVENTLOG_SEQUENTIAL_READ | (Int32)ReadFlags.EVENTLOG_FORWARDS_READ;
                else
                    flags = (Int32)ReadFlags.EVENTLOG_SEEK_READ | (Int32)ReadFlags.EVENTLOG_FORWARDS_READ;

                Int32 readLineCount = 0;

                while (ReadEventLog(handle, flags, (UInt32)Position, output, output.Length, ref bytesRead, ref minNumberOfBytesNeeded))
                {
                    Object rec = new EVENTLOGRECORD();
                    Int32 dw = 0;
                    bool changed = false;
                    while (bytesRead > dw)
                    {
                        Rec r = new Rec();

                        ByteArrayToStructure(output, dw, ref rec);
                        EVENTLOGRECORD recCast = (EVENTLOGRECORD)rec;

                        if (Position != recCast.RecordNumber)
                        {
                            changed = true;
                            try
                            {
                                DateTime d = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(recCast.TimeWritten));
                                r.Datetime = d.Year + "/" + d.Month + "/" + d.Day + " " + d.Hour + ":" + d.Minute + ":" + d.Second + "." + recCast.TimeWritten;
                                r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            catch
                            {
                                r.CustomInt1 = recCast.TimeWritten;
                            }

                            //r.EventId = recCast.EventID;

                            r.EventId = returneventid(recCast.EventID);
                            Log.Log(LogType.FILE, LogLevel.INFORM, "Event_Id :" + r.EventId);
                            r.EventType = ((EventType)(recCast.EventType)).ToString();
                            //r.EventCategory = recCast.EventCategory.ToString();
                            r.LogName = "NT-" + Dir;
                            r.Recordnum = recCast.RecordNumber;

                            IntPtr ptr = IntPtr.Zero;
                            String SourceName = "";
                            try
                            {
                                ptr = Marshal.AllocHGlobal(bytesRead);
                                Int32 total = dw + 56;
                                Marshal.Copy(output, total, ptr, bytesRead - total); //56 struct size
                                SourceName = Marshal.PtrToStringAnsi(ptr);
                                Marshal.FreeHGlobal(ptr);
                            }
                            catch
                            {
                            }

                            r.SourceName = SourceName;

                            //r.EventCategory = GetString((UInt32)recCast.EventCategory, r.SourceName, "CategoryMessageFile", new List<String>());
                            r.EventCategory = recCast.EventCategory.ToString();

                            String ComputerName = "";
                            try
                            {
                                ptr = Marshal.AllocHGlobal(bytesRead);
                                Int32 total = dw + 57 + SourceName.Length;
                                Marshal.Copy(output, total, ptr, bytesRead - total);
                                ComputerName = Marshal.PtrToStringAnsi(ptr);
                                Marshal.FreeHGlobal(ptr);
                            }
                            catch
                            {
                            }

                            r.ComputerName = ComputerName;

                            List<String> lst = new List<String>();
                            Int32 offset = dw + recCast.StringOffset;
                            for (Int32 i = 0; i < recCast.NumStrings; i++)
                            {
                                ptr = Marshal.AllocHGlobal(bytesRead);
                                Marshal.Copy(output, offset, ptr, bytesRead - offset);
                                String str = Marshal.PtrToStringAnsi(ptr);
                                lst.Add(str);
                                Marshal.FreeHGlobal(ptr);
                                offset += str.Length + 1;
                            }

                            try
                            {
                                r.Description = GetString((UInt32)recCast.EventID, r.SourceName, "EventMessageFile", lst);
                                string line = GetString((UInt32)recCast.EventID, r.SourceName, "EventMessageFile", lst);
                                string[] fields = line.Split(' ');
                                Log.Log(LogType.FILE, LogLevel.INFORM, "line : " + line);

                                if (r.EventId == 20271)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Event ıd71 : " + r.EventId);
                                    r.EventCategory = fields[8] + " " + fields[9] + " " + fields[10];
                                    r.UserName = fields[3];
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "UsersId : " + r.UserName);

                                    r.CustomStr3 = fields[6];

                                    string[] ss = fields[0].Split('{');
                                    string[] ss1 = ss[1].Split('}');
                                    r.CustomStr10 = ss1[0];
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr10 : " + r.CustomStr10);
                                }
                                else if (r.EventId == 20272)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Event ıd72 : " + r.EventId);
                                    r.EventCategory = fields[13];
                                    r.UserName = fields[3];
                                    r.CustomStr5 = fields[9] + " " + fields[11];
                                    r.CustomStr6 = fields[15] + " " + fields[17];
                                    r.CustomStr7 = fields[24] + " " + fields[25] + " " + fields[26] + " " + fields[27];
                                    r.CustomStr8 = fields[7];
                                    r.CustomStr9 = fields[49] + " " + fields[50] + fields[51].Trim('.');
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr9 : " + r.CustomStr9);

                                    string[] ss = fields[0].Split('{');
                                    string[] ss1 = ss[1].Split('}');
                                    r.CustomStr10 = ss1[0];

                                    r.CustomInt6 = Convert.ToInt64(fields[29]);
                                    r.CustomInt7 = Convert.ToInt64(fields[34]);
                                }
                                else if (r.EventId == 20274)
                                {
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "Event ıd74 : " + r.EventId);
                                    r.EventCategory = fields[4];
                                    r.UserName = fields[3];
                                    r.CustomStr4 = fields[12];
                                    r.CustomStr8 = fields[7];
                                    Log.Log(LogType.FILE, LogLevel.INFORM, "CustomStr8 : " + r.CustomStr8);

                                    string[] ss = fields[0].Split('{');
                                    string[] ss1 = ss[1].Split('}');
                                    r.CustomStr10 = ss1[0];
                                }

                                Log.Log(LogType.FILE, LogLevel.INFORM, "Parsing successful :");
                                r = setRecParse(r, r.Description);
                            }
                            catch (Exception e)
                            {
                                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                            }
                            /*if (lst.Count > 0)
                                r.CustomStr1 = lst[0];
                            if (lst.Count > 1)
                                r.CustomStr2 = lst[1];
                            if (lst.Count > 2)
                                r.CustomStr3 = lst[2];
                            if (lst.Count > 3)
                                r.CustomStr4 = lst[3];
                            if (lst.Count > 4)
                                r.CustomStr5 = lst[4];
                            if (lst.Count > 5)
                                r.CustomStr6 = lst[5];
                            if (lst.Count > 6)
                                r.CustomStr7 = lst[6];
                            if (lst.Count > 7)
                                r.CustomStr8 = lst[7];
                            if (lst.Count > 8)
                                r.CustomStr9 = lst[8];
                            if (lst.Count > 9)
                                r.CustomStr10 = lst[9];
                            
                            if (lst.Count > 10)
                            {
                                if (lst.Count > 11)
                                {
                                    for (Int32 i = 10; i < lst.Count; i++)
                                        r.Description += lst[i] + " ";
                                    r.Description = r.Description.Trim();
                                }
                                else
                                {
                                    r.Description = lst[10];
                                }                                
                            }
                            else
                                r.Description = "";*/

                            /*if (lst.Count > 6)
                                r.Description = lst[6];
                            else if (lst.Count != 0)
                                r.Description = lst[0];
                            else
                                r.Description = "";*/

                            Position = recCast.RecordNumber;

                            SetRegistry();
                            SetRecordData(r);
                        }

                        if (maxReadLineCount != -1)
                        {
                            readLineCount++;
                            if (readLineCount > maxReadLineCount)
                            {
                                if (threadSleepTime <= 0)
                                    Thread.Sleep(60000);
                                else
                                    Thread.Sleep(threadSleepTime);
                                readLineCount = 0;
                            }
                        }

                        dw += recCast.Length;
                    }

                    if (!changed)
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, e.StackTrace);
            }
            Int32 error = Marshal.GetLastWin32Error();
            if (error == 87)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, "Win Error on parse, probably eventlog cleared. Error code(" + error + ")");
                Log.Log(LogType.FILE, LogLevel.ERROR, "Starting from begining.");
                Position = 0;
                SetRegistry();
            }
            CloseEventLog(handle);
        }

        public Rec setRecParse(Rec r, string sMessage)
        {
            Rec rec = r;

            Log.Log(LogType.FILE, LogLevel.DEBUG, "Message:" + sMessage);
            string[] DescArr = sMessage.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

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

            for (int i = 0; i < DescArr.Length; i++)
            {

                try
                {
                    if (!DescArr[i].Contains(":"))
                    {
                        if (accessMode)
                        {
                            rec.CustomStr7 += " " + DescArr[i].Trim();
                            if (rec.CustomStr7.Length > 900)
                            {
                                rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                            }
                        }
                    }
                    else
                    {
                        string[] lineArr = DescArr[i].Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "DescArr[" + i + "]:" + DescArr[i]);
                        Log.Log(LogType.FILE, LogLevel.DEBUG, "lineArr.Length:" + lineArr.Length.ToString());

                        if (lineArr.Length > 1)
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
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;

                                        //case "Security ID":
                                        //    if (rec.CustomStr2 == null)
                                        //    {
                                        //        rec.CustomStr2 = appendArrayElements(lineArr);
                                        //    }
                                        //    break;
                                        case "Logon ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt6 = 0;
                                            }
                                            break;
                                        case "Client Context ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt6 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt6 = 0;
                                            }
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
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
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        //case "Target Server Name":
                                        //    rec.CustomStr2 = appendArrayElements(lineArr);
                                        //    break;
                                        case "Account Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Old Account Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "New Account Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
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
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;
                                        case "Object Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Operation Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Handle ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt7 = 0;
                                            }
                                            break;
                                        case "Primary User Name":
                                            if (rec.CustomStr1 == null)
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client User Name":
                                            if (rec.CustomStr2 == null)
                                            {
                                                rec.CustomStr2 = appendArrayElements(lineArr);
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
                                            if (rec.CustomStr7 == null)
                                            {
                                                rec.CustomStr7 = appendArrayElements(lineArr);
                                                if (rec.CustomStr7.Length > 900)
                                                {
                                                    rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                                                }
                                                dummyAccessControl = true;
                                            }
                                            break;
                                        case "Access Mask":
                                            if (dummyAccessControl)
                                            {
                                                rec.CustomStr7 += " " + appendArrayElements(lineArr);
                                                if (rec.CustomStr7.Length > 900)
                                                {
                                                    rec.CustomStr7 = rec.CustomStr7.Substring(0, 900);
                                                }
                                            }
                                            break;
                                        case "Operation Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
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
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "PID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
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
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Duration":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Application Instance ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Application Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                }
                                else if (networkMode)
                                {

                                    //L.Log(LogType.FILE, LogLevel.DEBUG, "lineArr[0]:" + lineArr[0]);

                                    #region NetworkMode==True
                                    switch (lineArr[0].Trim())
                                    {
                                        case "Client Address":
                                            rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                            break;
                                        case "Source Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Workstation Name":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        //case "ffff":
                                        //    rec.CustomStr3 = appendArrayElements(lineArr);
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
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Pre-Authentication Type":
                                            string authenPack3 = appendArrayElements(lineArr);
                                            if (authenPack3.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack3.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack3.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Logon Process":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Account":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
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
                                            if (rec.CustomStr1 != null)
                                            {
                                                rec.CustomStr2 = rec.CustomStr1;
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            else
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
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
                                                    rec.CustomInt3 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt3 = int.Parse(appendArrayElements(lineArr));
                                                }

                                            }
                                            else
                                            {
                                                rec.CustomInt3 = 0;
                                            }
                                            break;
                                        case "Error Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Status Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Failure Code":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt1 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt1 = 0;
                                            }
                                            break;
                                        case "Caller Workstation":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Workstation Name":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Workstation":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "User Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Name":
                                            if (rec.CustomStr1 != null)
                                            {
                                                rec.CustomStr2 = rec.CustomStr1;
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            else
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client Name":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Account":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Caller User Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Account Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Name":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Group Domain":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Caller Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Target Domain":
                                            rec.CustomStr7 = appendArrayElements(lineArr);
                                            break;
                                        case "Target User Name":
                                            rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        case "Source Network Address":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Client Address":
                                            rec.CustomStr3 = lineArr[lineArr.Length - 1];
                                            //rec.CustomStr3 = appendArrayElements(lineArr);dali
                                            break;
                                        case "Source Port":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Authentication Package":
                                            string authenPack = appendArrayElements(lineArr);
                                            if (authenPack.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack.Contains("Kerberos") || authenPack.Contains("KDS"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Pre-Authentication Type":
                                            string authenPack2 = appendArrayElements(lineArr);
                                            if (authenPack2.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack2.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack2.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Caller Process ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "PID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "Logon Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Logon Process":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Process Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Image File Name":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Duration":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt2 = int.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt2 = 0;
                                            }
                                            break;
                                        case "Object Name":
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;
                                        case "Object Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Operation Type":
                                            rec.CustomStr9 = appendArrayElements(lineArr);
                                            break;
                                        case "Handle ID":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt7 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt7 = 0;
                                            }
                                            break;
                                        case "Primary User Name":
                                            if (rec.CustomStr1 == null)
                                            {
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        case "Client User Name":
                                            if (rec.CustomStr2 == null)
                                            {
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                            }
                                            break;
                                        //case "ffff":
                                        //    rec.CustomStr3 = appendArrayElements(lineArr);
                                        //    break;


                                        //D.Ali Türkce Gelen Loglar İçin
                                        case "Kullanıcı Adı":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "İş İstasyonu Adı":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma işlemi":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma Türü":
                                            if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                            else
                                                rec.CustomInt5 = -1;
                                            break;
                                        case "Etki Alanı":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak Ağ Adresi":
                                            rec.CustomStr3 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Hesabı":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak İş İstasyonu":
                                            rec.CustomStr4 = appendArrayElements(lineArr);
                                            break;
                                        case "Share Name":
                                            rec.CustomStr8 = appendArrayElements(lineArr);
                                            break;
                                        case "Hesap Adı":
                                            if (string.IsNullOrEmpty(rec.CustomStr1))
                                                rec.CustomStr1 = appendArrayElements(lineArr);
                                            else
                                                rec.CustomStr2 = appendArrayElements(lineArr);
                                            break;
                                        /////////

                                        case "Güvenlik Kimliği":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Hesap Etki Alanı":
                                            rec.CustomStr5 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Açma Kimliği":
                                            rec.CustomStr1 = appendArrayElements(lineArr);
                                            break;
                                        case "Oturum Türü":
                                            if (string.IsNullOrEmpty(appendArrayElements(lineArr)) == false)
                                                rec.CustomInt5 = int.Parse(appendArrayElements(lineArr));
                                            else
                                                rec.CustomInt5 = -1;
                                            break;

                                        case "İşlem Kimliği":
                                            if (!lineArr[1].Contains("-"))
                                            {
                                                if (lineArr[1].Contains("0x"))
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr).TrimStart("0".ToCharArray()).TrimStart("x".ToCharArray()), System.Globalization.NumberStyles.HexNumber);
                                                }
                                                else
                                                {
                                                    rec.CustomInt8 = long.Parse(appendArrayElements(lineArr));
                                                }
                                            }
                                            else
                                            {
                                                rec.CustomInt8 = 0;
                                            }
                                            break;
                                        case "İşlem Adı":
                                            rec.CustomStr6 = appendArrayElements(lineArr);
                                            break;
                                        case "Kaynak Bağlantı Noktası":
                                            try
                                            {
                                                rec.CustomInt4 = int.Parse(appendArrayElements(lineArr));
                                            }
                                            catch (Exception)
                                            {
                                                rec.CustomInt4 = 0;
                                            }
                                            break;
                                        case "Kimlik Doğrulama Paketi":
                                            string authenPack4 = appendArrayElements(lineArr);
                                            if (authenPack4.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack4.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack4.Contains("Kerberos"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
                                            }
                                            break;
                                        case "Paket Adı (yalnızca NTLM)":
                                            string authenPack3 = appendArrayElements(lineArr);
                                            if (authenPack3.Contains("Negotiate"))
                                            {
                                                rec.CustomInt5 = 0;
                                            }
                                            else if (authenPack3.Contains("NTLM"))
                                            {
                                                rec.CustomInt5 = 1;
                                            }
                                            else if (authenPack3.Contains("Kerberos") || authenPack3.Contains("KDS"))
                                            {
                                                rec.CustomInt5 = 2;
                                            }
                                            else
                                            {
                                                rec.CustomInt5 = 3;
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
                catch (Exception exp)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "setREcParse :" + exp.Message);
                }
            }

            if (rec.Description.Length > 900)
            {

                if (rec.Description.Length > 1800)
                {
                    rec.CustomStr10 = rec.Description.Substring(900, 900);
                }
                else
                {
                    rec.CustomStr10 = rec.Description.Substring(900, rec.Description.Length - 900 - 2);
                }

                rec.Description = rec.Description.Substring(0, 900);
                Log.Log(LogType.FILE, LogLevel.DEBUG, "Description text splitted to CustomStr10");

            }


            return rec;
        }

        //Tools
        public byte[] StructureToByteArray(object obj)
        {
            Int32 len = Marshal.SizeOf(obj);
            Byte[] arr = new Byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;

        }

        public void ByteArrayToStructure(Byte[] bytearray, Int32 startIndex, ref object obj)
        {
            Int32 len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, startIndex, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
        }

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

        public int returneventid(int eventid)
        {
            int result = 0;
            int eventidmask = 0x0000FFFF;
            result = eventid & eventidmask;
            return result;
        }
    }
}
