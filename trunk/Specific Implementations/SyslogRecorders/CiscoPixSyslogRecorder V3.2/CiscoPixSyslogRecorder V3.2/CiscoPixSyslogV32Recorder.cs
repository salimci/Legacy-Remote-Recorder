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

namespace CiscoPixSyslogV32Recorder
{
    public class CiscoPixSyslogV32Recorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoPixSyslog Recorder functions may not be running");
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on CiscoPixSyslog Recorder functions may not be running");
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
                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening CiscoPixSyslogs on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing CiscoPixSyslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        public bool Get_logDir()
        {
            RegistryKey rk = null;
            DateTime dt = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\CiscoPixSyslogRecorder" + Id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder Read Registry", er.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public CiscoPixSyslogV32Recorder()
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

        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                rec.LogName = "CiscoPixSyslog Recorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                rec.Description = args.Message;

                String[] Desc = args.Message.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Message is null " + args.Message);
                    return;
                }
                if (Desc.Length < 6)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for lenght is small than 6 : " + args.Message);
                    goto Set;
                }

                for (Int32 i = 0; i < Desc.Length; ++i)
                {
                    Desc[i] = Desc[i].Trim();
                }

                /*
                 2010-08-11 08:33:40	Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-302013: Built inbound TCP connection 146454459412553183 for DMZ:172.16.0.20/1812 (172.16.0.20/1812) to inside:10.0.32.72/88 (10.0.32.72/88)
                 2010-08-11 08:33:40	Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-302014: Teardown TCP connection 146454459412553183 for DMZ:172.16.0.20/1812 to inside:10.0.32.72/88 duration 0:00:00 bytes 1076 TCP Reset-I
                 2010-08-11 08:33:40    Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-106015: Deny TCP (no connection) from 172.16.0.76/139 to 169.254.89.231/1879 flags SYN ACK  on interface DMZ
                 10.0.100.2:514 : local4.info Aug 02 2010 10:36:14: %FWSM-6-305011: Built dynamic tcp translation from inside:10.5.33.2/44207 to dmz_internet:88.255.237.133/38560
                 2010-08-11 08:32:36	Local4.Info	10.0.100.2	Aug 11 2009 11:26:58: %FWSM-6-302015: Built outbound UDP connection 146454463707492619 for DMZ:172.16.0.2/55574 (88.255.237.16/55574) to dmz_internet:204.74.67.132/53 (204.74.67.132/53)
                 2010-08-11 08:33:41	Local4.Warning	10.0.100.2	Aug 11 2009 11:28:03: %FWSM-4-106023: Deny tcp src dmz_internet:78.189.55.34/12551 dst inside:88.255.237.80/135 by access-group "dmz_internet_access_in" [0x0, 0x0]
                 2010-08-11 08:33:41	Local4.Critical	10.0.100.2	Aug 11 2009 11:28:03: %FWSM-2-106007: Deny inbound UDP from 66.185.162.248/53 to 88.255.237.242/64967 due to DNS Response
                 */

                //Real Log Format
                //10.0.100.2:514 : local4.info Aug 12 2009 14:57:58: %FWSM-6-302013: Built inbound TCP connection 146454459414565862 for DMZ:172.16.0.20/4341 (172.16.0.20/4341) to inside:10.0.32.72/88 (10.0.32.72/88)

                String[] typeArr = Desc[7].Split('-');

                if (typeArr.Length < 2)
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Error parsing message for incorrect type : " + args.Message);
                    L.Log(LogType.FILE, LogLevel.INFORM, "All information is sent to Desctiption");
                    goto Set;
                }

                //Common fields for all pix records

                rec.CustomInt1 = Convert.ToInt32(typeArr[1]);
                rec.EventId = Convert.ToInt64(typeArr[2].TrimEnd(':'));

                //Uncommon fields for all pix records
                //Now Parse According to EventId, start Desc[9] 
                switch (typeArr[2].TrimEnd(':'))
                {
                    #region 302013
                    //2010-08-11 08:33:40	Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-302013: Built inbound TCP connection 146454459412553183 for DMZ:172.16.0.20/1812 (172.16.0.20/1812) to inside:10.0.32.72/88 (10.0.32.72/88)
                    case "302013":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 11; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr8 = Desc[12];

                                rec.CustomStr3 = Desc[14].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt1 = Convert.ToInt32(Desc[14].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr4 = Desc[17].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt2 = Convert.ToInt32(Desc[17].Split(new char[] { ':', '/' })[2]);

                                rec.CustomStr5 = Desc[15].TrimStart('(').TrimEnd(')');
                                rec.CustomStr6 = Desc[18].TrimStart('(').TrimEnd(')');
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '302013'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 302014
                    //2010-08-11 08:33:40 Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-302014: Teardown TCP connection 146454459412553183 for DMZ:172.16.0.20/1812 to inside:10.0.32.72/88 duration 0:00:00 bytes 1076 TCP Reset-I
                    case "302014":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 10; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr8 = Desc[11];

                                rec.CustomStr3 = Desc[13].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt1 = Convert.ToInt32(Desc[13].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr4 = Desc[15].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt2 = Convert.ToInt32(Desc[15].Split(new char[] { ':', '/' })[2]);

                                rec.CustomInt3 = Convert.ToInt32(Desc[19]); //byte
                                rec.CustomStr9 = "";
                                for (int j = 20; j < Desc.Length; j++)
                                {
                                    rec.CustomStr9 += Desc[j] + " ";
                                }
                                rec.CustomStr9.Trim();
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '302014'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 302015
                    //2010-08-11 08:32:36	Local4.Info	10.0.100.2	Aug 11 2009 11:26:58: %FWSM-6-302015: Built outbound UDP connection 146454463707492619 for DMZ:172.16.0.2/55574 (88.255.237.16/55574) to dmz_internet:204.74.67.132/53 (204.74.67.132/53)
                    case "302015":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 11; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr8 = Desc[12];

                                rec.CustomStr3 = Desc[14].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt1 = Convert.ToInt32(Desc[14].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr7 = Desc[17].Split(new char[] { ':', '/' })[0];
                                rec.CustomStr4 = Desc[17].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt2 = Convert.ToInt32(Desc[17].Split(new char[] { ':', '/' })[2]);

                                rec.CustomStr5 = Desc[15].TrimStart('(').TrimEnd(')');
                                rec.CustomStr6 = Desc[18].TrimStart('(').TrimEnd(')');
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '302015'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 302016
                    // 2010-08-11 08:33:40	Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-302016: Teardown UDP connection 146454459412547723 for DMZ:172.16.0.20/4102 to inside:10.0.32.72/88 duration 0:02:02 bytes 512
                    case "302016":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 10; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr8 = Desc[11];

                                rec.CustomStr3 = Desc[13].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt1 = Convert.ToInt32(Desc[13].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr4 = Desc[15].Split(new char[] { ':', '/' })[1];
                                rec.CustomInt2 = Convert.ToInt32(Desc[15].Split(new char[] { ':', '/' })[2]);

                                rec.CustomInt3 = Convert.ToInt32(Desc[19]); //byte
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '302016'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 106015
                    //  2010-08-11 08:33:40	Local4.Info	10.0.100.2	Aug 11 2009 11:28:02: %FWSM-6-106015: Deny TCP (no connection) from 172.16.0.76/139 to 169.254.89.231/1884 flags SYN ACK  on interface DMZ
                    case "106015":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 11; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[13].Split(new char[] { '/' })[0];
                                rec.CustomInt1 = Convert.ToInt32(Desc[13].Split(new char[] { '/' })[1]);
                                rec.CustomStr4 = Desc[15].Split(new char[] { '/' })[0];
                                rec.CustomInt2 = Convert.ToInt32(Desc[15].Split(new char[] { '/' })[1]);

                                int cnt = 0;
                                rec.CustomStr7 = " "; //flags
                                for (int j = 17; j < Desc.Length; j++)
                                {
                                    if (Desc[j] != "on")
                                    {
                                        rec.CustomStr7 += " " + Desc[j];
                                    }
                                    else
                                    {
                                        cnt = j + 2;
                                        break;
                                    }
                                }
                                rec.CustomStr7.Trim();

                                for (int k = cnt; k < Desc.Length; k++)
                                {
                                    rec.CustomStr7 += " " + Desc[k];
                                }
                                rec.CustomStr7.Trim();
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '106015'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 106023
                    //2010-08-11 08:33:41	Local4.Warning	10.0.100.2	Aug 11 2009 11:28:03: %FWSM-4-106023: Deny tcp src dmz_internet:78.189.55.34/12551 dst inside:88.255.237.80/135 by access-group "dmz_internet_access_in" [0x0, 0x0]
                    case "106023":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 10; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[11].Split(new char[] { ':', '/' })[1];
                                if (Desc[11].Split(new char[] { ':', '/' }).Length > 2)
                                    rec.CustomInt1 = Convert.ToInt32(Desc[11].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr4 = Desc[13].Split(new char[] { ':', '/' })[1];
                                if (Desc[13].Split(new char[] { ':', '/' }).Length > 2)
                                    rec.CustomInt2 = Convert.ToInt32(Desc[13].Split(new char[] { ':', '/' })[2]);

                                rec.CustomStr7 = Desc[16].TrimStart('"').TrimEnd('"');
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '106023'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 106007
                    //2010-08-11 08:33:41	Local4.Critical	10.0.100.2	Aug 11 2009 11:28:03: %FWSM-2-106007: Deny inbound UDP from 66.185.162.248/53 to 88.255.237.242/64967 due to DNS Response
                    case "106007":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 10; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[12].Split(new char[] { '/' })[0];
                                rec.CustomInt1 = Convert.ToInt32(Desc[12].Split(new char[] { '/' })[1]);
                                rec.CustomStr4 = Desc[14].Split(new char[] { '/' })[0];
                                rec.CustomInt2 = Convert.ToInt32(Desc[14].Split(new char[] { '/' })[1]);

                                rec.CustomStr7 = " ";
                                for (int j = 17; j < Desc.Length; j++)
                                {
                                    rec.CustomStr7 += " " + Desc[j];
                                }
                                rec.CustomStr7.Trim();
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '106007'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 106010
                    //10.0.100.2:514 : local4.error Aug 16 2009 11:17:17: %FWSM-3-106010: Deny inbound udp src DMZ:169.254.112.94/57767 dst inside:10.0.32.71/53

                    case "106010":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 11; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];
                                if (Desc[12].Split(new char[] { ':', '/' }).Length > 2)
                                    rec.CustomInt1 = Convert.ToInt32(Desc[12].Split(new char[] { ':', '/' })[2]);
                                rec.CustomStr4 = Desc[14].Split(new char[] { ':', '/' })[1];
                                if (Desc[14].Split(new char[] { ':', '/' }).Length > 2)
                                    rec.CustomInt2 = Convert.ToInt32(Desc[14].Split(new char[] { ':', '/' })[2]);
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '106010'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 106025
                    //10.0.100.2:514 : local4.info Aug 16 2009 11:17:14: %FWSM-6-106025: Failed to determine security context for packet: vlan10 tcp src 88.179.144.34/3724 dest 88.255.237.162/135

                    case "106025":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];

                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[18].Split(new char[] { ':', '/' })[0];
                                rec.CustomInt1 = Convert.ToInt32(Desc[18].Split(new char[] { ':', '/' })[1]);
                                rec.CustomStr4 = Desc[20].Split(new char[] { ':', '/' })[0];
                                rec.CustomInt2 = Convert.ToInt32(Desc[20].Split(new char[] { ':', '/' })[1]);
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '106025'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 305009
                    //10.0.100.2:514 : local4.info Aug 16 2009 11:16:46: %FWSM-6-305009: Built dynamic translation from inside:10.1.17.21 to DMZ(inside_nat0_outbound):10.1.17.21

                    case "305009":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }

                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];

                                rec.CustomStr4 = Desc[14].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr7 = Desc[14].Split(new char[] { ':', '/' })[0];
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '305009'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 305010
                    //local4.info Aug 02 2010 13:14:22: %FWSM-6-305010: Teardown dynamic translation from inside:10.1.5.35 to DMZ:10.1.5.35 duration 3:00:00

                    case "305010":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }

                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];

                                rec.CustomStr4 = Desc[14].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr7 = Desc[14].Split(new char[] { ':', '/' })[0];
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '305010'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 305011
                    //10.0.100.2:514 : local4.info Aug 02 2010 13:14:22: %FWSM-6-305011: Built dynamic tcp translation from inside:10.5.33.2/26849 to dmz_internet:88.255.237.133/2378

                    case "305011":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }

                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                //rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr3 = Desc[12];

                                rec.CustomStr4 = Desc[15].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr7 = Desc[15].Split(new char[] { ':', '/' })[0];
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '305011'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 305012
                    //10.0.100.2:514 : local4.info Aug 02 2010 13:14:22: %FWSM-6-305012: Teardown dynamic tcp translation from inside:10.4.101.96/1904 to DMZ:172.16.0.1/20119 duration 0:00:30

                    case "305012":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }

                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                //rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr3 = Desc[12];

                                rec.CustomStr4 = Desc[15].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr7 = Desc[15].Split(new char[] { ':', '/' })[0];
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '305012'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 410001
                    //10.0.100.2:514 : local4.warning Aug 02 2010 13:14:29: %FWSM-4-410001: Dropped UDP DNS reply from DMZ:172.16.0.4/53 to dmz_internet:204.74.106.103/61982; packet length 687 bytes exceeds configured limit of 512 bytes

                    case "410001":
                        {
                            try
                            {
                                rec.EventCategory = Desc[8];
                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= Desc.Length; i++)
                                {
                                    if (Desc[i].Contains(":"))
                                    {
                                        break;
                                    }

                                    rec.CustomStr10 += " " + Desc[i];
                                }
                                rec.CustomStr10.Trim();

                                //rec.CustomStr3 = Desc[12].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr3 = Desc[12];
                                rec.CustomStr4 = Desc[13].Split(new char[] { ':', '/' })[1];
                                rec.CustomStr7 = Desc[13].Split(new char[] { ':', '/' })[0];
                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '410001'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region 110001
                    //10.0.100.2:514 : local4.info Aug 16 2009 11:17:00: %FWSM-6-110001: No route to ff02::1:2 from fe80::bd3b:4e58:b410:b56

                    case "110001":
                        {
                            try
                            {

                                rec.CustomStr10 = " ";
                                for (int i = 8; i <= 9; i++)
                                {
                                    rec.CustomStr10 += " " + Desc[i];
                                }

                                rec.CustomStr10.Trim();
                                rec.EventCategory = rec.CustomStr10;

                                rec.CustomStr3 = Desc[11];
                                rec.CustomStr4 = Desc[13];

                            }
                            catch (Exception ex)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Exception message: " + ex.ToString());
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing line in '110001'  : " + args.Message);
                            }
                        }
                        break;
                    #endregion

                    #region Unidentified
                    default:
                        {
                            L.Log(LogType.FILE, LogLevel.ERROR, "Line is not match any of types.");
                            L.Log(LogType.FILE, LogLevel.ERROR, "Line:  " + args.Message);
                        }
                        break;
                    #endregion

                }

            Set:
                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(rec);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() usingRegistry |  Finish Sending Data");
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "", "", "", rec.Datetime);
                    L.Log(LogType.FILE, LogLevel.DEBUG, "slog_SyslogEvent() |  Finish Sending Data");
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "En dýþdaki parsing hatasý.");
                L.Log(LogType.FILE, LogLevel.ERROR, ex.ToString());
            }
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\CiscoPixSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoPixSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("CiscoPixSyslogRecorder").GetValue("Trace Level"));
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager CiscoPixSyslogrecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager CiscoPixSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
