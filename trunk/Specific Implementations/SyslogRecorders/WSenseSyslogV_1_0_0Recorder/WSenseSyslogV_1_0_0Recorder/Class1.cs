//WSenseSyslogV_1_0_0Recorder
//Onur Sarıkaya
//21.01.2013
//29.05.2013 Update Türkçe karakter sorunu çözüldü. 
//DSI
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WSenseSyslogV_1_0_0Recorder;
//

namespace WSenseSyslogV_1_0_0Recorder
{
    public class WSenseSyslogV_1_0_0Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 4, Syslog_Port = 514, zone = 0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost, Dal;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        private Encoding SyslogEncoding = Encoding.ASCII;
        public string SysllogLogFile;
        public Int32 SyslogLogSize;

        private void InitializeComponent()
        {

        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal, Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            zone = Zone;

            if (!string.IsNullOrEmpty(CustomVar1))
            {
                string[] customArr = CustomVar1.Split(';');
                for (int i = 0; i < customArr.Length; i++)
                {
                    if (customArr[i].StartsWith("E="))
                    {
                        SyslogEncoding = Encoding.GetEncoding(customArr[i].Split('=')[1]);
                    }

                    if (customArr[i].StartsWith("Lf="))
                    {
                        SysllogLogFile = customArr[i].Split('=')[1];
                    }

                    if (customArr[i].StartsWith("Ls="))
                    {
                        SyslogLogSize = int.Parse(customArr[i].Split('=')[1]);
                    }
                }
            }
        }

