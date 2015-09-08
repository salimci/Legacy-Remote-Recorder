using System;
using System.Data.SqlClient;
using CustomTools;
using Log;
using DAL;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Data.Common;

namespace AOCEBYSRecorder
{
    public class AOCEBYSRecorder : CustomBase
    {
        private System.Timers.Timer timer1;
        private CLogger Log;

        private Int32 _traceLevel = 3, _timerInterval = 3000, _zone = 0, _id = 0;
        private uint _loggingInterval = 60000, _logSize = 1000000;
        private String _errLog, _location, _user, _password, _remoteHost = "", _dataBase, _customQuery, _IdColumnName;
        private Boolean _fromend = false;
        private String _virtualHost, _dal, _maxRecordSend = "100";

        private String _lastPosition;
        //
        private String _recorderName = "";
        private Database.Provider _dbProvider = Database.Provider.SQLServer;
        private string dateFormat = "yyyy-MM-dd HH:mm:ss";

        public AOCEBYSRecorder()
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
            _location = location;
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
            _dataBase = location;
            _customQuery = lastLine;
            _recorderName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            _IdColumnName = customVar1;
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
                        Log.Log(LogType.FILE, LogLevel.INFORM, " Init() -->> Start creating DAL");
                    }
                    else
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " Init() -->> An error occurred  : ");
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
                EventLog.WriteEntry("Security Manager AOCEBYSRecorder Init", ex.ToString(), EventLogEntryType.Error);
            }

        }

        private Boolean GetLogDir()
        {
            RegistryKey rk = null;
            DateTime dateTime = DateTime.Now;
            try
            {
                rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Natek\Security Manager\Remote Recorder");
                _errLog = rk.GetValue("Home Directory").ToString() + @"log\" + "AOCEBYSRecorder" + _id + ".log";
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
                Log = new CLogger();

                switch (_traceLevel)
                {
                    case 0:
                        Log.SetLogLevel(LogLevel.NONE);
                        break;
                    case 1:
                        Log.SetLogLevel(LogLevel.INFORM);
                        break;
                    case 2:
                        Log.SetLogLevel(LogLevel.WARN);
                        break;
                    case 3:
                        Log.SetLogLevel(LogLevel.ERROR);
                        break;
                    case 4:
                        Log.SetLogLevel(LogLevel.INFORM);
                        break;
                }

                Log.SetLogFile(_errLog);
                Log.SetTimerInterval(LogType.FILE, _loggingInterval);
                Log.SetLogFileSize(_logSize);

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
                Log.Log(LogType.FILE, LogLevel.INFORM, " Timer1_Tick() -->> is STARTED");
                timer1.Enabled = false;
                if (!string.IsNullOrEmpty(_customQuery))
                {
                    SaveData();
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " Timer1_Tick() -->> LastLine is null. Please type Query LastLine.");
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " Timer1_Tick() -->> An error occurred : " + ex);
            }
            finally
            {
                timer1.Enabled = true;
                Log.Log(LogType.FILE, LogLevel.INFORM, " Timer1_Tick() -->> is FINISHED");
            }
        }

        private void SaveData()
        {
            IDataReader dataReader = null;
            //SqlDataReader dataReader = null;
            DbCommand dbCommand = null;
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> is STARTED");
                Rec rec = new Rec();
                dataReader = GetDataOnDB(ref dbCommand);
                //dataReader = GetDataOnDB();

                if (dataReader != null)
                {
                    try
                    {
                        String line = "";
                        String tempPart;

                        if (dataReader.Read())
                        {
                            Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> DataReader !null" + _IdColumnName);
                            do
                            {
                                _lastPosition = dataReader["RECORD_NUMBER"].ToString();
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> DataReader !null_1");
                                if (dataReader != null)
                                {
                                    for (int i = 0; i < dataReader.FieldCount; i++)
                                    {
                                        Log.Log(LogType.FILE, LogLevel.INFORM,
                                                " SaveData() -->> Comun Names: " + i + ". " + dataReader.GetName(i));
                                        if (!dataReader.IsDBNull(0))
                                        {
                                            if (dataReader.GetName(i) == "DATE_TIME")
                                            {
                                                string date = dataReader[i].ToString();
                                                DateTime dt;
                                                dt = Convert.ToDateTime(date);
                                                rec.Datetime = dt.ToString(dateFormat);
                                            }

                                            if (dataReader.GetName(i) == "SOURCENAME")
                                            {
                                                rec.SourceName = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "EVENTCATEGORY")
                                            {
                                                rec.EventCategory = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "EVENTTYPE")
                                            {
                                                rec.EventType = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "EVENT_ID")
                                            {
                                                rec.EventId = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "USERSID")
                                            {
                                                rec.UserName = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "COMPUTERNAME")
                                            {
                                                rec.ComputerName = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR1")
                                            {
                                                rec.CustomStr1 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR2")
                                            {
                                                rec.CustomStr2 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR2")
                                            {
                                                rec.CustomStr3 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR3")
                                            {
                                                rec.CustomStr3 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR4")
                                            {
                                                rec.CustomStr4 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR5")
                                            {
                                                rec.CustomStr5 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR6")
                                            {
                                                rec.CustomStr6 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR7")
                                            {
                                                rec.CustomStr7 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR8")
                                            {
                                                rec.CustomStr8 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR9")
                                            {
                                                rec.CustomStr9 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMSTR10")
                                            {
                                                rec.CustomStr10 = dataReader[i].ToString();
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT1")
                                            {
                                                rec.CustomInt1 = Convert.ToInt32(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT2")
                                            {
                                                rec.CustomInt2 = Convert.ToInt32(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT3")
                                            {
                                                rec.CustomInt3 = Convert.ToInt32(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT4")
                                            {
                                                rec.CustomInt4 = Convert.ToInt32(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT5")
                                            {
                                                rec.CustomInt5 = Convert.ToInt32(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT6")
                                            {
                                                rec.CustomInt6 = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT7")
                                            {
                                                rec.CustomInt7 = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT8")
                                            {
                                                rec.CustomInt8 = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT9")
                                            {
                                                rec.CustomInt9 = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == "CUSTOMINT10")
                                            {
                                                rec.CustomInt10 = Convert.ToInt64(dataReader[i].ToString());
                                            }

                                            if (dataReader.GetName(i) == _IdColumnName)
                                            {
                                                rec.Recordnum = Convert.ToInt32(dataReader[i].ToString());
                                            }
                                        }

                                        tempPart = dataReader[i].ToString();
                                        if (String.IsNullOrEmpty(tempPart))
                                        {
                                            tempPart = "NULL";
                                        }
                                        line += tempPart + " | ";
                                    }

                                }

                                rec.Description = line;
                                rec.LogName = "AOCEBYSRecorder";

                                if (!string.IsNullOrEmpty(rec.SourceName))
                                {
                                    rec.LogName += "_" + rec.SourceName;
                                }
                                else
                                {
                                    rec.LogName = "_" +
                                                  After(_customQuery, "from").Replace(']', ' ').Replace('[', ' ').Trim();
                                }

                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> Description:  " + rec.Description);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> _lastPosition:  " + _lastPosition);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> LogName:  " + rec.LogName);

                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> SourceName:  " + rec.SourceName);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> Datetime:  " + rec.Datetime);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt1:  " + rec.CustomInt1);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt2:  " + rec.CustomInt2);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt3:  " + rec.CustomInt3);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt4:  " + rec.CustomInt4);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt5:  " + rec.CustomInt5);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomInt6:  " + rec.CustomInt6);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> CustomStr4:  " + rec.CustomStr4);

                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> Data sending. ");
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> _lastPosition. " + _lastPosition);
                                CustomServiceBase customServiceBase =
                                    base.GetInstanceService("Security Manager Remote Recorder");
                                customServiceBase.SetData(_dal, _virtualHost, rec);
                                customServiceBase.SetReg(_id, _lastPosition, _customQuery, "", "", rec.Datetime);
                                Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> Data sended. ");
                                line = "";
                            } while (dataReader.Read());
                        }
                        else
                        {
                            Log.Log(LogType.FILE, LogLevel.WARN, " SaveData() -->> No data:");
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Log(LogType.FILE, LogLevel.ERROR, " SaveData() -->> An error occurred_ic: " + exception.ToString());

                    }
                    finally
                    {
                        if (dbCommand != null && dbCommand.Connection != null)
                            Database.Drop(ref dbCommand);
                    }
                }
                else
                {
                    Log.Log(LogType.FILE, LogLevel.INFORM, " SaveData() -->> DataReader null");
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " SaveData() -->> An error occurred : " + ex.ToString());
            }
        }// SaveData

        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        public string After(string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        } // After

        /// <summary>
        /// string between function
        /// </summary>
        /// <param name="value"></param>
        /// gelen tüm string
        /// <param name="a"></param>
        /// başlangıç string
        /// <param name="b"></param>
        /// bitiş string
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, System.StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, System.StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        private IDataReader GetDataOnDB(ref DbCommand dbCommand)
        {
            try
            {
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> _recorderName: " +_recorderName );

                DbConnection dbConnection = Database.GetConnection(false, _recorderName);
                dbConnection.Open();
                dbCommand = dbConnection.CreateCommand();
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> is STARTED");
                String query = "";
                query = "SELECT TOP " + _maxRecordSend + " " + _customQuery + " Where " + _IdColumnName + " > " +
                    _lastPosition + " ORDER BY " + _IdColumnName;
                Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> Query will run. Query is : " + query);
                dbCommand.CommandText = query;
                try
                {
                    Database.Fast = false;
                    Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> _recorderName: " + _user + " - " + _password);
                    //IDataReader dataReader = Database.ExecuteReader("AOCEBYSRecorder", query, CommandBehavior.Default, out dbCommand);
                    return dbCommand.ExecuteReader();
                    //return dataReader;
                }
                catch (Exception exception)
                {
                    Log.Log(LogType.FILE, LogLevel.ERROR, "GetDataOnDB() -->> DataReader Error.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Log(LogType.FILE, LogLevel.ERROR, " GetDataOnDB() -->> An error occurred : " + ex.ToString());
                return null;
            }
        }//GetDataOnDb

        //private SqlDataReader GetDataOnDB()
        //{
        //    try
        //    {
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> is STARTED");
        //        String query = "";
        //        query = "SELECT TOP " + _maxRecordSend + " " + _customQuery + " Where " + _IdColumnName + " > " +
        //            _lastPosition + " ORDER BY " + _IdColumnName;
        //        Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> Query will run. Query is : " + query);
        //        string connectionString = "Data Source=" + _remoteHost + "; Initial Catalog=" + _location + "; User id=" +
        //                                  _user + "; Password=" + _password + "; Integrated Security=true";

        //        Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> connectionString: " + connectionString);

        //        try
        //        {
        //            SqlConnection connection = new SqlConnection(connectionString);
        //            connection.Open();
        //            SqlCommand command = new SqlCommand(query, connection);
        //            SqlDataReader reader;

        //            reader = command.ExecuteReader();
        //            Log.Log(LogType.FILE, LogLevel.INFORM, " GetDataOnDB() -->> is successfully FINISHED");
        //            return reader;

        //            reader.Close();
        //            connection.Close();
        //        }
        //        catch (Exception exception)
        //        {
        //            Log.Log(LogType.FILE, LogLevel.ERROR, "GetDataOnDB() -->> DataReader Error.");
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Log(LogType.FILE, LogLevel.ERROR, " GetDataOnDB() -->> An error occurred : " + ex.ToString());
        //        return null;
        //    }
        //}//GetDataOnDb

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
                Log.Log(LogType.FILE, LogLevel.INFORM, " ConvertToDateTime() -->> An error occurred : " + ex.ToString());
                return false;
            }
        }

        public override void Clear()
        {
            if (timer1 != null)
                timer1.Enabled = false;
        }
    }
}
