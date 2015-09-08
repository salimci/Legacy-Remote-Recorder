//Name: LabrisAccessSyslogRecorder
//Writer: Selahattin ÜNALAN
//Date: 16/01/2012

using System;
using System.Security.Cryptography;
using Log;
using LogMgr;
using CustomTools;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

namespace LabrisAccessSyslogRecorder
{
    public class LabrisAccessSyslogRecorder : CustomBase
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

        public LabrisAccessSyslogRecorder()
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

                _log.Log(LogType.FILE, LogLevel.INFORM, "Init() -->> Finish initializing LabrisAccessSyslogRecorder Event");

            }
            catch (Exception er)
            {
                EventLog.WriteEntry("Security Manager LabrisAccessSyslogRecorder Init", er.ToString(), EventLogEntryType.Error);
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
                EventLog.WriteEntry("Security Manager LabrisAccessSyslogRecorder", er.ToString(), EventLogEntryType.Error);
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
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "LabrisAccessSyslogRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager LabrisAccessSyslogRecorder Read Registry", ex.ToString(), EventLogEntryType.Error);
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
                rec.LogName = "LabrisAccessSyslogRecorder";
                rec.Datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                rec.SourceName = args.Source;
                if (args.Message.Length > 899)
                {
                    rec.Description = args.Message.Substring(0, 899);
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
                    String[] subLine0 = line.Split(new Char[] { '	' }, StringSplitOptions.RemoveEmptyEntries);
                    if (subLine0.Length == 14)
                    {
                        String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        String[] subLine0_3 = subLine0[3].Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        var description = subLine0.Length > 3 ? Convert.ToString(subLine0[3]) : string.Empty;
                        if (description.Length > 899)
                        {
                            rec.Description = description.Substring(0, 899);
                            rec.CustomStr8 = description.Substring(900, description.Length);
                        }
                        else
                        {
                            rec.Description = description;
                        }
                       
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 Description : " + rec.Description);
                        String[] subLine0_4 = subLine0[4].Split(new Char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                        rec.UserName = Convert.ToString(subLine0[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 UserName : " + rec.UserName);

                        rec.CustomStr3 = Convert.ToString(subLine0[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomStr3 : " + rec.CustomStr3);

                        rec.EventType = Convert.ToString(subLine0[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 EventType : " + rec.EventType);

                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(subLine0[6]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt1 : " + rec.CustomInt1);

                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 14 CustomInt1 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt2 = Convert.ToInt32(subLine0[7]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt2 : " + rec.CustomInt2);

                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 14 CustomInt2 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(subLine0[8]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt3 : " + rec.CustomInt3);

                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 14 CustomInt3 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt4 = Convert.ToInt32(subLine0[9]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt4 : " + rec.CustomInt4);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 14 CustomInt4 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt5 = Convert.ToInt32(subLine0[13]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt5 : " + rec.CustomInt5);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 14 CustomInt5 Error : " + exception.Message);
                        }

                        rec.CustomStr4 = Convert.ToString(subLine0_3[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomStr4 : " + rec.CustomStr4);

                        rec.CustomStr5 = Convert.ToString(subLine0[10]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomStr5 : " + rec.CustomStr5);

                        rec.CustomStr6 = Convert.ToString(subLine0[11]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomStr6 : " + rec.CustomStr6);

                        rec.CustomStr7 = Convert.ToString(subLine0[12]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomStr7 : " + rec.CustomStr7);
                        
                        rec.ComputerName = Convert.ToString(subLine0_0[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 ComputerName : " + rec.ComputerName);

                        rec.EventCategory = Convert.ToString(subLine0_4[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 EventCategory : " + rec.EventCategory);

                        //rec.CustomStr1 = Convert.ToString(subLine0_4[1]);
                    }

                    if (subLine0.Length == 13)
                    {
                        String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        String[] subLine0_3 = subLine0[3].Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        var description = subLine0.Length > 3 ? Convert.ToString(subLine0[3]) : string.Empty;
                        if (description.Length > 899)
                        {
                            rec.Description = description.Substring(0, 899);
                            rec.CustomStr8 = description.Substring(900, description.Length);
                        }
                        else
                        {
                            rec.Description = description;
                        }
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 Description : " + rec.Description);

                        String[] subLine0_4 = subLine0[4].Split(new Char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                        rec.UserName = Convert.ToString(subLine0[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 UserName : " + rec.UserName);

                        rec.EventCategory = Convert.ToString(subLine0_4[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 EventCategory : " + rec.EventCategory);

                        rec.ComputerName = Convert.ToString(subLine0_0[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 ComputerName : " + rec.ComputerName);

                        rec.EventType = Convert.ToString(subLine0[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 EventType : " + rec.EventType);


                        rec.CustomStr1 = Convert.ToString(subLine0_4[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr1 : " + rec.CustomStr1);

                        rec.CustomStr3 = Convert.ToString(subLine0[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr3 : " + rec.CustomStr3);

                        rec.CustomStr4 = Convert.ToString(subLine0_3[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr4 : " + rec.CustomStr4);

                        rec.CustomStr5 = Convert.ToString(subLine0[10]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr5 : " + rec.CustomStr5);

                        rec.CustomStr6 = Convert.ToString(subLine0[11]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr6 : " + rec.CustomStr6);

                        rec.CustomStr7 = Convert.ToString(subLine0[12]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomStr7 : " + rec.CustomStr7);

                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(subLine0[6]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 13 CustomInt1 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt2 = Convert.ToInt32(subLine0[7]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomInt2 : " + rec.CustomInt2);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 13 CustomInt2 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt3 = Convert.ToInt32(subLine0[8]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomInt3 : " + rec.CustomInt3);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 13 CustomInt3 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt4 = Convert.ToInt32(subLine0[9]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomInt4 : " + rec.CustomInt4);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 13 CustomInt4 Error : " + exception.Message);
                        }
                        //rec.CustomInt5 = Convert.ToInt32(subLine0[13]);
                        //_log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt5 : " + rec.CustomInt5);
                    }

                    if (subLine0.Length == 15)
                    {
                        String[] subLine0_0 = subLine0[0].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        String[] subLine0_3 = subLine0[3].Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        var description = subLine0.Length > 3 ? Convert.ToString(subLine0[3]) : string.Empty;
                        if (description.Length > 899)
                        {
                            rec.Description = description.Substring(0, 899);
                            rec.CustomStr8 = description.Substring(900, description.Length);
                        }
                        else
                        {
                            rec.Description = description;
                        }
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 Description : " + rec.Description);

                        String[] subLine0_4 = subLine0[4].Split(new Char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                        rec.UserName = Convert.ToString(subLine0[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 UserName : " + rec.UserName);

                        rec.EventType = Convert.ToString(subLine0[5]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 EventType : " + rec.EventType);

                        rec.ComputerName = Convert.ToString(subLine0_0[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 ComputerName : " + rec.ComputerName);

                        rec.EventCategory = Convert.ToString(subLine0_4[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 EventCategory : " + rec.EventCategory);

                        rec.CustomStr1 = Convert.ToString(subLine0_4[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr1 : " + rec.CustomStr1);

                        rec.CustomStr3 = Convert.ToString(subLine0[2]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr3 : " + rec.CustomStr3);

                        rec.CustomStr4 = Convert.ToString(subLine0_3[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr4 : " + rec.CustomStr4);

                        rec.CustomStr5 = Convert.ToString(subLine0[10]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr5 : " + rec.CustomStr5);

                        rec.CustomStr6 = Convert.ToString(subLine0[11]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr6 : " + rec.CustomStr6);

                        rec.CustomStr7 = Convert.ToString(subLine0[12]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomStr7 : " + rec.CustomStr7);

                        try
                        {
                            rec.CustomInt1 = Convert.ToInt32(subLine0[6]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomInt1 : " + rec.CustomInt1);
                           
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 15 CustomInt1 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt2 = Convert.ToInt32(subLine0[7]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomInt2 : " + rec.CustomInt2);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 15 CustomInt2 Error : " + exception.Message);
                        }

                        try
                        {
                            rec.CustomInt4 = Convert.ToInt32(subLine0[9]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 15 CustomInt4 : " + rec.CustomInt4);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 15 CustomInt4 Error : " + exception.Message);
                        }

                        //rec.CustomInt3 = Convert.ToInt32(subLine0[8]);
                        //_log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 13 CustomInt3 : " + rec.CustomInt3);

                        //rec.CustomInt5 = Convert.ToInt32(subLine0[13]);
                        //_log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 14 CustomInt5 : " + rec.CustomInt5);
                    }

                    if (subLine0.Length == 5)
                    {
                        String[] subLine0_4 = subLine0[4].Split(new Char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                        String[] subLine0_3 = subLine0[4].Split(new Char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                        String[] ss = subLine0_3[3].Split('<');
                        var description = ss.Length > 0 ? Convert.ToString(ss[0]) : string.Empty;
                        if (description.Length > 899)
                        {
                            rec.Description = description.Substring(0, 899);
                            rec.CustomStr8 = description.Substring(900, description.Length);
                        }
                        else
                        {
                            rec.Description = description;
                        }
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 Description : " + rec.Description);

                        rec.ComputerName = Convert.ToString(subLine0[3]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 Computername : " + rec.ComputerName);

                        String[] dd = subLine0_3[1].Split('<');
                        rec.UserName = Convert.ToString(dd[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 Username : " + rec.UserName);

                        String[] ds = subLine0_3[2].Split('<');
                        rec.CustomStr3 = Convert.ToString(ds[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr3 : " + rec.CustomStr3);

                        rec.EventCategory = Convert.ToString(subLine0_4[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 EventCategory : " + rec.EventCategory);

                        String[] dss = subLine0_4[2].Split('<');
                        rec.CustomStr1 = Convert.ToString(dss[0]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr1 : " + rec.CustomStr1);

                        String[] dss1 = dss[1].Split('>');
                        rec.EventType = Convert.ToString(dss1[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 EventType : " + rec.EventType);

                        try
                        {
                            String[] ss1 = dss[2].Split('>');
                            rec.CustomInt1 = Convert.ToInt32(ss1[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomInt1 : " + rec.CustomInt1);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 5 CustomInt1 Error : " + exception.Message);
                        }

                        try
                        {
                            String[] ss2 = dss[3].Split('>');
                            rec.CustomInt2 = Convert.ToInt32(ss2[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomInt2 : " + rec.CustomInt2);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 5 CustomInt2 Error : " + exception.Message);
                        }

                        try
                        {
                            String[] ss3 = dss[5].Split('>');
                            rec.CustomInt3 = Convert.ToInt32(ss3[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomInt3 : " + rec.CustomInt3);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 5 CustomInt3 Error : " + exception.Message);
                        }

                        try
                        {
                            String[] ss4 = dss[6].Split('>');
                            rec.CustomInt4 = Convert.ToInt32(ss4[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomInt4 : " + rec.CustomInt4);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 5 CustomInt4 Error : " + exception.Message);
                        }

                        String[] ss5 = dss[7].Split('>');
                        rec.CustomStr5 = Convert.ToString(ss5[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr5 : " + rec.CustomStr5);
                       
                        String[] ss6 = dss[9].Split('>');
                        rec.CustomStr6 = Convert.ToString(ss6[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr6 : " + rec.CustomStr6);
                        
                        String[] ss7 = dss[10].Split('>');
                        rec.CustomStr7 = Convert.ToString(ss7[1]);
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr7 : " + rec.CustomStr7);

                        try
                        {
                            String[] ss8 = dss[11].Split('>');
                            rec.CustomInt5 = Convert.ToInt32(ss8[1]);
                            _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomInt5 : " + rec.CustomInt5);
                        }
                        catch (Exception exception)
                        {
                            _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() Lenght 5 CustomInt5 Error : " + exception.Message);
                        }

                        String[] ss9 = ss[0].Split('/');
                        rec.CustomStr4 = ss9[2];
                        _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() Lenght 5 CustomStr4 : " + rec.CustomStr4);
                    }
                    else
                    {
                        if (line.Length > 899)
                        {
                            rec.Description = line.Substring(0, 899);
                            rec.CustomStr8 = line.Substring(900, line.Length);
                        }
                        else
                        {
                            rec.Description = line;
                        }
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
            String[] _skipKeyWords = line.Split(new Char[] { '	' }, StringSplitOptions.RemoveEmptyEntries);
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
