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

namespace CiscoPixSyslogRecorder
{
    public class CiscoPixSyslogV7Recorder : CustomBase
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
        public CiscoPixSyslogV7Recorder()
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
                //rec.Description = args.Message;

                String[] Desc = args.Message.Split(':');

                if (args.Message == "")
                {
                    L.Log(LogType.FILE, LogLevel.INFORM, "Message is null " + args.Message);
                    return;
                }

                if (Desc.Length < 6)
                {
                    L.Log(LogType.FILE,LogLevel.ERROR,"Error parsing message for 6: "+args.Message);
                    return;
                }

                for (Int32 i = 0; i < Desc.Length; ++i)
                {
                    Desc[i] = Desc[i].Trim();
                }

                //Parsing PIX
                //Remove %
                Desc[5] = Desc[5].TrimStart('%');
                String[] pixArr = Desc[5].Split('-');

                if(pixArr.Length < 2)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 2:" + args.Message);
                    return;
                }
                //rec.CustomStr4 = pixArr[0] + "-" + pixArr[1];

                //Common fields for all pix records
                //Parsing Date Field

                String [] dateArr = SpaceSplit(Desc[2]);
                if(dateArr.Length < 4)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 4: " + args.Message);
                    return;
                }

                StringBuilder dateString = new StringBuilder();
                //Date
                dateString.Append(dateArr[1]).Append(" ").Append(dateArr[2]).Append(" ").Append(dateArr[3]).Append(" ");
                //Time
                dateString.Append(dateArr[4]).Append(":").Append(Desc[3]).Append(":").Append(Desc[4]);
                DateTime dt = DateTime.Parse(dateString.ToString());
                rec.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss");

                //Uncommon fields for all pix records
                //Now Parse with id
                rec.EventId = Convert.ToInt64(pixArr[2]);
                switch (pixArr[2])
                {
                    case "106001"://Untested
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

                            StringBuilder customStr2 = new StringBuilder();
                            for (Int32 i = 0; i < firstIp; i++)
                            {
                                customStr2.Append(arrInbound[i]).Append(" ");
                            }

                            rec.CustomStr2 = customStr2.ToString().Trim();

                            StringBuilder customStr4 = new StringBuilder();
                            for (Int32 i = firstIp + 1; i < secondIp; i++)
                            {
                                customStr4.Append(arrInbound[i]).Append(" ");
                            }

                            rec.CustomStr7 = customStr4.ToString().Trim();

                            String[] arrInboundIp = arrInbound[firstIp].Split('/');

                            rec.CustomStr3 = arrInboundIp[0];
                            rec.CustomInt1 = Convert.ToInt32(arrInboundIp[1]);

                            StringBuilder customStr6 = new StringBuilder();
                            for (Int32 i = secondIp + 1; i < arrInbound.Length; i++)
                            {
                                customStr6.Append(arrInbound[i]).Append(" ");
                            }

                            rec.Description = customStr6.ToString().Trim();

                            String[] arrInboundDescIp = arrInbound[secondIp].Split('/');
                            rec.CustomStr6 = arrInboundDescIp[0];
                            rec.CustomInt3 = Convert.ToInt32(arrInboundDescIp[1]);

                        } break;
                    
                    case "106015":
                    case "302020":
                    case "302021":
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

                            StringBuilder customStr2 = new StringBuilder();
                            for (Int32 i = 0; i < firstIp; i++)
                            {
                                customStr2.Append(arrInbound[i]).Append(" ");
                            }

                            rec.CustomStr2 = customStr2.ToString().Trim();

                            StringBuilder customStr4 = new StringBuilder();
                            for (Int32 i = firstIp + 1; i < secondIp; i++)
                            {
                                customStr4.Append(arrInbound[i]).Append(" ");
                            }

                            rec.CustomStr4 = customStr4.ToString().Trim();

                            String[] arrInboundIp = arrInbound[firstIp].Split('/');

                            rec.CustomStr3 = arrInboundIp[0];
                            rec.CustomInt1 = Convert.ToInt32(arrInboundIp[1]);

                            StringBuilder customStr6 = new StringBuilder();
                            for (Int32 i = secondIp + 1; i < arrInbound.Length; i++)
                            {
                                customStr6.Append(arrInbound[i]).Append(" ");
                            }

                            rec.Description = customStr6.ToString().Trim();

                            String[] arrInboundDescIp = arrInbound[secondIp].Split('/');
                            rec.CustomStr5 = arrInboundDescIp[0];
                            rec.CustomInt3 = Convert.ToInt32(arrInboundDescIp[1]);

                        } break;
