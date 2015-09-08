//SonicWallV_TZ120_0_0UTMRecorder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace SonicWallV_TZ120_0_0UTMRecorder
{
    public class SonicWallV_TZ120_0_0UTMRecorder : CustomBase
    {



        #region Değişkenler
        private string sourceName;
        private int sourceportNumber;
        private string id;
        private string sn;
        private string time;
        private string fw;
        private int pri;
        private int c;
        private int m;
        private string msg;
        private int n;
        private string dst;
        private string proto;
        private string src;
        private int sent;
        private int rcvd;
        private string vpnpolicy;
        private string op;
        private int result;
        private string dstname;
        private int code;
        private string Category;
        private string log_Name;
        private string event_Type;
        private string arg;
        private string info;
        private string description;
        private string unexpectedDescription;
        #endregion

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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Syslog Recorder functions may not be running");
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
                }

                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\SyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager Syslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public SonicWallV_TZ120_0_0UTMRecorder()
        {
            /*
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Recorder");

                L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port,System.Net.Sockets.ProtocolType.Tcp);
                slog.Start();
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(slog_SyslogEvent);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager SyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }
             */
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.INFORM, " Log : " + args.Message);
                try
                {

                    rec.Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    rec.EventType = args.EventLogEntType.ToString();
                    rec.LogName = "SonicWallV_TZ120_0_0UTMRecorder";


                    if (args.Message.Length > 899)
                    {
                        rec.Description = args.Message.Substring(0, 899);
                    }
                    else
                    {
                        rec.Description = args.Message;
                    }

                    string[] lineArr = SpaceSplit(args.Message, false);

                    rec.SourceName = lineArr[2];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "SourceName: " + rec.SourceName);
                    rec.EventType = lineArr[2].Split('.')[1];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventType: " + rec.EventType);
                    rec.ComputerName = lineArr[0];
                    L.Log(LogType.FILE, LogLevel.DEBUG, "ComputerName: " + rec.ComputerName);

                    //if (lineArr[2] == "local0.info")
                    {
                        try
                        {
                            if (lineArr.Length > 16)
                            {
                                if (lineArr[16].Trim().StartsWith("op="))
                                {
                                    rec.EventCategory = SplitedLine(lineArr[16]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("op="))
                                        {
                                            rec.EventCategory = SplitedLine(lineArr[i]);
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "EventCategory: " + rec.EventCategory);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "EventCategory: " + exception.Message);
                        }
                        //192.168.3.1:514 : local0.error id=firewall sn=0017C56122AA time="2013-01-28 13:14:13 UTC" fw=none pri=3 c=4 m=14 msg="Web site access denied" n=223847 src=192.168.3.142:49562:X0:MEHMETSsTCs-PC dst=31.13.64.7:80:X1:star-01-01-ams2.facebook.com dstname=www.facebook.com arg=/plugins/like.php?href=http%3A%2F%2Fwww.facebook.com%2Fpages%2Fgazetea24com%2F168179866539250&send=false&layout code=58 Category="Social Networking" 
                        try
                        {
                            string userName = "";
                            if (lineArr.Length > 13)
                            {
                                if (lineArr[13].Trim().StartsWith("src="))
                                {
                                    if (SplitedLine(lineArr[13]).Split(':').Length > 2)
                                    {
                                        userName = After(SplitedLine(lineArr[13]), "X0:");
                                        L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("src="))
                                        {
                                            if (SplitedLine(lineArr[i]).Split(':').Length > 2)
                                            {
                                                userName = After(SplitedLine(lineArr[i]), "X0:");
                                                L.Log(LogType.FILE, LogLevel.DEBUG, "UserName: " + rec.UserName);
                                            }
                                        }
                                    }
                                }
                                if (userName.Contains(":"))
                                {
                                    rec.UserName = userName.Split(':')[0];
                                }
                                else
                                {
                                    rec.UserName = userName;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "UserName: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 22)
                            {
                                if (lineArr[22].Trim().StartsWith("Category="))
                                {
                                    rec.CustomStr1 = Between(args.Message, "Category=", " ");
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("Category="))
                                        {
                                            rec.CustomStr1 = Between(args.Message, "Category=", " ");
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr1: " + rec.CustomStr1);
                                        }
                                    }
                                }
                            }
                            rec.CustomStr1 = rec.CustomStr1.Replace('"', ' ').Trim();
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr1: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 15)
                            {
                                if (lineArr[15].Trim().StartsWith("proto="))
                                {
                                    rec.CustomStr2 = SplitedLine(lineArr[15]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr2: " + rec.CustomStr2);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("proto="))
                                        {
                                            rec.CustomStr2 = SplitedLine(lineArr[i]);
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
                            if (lineArr.Length > 13)
                            {
                                if (lineArr[13].Trim().StartsWith("src"))
                                {
                                    rec.CustomStr3 =
                                        SplitedLine(lineArr[13]).Split(':')[0];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("src"))
                                        {
                                            rec.CustomStr3 = SplitedLine(lineArr[i]).Split(':')[SplitedLine(lineArr[i]).Split(':').Length - 1];
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr3: " + rec.CustomStr3);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr3: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 14)
                            {
                                if (lineArr[14].Trim().StartsWith("dst"))
                                {
                                    rec.CustomStr4 =
                                        SplitedLine(lineArr[14]).Split(':')[0];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr4: " + rec.CustomStr4);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("dst"))
                                        {
                                            rec.CustomStr4 = SplitedLine(lineArr[i]).Split(':')[SplitedLine(lineArr[i]).Split(':').Length - 1];
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

                        rec.CustomStr5 = Between(args.Message, "msg=", "n=");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr5: " + rec.CustomStr5);

                        try
                        {
                            if (lineArr.Length > 20)
                            {
                                if (lineArr[20].Trim().StartsWith("arg"))
                                {
                                    rec.CustomStr7 =
                                        SplitedLine(lineArr[20]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr7);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("arg"))
                                        {
                                            rec.CustomStr7 = SplitedLine(lineArr[i]);
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr7: " + rec.CustomStr3);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr7: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 19)
                            {
                                if (lineArr[19].Trim().StartsWith("dstname"))
                                {
                                    rec.CustomStr8 =
                                        SplitedLine(lineArr[19]);
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr7);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("dstname"))
                                        {
                                            rec.CustomStr8 = SplitedLine(lineArr[i]);
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr8: " + rec.CustomStr8);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr8: " + exception.Message);
                        }

                        try
                        {
                            if (lineArr.Length > 14)
                            {
                                if (lineArr[14].Trim().StartsWith("dst"))
                                {
                                    rec.CustomStr10 =
                                        SplitedLine(lineArr[14]).Split(':')[SplitedLine(lineArr[14]).Split(':').Length - 1];
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("dst"))
                                        {
                                            rec.CustomStr10 = SplitedLine(lineArr[i]).Split(':')[SplitedLine(lineArr[i]).Split(':').Length - 1];
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomStr10: " + rec.CustomStr10);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomStr10: " + exception.Message);
                        }


                        try
                        {
                            if (lineArr.Length > 18)
                            {
                                if (lineArr[18].Trim().StartsWith("result"))
                                {
                                    rec.CustomInt1 = Convert.ToInt32(SplitedLine(lineArr[18]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomStr10);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("result"))
                                        {
                                            rec.CustomInt1 = Convert.ToInt32(SplitedLine(lineArr[18]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt1: " + rec.CustomInt1);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt1: " + exception.Message);
                            rec.CustomInt1 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 17)
                            {
                                if (lineArr[17].Trim().StartsWith("rcvd"))
                                {
                                    rec.CustomInt2 = Convert.ToInt32(SplitedLine(lineArr[17]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("rcvd"))
                                        {
                                            rec.CustomInt2 = Convert.ToInt32(SplitedLine(lineArr[17]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt2: " + rec.CustomInt2);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt2: " + exception.Message);
                            rec.CustomInt2 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 13)
                            {
                                if (lineArr[13].Trim().StartsWith("src"))
                                {
                                    rec.CustomInt3 = Convert.ToInt32(SplitedLine(lineArr[13].Split(':')[1]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("src"))
                                        {
                                            rec.CustomInt3 = Convert.ToInt32(SplitedLine(lineArr[i].Split(':')[1]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt3: " + rec.CustomInt3);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt3: " + exception.Message);
                            rec.CustomInt3 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 14)
                            {
                                if (lineArr[14].Trim().StartsWith("dst"))
                                {
                                    rec.CustomInt4 = Convert.ToInt32(SplitedLine(lineArr[14].Split(':')[1]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("src"))
                                        {
                                            rec.CustomInt4 = Convert.ToInt32(SplitedLine(lineArr[i].Split(':')[1]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt4: " + rec.CustomInt4);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt4: " + exception.Message);
                            rec.CustomInt4 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 9)
                            {
                                if (lineArr[9].Trim().StartsWith("pri"))
                                {
                                    rec.CustomInt5 = Convert.ToInt32(SplitedLine(lineArr[9]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("pri"))
                                        {
                                            rec.CustomInt5 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt5: " + rec.CustomInt5);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt5: " + exception.Message);
                            rec.CustomInt5 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 10)
                            {
                                if (lineArr[10].Trim().StartsWith("c="))
                                {
                                    rec.CustomInt6 = Convert.ToInt32(SplitedLine(lineArr[10]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("c="))
                                        {
                                            rec.CustomInt6 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt6: " + rec.CustomInt6);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt6: " + exception.Message);
                            rec.CustomInt6 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 11)
                            {
                                if (lineArr[11].Trim().StartsWith("m="))
                                {
                                    rec.CustomInt7 = Convert.ToInt32(SplitedLine(lineArr[11]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("m="))
                                        {
                                            rec.CustomInt7 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt7: " + rec.CustomInt7);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt7: " + exception.Message);
                            rec.CustomInt7 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 12)
                            {
                                if (lineArr[12].Trim().StartsWith("n="))
                                {
                                    rec.CustomInt8 = Convert.ToInt32(SplitedLine(lineArr[12]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("n="))
                                        {
                                            rec.CustomInt8 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt8: " + rec.CustomInt8);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt8: " + exception.Message);
                            rec.CustomInt8 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 21)
                            {
                                if (lineArr[21].Trim().StartsWith("code"))
                                {
                                    rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[21]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("n="))
                                        {
                                            rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt9: " + exception.Message);
                            rec.CustomInt9 = 0;
                        }

                        try
                        {
                            if (lineArr.Length > 21)
                            {
                                if (lineArr[21].Trim().StartsWith("code"))
                                {
                                    rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[21]));
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                                }
                                else
                                {
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i].StartsWith("n="))
                                        {
                                            rec.CustomInt9 = Convert.ToInt32(SplitedLine(lineArr[i]));
                                            L.Log(LogType.FILE, LogLevel.DEBUG, "CustomInt9: " + rec.CustomInt9);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "CustomInt9: " + exception.Message);
                            rec.CustomInt9 = 0;
                        }
                    }

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
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\SonicWallV_TZ120_0_0UTMRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallV_TZ120_0_0UTMRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("SonicWallV_TZ120_0_0UTMRecorder").GetValue("Trace Level"));
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
                EventLog.WriteEntry("Security Manager Syslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
