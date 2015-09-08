using System;
using System.Collections.Generic;
using System.Text;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net;
using System.Net.Sockets;

namespace NetRecorder
{
    public class NetRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514,zone=0;
        private string err_log, protocol = "UDP", location = "", remote_host = "localhost";
        private CLogger L;
        public Syslog slog = null;
        private bool reg_flag = false;
        protected bool usingRegistry = true;
        private ProtocolType pro;
        protected Int32 Id = 0;
        protected String virtualhost,Dal;

        private void InitializeComponent()
        {
        }
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost, String dal,Int32 Zone)
        {
            usingRegistry = false;
            Id = Identity;
            location = Location;
            //last_position = LastPosition;
            //fromend = FromEndOnLoss;
            //max_record_send = MaxLineToWait;
            //timer_interval = SleepTime;
            //user = User;
            //password = Password;
            //remote_host = RemoteHost;
            trc_level = TraceLevel;
            virtualhost = Virtualhost;
            Dal = dal;
            remote_host = RemoteHost;
            zone = Zone;
        }
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NetRecorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on NetRecorder functions may not be running");
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


                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening Syslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());

                //slog = new Syslog(Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString(), Syslog_Port, pro);
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing NetRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NetRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\NetRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NetRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public NetRecorder()
        {
            
        }

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec r = new CustomBase.Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                /////////////////////////////////////////////////////////////////////////////////////////////////
                String line = args.Message;

                if (line != "")
                {

                    String[] arr = line.Split(' ');
                 

                    DateTime dt = DateTime.Now.AddMinutes(zone);
                    r.Datetime = dt.Year + "/" + dt.Month + "/" + dt.Day + " " + (dt.Hour+zone) + ":" + dt.Minute + ":" + dt.Second;

                    r.SourceName = arr[0];
                    r.EventCategory = arr[2];

                    String[] arrId = arr[4].Split('-');
                    if (arrId.Length < 3)
                        Console.WriteLine("Log this");

                    bool togo=true;
                    try
                    {
                        r.EventId = Convert.ToInt32(arrId[2].TrimEnd(':'));
                    }
                    catch
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Log this");

                        togo = false;
                    }
                    if (togo)
                    {
                        r.CustomStr10 = arr[3];

                        switch (r.EventId)
                        {
                            case 106007:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();

                                    String[] arrInIp = arr[9].Split('/');
                                    r.CustomStr1 = arrInIp[0];
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                    }
                                    catch
                                    {
                                    }

                                    String[] arrInTwo = arr[11].Split('/');
                                    r.CustomStr2 = arrInTwo[0];
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrInTwo[1]);
                                    }
                                    catch
                                    {
                                    }

                                    r.Description = arr[14] + " " + arr[15];
                                } break;
                            case 106011:
                                {
                                    for (Int32 i = 5; i < 9; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[11].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[13].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                } break;
                            case 106015:
                                {
                                    for (Int32 i = 5; i < 9; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();

                                    String[] arrInIp = arr[10].Split('/');
                                    r.CustomStr1 = arrInIp[0];
                                    try
                                    {
                                        r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                    }
                                    catch
                                    {
                                    }

                                    String[] arrInTwo = arr[12].Split('/');
                                    r.CustomStr2 = arrInTwo[0];
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrInTwo[1]);
                                    }
                                    catch
                                    {
                                    }

                                    if (arr.Length > 13)
                                        r.CustomStr3 = arr[14];
                                    if (arr.Length > 15)
                                        r.CustomStr4 = arr[15];
                                    if (arr.Length > 19)
                                        r.CustomStr8 = arr[19];
                                } break;
                            case 106017:
                                {
                                    for (Int32 i = 5; i < 11; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();

                                    r.CustomStr1 = arr[12];
                                    r.CustomStr2 = arr[14];
                                } break;
                            case 106023:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[8].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[10].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    r.CustomStr3 = arr[13].TrimStart('"').TrimEnd('"');
                                    r.CustomStr4 = arr[14].TrimStart('[').TrimEnd(',');
                                    r.CustomStr5 = arr[15].TrimEnd(']');
                                } break;
                            case 302013:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[11].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    arr[12] = arr[12].TrimStart('(').TrimEnd(')');
                                    String[] arrInTwo = arr[12].Split('/');
                                    r.CustomStr2 = arrInTwo[0];
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrInTwo[1]);
                                    }
                                    catch
                                    {
                                    }
                                    String[] arrInThree = arr[14].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInThree[0];
                                        String[] arrInIpThree = arrInThree[1].Split('/');
                                        r.CustomStr3 = arrInIpThree[0];
                                        try
                                        {
                                            r.CustomInt3 = Convert.ToInt32(arrInIpThree[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    arr[15] = arr[15].TrimStart('(').TrimEnd(')');
                                    String[] arrInFour = arr[15].Split('/');
                                    r.CustomStr4 = arrInFour[0];
                                    try
                                    {
                                        r.CustomInt4 = Convert.ToInt32(arrInFour[1]);
                                    }
                                    catch
                                    {
                                    }
                                } break;
                            case 302014:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[10].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[12].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    r.CustomStr3 = arr[14];
                                    r.CustomStr4 = arr[16];
                                    r.Description = arr[17] + " " + arr[18];
                                } break;
                            case 302015:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[11].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    arr[12] = arr[12].TrimStart('(').TrimEnd(')');
                                    String[] arrInTwo = arr[12].Split('/');
                                    r.CustomStr2 = arrInTwo[0];
                                    try
                                    {
                                        r.CustomInt2 = Convert.ToInt32(arrInTwo[1]);
                                    }
                                    catch
                                    {
                                    }
                                    String[] arrInThree = arr[14].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInThree[0];
                                        String[] arrInIpThree = arrInThree[1].Split('/');
                                        r.CustomStr3 = arrInIpThree[0];
                                        try
                                        {
                                            r.CustomInt3 = Convert.ToInt32(arrInIpThree[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    arr[15] = arr[15].TrimStart('(').TrimEnd(')');
                                    String[] arrInFour = arr[15].Split('/');
                                    r.CustomStr4 = arrInFour[0];
                                    try
                                    {
                                        r.CustomInt4 = Convert.ToInt32(arrInFour[1]);
                                    }
                                    catch
                                    {
                                    }
                                } break;
                            case 302016:
                                {
                                    for (Int32 i = 5; i < 8; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[10].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[12].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    r.CustomStr3 = arr[14];
                                    r.CustomStr4 = arr[16];
                                } break;
                            case 305011:
                                {
                                    for (Int32 i = 5; i < 9; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[10].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[12].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                } break;
                            case 305012:
                                {
                                    for (Int32 i = 5; i < 9; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[10].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[12].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    r.CustomStr3 = arr[14];
                                } break;
                            case 410001:
                                {
                                    for (Int32 i = 5; i < 9; i++)
                                        r.EventType += arr[i] + " ";

                                    r.EventType = r.EventType.Trim();
                                    String[] arrIn = arr[10].Split(':');
                                    if (arrIn.Length > 1)
                                    {
                                        r.CustomStr8 = arrIn[0];
                                        String[] arrInIp = arrIn[1].Split('/');
                                        r.CustomStr1 = arrInIp[0];
                                        try
                                        {
                                            r.CustomInt1 = Convert.ToInt32(arrInIp[1]);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    String[] arrInTwo = arr[12].Split(':');
                                    if (arrInTwo.Length > 1)
                                    {
                                        r.CustomStr9 = arrInTwo[0];
                                        String[] arrInIpTwo = arrInTwo[1].Split('/');
                                        r.CustomStr2 = arrInIpTwo[0];
                                        try
                                        {
                                            r.CustomInt2 = Convert.ToInt32(arrInIpTwo[1].TrimEnd(';'));
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    for (Int32 j = 13; j < arr.Length; j++)
                                    {
                                        r.Description += arr[j] + " ";
                                    }
                                    r.Description = r.Description.Trim();
                                } break;
                            default:
                                {
                                    L.Log(LogType.FILE, LogLevel.DEBUG, "Log this");
                                } break;
                        };


                    }
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");

                    L.Log(LogType.FILE, LogLevel.DEBUG, "Start sending Data");
                    if (usingRegistry)
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                        s.SetData(r);
                    }
                    else
                    {
                        CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                        s.SetData(Dal,virtualhost,r);
                        s.SetReg(Id, r.Datetime, "","", "",r.Datetime);
                    }
                    L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");
                }
            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
            }
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\NetRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("NetRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager NetRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager NetRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }


}
