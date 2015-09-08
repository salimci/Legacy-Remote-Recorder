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

namespace CiscoFWSMSyslogRecorder
{       
    public class CiscoFWSMSyslogRecorder : CustomBase
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
        private int logType = 0; //0 for Desc[6] %FWSM - 1 for Desc[5] %FWSM

        private void InitializeComponent()
        {
        }
        public override void SetConfigData(Int32 Identity, String Location, String LastLine, String LastPosition,
        String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
        String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
        String CustomVar1, int CustomVar2, String Virtualhost,String dal,Int32 Zone)
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoFWSMSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoFWSMSyslog Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CiscoFWSMSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing CiscoFWSMSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoFWSMSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }
        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoFWSMSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoFWSMSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }
        public CiscoFWSMSyslogRecorder()
		{
		}
        public String[] SpaceSplit(String line)
        {
            List<String> lst = new List<String>();
            StringBuilder sb = new StringBuilder();
            bool space = false;
            foreach (Char c in line.ToCharArray())
            {
                if (c != ' ')
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
        }
        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }
        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Line Is : "+ args.Message);

                rec.LogName = "CiscoPixFW Recorder";
                
                rec.EventType = args.EventLogEntType.ToString();
                //rec.Description = args.Message;

                String[] Desc = args.Message.Split(':');
                
                if (args.Message == "")
                {                    
                    return;
                }

                if (Desc.Length < 6)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 6: " + args.Message);
                    return;
                }
                
                for (Int32 i = 0; i < Desc.Length; ++i)
                {
                    Desc[i] = Desc[i].Trim();
                }
                
                if (logType == 0)
                {   
                    Desc[5] = Desc[5].TrimStart('%');
                    String[] pixArr = Desc[5].Split('-');

                    if (pixArr.Length < 3)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error for log format --> Event id not like this format %FWSM-6-302014");
                        return;
                    }

                    String[] dateArr = SpaceSplit(Desc[2]);
                    if (dateArr.Length < 4)
                    {
                        L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 4: " + args.Message);
                        return;
                    }

                    try
                    {
                        string[] tempdate;
                        tempdate = Desc[2].Split(' ');
                        string date = "";
                        date = tempdate[2] + "/" + tempdate[1] + "/" + tempdate[3] + " " + tempdate[4] + ":" + Desc[3] + ":" + Desc[4].Split(' ')[0];
                        DateTime dt = DateTime.Parse(date.ToString());
                        rec.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss");  
                    }
                    catch (Exception ex)
                    {
                       L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                       L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                    }

                    rec.EventId = Convert.ToInt64(pixArr[2]);
                    
                    bool errorControl = false;

                    switch (pixArr[2])
                    {           
                        #region 111008
                        case "111008":
                            {
                                try
                                {
                                    rec.UserName = SpaceSplit(Desc[6])[1].Trim('\'');
                                    rec.EventType = "Command Execution";
                                    for (int i = 4; i < SpaceSplit(Desc[6]).Length - 1; i++)
                                        rec.CustomStr1 += " " + SpaceSplit(Desc[6])[i];
                                    rec.CustomStr1.Trim(' ').Trim('\'');
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for 111008  ");
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            }
                            break; 
                        #endregion
                        #region 106023
                        case "106023":
                            {
                                try
                                {
                                    rec.CustomStr2 = Desc[6];
                                    String[] arrDeny = Desc[7].Split(' ');
                                    String[] arrDenyIp = arrDeny[0].Split('/');

                                    rec.CustomStr3 = arrDenyIp[0];
                                    if (arrDenyIp.Length > 1)
                                        rec.CustomInt1 = Convert.ToInt32(arrDenyIp[1]);

                                    String[] arrDenyDesc = Desc[8].Split(' ');
                                    String[] arrDenyDescIp = arrDenyDesc[0].Split('/');

                                    StringBuilder sbTempDeny = new StringBuilder();
                                    sbTempDeny.Append(rec.CustomStr2).Append(" ");
                                    for (Int32 i = 1; i < arrDeny.Length; i++)
                                        sbTempDeny.Append(arrDeny[i]).Append(" ");
                                    rec.CustomStr2 = sbTempDeny.ToString().Trim();
                                    rec.CustomStr4 = arrDenyDescIp[0];
                                    if (arrDenyDescIp.Length > 1)
                                        rec.CustomInt2 = Convert.ToInt32(arrDenyDescIp[1]);

                                    StringBuilder sbTempDescDeny = new StringBuilder();
                                    sbTempDescDeny.Append(rec.CustomStr2).Append(" ");
                                    for (Int32 i = 1; i < arrDenyDesc.Length; ++i)
                                    {
                                        sbTempDescDeny.Append(arrDenyDesc[i]).Append(" ");
                                    }
                                    sbTempDescDeny.Remove(sbTempDescDeny.Length - 1, 1);
                                    rec.CustomStr2 = sbTempDescDeny.ToString();
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for 106023  ");
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                                break;
                            } 
                        #endregion
                        #region 302014 & 302016
                        case "302014":
                        case "302016":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(':');
                                    String[] x1 = null;
                                    String[] x2 = null;
                                    String[] x3 = null;
                                    String[] x5 = null;

                                    if (arrInbound.Length > 4)
                                    {
                                        x1 = arrInbound[0].Trim().Split(' ');
                                        x2 = arrInbound[1].Trim().Split(' ');
                                        x3 = arrInbound[2].Trim().Split(' ');
                                        x5 = arrInbound[4].Trim().Split(' ');
                                    }
                                    else if (arrInbound.Length == 1 && Desc.Length > 10)
                                    {
                                        x1 = Desc[6].Split(' ');
                                        x2 = Desc[7].Split(' ');
                                        x3 = Desc[8].Split(' ');
                                        x5 = Desc[10].Split(' ');
                                    }
                                    else
                                    {

                                    }

                                    if (x1.Length > 4)
                                    {
                                        StringBuilder desc = new StringBuilder();
                                        desc.Append(x1[0]);
                                        desc.Append(' ');
                                        desc.Append(x1[1]);
                                        desc.Append(' ');
                                        desc.Append(x1[2]);
                                        desc.Append(' ');
                                        if (x5.Length > 4)
                                        {
                                            desc.Append(x5[3]);
                                            desc.Append(' ');
                                            desc.Append(x5[4]);
                                        }
                                        rec.CustomStr2 = desc.ToString();

                                        if (x1.Length > 5)
                                        {
                                            rec.CustomStr7 = x1[5].Trim();
                                            rec.CustomStr10 = (x1[3].Trim());
                                        }
                                        else
                                            rec.CustomStr7 = x1[4].Trim();
                                    }

                                    if (x2.Length > 2)
                                    {
                                        String[] part2 = x2[0].Split('/');
                                        StringBuilder dest = new StringBuilder();
                                        for (int k = 1; k < x2.Length; k++)
                                        {
                                            dest.Append(x2[k].Trim()).Append(' ');
                                        }

                                        rec.CustomStr7 += ' ' + dest.ToString();
                                        rec.CustomStr3 = part2[0];
                                        if (part2.Length > 1)
                                            rec.CustomInt1 = Convert.ToInt32(part2[1].Trim());
                                    }
                                    if (x3.Length > 2 && x5.Length > 2)
                                    {
                                        String[] part3 = x3[0].Split('/');
                                        rec.CustomStr4 = part3[0];
                                        rec.CustomInt2 = Convert.ToInt32(part3[1].Trim());
                                        StringBuilder duration = new StringBuilder();
                                        duration.Append(x3[2]);
                                        duration.Append(':');
                                        duration.Append(Desc[9]);
                                        duration.Append(':');
                                        duration.Append(x5[0]);
                                        rec.CustomStr8 = duration.ToString();

                                        for (int k = 0; k < x5.Length; k++)
                                        {
                                            if (x5[k].ToString() == "bytes")
                                            {
                                                rec.CustomInt7 = Convert.ToInt32(x5[k + 1]);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error  for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            }
                            break; 
                        #endregion
                        #region 305011 & 305012
                        case "305011":
                        case "305012":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(':');
                                    String[] x1 = null;
                                    String[] x2 = null;
                                    String[] x3 = null;


                                    x1 = Desc[6].Split(' ');
                                    x2 = Desc[7].Split(' ');
                                    x3 = Desc[8].Split(' ');

                                    if (x1.Length > 4)
                                    {
                                        StringBuilder desc = new StringBuilder();
                                        desc.Append(x1[0]);
                                        desc.Append(' ');
                                        desc.Append(x1[1]);
                                        desc.Append(' ');
                                        desc.Append(x1[2]);
                                        desc.Append(' ');
                                        desc.Append(x1[3]);

                                        rec.CustomStr2 = desc.ToString();

                                        if (x1.Length >= 5)
                                        {
                                            rec.CustomStr7 = x1[5].Trim();
                                        }
                                    }

                                    if (x2.Length > 2)
                                    {
                                        String[] part2 = x2[0].Split('/');
                                        StringBuilder dest = new StringBuilder();
                                        for (int k = 1; k < x2.Length; k++)
                                        {
                                            dest.Append(x2[k].Trim()).Append(' ');
                                        }

                                        rec.CustomStr7 += ' ' + dest.ToString();
                                        rec.CustomStr7 = rec.CustomStr7.Trim();
                                        rec.CustomStr3 = part2[0];
                                        if (part2.Length > 1)
                                            rec.CustomInt1 = Convert.ToInt32(part2[1].Trim());
                                    }
                                    if (x3.Length >= 1)
                                    {
                                        //NAT ADRESS
                                        String[] part3 = x3[0].Split('/');
                                        rec.CustomStr5 = part3[0];
                                        rec.CustomInt3 = Convert.ToInt32(part3[1].Trim());

                                        if (x3.Length > 1)
                                        {
                                            StringBuilder duration = new StringBuilder();
                                            duration.Append(x3[2]);
                                            duration.Append(':');
                                            duration.Append(Desc[9]);
                                            duration.Append(':');
                                            duration.Append(Desc[10]);
                                            rec.CustomStr8 = duration.ToString();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);   
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break; 
                        #endregion
                        #region 302015 & 302013
                        case "302015"://Tested
                        case "302013":
                            {
                                String[] arrInbound = Desc[6].Split(':');
                                String[] x1 = null;
                                String[] x2 = null;
                                String[] x3 = null;
                                if (arrInbound.Length > 2)
                                {
                                    x1 = arrInbound[0].Trim().Split(' ');
                                    x2 = arrInbound[1].Trim().Split(' ');
                                    x3 = arrInbound[2].Trim().Split(' ');
                                }
                                else if (arrInbound.Length == 1 && Desc.Length > 7)
                                {
                                    x1 = Desc[6].Split(' ');
                                    x2 = Desc[7].Split(' ');
                                    x3 = Desc[8].Split(' ');
                                }
                                else
                                {
                                    //L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302013:" + args.Message);
                                }

                                try
                                {
                                    if (x1.Length > 6)
                                    {
                                        StringBuilder desc = new StringBuilder();
                                        desc.Append(x1[0]);
                                        desc.Append(' ');
                                        desc.Append(x1[1]);
                                        desc.Append(' ');
                                        desc.Append(x1[2]);
                                        desc.Append(' ');
                                        desc.Append(x1[3]);
                                        rec.CustomStr2 = desc.ToString();
                                        rec.CustomStr10 = (x1[4].Trim());
                                        rec.CustomStr7 = x1[6];
                                    }

                                    if (x2.Length > 2)
                                    {
                                        StringBuilder dest = new StringBuilder();
                                        for (int k = 2; k < x2.Length; k++)
                                        {
                                            dest.Append(x2[k].Trim()).Append(' ');
                                        }
                                        rec.CustomStr7 += ' ' + dest.ToString();
                                        rec.CustomStr7 = rec.CustomStr7.Trim();


                                        String[] part3 = x2[0].Split('/');
                                        String[] part3dest = x2[1].Trim('(', ')').Split('/');
                                        rec.CustomStr3 = part3[0].Trim();
                                        if (part3.Length > 1)
                                            rec.CustomInt1 = Convert.ToInt32(part3[1].Trim());
                                        rec.CustomStr5 = part3dest[0].Trim();
                                        rec.CustomInt3 = Convert.ToInt32(part3dest[1].Trim());
                                    }

                                    if (x3.Length > 1)
                                    {
                                        String[] part2 = x3[0].Split('/');
                                        String[] part2dest = x3[1].Trim('(', ')').Split('/');

                                        rec.CustomStr4 = part2[0].Trim();
                                        if (part2.Length > 1)
                                            rec.CustomInt2 = Convert.ToInt32(part2[1].Trim());
                                        rec.CustomStr6 = part2dest[0].Trim();
                                        rec.CustomInt4 = Convert.ToInt32(part2dest[1].Trim());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            }
                            break; 
                        #endregion
                        #region 109001
                        case "109001":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(':');
                                    String[] x1 = null;
                                    x1 = Desc[6].Split(' ');


                                    if (x1.Length > 4)
                                    {
                                        StringBuilder desc = new StringBuilder();
                                        desc.Append(x1[0]);
                                        desc.Append(' ');
                                        desc.Append(x1[1]);
                                        desc.Append(' ');
                                        desc.Append(x1[2]);
                                        desc.Append(' ');
                                        desc.Append(x1[3]);

                                        rec.CustomStr2 = desc.ToString();

                                        rec.UserName = x1[4].Trim();

                                        int indexSource = 0;

                                        for (int i = 0; i < x1.Length; i++)
                                        {
                                            if (x1[i].Trim() == "from")
                                            {
                                                indexSource = i;
                                                break;
                                            }
                                        }

                                        String[] partsource = x1[indexSource + 1].Split('/');
                                        rec.CustomStr3 = partsource[0];
                                        rec.CustomInt1 = Convert.ToInt32(partsource[1].Trim());

                                        int indexDestination = 0;

                                        for (int j = 0; j < x1.Length; j++)
                                        {
                                            if (x1[j].Trim() == "to")
                                            {
                                                indexDestination = j;
                                                break;
                                            }
                                        }

                                        String[] partdestination = x1[indexDestination + 1].Split('/');
                                        rec.CustomStr4 = partdestination[0];
                                        rec.CustomInt2 = Convert.ToInt32(partdestination[1].Trim());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break;
                        #endregion
                        #region 106021
                        case "106021":
                            {
                                try
                                {
                                    String[] arrDeny = Desc[6].Split(' ');
                                    rec.CustomStr3 = arrDeny[6];
                                    rec.CustomStr4 = arrDeny[8];
                                    rec.CustomStr2 = arrDeny[0] + " " + arrDeny[1] + " " + arrDeny[2] + " " + arrDeny[3];
                                    rec.CustomStr7 = arrDeny[11].Trim();
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            }
                            break; 
                        #endregion
                        #region 111001 & 111002 & 111003 & 111004 & 111005 & 111006 & 111007
                        case "111001":
                        case "111002":
                        case "111003":
                        case "111004":
                        case "111005":
                        case "111006":
                        case "111007":
                            {
                                try
                                {
                                    rec.EventType = "Admin Action";

                                    string message = "";
                                    for (int i = 6; i < Desc.Length; i++)
                                    {
                                        message += " " + Desc[i];
                                    }
                                    rec.CustomStr7 = message.Trim();
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            }
                            break; 
                        #endregion
                        #region 106015 & 106028 & 302020 & 302021
                        case "106015":
                        case "106028": //yeni eklendi
                        case "302020":
                        case "302021":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(' ');

                                    Int32 firstIp = 0;
                                    Int32 secondIp = 0;
                                    bool first = true;

                                    for (Int32 i = 0; i < arrInbound.Length; i++)
                                    {
                                        if (arrInbound[i].Contains("/"))
                                        {
                                            if (first)
                                            {
                                                firstIp = i;
                                                first = false;
                                            }
                                            else
                                            {
                                                secondIp = i;
                                                break;
                                            }
                                        }
                                    }

                                    StringBuilder customStr7 = new StringBuilder();

                                    rec.CustomStr2 = arrInbound[0] + " " + arrInbound[1];

                                    for (Int32 i = 2; i < firstIp - 1; i++)
                                    {
                                        customStr7.Append(arrInbound[i]).Append(" ");
                                    }

                                    rec.CustomStr7 = customStr7.ToString().Trim();

                                    String[] arrInboundIp = arrInbound[firstIp].Split('/');


                                    if (arrInboundIp[0].Contains("-"))
                                    {
                                        rec.CustomStr3 = arrInboundIp[0].Split('-')[1].Trim();
                                    }
                                    else
                                    {
                                        rec.CustomStr3 = arrInboundIp[0];
                                    }

                                    rec.CustomInt1 = Convert.ToInt32(arrInboundIp[1]);

                                    StringBuilder customStr6 = new StringBuilder();
                                    for (Int32 i = secondIp + 1; i < arrInbound.Length; i++)
                                    {
                                        customStr6.Append(arrInbound[i]).Append(" ");
                                    }

                                    rec.CustomStr7 += " " + customStr6.ToString().Trim();

                                    String[] arrInboundDescIp = arrInbound[secondIp].Split('/');
                                    rec.CustomStr4 = arrInboundDescIp[0];
                                    rec.CustomInt2 = Convert.ToInt32(arrInboundDescIp[1]);
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break; 
                        #endregion
                        #region 313004
                        case "313004":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(' ');
                                    String[] x1 = null;
                                    x1 = Desc[6].Split(' ');

                                    if (x1.Length > 4)
                                    {
                                        rec.CustomStr2 = x1[0] + " " + x1[1];

                                        rec.CustomStr6 = x1[2].Trim().Split('=')[1].Trim(',');

                                        int indexfrom = 0;

                                        for (int i = 3; i < x1.Length; i++)
                                        {
                                            if (x1[i].Trim() == "from")
                                            {
                                                indexfrom = i;
                                                break;
                                            }
                                        }

                                        int indexto = 0;

                                        for (int j = 0; j < x1.Length; j++)
                                        {
                                            if (x1[j].Trim() == "to")
                                            {
                                                indexto = j;
                                                break;
                                            }
                                        }

                                        int indexon = 0;

                                        for (int k = 0; k < x1.Length; k++)
                                        {
                                            if (x1[k].Trim().Contains("on"))
                                            {
                                                if (x1[k].Trim() == "on" && x1[k + 1] == "interface")
                                                {
                                                    indexon = k;
                                                }
                                                if (x1[k].Trim() == "oninterface")
                                                {
                                                    indexon = k;
                                                }
                                            }
                                        }

                                        string sourceip = "";
                                        for (int g = indexfrom + 1; g < indexon; g++)
                                        {
                                            sourceip += " " + x1[g];
                                        }

                                        sourceip = sourceip.Trim();

                                        if (sourceip.Contains(" "))
                                        {
                                            string[] tempsourceip = sourceip.Split(' ');
                                            if (tempsourceip.Length > 1)
                                            {
                                                sourceip = tempsourceip[tempsourceip.Length - 1];
                                            }
                                        }

                                        rec.CustomStr3 = sourceip;

                                        string interfacename = "";

                                        for (int y = indexon + 1; y < indexto; y++)
                                        {
                                            if (x1[y].Trim() == "interface")
                                            {
                                                continue;
                                            }
                                            interfacename += " " + x1[y];
                                        }

                                        interfacename = interfacename.Trim();
                                        rec.CustomStr7 = interfacename;

                                        string destinationip = x1[indexto + 1].Trim();
                                        rec.CustomStr4 = destinationip;

                                        rec.CustomStr5 = Desc[7];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break;
                        #endregion
                        #region 710003
                        case "710003":
                            {
                                try
                                {
                                    String[] x1 = null;
                                    x1 = Desc[6].Split(' ');

                                    rec.CustomStr2 = x1[0] + " " + x1[1] + " " + x1[2];

                                    int indexfrom = 0;

                                    for (int i = 3; i < x1.Length; i++)
                                    {
                                        if (x1[i].Trim() == "from")
                                        {
                                            indexfrom = i;
                                            break;
                                        }
                                    }

                                    string sourceip = x1[indexfrom + 1];
                                    string sourceport = "0";

                                    if (sourceip.Contains("/"))
                                    {
                                        string[] source = sourceip.Split('/');
                                        sourceip = source[0];
                                        sourceport = source[1];
                                    }

                                    rec.CustomStr3 = sourceip.Trim();
                                    rec.CustomInt1 = Convert.ToInt32(sourceport);

                                    String[] arrDeny = Desc[7].Split(' ');
                                    String[] arrDenyIp = arrDeny[0].Split('/');

                                    rec.CustomStr4 = arrDenyIp[0];
                                    if (arrDenyIp.Length > 1)
                                        rec.CustomInt2 = Convert.ToInt32(arrDenyIp[1]);
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break; 
                        #endregion
                        #region 405001
                        case "405001":
                            {
                                try
                                {
                                    String[] arrInbound = Desc[6].Split(' ');
                                    String[] x1 = null;
                                    x1 = Desc[6].Split(' ');

                                    if (x1.Length > 4)
                                    {
                                        int indexfrom = 0;

                                        for (int i = 0; i < x1.Length; i++)
                                        {
                                            if (x1[i].Trim() == "from")
                                            {
                                                indexfrom = i;
                                                break;
                                            }
                                        }

                                        string customstr2 = "";
                                        for (int g = 0; g < indexfrom; g++)
                                        {
                                            customstr2 += ' ' + x1[g];
                                        }
                                        customstr2 = customstr2.Trim();
                                        rec.CustomStr2 = customstr2;

                                        string sourceipandmacaddress = "";
                                        string sourceip = "";
                                        string sourcemac = "";

                                        sourceipandmacaddress = x1[indexfrom + 1];

                                        if (sourceipandmacaddress.Contains("/"))
                                        {
                                            sourceip = sourceipandmacaddress.Split('/')[0].Trim();
                                            sourcemac = sourceipandmacaddress.Split('/')[1].Trim();
                                        }

                                        rec.CustomStr3 = sourceip;
                                        rec.CustomStr8 = sourcemac;

                                        int indexon = 0;

                                        for (int k = 0; k < x1.Length; k++)
                                        {
                                            if (x1[k].Trim().Contains("on"))
                                            {
                                                if (x1[k].Trim() == "on" && x1[k + 1] == "interface")
                                                {
                                                    indexon = k + 1;
                                                    break;
                                                }
                                                if (x1[k].Trim() == "oninterface")
                                                {
                                                    indexon = k;
                                                    break;
                                                }
                                            }
                                        }

                                        string interfacename = "";

                                        interfacename = x1[indexon + 1].Trim();
                                        rec.CustomStr7 = interfacename;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error for " + pixArr[2].ToString());
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.Message);
                                    L.Log(LogType.FILE, LogLevel.ERROR, ex.StackTrace);
                                    errorControl = true;
                                }
                            } break;
                        #endregion
                        default:
                            L.Log(LogType.FILE, LogLevel.DEBUG, "No match for the mesage: " + args.Message);
                            rec.Description = args.Message;
                            break;
                    }

                    if (errorControl) 
                    {
                        rec.Description = args.Message.ToString();
                    }
                }
                else if (logType == 0)
                {
                 
                }
                
                rec.SourceName = Desc[0];
                

               

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
                L.Log(LogType.FILE, LogLevel.ERROR, args.Message);
            }
        }
        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoFWSMSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoFWSMSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoFWSMSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoFWSMSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CiscoFWSMSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }   
}       