        public override void Init()
        {
            try
            {
                if (usingRegistry)
                {
                    if (!reg_flag)
                    {
                        if (!Read_Registry())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Reading the Registry ");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WSenseSyslogV_1_0_0Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }
                }
                else
                {
                    if (!reg_flag)
                    {
                        if (!Get_logDir())
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Error on Getting the log dir");
                            return;
                        }
                        else
                            if (!Initialize_Logger())
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on WSenseSyslogV_1_0_0Recorder functions may not be running");
                                return;
                            }
                        reg_flag = true;
                    }

                    if (location.Length > 1)
                    {
                        if (location.Contains(':'.ToString()))
                        {
                            protocol = location.Split(':')[0];
                            Syslog_Port = Convert.ToInt32(location.Split(':')[1]);
                            if (protocol.ToLower() == "tcp")
                                pro = ProtocolType.Tcp;
                            else
                                pro = ProtocolType.Udp;
                        }
                        else
                        {
                            protocol = location;
                            Syslog_Port = 514;
                        }
                    }
                    else
                    {
                        pro = ProtocolType.Udp;
                        Syslog_Port = 514;
                    }
                }

                if (usingRegistry)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
                }
                else
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + remote_host + " port: " + Syslog_Port.ToString());
                    slog = new Syslog(remote_host, Syslog_Port, pro);

                    if (!string.IsNullOrEmpty(SysllogLogFile))
                    {
                        slog.EnablePacketLog = true;
                        slog.LogFile = SysllogLogFile;
                        slog.LogSize = SyslogLogSize;
                    }
                    slog.Encoding = SyslogEncoding;
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_0Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\WSenseSyslogV_1_0_0Recorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_0Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public WSenseSyslogV_1_0_0Recorder()
            : base()
        {
            // TODO: Add any initialization after the InitComponent call       
            //   enc = Encoding.Unicode;
        }

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

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent Line: " + args.Message);
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                try
                {
                    rec.LogName = "WSenseSyslogV_1_0_0Recorder";
                    rec.EventType = args.EventLogEntType.ToString();
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);

                    if (args.Message.Length > 899)
                    {
                        rec.Description = args.Message.Substring(0, 899);
                    }
                    else
                    {
                        rec.Description = args.Message;
                    }

                    string line = args.Message;
                    string[] lineArr = SpaceSplit(line, false);

                    try
                    {
                        DateTime dt;
                        string dateNow = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
                        string myDateTimeString = lineArr[4] + lineArr[3] + "," + dateNow + "  ," + lineArr[5];
                        dt = Convert.ToDateTime(myDateTimeString);
                        rec.Datetime = dt.ToString(dateFormat);
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Datetime: " + rec.Datetime);
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Date Error: " + exception.Message);
                    }

                    //L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);

                    if (lineArr.Length > 6)
                    {
                        rec.CustomStr1 = lineArr[6];
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                    }
                    try
                    {
                        //if (lineArr[i].StartsWith("category"))
                        if (lineArr.Length > 12)
                        {
                            if (lineArr[12].Trim().StartsWith("category"))
                            {
                                rec.EventCategory = SplitedLine(lineArr[12]);
                                //Console.WriteLine("EventCategory: " + rec.EventCategory);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("category"))
                                    {
                                        rec.EventCategory = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("EventCategory: " + rec.EventCategory);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + exception.Message);
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("user"))
                        if (lineArr.Length > 13)
                        {
                            if (lineArr[13].Trim().StartsWith("user"))
                            {
                                if (lineArr[13].Contains("://"))
                                {
                                    rec.ComputerName = After(SplitedLine(lineArr[13]), "://");
                                    //Console.WriteLine("ComputerName: " + rec.ComputerName);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);

                                    if (lineArr.Length > 14)
                                    {
                                        string d = lineArr[14].Split('/')[lineArr[14].Split('/').Length - 1];
                                        rec.UserName = d + " " + lineArr[15];
                                        //Console.WriteLine("UserName: " + rec.UserName);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                                        string df = Before(lineArr[14], "DC=local");
                                        try
                                        {
                                            if (df.EndsWith(","))
                                            {
                                                rec.SourceName = df.Substring(0, df.Length - 1);
                                                //Console.WriteLine("SourceName: " + rec.SourceName);
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            L.Log(LogType.FILE, LogLevel.ERROR, "SourceName: " + exception.Message);
                                        }
                                    }

                                }
                                else
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName = null");
                                }
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("user"))
                                    {
                                        rec.ComputerName = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("ComputerName: " + rec.ComputerName);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + exception.Message);
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("action"))
                        if (lineArr.Length > 10)
                        {
                            if (lineArr[10].Trim().StartsWith("action"))
                            {
                                rec.CustomStr2 = SplitedLine(lineArr[10]);
                                //Console.WriteLine("CustomStr2: " + rec.CustomStr2);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("action"))
                                    {
                                        rec.CustomStr2 = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("CustomStr2: " + rec.CustomStr2);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2: " + exception.Message);
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("src_host"))
                        if (lineArr.Length > 16)
                        {
                            if (lineArr[10].Trim().StartsWith("src_host"))
                            {
                                rec.CustomStr3 = SplitedLine(lineArr[16]);
                                //Console.WriteLine("CustomStr3: " + rec.CustomStr3);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("src_host"))
                                    {
                                        rec.CustomStr3 = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("CustomStr3: " + rec.CustomStr3);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr2: " + exception.Message);
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("dst_ip"))
                        if (lineArr.Length > 19)
                        {
                            if (lineArr[19].Trim().StartsWith("dst_ip"))
                            {
                                rec.CustomStr4 = SplitedLine(lineArr[19]);
                                //Console.WriteLine("CustomStr4: " + rec.CustomStr4);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("dst_ip"))
                                    {
                                        rec.CustomStr4 = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("CustomStr4: " + rec.CustomStr4);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr4: " + exception.Message);
                    }


                    try
                    {
                        //if (lineArr[i].StartsWith("dst_ip"))
                        if (lineArr.Length > 18)
                        {
                            if (lineArr[18].Trim().StartsWith("dst_host"))
                            {
                                rec.CustomStr5 = SplitedLine(lineArr[18]);
                                //Console.WriteLine("CustomStr5: " + rec.CustomStr5);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("dst_host"))
                                    {
                                        rec.CustomStr5 = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("CustomStr5: " + rec.CustomStr5);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr5: " + exception.Message);
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("url"))
                        if (lineArr.Length > 33)
                        {
                            if (lineArr[33].StartsWith("url"))
                            {
                                rec.CustomStr6 = SplitedLine(lineArr[33]);
                                //Console.WriteLine("CustomStr6: " + rec.CustomStr6);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("url"))
                                    {
                                        rec.CustomStr6 = SplitedLine(lineArr[i]);
                                        //Console.WriteLine("CustomStr6: " + rec.CustomStr6);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                                    }
                                }
                            }

                        }
                        else
                        {
                            rec.CustomStr6 = SplitedLine(lineArr[lineArr.Length - 1]);
                            //Console.WriteLine("CustomStr6: " + rec.CustomStr6);
                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr6: " + rec.CustomStr6);
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr6: " + exception.Message);
                    }

                    //try
                    //{
                    //    if (rec.CustomStr6.Length > 899)
                    //    {
                    //        rec.CustomStr7 = rec.CustomStr6.Substring(899, 1799);
                    //    }
                    //}
                    //catch (Exception exception)
                    //{
                    //    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + exception.Message);
                    //}

                    try
                    {
                        //if (lineArr[i].StartsWith("disposition"))
                        if (lineArr.Length > 29)
                        {
                            if (lineArr[29].StartsWith("disposition"))
                            {
                                rec.CustomInt1 = Convert.ToInt32(SplitedLine(lineArr[29]));
                                //Console.WriteLine("CustomInt1: " + rec.CustomInt1);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("disposition"))
                                    {
                                        rec.CustomInt1 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        //Console.WriteLine("CustomInt1: " + rec.CustomInt1);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1 Casting error, CustomInt1 setted 0 " + exception.Message);
                        rec.CustomInt1 = 0;
                    }
                    try
                    {
                        //if (lineArr[i].StartsWith("http_response"))
                        if (lineArr.Length > 23)
                        {
                            if (lineArr[23].StartsWith("http_response"))
                            {
                                rec.CustomInt3 = Convert.ToInt32(SplitedLine(lineArr[23]));
                                //Console.WriteLine("CustomInt3: " + rec.CustomInt3);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt1);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("http_response"))
                                    {
                                        rec.CustomInt3 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        //Console.WriteLine("CustomInt3: " + rec.CustomInt3);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3 Casting error, CustomInt3 setted 0 " + exception.Message);
                        rec.CustomInt3 = 0;
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("severity"))
                        if (lineArr.Length > 11)
                        {
                            if (lineArr[11].StartsWith("severity"))
                            {
                                rec.CustomInt5 = Convert.ToInt32(SplitedLine(lineArr[11]));
                                //Console.WriteLine("CustomInt5: " + rec.CustomInt5);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("severity"))
                                    {
                                        rec.CustomInt5 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        //Console.WriteLine("CustomInt5: " + rec.CustomInt5);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5 Casting error, CustomInt5 setted 0 " + exception.Message);
                        rec.CustomInt5 = 0;
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("bytes_out"))
                        if (lineArr.Length > 21)
                        {
                            if (lineArr[21].StartsWith("bytes_out"))
                            {
                                rec.CustomInt7 = Convert.ToInt32(SplitedLine(lineArr[21]));
                                //Console.WriteLine("CustomInt7: " + rec.CustomInt7);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("bytes_out"))
                                    {
                                        rec.CustomInt7 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        //Console.WriteLine("CustomInt7: " + rec.CustomInt7);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt7 Casting error, CustomInt7 setted 0 " + exception.Message);
                        rec.CustomInt7 = 0;
                    }

                    try
                    {
                        //if (lineArr[i].StartsWith("bytes_in"))
                        if (lineArr.Length > 22)
                        {
                            if (lineArr[22].StartsWith("bytes_in"))
                            {
                                rec.CustomInt8 = Convert.ToInt32(SplitedLine(lineArr[22]));
                                //Console.WriteLine("CustomInt8: " + rec.CustomInt8);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("bytes_in"))
                                    {
                                        rec.CustomInt8 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        //Console.WriteLine("CustomInt8: " + rec.CustomInt8);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt8 Casting error, CustomInt8 setted 0 " + exception.Message);
                        rec.CustomInt8 = 0;
                    }
                    try
                    {
                        //if (lineArr[i].StartsWith("src_port"))
                        if (lineArr.Length > 17)
                        {
                            if (lineArr[17].StartsWith("src_port"))
                            {
                                rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[17]));
                                //Console.WriteLine("CustomInt9: " + rec.CustomInt9);
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("src_port"))
                                    {
                                        rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        ////Console.WriteLine("CustomInt9: " + rec.CustomInt9);
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt9 Casting error, CustomInt9 setted 0 " + exception.Message);
                        rec.CustomInt9 = 0;
                    }
                    try
                    {
                        //if (lineArr[i].StartsWith("dst_port"))
                        if (lineArr.Length > 20)
                        {
                            if (lineArr[20].StartsWith("dst_port"))
                            {
                                rec.CustomInt10 = Convert.ToInt32(SplitedLine(lineArr[20]));
                                L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt10: " + rec.CustomInt10);
                            }
                            else
                            {
                                for (int i = 0; i < lineArr.Length; i++)
                                {
                                    if (lineArr[i].StartsWith("dst_port"))
                                    {
                                        rec.CustomInt10 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt10: " + rec.CustomInt10);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt10 Casting error, CustomInt10 setted 0 " + exception.Message);
                        rec.CustomInt10 = 0;
                    }

                    //L.Log(LogType.FILE, LogLevel.DEBUG, " Source Is : " + args.Source.ToString());
                    //rec.SourceName = args.Source;
                    L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);

                }
                catch (Exception e)
                {
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ERROR------------");
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.Message);
                    L.Log(LogType.FILE, LogLevel.DEBUG, e.StackTrace);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
            }
            catch (Exception er)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.ERROR, args.EventLogEntType + " " + args.Message);
            }
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


        public string SplitedLine(string line)
        {
            string returnLine = "";
            try
            {
                returnLine = line.Split('=')[1];
            }
            catch (Exception exception)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Splitted Line()-->" + exception.ToString());
            }
            return returnLine;
        }


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
        }// SpaceSplit

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public bool Initialize_Logger()
        {
            try
            {
                L = new CLogger();
                switch (trc_level)
                {
                    case 0:
                        {
                            L.SetLogLevel(LogLevel.NONE);
                        } break;
                    case 1:
                        {
                            L.SetLogLevel(LogLevel.INFORM);
                        } break;
                    case 2:
                        {
                            L.SetLogLevel(LogLevel.WARN);
                        } break;
                    case 3:
                        {
                            L.SetLogLevel(LogLevel.ERROR);
                        } break;
                    case 4:
                        {
                            L.SetLogLevel(LogLevel.DEBUG);
                        } break;
                }

                L.SetLogFile(err_log);
                L.SetTimerInterval(LogType.FILE, logging_interval);
                L.SetLogFileSize(log_size);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager WSenseSyslogV_1_0_0Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