/*                        {
                            String[] arrDenyInbound = Desc[6].Split(' ');

                            Int32 firstIpDenyInbound = 0;
                            Int32 secondIpDenyInbound = 0;
                            bool firstDenyInbound = true;

                            for (Int32 i = 0; i < arrDenyInbound.Length; i++)
                            {
                                if (arrDenyInbound[i].Contains("/"))
                                {
                                    if (firstDenyInbound)
                                    {
                                        firstIpDenyInbound = i;
                                        firstDenyInbound = false;
                                    }
                                    else
                                    {
                                        secondIpDenyInbound = i;
                                        break;
                                    }
                                }

                            }

                            StringBuilder customStr2DenyInbound = new StringBuilder();
                            for (Int32 i = 0; i < firstIpDenyInbound; i++)
                            {
                                customStr2DenyInbound.Append(arrDenyInbound[i]).Append(" ");
                            }

                            rec.CustomStr2 = customStr2DenyInbound.ToString().Trim();

                            StringBuilder customStr4 = new StringBuilder();
                            for (Int32 i = firstIpDenyInbound + 1; i < secondIpDenyInbound; i++)
                            {
                                customStr4.Append(arrDenyInbound[i]).Append(" ");
                            }

                            rec.CustomStr4 = customStr4.ToString().Trim();

                            String[] arrDenyInboundIp = arrDenyInbound[firstIpDenyInbound].Split('/');

                            rec.CustomStr3 = arrDenyInboundIp[0];
                            rec.CustomInt1 = Convert.ToInt32(arrDenyInboundIp[1]);

                            StringBuilder customStr6 = new StringBuilder();
                            for (Int32 i = secondIpDenyInbound + 1; i < arrDenyInbound.Length; i++)
                            {
                                customStr6.Append(arrDenyInbound[i]).Append(" ");
                            }

                            rec.Description = customStr6.ToString().Trim();

                            String[] arrDenyInboundDescIp = arrDenyInbound[secondIpDenyInbound].Split('/');
                            rec.CustomStr6 = arrDenyInboundDescIp[0];
                            rec.CustomInt3 = Convert.ToInt32(arrDenyInboundDescIp[1]);

                        }
                        break;
 */
                    case "106021":
                        {
                            String[] arrDeny = Desc[6].Split(' ');
                            rec.CustomStr3 = arrDeny[6];
                            rec.CustomStr6 = arrDeny[8];

                            StringBuilder sbTempDeny = new StringBuilder();

                            for (Int32 i = 1; i < arrDeny.Length; i++)
                            {
                                if (i != 6 && i != 8)
                                {
                                    sbTempDeny.Append(arrDeny[i].ToString());
                                    sbTempDeny.Append(" ");
                                }
                            }

                            rec.CustomStr2 = sbTempDeny.ToString().Trim();
                        }
                        break;
                    case "106006":
                        {
                            String[] arrDeny = Desc[6].Split(' ');
                            String[] arrDenyIp = arrDeny[4].Split('/');

                            rec.CustomStr3 = arrDenyIp[0];
                            if (arrDenyIp.Length > 1)
                                rec.CustomInt1 = Convert.ToInt32(arrDenyIp[1]);


                            String[] arrDenyDescIp = arrDeny[6].Split('/');

                            StringBuilder sbTempDeny = new StringBuilder();

                            for (Int32 i = 1; i < arrDeny.Length; i++)
                            {
                                if (i != 4 && i != 6)
                                {
                                    sbTempDeny.Append(arrDeny[i].ToString());
                                    sbTempDeny.Append(" ");
                                }
                            }

                            rec.CustomStr2 = sbTempDeny.ToString().Trim();

                            rec.CustomStr6 = arrDenyDescIp[0];
                            if (arrDenyDescIp.Length > 1)
                                rec.CustomInt2 = Convert.ToInt32(arrDenyDescIp[1]);

                        }
                        break;
                    case "106007":
                        {                            
                            String[] arrDeny = Desc[6].Split(' ');
                            StringBuilder sbTempDeny = new StringBuilder();                            
                            for (Int32 i = 1; i < arrDeny.Length; i++)
                                if(i!=4 && i!=6)
                                    sbTempDeny.Append(arrDeny[i]).Append(" ");
                            rec.CustomStr2 = sbTempDeny.ToString().Trim();
                            String[] arrDeny2 = arrDeny[6].Split('/');
                            String[] arrDeny3 = arrDeny[4].Split('/');
                            rec.CustomStr6 = arrDeny2[0];
                            rec.CustomStr3 = arrDeny3[0];
                            if (arrDeny2.Length > 1)
                                rec.CustomInt2 = Convert.ToInt32(arrDeny2[1]);
                            if (arrDeny3.Length > 1)
                                rec.CustomInt1 = Convert.ToInt32(arrDeny3[1]);


                        }
                        break;
                    
	                  case "106017":
	                      {
                            try
                            {                                
                                String[] arrDeny = Desc[6].Split(' ');
		                        for (Int32 i = 1; i < 6; i++)
                                    rec.CustomStr2 = rec.CustomStr2 + arrDeny[i];                               
                                rec.CustomStr3 = arrDeny[7];
                                rec.CustomStr5 = arrDeny[9];                            
		                    }
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 106023:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                                break;
                            }
                            break;
                        }
                    case "710003":
                        {
                            //Desc[6] = TCP access denied by ACL from 131.162.130.192/43789 to outside
                            //Desc[7] = 193.140.76.0/80
                            try
                            {
                                String[] spSplit = Desc[6].Split(' ');
                                String[] destIp = Desc[7].Trim().Split('/');
                                String[] srcIp = spSplit[6].Split('/');
                                StringBuilder sb = new StringBuilder();
                                rec.CustomStr3 = srcIp[0];
                                rec.CustomInt1 = Convert.ToInt32(srcIp[1]);
                                rec.CustomStr6 = destIp[0];
                                rec.CustomInt2 = Convert.ToInt32(destIp[1]);
                                rec.CustomStr7 = spSplit[7] + spSplit[8];
                                for (int i = 0; i < 5; i++)
                                {
                                    sb.Append(spSplit[i]);
                                }
                                rec.CustomStr2 = sb.ToString();

                            }
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, Desc[5] + Desc[6]);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());                                
                            }
                        }
                        break;
          case "106016":                                        
          case "106014":                    
                    case "106023":
                        {
                            try
                            {
                                rec.CustomStr2 = Desc[6];
                                String[] arrDeny = Desc[7].Split(' ');
                                String[] arrDenyIp = arrDeny[0].Split('/');

                                rec.CustomStr3 = arrDenyIp[0];
                                if(arrDenyIp.Length > 1)
                                rec.CustomInt1 = Convert.ToInt32(arrDenyIp[1]);

                                String[] arrDenyDesc = Desc[8].Split(' ');
                                String[] arrDenyDescIp = arrDenyDesc[0].Split('/');

                                StringBuilder sbTempDeny = new StringBuilder();
                                sbTempDeny.Append(rec.CustomStr2).Append(" ");
                                for (Int32 i = 1; i < arrDeny.Length; i++)
                                    sbTempDeny.Append(arrDeny[i]).Append(" ");
                                rec.CustomStr2 = sbTempDeny.ToString().Trim();
                                rec.CustomStr6 = arrDenyDescIp[0];
                                if(arrDenyDescIp.Length > 1)
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
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 106023:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                                break;
                            }
                            break;
                        }
                    case "715001":
                        {
                            String[] x1 = Desc[6].Split(' ');
                            String[] x2 = Desc[7].Split('/');

                            try
                            {
                                //CustomStr7 is -->  to outside || to inside
                                //CustomStr3 --> SourceIP                                    
                                //CustomInt1 is --> SourcePort
                                if (x1.Length > 5)
                                {
                                    StringBuilder desc = new StringBuilder();
                                    desc.Append(x1[0]);
                                    desc.Append(' ');
                                    desc.Append(x1[1]);
                                    desc.Append(' ');
                                    desc.Append(x1[2]);
                                    desc.Append(' ');
                                    desc.Append(x1[3]);
                                    String[] part1 = x1[4].Split('/');
                                    rec.CustomStr3 = part1[0];
                                    rec.CustomInt1 = Convert.ToInt32(part1[1]);
                                    rec.CustomStr2 = desc.ToString();
                                    rec.CustomStr7 = x1[5] + x1[6];
                                }

                                if (x2.Length > 2)
                                {
                                    //CustomStr6 --> DestIP                                    
                                    //CustomInt6 is --> DestPort
                                    String[] part2 = x2[0].Split('/');                                                                        
                                    rec.CustomStr6 = part2[0];                                    
                                    if (part2.Length > 1)
                                        rec.CustomInt6 = Convert.ToInt32(part2[1].Trim());                                                                       
                                }                                                 
                            }
                            catch
                            {
                            }
                            
                        }
                        break;
                    case "305009":
                        {
                            String[] x1 = Desc[6].Split(' ');
                            String[] x2 = Desc[7].Split(' ');

                            try
                            {
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
                                    rec.CustomStr7 = x1[4];
                                }

                                if (x2.Length > 2)
                                {
                                    String[] part2 = x2[0].Split('/');
                                    StringBuilder dest = new StringBuilder();
                                    for (int k = 1; k < x2.Length; k++)
                                    {
                                        dest.Append(x2[k].Trim());
                                    }
                                    //CustomStr7 is -->  to outside || to inside
                                    //CustomStr3 --> SourceIP                                    
                                    //CustomInt1 is --> SourcePort
                                    rec.CustomStr7 += dest.ToString();
                                    rec.CustomStr3 = part2[0].Trim();
                                    if (part2.Length > 1)
                                        rec.CustomInt1 = Convert.ToInt32(part2[1].Trim());                                                                        
                                }

                                //CustomStr6 --> DestIP                                    
                                //CustomInt6 is --> DestPort
                                rec.CustomStr6 = Desc[8].ToString();                           
                            }
                            catch
                            {
                            }
                            
                        }
                        break;
                    case "302015"://Tested
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302015:" + args.Message);
                            }
                            //CustomStr --> Description Ex:Built Outbound TCP Connection
                            //CustomInt2 --> SessionID
                            //CustomStr7 --> to outside || to inside
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
                                    rec.CustomStr1 = x1[4].Trim();
                                    rec.CustomStr7 = x1[6];
                                }

                                if (x2.Length > 2)
                                {
                                    String[] part2 = x2[0].Split('/');
                                    String[] part2dest = x2[1].Trim('(', ')').Split('/');
                                    StringBuilder dest = new StringBuilder();
                                    for (int k = 2; k < x2.Length; k++)
                                    {
                                        dest.Append(x2[k].Trim());
                                    }
                                    //CustomStr7 is -->  to outside || to inside
                                    //CustomStr3 --> SourceIP
                                    //CustomStr4 --> XSourceIP
                                    //CustomInt1 and CustomInt4 is --> SourcePort and XsourcePort
                                    rec.CustomStr7 += dest.ToString();
                                    rec.CustomStr3 = part2[0].Trim();
                                    if (part2.Length > 1)
                                        rec.CustomInt1 = Convert.ToInt32(part2[1].Trim());
                                    rec.CustomStr4 = part2dest[0].Trim();
                                    rec.CustomInt4 = Convert.ToInt32(part2dest[1].Trim());
                                }

                                if (x3.Length > 1)
                                {
                                    //CustomStr6 --> DestIP
                                    //CustomStr5 --> XDestIP
                                    //CustomInt6 and CustomInt5 is --> DestPort and XDestePort
                                    String[] part3 = x3[0].Split('/');
                                    String[] part3dest = x3[1].Trim('(', ')').Split('/');
                                    rec.CustomStr6 = part3[0].Trim();
                                    if (part3.Length > 1)
                                        rec.CustomInt6 = Convert.ToInt32(part3[1].Trim());
                                    rec.CustomStr5 = part3dest[0].Trim();
                                    rec.CustomInt5 = Convert.ToInt32(part3dest[1].Trim());
                                }
                            }

                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302015:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                            }

                        }
                        break;
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302013:" + args.Message);
                            }
                            //CustomStr --> Description Ex:Built Outbound TCP Connection
                            //CustomInt2 --> SessionID
                            //CustomStr7 --> to outside || to inside
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
                                    rec.CustomStr1 = x1[4].Trim();
                                    rec.CustomStr7 = x1[6];
                                }

                                if (x2.Length > 2)
                                {
                                    String[] part2 = x2[0].Split('/');
                                    String[] part2dest = x2[1].Trim('(', ')').Split('/');
                                    StringBuilder dest = new StringBuilder();
                                    for (int k = 2; k < x2.Length; k++)
                                    {
                                        dest.Append(x2[k].Trim());
                                    }
                                    //CustomStr7 is -->  to outside || to inside
                                    //CustomStr6 --> DestIP
                                    //CustomStr5 --> XDestIP
                                    //CustomInt6 and CustomInt5 is --> DestPort and XDestePort
                                    rec.CustomStr7 += dest.ToString();
                                    //rec.CustomStr3 = part2[0].Trim();
                                    rec.CustomStr6 = part2[0].Trim();
                                    if (part2.Length > 1)
                                        rec.CustomInt6 = Convert.ToInt32(part2[1].Trim());
                                    //rec.CustomStr4 = part2dest[0].Trim();
                                    rec.CustomStr5 = part2dest[0].Trim();
                                    rec.CustomInt5 = Convert.ToInt32(part2dest[1].Trim());
                                }
                                
                                if (x3.Length > 1)
                                {
                                    //CustomStr3 --> SourceIP
                                    //CustomStr4 --> XSourceIP
                                    //CustomInt1 and CustomInt4 is --> SourcePort and XsourcePort
                                    String[] part3 = x3[0].Split('/');
                                    String[] part3dest = x3[1].Trim('(', ')').Split('/');
                                    rec.CustomStr3 = part3[0].Trim();
                                    if (part3.Length > 1)
                                        rec.CustomInt1 = Convert.ToInt32(part3[1].Trim());
                                    rec.CustomStr4 = part3dest[0].Trim();
                                    rec.CustomInt4 = Convert.ToInt32(part3dest[1].Trim());
                                }
                            }

                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302013:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                            }
                            
                        }
                        break;
                    case "305010":
                    case "302016":
                    case "302014":
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
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302014:" + args.Message);
                            }
                            //CustomStr2 --> Description Ex:Built Outbound TCP Connection
                            //CustomInt2 --> SessionID
                            //CustomStr7 --> to outside || to inside
                            
                                if (x1.Length > 4)
                                {
                                    StringBuilder desc = new StringBuilder();
                                    desc.Append(x1[0]);
                                    desc.Append(' ');
                                    desc.Append(x1[1]);
                                    desc.Append(' ');
                                    desc.Append(x1[2]);
                                    desc.Append(' ');
                                    //desc.Append(x1[3]);
                                    if (x5.Length > 4)
                                    {
                                        desc.Append(x5[3]);
                                        desc.Append(' ');
                                        desc.Append(x5[4]);
                                    }
                                    rec.CustomStr2 = desc.ToString();

                                    if (x1.Length > 5)
                                    {
                                        rec.CustomStr7 = x1[5];
                                        rec.CustomStr1 = x1[3].Trim();
                                    }
                                    else
                                        rec.CustomStr7 = x1[4];
                                    
                                }

                                if (x2.Length > 2)
                                {
                                    String[] part2 = x2[0].Split('/');
                                    StringBuilder dest = new StringBuilder();
                                    for (int k = 1; k < x2.Length; k++)
                                    {
                                        dest.Append(x2[k].Trim());
                                    }
                                    //CustomStr7 is -->  to outside || to inside
                                    //CustomStr3 --> SourceIP

                                    //CustomInt1 and CustomInt4 is --> SourcePort and XsourcePort
                                    rec.CustomStr7 += dest.ToString();
                                    rec.CustomStr3 = part2[0];
                                    if (part2.Length > 1)
                                        rec.CustomInt1 = Convert.ToInt32(part2[1].Trim());
                                    
                                }
                                if (x3.Length > 2 && x5.Length > 2)
                                {
                                    //CustomStr6 --> DestIP
                                    //CustomStr8 --> Duration
                                    //CustomInt6 --> DestPort
                                    //CustomInt7 --> Bytes
                                    //CustomStr4 is Reset-O
                                    String[] part3 = x3[0].Split('/');
                                    rec.CustomStr6 = part3[0];
                                    rec.CustomInt6 = Convert.ToInt32(part3[1].Trim());
                                    StringBuilder duration = new StringBuilder();
                                    duration.Append(x3[2]);
                                    duration.Append(':');
                                    duration.Append(Desc[9]);
                                    duration.Append(':');
                                    duration.Append(x5[0]);
                                    rec.CustomStr8 = duration.ToString();
                                    if(x5.Length > 4)
                                    rec.CustomInt7 = Convert.ToInt32(x5[2]);
                                    
                                }
                            }
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 302014:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                            }
                            
                        }
                        break;
                    case "609001":
                        {
                            
                            try
                            {
                                //Parsing description
                                //CustomStr3 --> localhost
                                //CUstomStr6 --> Dest
                                String[] arrAccess = Desc[6].Split(':');
                                if (arrAccess.Length > 1)
                                {
                                    rec.CustomStr3 = arrAccess[0].Split(' ')[1];
                                    rec.CustomStr6 = arrAccess[1];
                                }
                                else if (Desc.Length > 7)
                                {
                                    rec.CustomStr3 = Desc[6].Split(' ')[1];
                                    rec.CustomStr6 = Desc[7];
                                }
                                else
                                {
                                    rec.Description = args.Message;
                                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 609001 -insert into description-:" + args.Message);
                                }
                                rec.Description = Desc[6];
                                if (Desc.Length > 7)
                                    rec.CustomStr2 = Desc[6] +' '+  Desc[7];
                            }
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 609001:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                            }
                            
                        } break;
                    case "609002":
                        {
                            
                            try
                            {
                                //Parsing description
                                //CustomStr3 --> localhost
                                //CUstomStr6 --> Dest
                                //CustomStr8 --> Duration
                                String[] arrAccess = Desc[6].Split(':');
                                StringBuilder input = new StringBuilder();
                                


                                if (arrAccess.Length < 4)
                                {
                                    if (Desc.Length > 9)
                                    {
                                        
                                        input.Append(Desc[6]);
                                        input.Append(':');
                                        input.Append(Desc[7]);
                                        input.Append(':');
                                        input.Append(Desc[8]);
                                        input.Append(':');
                                        input.Append(Desc[9]);
                                        arrAccess = input.ToString().Split(':');
                                    }
                                    else
                                    {
                                        rec.Description = args.Message;
                                        L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 609002 -insert into description-:" + args.Message);
                                        break;
                                    }
                                }                                
                                StringBuilder duration = new StringBuilder();
                                rec.CustomStr3 = arrAccess[0].Split(' ')[1];
                                rec.CustomStr6 = arrAccess[1].Split(' ')[0];
                                duration.Append(arrAccess[1].Split(' ')[2]);
                                duration.Append(':');
                                duration.Append(arrAccess[2]);
                                duration.Append(':');
                                duration.Append(arrAccess[3]);
                                rec.CustomStr8 = duration.ToString();
                                rec.Description = Desc[6];
                                rec.CustomStr2 = input.ToString();
                            }
                            catch (Exception e)
                            {
                                L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message for 609002:" + args.Message);
                                L.Log(LogType.FILE, LogLevel.ERROR, e.ToString());
                            }
                            
                        } break;

			                    
                    case "305011"://Tested
                    case "305012"://Tested
                        {
                            rec.CustomStr2 = Desc[6].Trim();
                            Desc[7] += ":";
                            for (Int32 i = 8; i < Desc.Length; i++)
                                Desc[7] += Desc[i] + ":";
                            Desc[7] = Desc[7].TrimEnd(':');

                            String[] arrInbound = Desc[7].Split(' ');

                            Int32 firstIp = 0;
                            Int32 secondIp = 0;
                            bool first = true;

                            for (Int32 i = 0; i < arrInbound.Length; i++)
                            {
                                if (arrInbound[i].Contains("/") && !arrInbound[i].Contains("("))
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

                            StringBuilder customStr4 = new StringBuilder();
                            for (Int32 i = firstIp + 1; i < secondIp; i++)
                            {
                                customStr4.Append(arrInbound[i]).Append(" ");
                            }

                            rec.CustomStr4 = customStr4.ToString().Trim();

                            String[] arrInboundIp = arrInbound[firstIp].Split('/');
                            if (arrInboundIp[0].Contains(":"))
                            {
                                String[] DescIpSplit = arrInboundIp[0].Split(':');
                                rec.CustomStr2 += " " + DescIpSplit[0];
                                rec.CustomStr3 = DescIpSplit[1];
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

                            rec.Description = customStr6.ToString().Trim();

                            String[] arrInboundDescIp = arrInbound[secondIp].Split('/');
                            if (arrInboundDescIp[0].Contains(":"))
                            {
                                String[] DescIpSplit = arrInboundDescIp[0].Split(':');
                                rec.CustomStr4 += " " + DescIpSplit[0];
                                rec.CustomStr6 = DescIpSplit[1];
                            }
                            else
                            {
                                rec.CustomStr6 = arrInboundDescIp[0];
                            }
                            rec.CustomInt3 = Convert.ToInt32(arrInboundDescIp[1]);

                        } break;
                    case "304001":
                        {
                            StringBuilder sbTemp = new StringBuilder();
                            //Parsing description
                            String[] arrAccess = Desc[6].Split(' ');
                            for (Int32 i = 1; i < arrAccess.Length; ++i)
                            {
                                sbTemp.Append(arrAccess[i]).Append(' ');
                            }
                            sbTemp.Remove(sbTemp.Length - 1, 1);
                            sbTemp.Append(':').Append(Desc[7]);

                            rec.CustomStr5 = arrAccess[0];
                            rec.Description = sbTemp.ToString();

                        } break;
                    case "419001":
                        rec.CustomStr2 = Desc[6];
                        String[] arrDrop = Desc[7].Split(' ');
                        String[] arrDropIp = arrDrop[0].Split('/');

                        rec.CustomStr3 = arrDropIp[0];
                        rec.CustomInt1 = Convert.ToInt32(arrDropIp[1]);

                        String[] arrDropDesc = Desc[8].Split(',');
                        String[] arrDropDescIp = arrDropDesc[0].Split('/');
                        StringBuilder sbTempDrop = new StringBuilder();
                        sbTempDrop.Append(arrDropDescIp[0]);
                        rec.CustomStr7 = sbTempDrop.ToString();
                        rec.CustomInt3 = Convert.ToInt32(arrDropDescIp[1]);

                        StringBuilder sbTempDescDrop = new StringBuilder();
                        for (Int32 i = 1; i < arrDropDesc.Length; ++i)
                        {
                            sbTempDescDrop.Append(arrDropDesc[i]).Append(" ");
                        }
                        if (sbTempDescDrop.Length > 0)
                            sbTempDescDrop.Remove(sbTempDescDrop.Length - 1, 1);
                        rec.Description = sbTempDescDrop.ToString();

                        break;
                    default:
                        L.Log(LogType.FILE, LogLevel.DEBUG, "No match for the mesage: "+args.Message);
                        rec.Description = args.Message;
                        break;
                }                            
                rec.SourceName = args.Source;
                // Fields are changed like other firewall for standartization

                string backup = null;
                backup = rec.CustomStr4;
                rec.CustomStr4 = rec.CustomStr6;
                rec.CustomStr6 = rec.CustomStr5;
                rec.CustomStr5 = backup;

                int bakcup = 0;                
                rec.CustomInt2 = rec.CustomInt1;
                rec.CustomInt1 = rec.CustomInt3;
                rec.CustomInt3 = bakcup;



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
                    s.SetData(Dal,virtualhost, rec);
                    s.SetReg(Id, rec.Datetime, "","", "",rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.LogTimed(LogType.FILE, LogLevel.ERROR,args.Message);
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
                EventLog.WriteEntry("Security Manager CiscoPixSyslog Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

    }
}
