//Name: IronPortSyslogRecorder
//Writer: Selahattin ÜNALAN
//Date: 29/12/2011

using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace IronPortSyslogRecorder
{

    public class IronPortSyslogRecorder : CustomBase
    {
        private CLogger _log;
        private System.Net.Sockets.ProtocolType _protocolType;
        private Syslog _sysLog = null;

        private String[] _skipKeyWords = null;
        private String _recorderName = "";

        private uint logging_interval = 60000, _logSize = 1000000;
        private Int32 _traceLevel = 3, _syslogPort = 514, _zone = 0;
        private String _errLog, _remoteHost = "localhost";

        private Int32 _id = 0;
        private String _virtualHost, _dal;

        public IronPortSyslogRecorder()
        {
        }

        public override void SetConfigData(
           Int32 identity, String location, String lastLine, String lastPosition,
           String lastFile, String lastKeywords, Boolean fromEnd, Int32 maxRecordSend, String user,
           String password, String remoteHost, Int32 sleepTime, Int32 traceLevel,
           String customVar1, Int32 customVar2, String virtualhost, String dal, Int32 zone)
        {
            _id = identity;
            _traceLevel = traceLevel;
            _virtualHost = virtualhost;
            _zone = zone;
            _remoteHost = remoteHost;
            _dal = dal;
            SetProtocolVariables(location);
        }

        private void SetProtocolVariables(String location)
        {
            try
            {
                String proType = "";
                if (location.Contains(":"))
                {
                    String[] parts = location.Split(new Char[] { ':' });
                    proType = parts[0];
                    _syslogPort = Convert.ToInt32(parts[1]);
                }
                else
                {
                    proType = "udp";
                    _syslogPort = 514;
                }

                switch (proType.ToLower())
                {
                    case "tcp": _protocolType = ProtocolType.Tcp; break;
                    default: _protocolType = ProtocolType.Udp; break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override void Init()
        {
            try
            {
                if (GetLogDir())
                {
                    if (InitializeLogger())
                    {
                        _log.Log(LogType.FILE, LogLevel.INFORM, " Init() -->> Start creating DAL");
                    }
                    else
                    {
                        _log.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> An error occurred  : ");
                        return;
                    }
                }
                else
                {
                    return;
                }

                _log.Log(LogType.FILE, LogLevel.INFORM, " Init() -->> Start listening syslogs on ip: " + _remoteHost + " port: " + _syslogPort.ToString());
                _sysLog = new Syslog(_remoteHost, _syslogPort, _protocolType);

                _sysLog.Start();
                _sysLog.SyslogEvent += new Syslog.SyslogEventDelegate(SlogSyslogEvent);

                _log.Log(LogType.FILE, LogLevel.INFORM, "Init() -->> Finish initializing IronPortSyslogRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IronPortSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
            }
        }

        private Boolean InitializeLogger()
        {
            try
            {
                _log = new CLogger();
                switch (_traceLevel)
                {
                    case 0: { _log.SetLogLevel(LogLevel.NONE); } break;
                    case 1: { _log.SetLogLevel(LogLevel.INFORM); } break;
                    case 2: { _log.SetLogLevel(LogLevel.WARN); } break;
                    case 3: { _log.SetLogLevel(LogLevel.ERROR); } break;
                    case 4: { _log.SetLogLevel(LogLevel.DEBUG); } break;
                }

                _log.SetLogFile(_errLog);
                _log.SetTimerInterval(LogType.FILE, logging_interval);
                _log.SetLogFileSize(_logSize);

                return true;
            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager IronPortSyslogRecorder", er.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        public override void Clear()
        {
            if (_sysLog != null)
                _sysLog.Stop();
        }

        private Boolean GetLogDir()
        {
            RegistryKey rk = null;
            DateTime dateTime = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder");
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "IronPortSyslogRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager IronPortSyslogRecorder Read Registry", ex.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        void SlogSyslogEvent(LogMgrEventArgs args)
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " SlogSyslogEvent() --> is STARTED");
                _log.Log(LogType.FILE, LogLevel.DEBUG, " SlogSyslogEvent() --> will parse data : " + args.Message);

                CustomBase.Rec rec = new CustomBase.Rec();
                rec.LogName = "IronPortSyslogRecorder";
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                rec.SourceName = args.Source;
                if (args.Message.Length > 895)
                {
                    rec.Description = args.Message.Substring(0, 894);
                }
                else
                {
                    rec.Description = args.Message;
                }

                CoderParse(args.Message, ref rec);

                CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                customServiceBase.SetData(_dal, _virtualHost, rec);
                customServiceBase.SetReg(_id, rec.Datetime, "", "", "", rec.Datetime);

                _log.Log(LogType.FILE, LogLevel.DEBUG, " SlogSyslogEvent() --> is succesfully FINISHED.");
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " SlogSyslogEvent() --> An error occurred : " + ex.ToString());
            }
        }

        private void CoderParse(String line, ref CustomTools.CustomBase.Rec rec)
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
                _log.Log(LogType.FILE, LogLevel.DEBUG, " Line start -->> " + line);

                if (!IsSkipKeyWord(line))
                {
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord Line -->> " + line);
                    String[] subLine0 = line.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    //rec.CustomStr10 = Convert.ToString(subLine0[6]);
                    string line1 = Convert.ToString(subLine0[6]);
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " Line1 -->> " + line1);

                    rec.ComputerName = Convert.ToString(subLine0[0]);
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " Computername -->> " + subLine0[0]);

                    if (line1.Contains("New SMTP ICID") && line1.Contains("reverse dns host") && !line1.Contains("interface Outgoing"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> is STARTED : " + line1);
                        string[] smtp = line1.Split(' ');
                        rec.SourceName = Convert.ToString(smtp[13]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> SourceName : " + rec.SourceName);
                        rec.EventCategory = Convert.ToString(smtp[1]) + " " + Convert.ToString(smtp[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> EventCategory : " + rec.EventCategory);
                        rec.EventType = Convert.ToString(smtp[5]) + " " + Convert.ToString(smtp[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> EventType : " + rec.EventType);
                        rec.CustomStr4 = Convert.ToString(smtp[7].TrimStart('(').TrimEnd(')'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> CustomStr4 : " + rec.CustomStr4);
                        rec.CustomStr3 = Convert.ToString(smtp[9]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> CustomStr3 : " + rec.CustomStr3);
                        rec.CustomStr9 = Convert.ToString(smtp[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> CustomStr9 : " + rec.CustomStr9);
                        rec.CustomInt9 = Convert.ToInt32(smtp[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID -->> CustomInt9 : " + rec.CustomInt9);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("New SMTP DCID"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> is STARTED : " + line1);
                        string[] dcdi = line1.Split(' ');
                        rec.EventCategory = Convert.ToString(dcdi[1]) + " " + Convert.ToString(dcdi[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> EventCategory : " + rec.EventCategory);
                        rec.EventType = Convert.ToString(dcdi[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> EventType : " + rec.EventType);
                        rec.CustomStr3 = Convert.ToString(dcdi[8]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> CustomStr3 : " + rec.CustomStr3);
                        rec.CustomStr4 = Convert.ToString(dcdi[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> CustomStr4 : " + rec.CustomStr4);
                        rec.CustomInt4 = Convert.ToInt32(dcdi[10]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> CustomInt4 : " + rec.CustomInt4);
                        rec.CustomInt9 = Convert.ToInt32(dcdi[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP DCID -->> CustomInt9 : " + rec.CustomInt9);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("Start") && line1.Contains("MID"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Start MID -->> is STARTED : " + line1);
                        string[] sec = subLine0[2].Split(' ');
                        rec.SourceName = Convert.ToString(sec[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Start MID -->> Sourcename : " + rec.SourceName);
                        string[] mid = line1.Split(' ');
                        rec.EventType = Convert.ToString(mid[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Start MID -->> EventType : " + rec.EventType);
                        rec.CustomInt9 = Convert.ToInt32(mid[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Start MID -->> CustomInt9 : " + rec.CustomInt9);
                        rec.CustomInt10 = Convert.ToInt32(mid[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Start MID -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("From") && line1.Contains("ICID"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> is STARTED : " + line1);
                        string[] sec1 = subLine0[2].Split(' ');
                        rec.SourceName = Convert.ToString(sec1[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> Sourcename : " + rec.SourceName);
                        string[] frm = line1.Split(' ');
                        rec.EventType = Convert.ToString(frm[5].Trim(':'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> EventType : " + rec.EventType);
                        rec.CustomStr3 = Convert.ToString(subLine0[7].Substring(1).TrimStart('<').TrimEnd('>'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> CustomStr3 : " + rec.CustomStr3);
                        rec.CustomInt9 = Convert.ToInt32(frm[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> CustomInt9 : " + rec.CustomInt9);
                        rec.CustomInt10 = Convert.ToInt32(frm[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID From ICID -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1) + ":" + Convert.ToString(subLine0[7]);
                    }

                    else if (line1.Contains("MID") && line1.Contains("ICID") && line1.Contains("To"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> is STARTED : " + line1);
                        string[] sec2 = subLine0[2].Split(' ');
                        rec.SourceName = Convert.ToString(sec2[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> Sourcename : " + rec.SourceName);
                        string[] rid = line1.Split(' ');
                        rec.EventType = Convert.ToString(rid[7].Trim(':'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> EventType : " + rec.EventType);
                        rec.CustomStr4 = Convert.ToString(subLine0[7].Substring(1).TrimStart('<').TrimEnd('>'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> CustomStr4 : " + rec.CustomStr4);
                        rec.CustomInt1 = Convert.ToInt32(rid[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> CustomInt1 : " + rec.CustomInt1);
                        rec.CustomInt9 = Convert.ToInt32(rid[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> CustomInt9 : " + rec.CustomInt9);
                        rec.CustomInt10 = Convert.ToInt32(rid[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID ICID To -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1) + ":" + Convert.ToString(subLine0[7]);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Message-ID"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Message-ID -->> is STARTED : " + line1);
                        string[] msg = line1.Split(' ');
                        rec.EventType = Convert.ToString(msg[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Message-ID -->> EventType : " + rec.EventType);
                        rec.CustomStr5 = Convert.ToString(msg[4].Substring(1).TrimStart('<').TrimEnd('>'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Message-ID -->> CustomStr5 : " + rec.CustomStr5);
                        rec.CustomInt10 = Convert.ToInt32(msg[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Message-ID -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Subject"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Subject -->> is STARTED : " + line1);
                        string[] subject = line1.Split(' ');
                        rec.EventType = Convert.ToString(subject[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Subject -->> EventType : " + rec.EventType);
                        rec.CustomStr5 = Convert.ToString(line1.Substring(21));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Subject -->> CustomStr5 : " + rec.CustomStr5);
                        rec.CustomInt10 = Convert.ToInt32(subject[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Subject -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("bytes from"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID bytes from -->> is STARTED : " + line1);
                        string[] bytfrm = line1.Split(' ');
                        rec.EventType = Convert.ToString(bytfrm[5]) + " " + Convert.ToString(bytfrm[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID bytes from -->> EventType : " + rec.EventType);
                        rec.CustomStr3 = Convert.ToString(bytfrm[7].TrimStart('<').TrimEnd('>'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID bytes from -->> CustomStr3 : " + rec.CustomStr3);
                        rec.CustomInt8 = Convert.ToInt32(bytfrm[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID bytes from -->> CustomInt8 : " + rec.CustomInt8);
                        rec.CustomInt10 = Convert.ToInt32(bytfrm[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID bytes from -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("interim") && !(line1.Contains("AV") || line1.Contains("av")))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim : -->> is STARTED : " + line1);
                        string[] intrm = line1.Split(' ');
                        rec.EventType = Convert.ToString(intrm[3]) + " " + Convert.ToString(intrm[4]) + " " + Convert.ToString(intrm[5]) + " " + Convert.ToString(intrm[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim : -->> EventType : " + rec.EventType);
                        rec.CustomStr6 = Convert.ToString(subLine0[7]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim : -->> CustomStr6 : " + rec.CustomStr6);
                        rec.CustomInt10 = Convert.ToInt32(intrm[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim : -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("using engine") && !line1.Contains("interim"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID using engine -->> is STARTED : " + line1);
                        string[] engine = line1.Split(' ');
                        rec.EventType = Convert.ToString(engine[3]) + " " + Convert.ToString(engine[4].Trim(':'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID using engine -->> EventType : " + rec.EventType);
                        rec.CustomStr6 = Convert.ToString(subLine0[7]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID using engine -->> CustomStr6 : " + rec.CustomStr6);
                        rec.CustomInt10 = Convert.ToInt32(engine[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID using engine -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1) + ":" + Convert.ToString(subLine0[7]);
                    }

                    else if (line1.Contains("MID") && line1.Contains("interim") && (line1.Contains("AV") || line1.Contains("av")))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim -->> is STARTED : " + line1);
                        string[] midd = line1.Split(' ');
                        rec.EventType = Convert.ToString(midd[3]) + " " + Convert.ToString(midd[4]) + " " + Convert.ToString(midd[5]) + " " + Convert.ToString(midd[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim -->> EventType : " + rec.EventType);
                        rec.CustomStr7 = Convert.ToString(midd[7]) + " " + Convert.ToString(midd[8]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim -->> CustomStr7 : " + rec.CustomStr7);
                        rec.CustomInt10 = Convert.ToInt32(midd[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID interim -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("antivirus"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID antivirus -->> is STARTED : " + line1);
                        string[] anti = line1.Split(' ');
                        rec.EventType = Convert.ToString(anti[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID antivirus -->> EventType : " + rec.EventType);
                        rec.CustomStr7 = Convert.ToString(anti[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID antivirus -->> CustomStr7 : " + rec.CustomStr7);
                        rec.CustomInt10 = Convert.ToInt32(anti[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID antivirus -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Response") && line1.Contains("@"))
                    {
                        string k = "''";
                        char[] kk = k.ToCharArray();
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response @ -->> is STARTED : " + line1);
                        string[] resp = line1.Split(' ');
                        rec.EventType = Convert.ToString(resp[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response @ -->> EventType : " + rec.EventType);
                        rec.CustomStr5 = Convert.ToString(resp[7].TrimStart('<').TrimEnd('>'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response @ -->> CustomStr5 : " + rec.CustomStr5);
                        rec.CustomInt10 = Convert.ToInt32(resp[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response @ -->> CustomInt10 : " + rec.CustomInt10);
                        string[] s = line1.Split(kk);
                        rec.CustomStr8 = Convert.ToString(s[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response @ -->> CustomStr8 : " + rec.CustomStr8);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Response") && !line1.Contains("@") && subLine0.Length == 7)
                    {
                        string k = "''";
                        char[] kk = k.ToCharArray();
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length=7 -->> is STARTED : " + line1);
                        string[] respp = line1.Split(' ');
                        rec.EventType = Convert.ToString(respp[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length=7  -->> EventType : " + rec.EventType);
                        rec.CustomInt10 = Convert.ToInt32(respp[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length=7  -->> CustomInt10 : " + rec.CustomInt10);
                        string[] s = line1.Split(kk);
                        rec.CustomStr8 = Convert.ToString(s[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length=7  -->> CustomStr8 : " + rec.CustomStr8);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Response") && !line1.Contains("@") && subLine0.Length != 7 && subLine0.Length != 8)
                    {
                        string k = "''";
                        char[] kk = k.ToCharArray();
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length != 7 && 8 -->> is STARTED : " + line1);
                        string[] respp = line1.Split(' ');
                        rec.EventType = Convert.ToString(respp[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length != 7 && 8 -->> EventType : " + rec.EventType);
                        rec.CustomInt10 = Convert.ToInt32(respp[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length != 7 && 8 -->> CustomInt10 : " + rec.CustomInt10);
                        string[] s = line1.Split(kk);
                        rec.CustomStr8 = Convert.ToString(s[1]) + " " + Convert.ToString(subLine0[7]) + " " + Convert.ToString(subLine0[8]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length != 7 && 8 -->> CustomStr8 : " + rec.CustomStr8);
                        rec.Description = Convert.ToString(line1) + " " + Convert.ToString(subLine0[7]) + " " + Convert.ToString(subLine0[8]);
                    }

                    else if (line1.Contains("MID") && line1.Contains("Response") && !line1.Contains("@") && subLine0.Length == 8)
                    {
                        string k = "''";
                        char[] kk = k.ToCharArray();
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length = 8 -->> is STARTED : " + line1);
                        string[] respp = line1.Split(' ');
                        rec.EventType = Convert.ToString(respp[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length = 8 -->> EventType : " + rec.EventType);
                        rec.CustomInt10 = Convert.ToInt32(respp[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length = 8 -->> CustomInt10 : " + rec.CustomInt10);
                        string[] s = line1.Split(kk);
                        rec.CustomStr8 = Convert.ToString(s[1]) + " " + Convert.ToString(subLine0[7]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID Response Length = 8 -->> CustomStr8 : " + rec.CustomStr8);
                        rec.Description = Convert.ToString(line1) + " " + Convert.ToString(subLine0[7]);
                    }

                    else if (line1.Contains("New SMTP ICID") && line1.Contains("interface Outgoing") && line1.Contains("reverse dns host"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> is STARTED : " + line1);
                        string[] facee = line1.Split(' ');
                        rec.EventCategory = Convert.ToString(facee[1]) + " " + Convert.ToString(facee[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> EventCategory : " + rec.EventCategory);
                        rec.EventType = Convert.ToString(facee[5]) + " " + Convert.ToString(facee[6]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> EventType : " + rec.EventType);
                        rec.SourceName = Convert.ToString(facee[13]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> SourceName : " + rec.SourceName);
                        rec.CustomStr3 = Convert.ToString(facee[9]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> CustomStr3 : " + rec.CustomStr3);
                        rec.CustomStr4 = Convert.ToString(facee[7].TrimStart('(').TrimEnd(')'));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> CustomStr4 : " + rec.CustomStr4);
                        rec.CustomInt9 = Convert.ToInt32(facee[4]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " New SMTP ICID interface Outgoing reverse dns host -->> CustomInt9 : " + rec.CustomInt9);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("ICID") && (line1.Contains("SBRS") || line1.Contains("sbrs")) && subLine0.Length == 8)
                    {
                        string[] sec4 = subLine0[2].Split(' ');
                        rec.SourceName = Convert.ToString(sec4[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID [SBRS] -->> is STARTED : " + line1);
                        string[] sbrs = line1.Split(' ');
                        rec.EventType = Convert.ToString(sbrs[3]) + " " + Convert.ToString(sbrs[4]) + " " + Convert.ToString(sbrs[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID [SBRS] -->> EventType : " + rec.EventType);
                        rec.CustomStr1 = Convert.ToString(subLine0[7].Substring(6));
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID [SBRS] -->> Customstr1 : " + rec.CustomStr1);
                        rec.CustomInt9 = Convert.ToInt32(sbrs[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID [SBRS] -->> CustomInt9 : " + rec.CustomInt9);
                        rec.Description = Convert.ToString(line1) + ":" + Convert.ToString(subLine0[7]);
                    }

                    else if (line1.Contains("ICID") && (line1.Contains("SBRS") || line1.Contains("sbrs")) && subLine0.Length == 7)
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID SBRS -->> is STARTED : " + line1);
                        string[] sbrss = line1.Split(' ');
                        rec.EventType = Convert.ToString(sbrss[3]) + " " + Convert.ToString(sbrss[4]) + " " + Convert.ToString(sbrss[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID SBRS -->> EventType : " + rec.EventType);
                        if (sbrss.Length == 10)
                        {
                            rec.CustomStr1 = Convert.ToString(sbrss[8]) + " " + Convert.ToString(sbrss[9]);
                        }
                        else
                        {
                            rec.CustomStr1 = Convert.ToString(sbrss[8]) + " " + Convert.ToString(sbrss[9]) + " " + Convert.ToString(sbrss[10]);
                        }
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID SBRS -->> CustomStr1 : " + rec.CustomStr1);
                        rec.CustomInt9 = Convert.ToInt32(sbrss[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " ICID SBRS -->> CustomInt9 : " + rec.CustomInt9);
                        rec.Description = Convert.ToString(line1);
                    }

                    else if (line1.Contains("MID") && line1.Contains("queued"))
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID queued -->> is STARTED : " + line1);
                        string[] que = line1.Split(' ');
                        rec.EventType = Convert.ToString(que[3]) + " " + Convert.ToString(que[4]) + " " + Convert.ToString(que[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID queued -->> EventType : " + rec.EventType);
                        rec.CustomInt10 = Convert.ToInt32(que[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " MID queued -->> CustomInt10 : " + rec.CustomInt10);
                        rec.Description = Convert.ToString(line1);
                    }

                    else
                    {
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " Different Line -->> is STARTED : " + line1);
                        rec.Description = Convert.ToString(line1);
                    }
                }

                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
            }
        }

        /// <summary>
        /// Control giving line will reading or skipping
        /// </summary>
        /// <param name="line">Will checking line</param>
        /// <returns>Returns line will reading or skipping</returns>
        private Boolean IsSkipKeyWord(String line)
        {
            String[] _skipKeyWords = line.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is STARTED ");
                if (_skipKeyWords.Length > 1)
                {
                    foreach (String item in _skipKeyWords)
                    {
                        if (line.StartsWith(item))
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned True ");
                            return false;
                        }
                    }
                }
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned False");
                return true;
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " IsSkipKeyWord() -->> An error occured" + ex.ToString());
                return false;
            }
        }
    }
}
