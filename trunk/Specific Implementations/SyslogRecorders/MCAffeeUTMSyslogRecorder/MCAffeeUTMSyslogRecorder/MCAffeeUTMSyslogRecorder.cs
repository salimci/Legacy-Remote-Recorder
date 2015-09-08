/* Company : Natek Bilisim
 * Author  : Onur Sarýkaya
 * Date    : 26.04.2012 
*/

using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net.Sockets;
using Parser;


namespace MCAffeeUTMSyslogRecorder
{
    public class MCAffeeUTMSyslogRecorder : CustomBase
    {
        private CLogger _log;
        private System.Net.Sockets.ProtocolType _protocolType;
        private Syslog _sysLog = null;

        private String[] _skipKeyWords = null;//SkipKeyWords
        private String _recorderName = "";//Coder Recorder Name

        private uint logging_interval = 60000, _logSize = 1000000;
        private Int32 _traceLevel = 3, _syslogPort = 514, _zone = 0;
        private String _errLog, _remoteHost = "localhost";

        private Int32 _id = 0;
        private String _virtualHost, _dal;

        public MCAffeeUTMSyslogRecorder()
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

                _log.Log(LogType.FILE, LogLevel.INFORM, "Init() -->> Finish initializing MCAffeeUTMSyslogRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager MCAffeeUTMSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager MCAffeeUTMSyslogRecorder", er.ToString(), EventLogEntryType.Error);
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
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "MCAffeeUTMSyslogRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager MCAffeeUTMSyslogRecorder Read Registry", ex.ToString(), EventLogEntryType.Error);
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
                rec.LogName = "MCAffeeUTMSyslogRecorder";
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

                //if (!IsSkipKeyWord(line))
                //{
                    String[] subLine0 = line.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < subLine0.Length; i++)
                    {
                        if (subLine0[i].Contains("fac"))
                        {
                            rec.CustomStr10 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("area"))
                        {
                            rec.SourceName = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("type"))
                        {
                            rec.EventType = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("hostname"))
                        {
                            rec.ComputerName = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("app_risk"))
                        {
                            rec.CustomStr5= StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("app_categories"))
                        {
                            rec.EventCategory = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("srcip"))
                        {
                            rec.CustomStr3 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("srcport"))
                        {
                            rec.CustomInt3 = Convert.ToInt32(StringParse(subLine0[i]));
                        }

                        if (subLine0[i].Contains("dstip"))
                        {
                            rec.CustomStr4 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("dstport"))
                        {
                            rec.CustomInt4 = Convert.ToInt32(StringParse(subLine0[i]));
                        }

                        if (subLine0[i].Contains("protocol"))
                        {
                            rec.CustomInt1 = Convert.ToInt32(StringParse(subLine0[i]));
                        }

                        if (subLine0[i].Contains("dest_geo"))
                        {
                            rec.CustomStr3 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("src_geo"))
                        {
                            rec.CustomStr2 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("bytes_written_to_cliend"))
                        {
                            rec.CustomInt9 = Convert.ToInt32(StringParse(subLine0[i]));
                        }

                        if (subLine0[i].Contains("bytes_written_to_server"))
                        {
                            rec.CustomInt10 = Convert.ToInt32(StringParse(subLine0[i]));
                        }

                        if (subLine0[i].Contains("rule_name"))
                        {
                            rec.UserName = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("application"))
                        {
                            rec.CustomStr6 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("information"))
                        {
                            rec.CustomStr7 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("event"))
                        {
                            rec.CustomStr8 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("alert_type"))
                        {
                            rec.CustomStr9 = StringParse(subLine0[i]);
                        }

                        if (subLine0[i].Contains("reason"))
                        {
                            rec.CustomStr1 = StringParse(subLine0[i]);
                        }

                        if (line.Length>900)
                        {
                            rec.Description = line.Substring(0, 899);
                        }
                        else
                        {
                            rec.Description = line;    
                        }
                    //}
                }

                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
            }
        }

        public string StringParse(string value)
        {
            return value.Split('=')[1].Trim();
        } // StringParse

       

        /// <summary>
        /// Control giving line will reading or skipping
        /// </summary>
        /// <param name="line">Will checking line</param>
        /// <returns>Returns line will reading or skipping</returns>
        private Boolean IsSkipKeyWord(String line)
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is STARTED ");
                if (_skipKeyWords.Length > 0)
                {
                    foreach (String item in _skipKeyWords)
                    {
                        if (line.StartsWith(item))
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned True ");
                            return true;
                        }
                    }
                }
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned False");
                return false;
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " IsSkipKeyWord() -->> An error occured" + ex.ToString());
                return false;
            }
        }
    }
}
