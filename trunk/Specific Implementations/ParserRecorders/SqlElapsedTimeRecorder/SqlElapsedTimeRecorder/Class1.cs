/* Company : Natek Bilisim
 * Author  : ibrahim Luy
 * 
 * Created By Recorder Wizard
*/

using System;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Data.Common;

namespace SQLElapsedTimeRecorder
{
    public class SQLElapsedTimeRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private CLogger _log;

        private Int32 _traceLevel = 3, _timerInterval = 3000, _zone = 0, _id = 0;
        private uint _loggingInterval = 60000, _logSize = 1000000;
        private String _errLog, _location, _user, _password, _remoteHost = "";
        private Boolean _fromend = false;
        private String _virtualHost, _dal, _maxRecordSend = "100";

        private String _lastPosition;

        private String _recorderName = "SQLElapsedTimeRecorder";
        private Database.Provider _dbProvider = Database.Provider.SQLServer;

        public SQLElapsedTimeRecorder()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
        }

        public override void SetConfigData(
            Int32 identity, String location, String lastLine, String lastPosition,
            String lastFile, String lastKeywords, Boolean fromEnd, Int32 maxRecordSend, String user,
            String password, String remoteHost, Int32 sleepTime, Int32 traceLevel,
            String customVar1, Int32 customVar2, String virtualhost, String dal, Int32 Zone)
        {
            _id = identity;
            _location = location;  //Database Name
            _fromend = fromEnd;
            _maxRecordSend = maxRecordSend.ToString();
            _timerInterval = sleepTime;
            _user = user;
            _password = password;
            _remoteHost = remoteHost;
            _traceLevel = traceLevel;
            _virtualHost = virtualhost;
            _lastPosition = lastPosition;
            _zone = Zone;
            _dal = dal;
        }

        public override void Init()
        {
            try
            {
                Database.AddProviderToRegister(_dbProvider, _recorderName, _remoteHost, _location, _user, _password);

                timer1 = new System.Timers.Timer();
                timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.Timer1_Tick);
                timer1.Interval = _timerInterval;
                timer1.Enabled = true;

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
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager SQLElapsedTimeRecorder Init", ex.ToString(), EventLogEntryType.Error);
            }
        }

        private Boolean GetLogDir()
        {
            RegistryKey rk = null;
            DateTime dateTime = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder");
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "SQLElapsedTimeRecorder" + _id + ".log";
                rk.Close();
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager SQLServer Recorder Read Registry", ex.ToString(), EventLogEntryType.Error);
                return false;
            }
            finally
            {
                if (rk != null)
                    GC.SuppressFinalize(rk);
            }
        }

        public Boolean InitializeLogger()
        {
            try
            {
                _log = new CLogger();

                switch (_traceLevel)
                {
                    case 0:
                        _log.SetLogLevel(LogLevel.NONE);
                        break;
                    case 1:
                        _log.SetLogLevel(LogLevel.INFORM);
                        break;
                    case 2:
                        _log.SetLogLevel(LogLevel.WARN);
                        break;
                    case 3:
                        _log.SetLogLevel(LogLevel.ERROR);
                        break;
                    case 4:
                        _log.SetLogLevel(LogLevel.DEBUG);
                        break;
                }

                _log.SetLogFile(_errLog);
                _log.SetTimerInterval(LogType.FILE, _loggingInterval);
                _log.SetLogFileSize(_logSize);

                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Security Manager SQLServer Recorder", ex.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        private void Timer1_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.INFORM, " Timer1_Tick() -->> is STARTED");
                timer1.Enabled = false;
                SaveData();

            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " Timer1_Tick() -->> An error occurred : " + ex.ToString());
            }
            finally
            {
                timer1.Enabled = true;
                _log.Log(LogType.FILE, LogLevel.INFORM, " Timer1_Tick() -->> is FINISHED");
            }
        }

        private void SaveData()
        {
            IDataReader dataReader;
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " SaveData() -->> is STARTED");

                Rec rec = new Rec();
                dataReader = GetDataOnDB();

                while (dataReader.Read())
                {
                    String line = "";
                    String tempPart;

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        tempPart = dataReader[i].ToString();
                        if (String.IsNullOrEmpty(tempPart))
                        {
                            tempPart = "NULL";
                        }
                        line += tempPart + "-#-#-";
                    }

                    rec.LogName = "SQLElapsedTimeRecorder";
                    _lastPosition = dataReader["view_execution_time"].ToString();
                    CoderParse(line, ref rec);

                    CustomServiceBase customServiceBase = base.GetInstanceService("Security Manager Remote Recorder");
                    customServiceBase.SetData(_dal, _virtualHost, rec);
                    customServiceBase.SetReg(_id, _lastPosition, "", "", "", rec.Datetime);

                }
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " SaveData() -->> An error occurred : " + ex.ToString());
            }
            finally
            {
            }
        }

        private IDataReader GetDataOnDB()
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " GetDataOnDB() -->> is STARTED");
                String query = "";
                if (_lastPosition == "0")
                {
                    query = "SELECT  TOP " + _maxRecordSend + "   view_execution_time,login_name,database_name,command,statement_text,start_time,total_elapsed_time,elepsed_time_sec FROM QUERY_ELEPSED_TIME where 1=1  ORDER BY QUERY_ELEPSED_TIME.view_execution_time";
                }
                else
                {
                    ConvertToDateTime();
                    query = "SELECT  TOP " + _maxRecordSend + "   view_execution_time,login_name,database_name,command,statement_text,start_time,total_elapsed_time,elepsed_time_sec FROM QUERY_ELEPSED_TIME where 1=1 AND QUERY_ELEPSED_TIME.view_execution_time>Convert(DateTime,'" + _lastPosition + "',102)  ORDER BY QUERY_ELEPSED_TIME.view_execution_time";
                }
                _log.Log(LogType.FILE, LogLevel.DEBUG, " GetDataOnDB() -->> Query will run. Query is : " + query);

                DbCommand dbCommand = null;
                Database.Fast = false;
                IDataReader dataReader = Database.ExecuteReader(_recorderName, query, CommandBehavior.CloseConnection, out dbCommand);
                _log.Log(LogType.FILE, LogLevel.DEBUG, " GetDataOnDB() -->> is successfully FINISHED");
                return dataReader;
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " GetDataOnDB() -->> An error occurred : " + ex.ToString());
                return null;
            }
        }

        private Boolean ConvertToDateTime()
        {
            try
            {
                DateTime dateTime = Convert.ToDateTime(_lastPosition);
                _lastPosition = String.Format("{0:yyyy/MM/dd HH:mm:ss}", dateTime);
                return true;

            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " ConvertToDateTime() -->> An error occurred : " + ex.ToString());
                return false;
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }

        private void CoderParse(String line, ref CustomTools.CustomBase.Rec rec)
        {
            try
            {
                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is STARTED ");
                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> line is : " + line);

                String[] subLine0 = line.Split(new String[] { "-#-#-" }, StringSplitOptions.RemoveEmptyEntries);
                rec.Datetime = Convert.ToString(subLine0[0]);
                rec.UserName = Convert.ToString(subLine0[1]);
                rec.CustomStr1 = Convert.ToString(subLine0[2]);
                rec.CustomStr2 = Convert.ToString(subLine0[3]);
                rec.Description = Convert.ToString(subLine0[4]);
                rec.CustomStr3 = Convert.ToString(subLine0[5]);
                rec.CustomStr4 = Convert.ToString(subLine0[6]);
                rec.CustomStr5 = Convert.ToString(subLine0[7]);


                if (rec.Datetime != null)
                {
                    DateTime dateTime = Convert.ToDateTime(rec.Datetime);
                    rec.Datetime = String.Format("{0:yyyy/MM/dd HH:mm:ss}", dateTime);
                }
                _log.Log(LogType.FILE, LogLevel.DEBUG, " CoderParse() -->> is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                _log.Log(LogType.FILE, LogLevel.ERROR, " CoderParse() -->> An error occurred. " + ex.ToString());
            }
        }
    }
}
