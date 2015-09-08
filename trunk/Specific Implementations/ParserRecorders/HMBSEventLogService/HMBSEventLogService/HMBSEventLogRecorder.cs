/*
 * HMBS Event Log Recorder
 * Copyright (C) 2011 İbrahim Luy
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

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
    public partial class HMBSEventLogRecorder : AppParser
    {
        public HMBSEventLogRecorder()
            : base()
        {
            usingRegistry = true;
            LogName = "HMBSEventLogRecorder";
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
                RegistryKey key = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\HMBSEventLog");
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
            IntPtr handle = OpenEventLog(remoteHost, "Application");
            Byte[] output = new byte[65536];
            Int32 bytesRead = 0;
            Int32 minNumberOfBytesNeeded = 0;
            try
            {
                Int32 flags = 0;
                if (Position == 0)
                {
                    flags = (Int32)ReadFlags.EVENTLOG_SEQUENTIAL_READ | (Int32)ReadFlags.EVENTLOG_FORWARDS_READ;
                }
                else
                {
                    flags = (Int32)ReadFlags.EVENTLOG_SEEK_READ | (Int32)ReadFlags.EVENTLOG_FORWARDS_READ;
                }
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
                            IntPtr ptr = IntPtr.Zero;

                            SetDateTime(ref r, recCast);
                            r.SourceName = GetSourceName(ptr, bytesRead, dw, output);
                            //r.EventCategory = recCast.EventCategory.ToString();
                            //r.EventType = ((EventType)(recCast.EventType)).ToString();

                            if (r.SourceName.ToLower() == "hmbs")
                            {
                                r.ComputerName = GetComputerName(ptr, bytesRead, dw, output, r.SourceName.Length);

                                //r.EventId = GetEventId(recCast.EventID);
                                //Log.Log(LogType.FILE, LogLevel.INFORM, "Event_Id :" + r.EventId);
                                //r.Recordnum = recCast.RecordNumber;
                                //r.LogName = Dir;

                                Int32 offset = dw + recCast.StringOffset;
                                ptr = Marshal.AllocHGlobal(bytesRead);
                                Marshal.Copy(output, offset, ptr, bytesRead - offset);
                                String str = Marshal.PtrToStringAnsi(ptr);

                                //List<String> lst = new List<String>();
                                //for (Int32 i = 0; i < recCast.NumStrings; i++)
                                //{
                                //    ptr = Marshal.AllocHGlobal(bytesRead);
                                //    Marshal.Copy(output, offset, ptr, bytesRead - offset);
                                //    String str = Marshal.PtrToStringAnsi(ptr);
                                //    lst.Add(str);
                                //    Marshal.FreeHGlobal(ptr);
                                //    offset += str.Length + 1;
                                //}

                                //r.Description = GetString((UInt32)recCast.EventID, r.SourceName, "EventMessageFile", lst);

                                PrivateParse(ref r, str);
                                SetRecordData(r);
                                Position = recCast.RecordNumber;
                                SetRegistry();

                            }

                        }//end of if

                        if (maxReadLineCount != -1)
                        {
                            readLineCount++;
                            if (readLineCount > maxReadLineCount)
                            {
                                if (threadSleepTime <= 0)
                                {
                                    Thread.Sleep(60000);
                                }
                                else
                                {
                                    Thread.Sleep(threadSleepTime);
                                }
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
                Log.Log(LogType.FILE, LogLevel.ERROR, " Parse() -->> An error occurred : " + e.Message);
                Log.Log(LogType.FILE, LogLevel.ERROR, " Parse() -->> An error occurred : " + e.StackTrace);
            }
            Int32 error = Marshal.GetLastWin32Error();
            if (error == 87)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " Parse() -->> Win Error on parse, probably eventlog cleared. Error code(" + error + ")");
                Log.Log(LogType.FILE, LogLevel.ERROR, " Parse() -->> Starting from begining.");
                Position = 0;
                SetRegistry();
            }
            CloseEventLog(handle);
        }

        private void SetDateTime(ref Rec r, EVENTLOGRECORD recCast)
        {
            try
            {
                DateTime d = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(recCast.TimeWritten));
                r.Datetime = d.Year + "/" + d.Month + "/" + d.Day + " " + d.Hour + ":" + d.Minute + ":" + d.Second + "." + recCast.TimeWritten;
                r.Datetime = Convert.ToDateTime(r.Datetime).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");

            }
            catch (Exception ex)
            {
                r.CustomInt1 = recCast.TimeWritten;
                Log.Log(LogType.FILE, LogLevel.ERROR, " SetDateTime() -->> An error occurred." + ex.ToString());
            }
        }

        private string GetSourceName(IntPtr ptr, int bytesRead, int dw, byte[] output)
        {
            try
            {
                String SourceName = "";

                ptr = Marshal.AllocHGlobal(bytesRead);
                Int32 total = dw + 56;
                Marshal.Copy(output, total, ptr, bytesRead - total); //56 struct size
                SourceName = Marshal.PtrToStringAnsi(ptr);
                Marshal.FreeHGlobal(ptr);

                return SourceName;

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetSourceName() -->> An error occurred." + ex.ToString());
                return null;
            }
        }

        private string GetComputerName(IntPtr ptr, int bytesRead, int dw, byte[] output, int sourceNameLength)
        {
            try
            {
                String ComputerName = "";
                ptr = Marshal.AllocHGlobal(bytesRead);
                Int32 total = dw + 57 + sourceNameLength;
                Marshal.Copy(output, total, ptr, bytesRead - total);
                ComputerName = Marshal.PtrToStringAnsi(ptr);
                Marshal.FreeHGlobal(ptr);

                return ComputerName;
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetComputerName() -->> An error occurred." + ex.ToString());
                return null;
            }
        }

        private void PrivateParse(ref Rec r, string str)
        {
            try
            {
                /*LOG[HMBS|2011.1.31-17:03:33|volkan.ak|UPDATE|Alacak Bilgi Sistemi|Banka-Şube Girişi|
                 * d_u_f_branch|branch_bank_code("1"), branch_code("1"), branch_addrress(""), branch_phone(""), 
                 * branch_fax(""), branche_name("ANKARA SUBESI 2")|branch_bank_code("1"), branch_code("1"), 
                 * branch_addrress("2"), branch_phone("3"), branch_fax("4"), branche_name("ANKARA SUBESI")|1-1-||]*/


                if (str.Length > 4000)
                {
                    r.Description = str.Remove(3999);
                }
                r.Description = str;
                string[] parts = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                r.UserName = parts[2];
                r.EventCategory = parts[3];
                r.EventType = parts[10];
                r.CustomStr1 = parts[4];
                r.CustomStr2 = parts[5];

                if (parts[7].Length > 4000)
                {
                    parts[7] = parts[7].Remove(3999);
                }
                r.CustomStr3 = parts[7];

                if (parts[8].Length > 4000)
                {
                    parts[8] = parts[8].Remove(3999);
                }

                r.CustomStr4 = parts[8];
                r.CustomStr5 = parts[6];
                r.CustomStr7 = parts[9];
                r.CustomStr8 = parts[11];
                r.CustomStr9 = parts[12];
                r.CustomStr10 = parts[13];

            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " PrivateParse() -->> An error occurred." + ex.ToString());
            }
        }

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

        public void ByteArrayToStructure(byte[] bytearray, Int32 startIndex, ref object obj)
        {
            Int32 len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, startIndex, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
        }

        public int GetEventId(int eventid)
        {
            int result = 0;
            int eventidmask = 0x0000FFFF;
            result = eventid & eventidmask;
            return result;
        }

    }
}
