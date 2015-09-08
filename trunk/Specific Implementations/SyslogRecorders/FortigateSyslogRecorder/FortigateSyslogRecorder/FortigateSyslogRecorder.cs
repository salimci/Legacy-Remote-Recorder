using System.IO;
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

namespace FortigateSyslogRecorder
{
    public class FortigateSyslogRecorder : CustomBase
    {
        private uint logging_interval = 60000, log_size = 1000000;
        private int trc_level = 3, Syslog_Port = 514, zone = 0;
        private string err_log;
        private CLogger L;
        public Syslog slog = null;
        private string protocol, remote_host = "localhost";

        private bool usingRegistry = true;
        private string virtualHost, Dal;
        private int identity;
        private string location;

        private void InitializeComponent()
        {
            //   Init();
        }
        public override void Init()
        {
            if (usingRegistry)
            {
                if (!Read_Registry())
                {
                    EventLog.WriteEntry("Security Manager FortigateSyslogRecorder Read Registry", "FortigateSyslogRecorder may not working properly ", EventLogEntryType.Error);
                    return;
                }
            }
            else
            {
                get_logDir();
            }
            if (!Initialize_Logger())
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error on Intialize Logger on Recorder Service functions may not be running");
                return;
            }

        }
        public void get_logDir()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Remote Recorder").GetValue("Home Directory").ToString() + @"log\FortigateSyslogRecorder" + identity + ".log";
            }
            catch (Exception ess)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "" + ess.ToString());
            }

        }
        public override void SetReg(int Identity, string LastPosition, string SleepTime, string LastFile, string LastKeywords, string LastRecDate)
        {
            base.SetReg(Identity, LastPosition, SleepTime, LastFile, LastKeywords, LastRecDate);
        }

        public override void SetConfigData(Int32 Identity, String Location, String LastLine, string LastPosition,
            String LastFile, String LastKeywords, bool FromEndOnLoss, Int32 MaxLineToWait, String User,
            String Password, String RemoteHost, Int32 SleepTime, Int32 TraceLevel,
            String CustomVar1, Int32 CustomVar2, String virtualhost, String dal, Int32 Zone)
        {

            try
            {

                trc_level = TraceLevel;


                location = Location;
                if (location.Contains(':'.ToString()))
                {
                    String[] parse = location.Split(':');
                    protocol = parse[0];
                    Syslog_Port = Convert.ToInt32(parse[1]);
                }
                else
                {
                    Syslog_Port = 514;
                }

                usingRegistry = false;
                virtualHost = virtualhost;
                Dal = dal;
                identity = Identity;
                remote_host = RemoteHost;
                zone = Zone;
            }
            catch (Exception err)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Error at setConfigData " + err.ToString());
            }


        }// end of setconfigdata

        public override void Clear()
        {
            if (slog != null)
                slog.Stop();
        }

        public override void Start()
        {
            try
            {
                // TODO: Add any initialization after the InitComponent call          
                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing  FortigateSyslogRecorder");

                ProtocolType pro;
                if (protocol.ToLower() == "udp")
                    pro = ProtocolType.Udp;
                else
                    pro = ProtocolType.Tcp;

                //L.Log(LogType.FILE, LogLevel.INFORM, "Start listening FortigateSyslog on ip: " + Dns.GetHostEntry(Environment.MachineName.Trim()).AddressList[0].ToString() + " port: " + Syslog_Port.ToString());
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
                slog.SyslogEvent += new Syslog.SyslogEventDelegate(fortigate_Syslog);

                L.Log(LogType.FILE, LogLevel.INFORM, "Finish initializing FORTIGATE Syslog Event");
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FortigateSyslogRecorder Constructor", er.ToString(), EventLogEntryType.Error);
            }

        }

        public FortigateSyslogRecorder()
        {

        }

        static String[] SpaceSplit(String line, bool useTabs)
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
        }

        void parsTRAFFIC(ref CustomBase.Rec r, string line, string line2)
        {
            #region Traffic
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //  L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;

                case "devname":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "device_id":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "log_id":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "type":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "pri":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting pri--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "vd":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "sn":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sn--");
                        r.CustomInt6 = Convert.ToInt64(line2);
                    }
                    break;
                case "duration":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting duration--");
                        r.CustomInt4 = Convert.ToInt32(line2);
                    }
                    break;
                case "user":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "ou":
                    r.CustomStr6 = r.CustomStr6.ToString() + ", " + line + "=" + line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                //     case "proto":
                //         r.CustomInt4 = Convert.ToInt32(line2);
                //         break;
                case "service":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "app_tpye":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting app type--");
                        r.CustomStr8 = line2;
                    }
                    break;
                case "src":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dstname":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "sent":
                    r.CustomInt9 = Convert.ToInt32(line2);
                    break;
                case "rcvd":
                    r.CustomInt10 = Convert.ToInt32(line2);
                    break;
                case "src_int":
                    {
                        //20.07.2009
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "dst_port":
                    r.CustomInt2 = Convert.ToInt32(line2);
                    break;
                //         case "srcname":
                //             r.CustomInt6 = Convert.ToInt32(line2);
                //             break;
                case "src_port":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "status":
                    {
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + "=" + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.CustomStr8.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parsWEBFILTER(ref CustomBase.Rec r, string line, string line2)
        {
            #region WebFilter
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "cat_desc":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "log_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "type":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "url":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting pri--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "msg":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "hostname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr8 = line2;
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "service":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dstname":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "src_int":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        L.Log(LogType.FILE, LogLevel.DEBUG, "Setting src int : " + line2);
                        r.CustomStr5 = line2;
                    }
                    break;
                case "dst_port":
                    {
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;

                case "src_port":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "status":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting status--");
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parsIPS(ref CustomBase.Rec r, string line, string line2)
        {
            #region AttackIPS
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "msg":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "log_id":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "type":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");

                        //tcp_reassembler: TCP.Data.On.SYN  seq 125068724  ack 2030626367  flags AS

                        string[] parts = line2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        r.CustomStr2 = parts[0].Trim() + " " + parts[1].Trim();
                    }
                    break;
                case "sensor":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting pri--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "ref":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "user":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "service":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dstname":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "src_int":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "src_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "dst_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "attack_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting attack_id--");
                        try
                        {
                            r.CustomInt3 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt3 = -1;
                        }
                    } break;

                case "count":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting count--");
                        try
                        {
                            r.CustomInt4 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt4 = -1;
                        }
                    } break;

                case "status":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting status--");
                        r.EventCategory = line2;
                    } break;
                case "severity":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting severity--");
                        r.CustomStr8 = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }
        
        void parsVIRUS(ref CustomBase.Rec r, string line, string line2)
        {
            #region AttackIPS
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "log_id":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "device_id":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "type":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "url":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting pri--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "msg":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "service":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dstname":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "src_int":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "src_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "sport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "dst_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "dport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "status":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting status--");
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parsAPPCTRL(ref CustomBase.Rec r, string line, string line2)
        {
            #region AttackIPS
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "log_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "group":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "type":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "app":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "app_list":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting app_list--");
                        r.CustomStr8 = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "msg":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "service":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "src_name":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dst_name":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "action":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "count":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;

                case "dst_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "dport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "src_port":
                case "sport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting count--");
                        try
                        {
                            r.CustomInt3 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt3 = -1;
                        }
                    } break;
                case "app_type":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting status--");
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parseDLP(ref CustomBase.Rec r, string line, string line2)
        {
            #region DLP
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "log_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "msg":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "type":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "url":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting url--");
                        r.CustomStr8 = line2;
                    }
                    break;
                case "from":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting from--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "to":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "service":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "src_name":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dst_name":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "hostname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "src_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "sport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "dst_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "dport":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting dst_port--");
                        try
                        {
                            r.CustomInt2 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt2 = -1;
                        }
                    } break;
                case "count":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting count--");
                        try
                        {
                            r.CustomInt3 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt3 = -1;
                        }
                    } break;
                case "rulename":
                    {
                        if (line2.Contains("\""))
                        {
                            int ind = line2.IndexOf('"');
                            line2 = line2.Substring(0, ind);
                        }
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parseEVENT(ref CustomBase.Rec r, string line, string line2)
        {
            #region DLP
            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {
                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "device_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "devname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "log_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "msg":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "type":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "pri":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting from--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "vd":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "server":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr4 = line2;
                    }
                    break;
                case "action":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "status":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.EventCategory = line2;
                    }
                    break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void parsOTHER(ref CustomBase.Rec r, string line, string line2)
        {
            #region Traffic

            string tarih = "";
            line2 = line2.Trim('"');
            switch (line.ToLower())
            {

                case "date":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting date--");
                        tarih += line2 + " ";
                    }
                    break;
                case "time":
                    {
                        tarih += line2;
                        r.Datetime = Convert.ToDateTime(tarih).AddMinutes(zone).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    break;
                case "devname":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting devname--");
                        r.ComputerName = line2;
                    }
                    break;
                case "device_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting device id--");
                        r.CustomStr1 = line2;
                    }
                    break;
                case "log_id":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting log id--");
                        r.EventId = Convert.ToInt32(line2);
                    }
                    break;
                case "type":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting type--");
                        r.EventType = line2;
                    }
                    break;
                case "subtype":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting subtype--");
                        r.CustomStr2 = line2;
                    }
                    break;
                case "pri":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting pri--");
                        r.CustomStr9 = line2;
                    }
                    break;
                case "vd":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting vd--");
                        r.CustomStr10 = line2;
                    }
                    break;
                case "sn":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sn--");
                        r.CustomInt6 = Convert.ToInt64(line2);
                    }
                    break;
                case "duration":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting duration--");
                        r.CustomInt4 = Convert.ToInt32(line2);
                    }
                    break;
                case "user":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting user--");
                        r.UserName = line2;
                    }
                    break;
                case "group":
                    r.CustomStr6 = line2;
                    break;
                case "policyid":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting policy id--");
                        r.Recordnum = Convert.ToInt32(line2);
                    }
                    break;
                case "proto":
                    r.CustomInt4 = Convert.ToInt32(line2);
                    break;
                case "service":
                    {
                        // L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting service--");
                        r.CustomStr7 = line2;
                    }
                    break;
                case "src":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src--");
                        r.CustomStr3 = line2;
                    }
                    break;
                case "dst":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "dstname":
                    {
                        r.CustomStr4 = line2;
                    }
                    break;
                case "sent":
                    r.CustomInt9 = Convert.ToInt32(line2);
                    break;
                case "rcvd":
                    r.CustomInt10 = Convert.ToInt32(line2);
                    break;
                case "src_int":
                    {
                        //20.07.2009
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting src int--");
                        r.CustomStr5 = line2;
                    }
                    break;
                case "dst_port":
                    r.CustomInt2 = Convert.ToInt32(line2);
                    break;
                case "src_port":
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting sport--");
                        try
                        {
                            r.CustomInt1 = Convert.ToInt32(line2);
                        }
                        catch
                        {
                            r.CustomInt1 = -1;
                        }
                    } break;
                case "status":
                    {
                        r.EventCategory = line2;
                    } break;
                default:
                    {
                        //L.Log(LogType.FILE, LogLevel.DEBUG, "-- setting description--");
                        r.Description += " | " + line + " = " + line2;
                        //L.Log(LogType.FILE, LogLevel.DEBUG, r.Description.ToString());
                    }
                    break;
            };
            #endregion
        }

        void fortigate_Syslog(LogMgrEventArgs args)
        {
            Rec r = new Rec();
            //CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
            try
            {

                string line2 = args.Message.ToString();

                string line = line2.Replace('\0', ' ').TrimEnd(' ');
                line = line.Replace("page=", "page ").TrimEnd('\0');
                line = line.Replace(";", ",").TrimEnd('\0');

                if (line.Contains("\t"))
                {
                    line = line.Replace("\t", " ");
                }
                if (line.Contains(" "))
                {
                    line = line.Replace(" ", ",");
                }

                string[] tempArray = line.Split(',');
                string _type = "";
                for (int k = 0; k < tempArray.Length; k++)
                {
                    if (tempArray[k].Contains("type="))
                    {
                        _type = tempArray[k].Split('=')[1];
                        break;
                    }
                }

                string[] arr = line.Split(',');

                r = CreateRecord(arr, _type);

                // Console.WriteLine(r.Description);

                //sending data
                r.LogName = "Fortigate Syslog Recorder";
                r.SourceName = args.Source;

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(r);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "", "", r.Datetime);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "Finish Sending Data");

            }//end of try
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "Wrong data: " + ex.Message);
                L.Log(LogType.FILE, LogLevel.ERROR, "Wrong data: " + args.Message.Replace('\0', ' '));
                r.SourceName = args.Source;
                r.LogName = "FortigateSyslogRecorder";
                r.CustomStr8 = args.Message.Replace('\0', ' ');
                L.Log(LogType.FILE, LogLevel.DEBUG, "(err) Finish preparing record");
                L.Log(LogType.FILE, LogLevel.DEBUG, "(err) Start sending Data");

                if (usingRegistry)
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Sender");
                    s.SetData(r);
                }
                else
                {
                    CustomServiceBase s = base.GetInstanceService("Security Manager Remote Recorder");
                    s.SetData(Dal, virtualHost, r);
                    s.SetReg(identity, r.Datetime, "", "", "", r.Datetime);
                }

                L.Log(LogType.FILE, LogLevel.DEBUG, "(err)Finish Sending Data");

                L.LogTimed(LogType.FILE, LogLevel.ERROR, "Error at parsing" + ex.ToString());
            }
            finally
            {
                //s.Dispose();
            }
        }

        private Rec CreateRecord(string[] arr, string type)
        {
            CustomBase.Rec rec = new CustomBase.Rec();
            try
            {
                string propertyandvalue = "";

                int indexofequal = 0;

                for (int j = 0; j < arr.Length; j++)
                {
                    if (arr[j].Contains("="))
                    {
                        indexofequal = j;
                        break;
                    }
                }

                for (int i = indexofequal; i < arr.Length - 1; i++)
                {
                    if (arr[i].Contains("?"))
                        arr[i] = arr[i].Replace("?", ",");
                    if (arr[i + 1].Contains("?"))
                        arr[i + 1] = arr[i + 1].Replace("?", ",");

                    if (i + 1 < arr.Length)
                    {
                        if (arr[i + 1].Contains("="))
                        {
                            propertyandvalue = arr[i];
                        }
                        else
                        {
                            propertyandvalue = arr[i];

                            int controlff = i;
                            while (!arr[controlff + 1].Contains("="))
                            {
                                propertyandvalue = propertyandvalue + " " + arr[controlff + 1];
                                controlff++;
                                if (controlff == arr.Length - 1)
                                {
                                    break;
                                }
                            }
                            i = controlff;
                        }
                    }
                    else
                    {
                        for (int j = i; j < arr.Length; j++)
                        {
                            propertyandvalue = propertyandvalue + " " + arr[j];
                        }
                    }

                    string[] bol = propertyandvalue.Split('=');
                    if (bol.Length == 3)
                    {
                        bol[1] = bol[1] + "=" + bol[2];
                    }

                    propertyandvalue = "";
                    if (bol.Length > 1)
                    {
                        switch (type)
                        {
                            case "traffic":
                                parsTRAFFIC(ref rec, bol[0], bol[1]);
                                break;
                            case "virus":
                                parsVIRUS(ref rec, bol[0], bol[1]);
                                break;
                            case "webfilter":
                                parsWEBFILTER(ref rec, bol[0], bol[1]);
                                break;
                            case "ips":
                                parsIPS(ref rec, bol[0], bol[1]);
                                break;
                            case "app-ctrl":
                                parsAPPCTRL(ref rec, bol[0], bol[1]);
                                break;
                            case "dlp":
                                parseDLP(ref rec, bol[0], bol[1]);
                                break;
                            case "event":
                                parseEVENT(ref rec, bol[0], bol[1]);
                                break;
                            default:
                                parsOTHER(ref rec, bol[0], bol[1]);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.Log(LogType.FILE, LogLevel.ERROR, "An Error Occured While Creating Record" + ex.Message);
            }

            return rec;
        }

        public bool Read_Registry()
        {
            RegistryKey rk = null;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Natek").OpenSubKey("Security Manager");
                err_log = rk.OpenSubKey("Agent").GetValue("Home Directory").ToString() + @"log\FortigateSyslogRecorder.log";
                Syslog_Port = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("FortigateSyslogRecorder").GetValue("Syslog Port"));
                trc_level = Convert.ToInt32(rk.OpenSubKey("Recorder").OpenSubKey("FortigateSyslogRecorder").GetValue("Trace Level"));
                protocol = rk.OpenSubKey("Recorder").OpenSubKey("FortigateSyslogRecorder").GetValue("Protocol").ToString();
                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager FortigateSyslogRecorder Read Registry", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager FortigateSyslogRecorder Recorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }
    }
}
