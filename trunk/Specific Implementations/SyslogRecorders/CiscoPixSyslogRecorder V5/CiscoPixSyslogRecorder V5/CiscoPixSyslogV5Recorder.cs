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
    public class CiscoPixSyslogV5Recorder : CustomBase
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
        public CiscoPixSyslogV5Recorder()
		{
		}
        void slog_SyslogEvent(LogMgrEventArgs args)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                L.Log(LogType.FILE, LogLevel.DEBUG, "Start preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "Log is :" + args.Message);
                rec.LogName = "CiscoPixSyslog Recorder";
                rec.Datetime = DateTime.Now.AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = args.EventLogEntType.ToString();
                //rec.Description = args.Message;

                String[] Desc = args.Message.Split(':');

                if (Desc.Length < 5)
                {
                    L.Log(LogType.FILE,LogLevel.ERROR,"Error parsing message: "+args.Message);
                    return;
                }

                for (Int32 i = 0; i < Desc.Length; ++i)
                {
                    Desc[i] = Desc[i].Trim();
                }

                //Parsing PIX
                //Remove %
                

                //Desc[2] = Desc[2].TrimStart('%');
                String[] pixArr = Desc[3].Split('-');

                if(pixArr.Length < 2)
                {
                    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message: " + args.Message);
                    return;
                }

                rec.CustomStr10 = Desc[0].Trim();
                rec.CustomStr9 = Desc[1].Trim();

                //Common fields for all pix records
                //Parsing Date Field
                
                ////if(dateArr.Length < 4)
                ////{
                ////    L.Log(LogType.FILE, LogLevel.ERROR, "Error parsing message: " + args.Message);
                ////    return;
                ////}

                //StringBuilder dateString = new StringBuilder();
                //Date
                //dateString.Append(Desc[0]).Append(":").Append(Desc[1]).Append(":").Append(genArr[0]);
                
                //DateTime dt = DateTime.Parse(dateString.ToString());
                //rec.Datetime = dt.ToString("yyyy/MM/dd HH:mm:ss");
                rec.EventType = pixArr[2];
                //Uncommon fields for all pix records
                //Now Parse with id
                //switch (pixArr[2])
                //{
                //    case "106001"://Untested
                //        {
                //        rec.CustomStr3 = Desc[6];
                //        String[] arrInbound = Desc[7].Split(' ');
                //        String[] arrInboundIp = arrInbound[0].Split('/');

                //        rec.CustomStr5 = arrInboundIp[0];
                //        rec.CustomInt2 = Convert.ToInt32(arrInboundIp[1]);

                //        String[] arrInboundDesc = Desc[8].Split(' ');
                //        String[] arrInboundDescIp = arrInboundDesc[0].Split('/');
                //        StringBuilder sbTempInbound = new StringBuilder();
                //        sbTempInbound.Append(arrInbound[2]).Append(":").Append(arrInboundDescIp[0]);
                //        rec.CustomStr6 = sbTempInbound.ToString();
                //        rec.CustomInt3 = Convert.ToInt32(arrInboundDescIp[1]);

                //        StringBuilder sbTempDescInbound = new StringBuilder();
                //        for (Int32 i = 1; i < arrInboundDesc.Length; ++i)
                //        {
                //            sbTempDescInbound.Append(arrInboundDesc[i]).Append(" ");
                //        }
                //        if (sbTempDescInbound.Length > 0)
                //            sbTempDescInbound.Remove(sbTempDescInbound.Length - 1, 1);
                //        rec.Description = sbTempDescInbound.ToString();
                //        } break;
                //    case "106006"://Untested
                //        {
                //            rec.CustomStr3 = Desc[6];
                //            String[] arrDenyTcp = Desc[7].Split(' ');
                //            String[] arrDenyTcpIp = arrDenyTcp[0].Split('/');

                //            rec.CustomStr5 = arrDenyTcpIp[0];
                //            rec.CustomInt2 = Convert.ToInt32(arrDenyTcpIp[1]);

                //            String[] arrDenyTcpDesc = Desc[8].Split(' ');
                //            String[] arrDenyTcpDescIp = arrDenyTcpDesc[0].Split('/');
                //            StringBuilder sbTempDenyTcp = new StringBuilder();
                //            sbTempDenyTcp.Append(arrDenyTcp[2]).Append(":").Append(arrDenyTcpDescIp[0]);
                //            rec.CustomStr6 = sbTempDenyTcp.ToString();
                //            rec.CustomInt3 = Convert.ToInt32(arrDenyTcpDescIp[1]);

                //            StringBuilder sbTempDescDenyTcp = new StringBuilder();
                //            for (Int32 i = 1; i < arrDenyTcpDesc.Length; ++i)
                //            {
                //                sbTempDescDenyTcp.Append(arrDenyTcpDesc[i]).Append(" ");
                //            }
                //            if (sbTempDescDenyTcp.Length > 0)
                //                sbTempDescDenyTcp.Remove(sbTempDescDenyTcp.Length - 1, 1);
                //            rec.Description = sbTempDescDenyTcp.ToString();
                //        } break;
                //    case "106023":
                //        {
                //            rec.CustomStr3 = Desc[6];
                //            String[] arrDeny = Desc[7].Split(' ');
                //            String[] arrDenyIp = arrDeny[0].Split('/');

                //            rec.CustomStr5 = arrDenyIp[0];
                //            rec.CustomInt2 = Convert.ToInt32(arrDenyIp[1]);

                //            String[] arrDenyDesc = Desc[8].Split(' ');
                //            String[] arrDenyDescIp = arrDenyDesc[0].Split('/');
                //            StringBuilder sbTempDeny = new StringBuilder();
                //            sbTempDeny.Append(arrDeny[1]).Append(" ").Append(arrDeny[2]).Append(":").Append(arrDenyDescIp[0]);
                //            rec.CustomStr6 = sbTempDeny.ToString();
                //            rec.CustomInt3 = Convert.ToInt32(arrDenyDescIp[1]);

                //            StringBuilder sbTempDescDeny = new StringBuilder();
                //            for (Int32 i = 1; i < arrDenyDesc.Length; ++i)
                //            {
                //                sbTempDescDeny.Append(arrDenyDesc[i]).Append(" ");
                //            }
                //            sbTempDescDeny.Remove(sbTempDescDeny.Length - 1, 1);
                //            rec.Description = sbTempDescDeny.ToString();
                //        } break;
                //    case "304001":
                //        {
                //            StringBuilder sbTemp = new StringBuilder();
                //            //Parsing description
                //            String[] arrAccess = Desc[6].Split(' ');
                //            for (Int32 i = 1; i < arrAccess.Length; ++i)
                //            {
                //                sbTemp.Append(arrAccess[i]).Append(' ');
                //            }
                //            sbTemp.Remove(sbTemp.Length - 1, 1);
                //            sbTemp.Append(':').Append(Desc[7]);

                //            rec.CustomStr5 = arrAccess[0];
                //            rec.Description = sbTemp.ToString();

                //        } break;
                //    case "419001":
                //        {
                //            rec.CustomStr3 = Desc[6];
                //            String[] arrDrop = Desc[7].Split(' ');
                //            String[] arrDropIp = arrDrop[0].Split('/');

                //            rec.CustomStr5 = arrDropIp[0];
                //            rec.CustomInt2 = Convert.ToInt32(arrDropIp[1]);

                //            String[] arrDropDesc = Desc[8].Split(' ');
                //            String[] arrDropDescIp = arrDropDesc[0].Split('/');
                //            StringBuilder sbTempDrop = new StringBuilder();
                //            sbTempDrop.Append(arrDrop[2]).Append(":").Append(arrDropDescIp[0]);
                //            rec.CustomStr6 = sbTempDrop.ToString();
                //            rec.CustomInt3 = Convert.ToInt32(arrDropDescIp[1]);

                //            StringBuilder sbTempDescDrop = new StringBuilder();
                //            for (Int32 i = 1; i < arrDropDesc.Length; ++i)
                //            {
                //                sbTempDescDrop.Append(arrDropDesc[i]).Append(" ");
                //            }
                //            if (sbTempDescDrop.Length > 0)
                //                sbTempDescDrop.Remove(sbTempDescDrop.Length - 1, 1);
                //            rec.Description = sbTempDescDrop.ToString();
                //        } break;
                //    default:
                //        rec.Description = args.Message;
                //        break;
                //}     
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish preparing record");
                switch(pixArr[2])
                {
                    case "304001"://Untested
                        {
                            StringBuilder sbTemp = new StringBuilder();
                            //Parsing description
                            String[] arrAccess = Desc[4].Split(' ');
                            rec.CustomStr3 = arrAccess[0];
                            rec.CustomStr6 = arrAccess[3];
                            rec.CustomStr2 = sbTemp.Append(arrAccess[1]).Append(arrAccess[2]).ToString();
                            rec.Description = Desc[5];
                        } break;
                    case "106023"://Untested
                        {
                            StringBuilder sbTemp = new StringBuilder();
                            StringBuilder sbDesc = new StringBuilder();
                            //Parsing description
                            String[] arrAccess = Desc[5].Split(' ');
                            String[] arrDest = Desc[6].Split(' ');                            
                            rec.CustomStr3 = arrAccess[0].Split('/')[0];
                            rec.CustomInt3 = Convert.ToInt32(arrAccess[0].Split('/')[1]);
                            rec.CustomStr6 = arrDest[0].Split('/')[0];
                            rec.CustomInt1 = Convert.ToInt32(arrDest[0].Split('/')[1]);
                            rec.CustomStr2 = sbTemp.Append(Desc[4].Trim()).Append(' ').Append(arrAccess[1]).Append(arrAccess[2]).ToString();
                            
                            

                            for(int i=1;i < arrDest.Length;i++)
                            {
                                sbDesc.Append(arrDest[i]);
                            }
                            rec.Description = sbDesc.ToString();
                        } break;
                    default:
                        rec.Description = args.Message;
                        L.Log(LogType.FILE, LogLevel.WARN, "Could not parse this log: " + args.Message);
                        break;
                }
                rec.SourceName = args.Source;
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
                    s.SetReg(Id, rec.Datetime, "","","",rec.Datetime);
                }
                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }
            catch (Exception er)
            {
                L.LogTimed(LogType.FILE, LogLevel.ERROR, er.ToString());
                L.Log(LogType.FILE, LogLevel.DEBUG, "Exception:"+args.Message);
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
