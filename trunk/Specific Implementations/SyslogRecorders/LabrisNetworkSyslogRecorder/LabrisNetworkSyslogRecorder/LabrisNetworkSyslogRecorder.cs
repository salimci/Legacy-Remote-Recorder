//Name: LabrisNetworkSyslogRecorder
//Writer: Selahattin ÜNALAN
//Date: 13/01/2012


// Recorder Name : LabrisNetworkSyslogRecorder
// Writer : Onur SARIKAYA
// Date : 09.04.2012
using System;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace LabrisNetworkSyslogRecorder
{
    public class LabrisNetworkSyslogRecorder : CustomBase
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

        public LabrisNetworkSyslogRecorder()
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

                _log.Log(LogType.FILE, LogLevel.INFORM, "Init() -->> Finish initializing LabrisNetworkSyslogRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager LabrisNetworkSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager LabrisNetworkSyslogRecorder", er.ToString(), EventLogEntryType.Error);
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
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "LabrisNetworkSyslogRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager LabrisNetworkSyslogRecorder Read Registry", ex.ToString(), EventLogEntryType.Error);
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
                rec.LogName = "LabrisNetworkSyslogRecorder";
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
                    String[] subLine0 = line.Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (subLine0[0].Contains("ACCEPTIN") && subLine0.Length == 17)
                    {
                        try
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN -->> is STARTED ");
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');
                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN Sourcename : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN ComputerName : " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception ex)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 1");
                        }
                    }


                    else if (subLine0[0].Contains("ACCEPTIN") && subLine0.Length == 14)
                    {
                        try
                        {
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');
                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN Sourcename : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = subLine0[10].Split(' ')[0].ToString(); //Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_11[0]);//Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_12[0]); //Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = "";//Convert.ToString(subLine0_3[0]);;
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN ComputerName : " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_3[0]); //Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_4[0]); //Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_5[0]);// Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt1 : " + rec.CustomInt1);
                            rec.CustomInt5 = Convert.ToInt32(subLine0_13[0]);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 2");
                        }
                    }

                    else if (subLine0[0].Contains("ACCEPTIN") && subLine0.Length == 16)
                    {
                        try
                        {
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');
                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN Sourcename : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0[2].Split(' ')[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = subLine0[10].Split(' ')[0].ToString(); //Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_11[0]);//Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_12[0]); //Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = "";//Convert.ToString(subLine0_3[0]);;
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN ComputerName : " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_3[0]); //Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_4[0]); //Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_5[0]);// Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for ACCEPTIN CustomInt1 : " + rec.CustomInt1);
                            rec.CustomInt5 = Convert.ToInt32(subLine0_13[0]);
                        }
                        catch (Exception ex)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 3");
                        }
                    }

                    else if (subLine0[0].Contains("DROP") && !subLine0[0].Contains("_DROP") && line.Contains("MAC"))
                    {
                        try
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP -->> is STARTED ");
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_10 = subLine0[10].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP SourceName : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP ComputerName: " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception ex)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 4");
                        }
                    }

                    else if (subLine0[0].Contains("DROP") && !subLine0[0].Contains("_DROP") && !line.Contains("MAC"))
                    {
                        try
                        {
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_10 = subLine0[10].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr1 no MAC : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP SourceName  no MAC: " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventCategory no MAC : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr2 no MAC : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_10[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventType no MAC : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt3 no MAC : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt4 no MAC : " + rec.CustomInt4);
                            //string ComputerName = Convert.ToString(subLine0_3[0]);
                            rec.CustomStr3 = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr3 no MAC : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr4 no MAC : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt1 no MAC : " + rec.CustomInt1);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 5");
                        }
                    }

                    else if (subLine0[0].Contains("_DROP"))
                    {
                        try
                        {
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subLine0_0_5_2[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP SourceName : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP ComputerName : " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DROP CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 6");
                        }
                    }

                    else if (subLine0[0].Contains("DENYIN") && subLine0.Length == 16)
                    {
                        try
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN -->> is STARTED ");
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_10 = subLine0[10].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN SourceName : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_10[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomInt4 : " + rec.CustomInt4);
                            //string ComputerName = Convert.ToString(subLine0_3[0]);
                            rec.CustomStr3 = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DENYIN CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 7");
                        }
                    }

                    else if (subLine0[0].Contains("DENYIN") && subLine0.Length != 16)
                    {
                        try
                        {
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_10 = subLine0[10].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN SourceName : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN ComputerName : " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for _DENYIN CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 8");
                        }
                    }
                    else if (subLine0.Length == 15 && line.Contains("MAC"))
                    {
                        try
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP -->> is STARTED ");
                            String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_1 = subLine0[1].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_3 = subLine0[3].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_4 = subLine0[4].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_5 = subLine0[5].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_6 = subLine0[6].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_10 = subLine0[10].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_11 = subLine0[11].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_12 = subLine0[12].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_13 = subLine0[13].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5 = subLine0_0[5].Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subLine0_0_5_2 = subLine0_0_5[2].Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            String[] subline0_0_5_1 = subLine0_0_5[2].Split('-');

                            rec.CustomStr1 = Convert.ToString(subline0_0_5_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr1 : " + rec.CustomStr1);
                            rec.SourceName = Convert.ToString(subLine0_0[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP SourceName : " + rec.SourceName);
                            rec.EventCategory = Convert.ToString(subLine0_0_5_2[3]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventCategory : " + rec.EventCategory);
                            rec.CustomStr2 = Convert.ToString(subLine0_1[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr2 : " + rec.CustomStr2);
                            rec.EventType = Convert.ToString(subLine0_11[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP EventType : " + rec.EventType);
                            rec.CustomInt3 = Convert.ToInt32(subLine0_12[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt3 : " + rec.CustomInt3);
                            rec.CustomInt4 = Convert.ToInt32(subLine0_13[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt4 : " + rec.CustomInt4);
                            rec.ComputerName = Convert.ToString(subLine0_3[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP ComputerName: " + rec.ComputerName);
                            rec.CustomStr3 = Convert.ToString(subLine0_4[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr3 : " + rec.CustomStr3);
                            rec.CustomStr4 = Convert.ToString(subLine0_5[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomStr4 : " + rec.CustomStr4);
                            rec.CustomInt1 = Convert.ToInt32(subLine0_6[0]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() for DROP CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() ERROR -- 9");
                        }
                    }
                    else
                    {
                        rec.Description = line;
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
            String[] _skipKeyWords = line.Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is STARTED ");
                if (_skipKeyWords.Length > 1)
                {
                    foreach (String item in _skipKeyWords)
                    {
                        if (line.StartsWith(item))
                        {
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned False ");
                            return false;
                        }
                    }
                }
                _log.Log(LogType.FILE, LogLevel.DEBUG, " IsSkipKeyWord() -->> is successfully FINISHED. Returned True");
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
