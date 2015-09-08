//Name: McaffeeIpsV6SyslogRecorder
//Writer: Selahattin ÜNALAN
//Date: 12/03/2012

using System;
using Log;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using LogMgr;

namespace McaffeeIpsV6SyslogRecorder
{
    public class McaffeeIpsV6SyslogRecorder : CustomBase
    {

        private CLogger _log;
        private System.Net.Sockets.ProtocolType _protocolType;
        private Syslog _sysLog = null;

        private String[] _skipKeyWords = new String[] { "McaffeeIpsV6SyslogRecorder" };
        private String _recorderName = "";

        private uint logging_interval = 60000, _logSize = 1000000;
        private Int32 _traceLevel = 3, _syslogPort = 514, _zone = 0;
        private String _errLog, _remoteHost = "localhost";

        private Int32 _id = 0;
        private String _virtualHost, _dal;

        public McaffeeIpsV6SyslogRecorder()
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

                _log.Log(LogType.FILE, LogLevel.INFORM, "Init() -->> Finish initializing McaffeeIpsV6SyslogRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager McaffeeIpsV6SyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager McaffeeIpsV6SyslogRecorder", er.ToString(), EventLogEntryType.Error);
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
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "McaffeeIpsV6SyslogRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager McaffeeIpsV6SyslogRecorder Read Registry", ex.ToString(), EventLogEntryType.Error);
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
                rec.LogName = "McaffeeIpsV6SyslogRecorder";
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

                if (!IsSkipKeyWord(line))
                {
                    String[] subLine0 = line.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    String[] subLine0_0 = subLine0[0].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    String[] subLine0_1 = subLine0[1].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    string[] ss = subLine0_0[0].Split('\t');
                    string[] tarih = ss[0].Split('-');
                    string date = tarih[0] + "/" + tarih[1] + "/" + tarih[2] + " " + ss[1];
                    rec.Datetime = Convert.ToDateTime(date, CultureInfo.InvariantCulture).ToString("yyyy/MM/dd HH:mm:ss");
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Datetime : " + rec.Datetime);
                    rec.SourceName = ss[2];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> SourceName : " + rec.SourceName);
                    rec.ComputerName = ss[3];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> ComputerName : " + rec.ComputerName);
                    string[] sd = subLine0[8].Split('<');
                    rec.CustomStr6 = sd[0];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr6 : " + rec.CustomStr6);
                    rec.Description = subLine0[1];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> Description : " + rec.Description);
                    rec.CustomInt4 = Convert.ToInt32(subLine0[7]);
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt4 : " + rec.CustomInt4);
                    rec.CustomInt3 = Convert.ToInt32(subLine0[5]);
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomInt3 : " + rec.CustomInt3);
                    rec.EventCategory = subLine0[3];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> EventCategory : " + rec.EventCategory);
                    rec.CustomStr3 = subLine0[4];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr3 : " + rec.CustomStr3);
                    rec.CustomStr1 = subLine0[6];
                    _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> CustomStr1 : " + rec.CustomStr1);

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
