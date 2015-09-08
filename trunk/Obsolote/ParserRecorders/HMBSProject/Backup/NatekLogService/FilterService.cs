using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using DAL;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Win32;
using Log;
using System.Data.OleDb;
using System.Data;
using System.Threading;

namespace NatekLogService
{
    public class FilterService
    {
        private System.Timers.Timer filterTimer;
        private String dataBaseNameforMainServer;
        private String dateFormat;
        private OleDbConnection connectSPDatabase;
        private ConnectionClass connectionClass;

        public FilterService()
        {
            try
            {
                dateFormat = "yyyy/MM/dd HH:mm:ss";
                dataBaseNameforMainServer = GetDatabaseName(Rsc.SubKey, Rsc.DalNameSql);
                filterTimer = new System.Timers.Timer();
                filterTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.filterTimerTick);
                filterTimer.Interval = Rsc.TimerInterval;
                filterTimer.Enabled = true;
                filterTimer.Start();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private void filterTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                filterTimer.Enabled = false;
                MainFunc();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                filterTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Belli zaman aralıklarında gidecek tabloyu kontrol edecek.
        /// datetime gelmişse stored procedure ü çağıracak
        /// stored procedureden gelen değer "TRUE" ise 
        /// belirlenmiş olan database e veri yazılacak
        /// alert üretilecek.
        /// </summary>
        private void MainFunc()
        {
            DataTable dataTable;
            List<RowProperties> rowPropList;
            String querySelFilters;
            String queryGetLogTableValues;
            String filterID, usedFunction;
            String lastPosition, tableName;
            String filterName, target;
            try
            {
                WriteLogFile("MainFunc() -->> ", "is STARTED");
                //Fonksiyonun çalışıp çalışmayacağı kontrol ediliyor.
                if (RunControl())
                {
                    connectionClass = new ConnectionClass();
                    connectSPDatabase = connectionClass.Connect();
                    if (connectSPDatabase != null)
                    {
                        if (connectSPDatabase.State == ConnectionState.Open)
                        {
                            querySelFilters = " SELECT " +
                                              Rsc.Filters_ID + "," +
                                              Rsc.Filters_FilterName + "," +
                                              Rsc.Filters_UsedFunc + "," +
                                              Rsc.Filters_LastPos + "," +
                                              Rsc.Filters_Target + "," +
                                              Rsc.Filters_TableName +
                                              " FROM " +
                                              Rsc.FiltersTbl;

                            WriteLogFile("MainFunc() -->> Server log files stored: ", Rsc.DalNameSql);
                            WriteLogFile("MainFunc() -->> Server stored procedure stored :", Rsc.DalNameOra);
                            dataTable = GetDataTable(dataBaseNameforMainServer, querySelFilters);
                            if (dataTable != null)
                            {
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    filterID = row[Rsc.Filters_ID].ToString();
                                    filterName = row[Rsc.Filters_FilterName].ToString();
                                    usedFunction = row[Rsc.Filters_UsedFunc].ToString();
                                    lastPosition = row[Rsc.Filters_LastPos].ToString();
                                    target = row[Rsc.Filters_Target].ToString();
                                    tableName = row[Rsc.Filters_TableName].ToString();

                                    queryGetLogTableValues = CreateQuery(filterID, lastPosition, tableName);
                                    rowPropList = RunFilter_GetValues(queryGetLogTableValues, lastPosition);

                                    if (rowPropList != null && rowPropList.Count > 0)
                                    {
                                        ParseKey_CallSP(rowPropList, filterName, target, usedFunction, filterID);
                                    }
                                }
                            }
                            connectSPDatabase.Close();
                        }
                        RefreshRuntime();
                    }
                }
                WriteLogFile("MainFunc() -->> ", " is successfully FINISHED");
            }
            catch (Exception ex)
            {
                WriteLogFile("MainFunc() -->> ", ex);
            }
        }

        private void RefreshRuntime()
        {
            String querySelRuntime;
            String queryUpdateRuntime;
            DataTable dataTable;
            DateTime runTime;
            Double period;
            try
            {
                WriteLogFile("RefreshRuntime() -->> ", "is STARTED");
                querySelRuntime = " SELECT " +
                                  Rsc.Configuration_Runtime + "," +
                                  Rsc.Configuration_Period +
                                  " FROM " +
                                  Rsc.Configuration;
                dataTable = GetDataTable(dataBaseNameforMainServer, querySelRuntime);
                runTime = Convert.ToDateTime(dataTable.Rows[0].ItemArray[0]);
                period = Convert.ToDouble(dataTable.Rows[0].ItemArray[1]);
                runTime = runTime.AddMinutes(period);

                queryUpdateRuntime = " UPDATE " +
                                     Rsc.Configuration +
                                     " SET " +
                                     Rsc.Configuration_Runtime + "='" + runTime.ToString(dateFormat) + "'";
                Database.ExecuteNonQuery(dataBaseNameforMainServer, queryUpdateRuntime);
                WriteLogFile("RefreshRuntime() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                WriteLogFile("RefreshRuntime", ex);
            }
        }

        private Boolean RunControl()
        {
            String querySelRunTime;
            DateTime thisTime, runtime;
            DataTable dataTable;
            try
            {
                WriteLogFile("RunControl() -->> ", "is STARTED");
                querySelRunTime = " SELECT " +
                                  Rsc.Configuration_Runtime +
                                  " FROM " +
                                  Rsc.Configuration;

                dataTable = GetDataTable(dataBaseNameforMainServer, querySelRunTime);
                if (dataTable != null)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        runtime = Convert.ToDateTime(dataTable.Rows[0].ItemArray[0]);
                        thisTime = Convert.ToDateTime(DateTime.Now.ToString(dateFormat));
                        if (thisTime > runtime)
                        {
                            WriteLogFile("RunControl() -->> ", "is successfully FINISHED.");
                            return true;
                        }
                    }
                }
                WriteLogFile("RunControl() -->> ", "is successfully FINISHED.");
                return false;

            }
            catch (Exception ex)
            {
                WriteLogFile("RunControl() -->> ", ex);
                return false;
            }
        }

        private Int32 GetDataBaseType()
        {
            String queryGetDataBaseType;
            Int32 dataBaseType = 0;
            DataTable dataTable;
            try
            {
                queryGetDataBaseType = " SELECT " +
                                       Rsc.Configuration_DataBaseType +
                                       " FROM " +
                                       Rsc.Configuration;

                dataTable = GetDataTable(dataBaseNameforMainServer, queryGetDataBaseType);
                if (dataTable != null)
                {

                }
                return dataBaseType;
            }
            catch (Exception ex)
            {
                WriteLogFile("GetDataBaseType", ex);
                return -1;
            }
        }

        private List<RowProperties> RunFilter_GetValues(String queryGetLogTableValues, String lastPosition)
        {
            DataTable dataTable;
            List<RowProperties> rowPropList;
            RowProperties rowProp;
            try
            {
                WriteLogFile("RunFilter_GetValues() -->> ", "is STARTED");
                rowPropList = new List<RowProperties>();
                int count = 0;
                dataTable = GetDataTable(dataBaseNameforMainServer, queryGetLogTableValues);

                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        count++;
                        rowProp = new RowProperties();
                        rowProp.ID = row[Rsc.LogTbl_ID].ToString();
                        rowProp.dateTime = row[Rsc.LogTbl_DateTime].ToString();
                        rowProp.category = row[Rsc.LogTbl_EventCateg].ToString();
                        rowProp.userName = row[Rsc.LogTbl_UserName].ToString();
                        rowProp.computerName = row[Rsc.LogTbl_CompName].ToString();
                        rowProp.windowName = row[Rsc.LogTbl_WindowName].ToString();
                        rowProp.primaryKeyValue = row[Rsc.LogTbl_PriKeyVal].ToString();
                        rowProp.dataWindowName = row[Rsc.LogTbl_DataWindowName].ToString();
                        rowProp.recordNumber = row[Rsc.LogTbl_RecordNumber].ToString();
                        if (rowProp.primaryKeyValue.Contains("WB") || rowProp.primaryKeyValue.Contains("S"))
                        {
                            continue;
                            //rowProp.primaryKeyValue = rowProp.primaryKeyValue.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        }
                        if (rowProp.primaryKeyValue == "")
                        {
                            continue;
                        }
                        rowPropList.Add(rowProp);
                        if (count > 100)
                        {
                            break;
                        }
                    }

                    WriteLogFile("RunFilter_GetValues() -->> ", " Record count is : " + count.ToString());
                    WriteLogFile("RunFilter_GetValues() -->> ", "is successfully FINISHED.");
                    return rowPropList;
                }
                WriteLogFile("RunFilter_GetValues() -->> ", "is successfully FINISHED.");
                return null;
            }
            catch (Exception ex)
            {
                WriteLogFile("RunFilter_GetValues() -->> ", ex);
                return null;
            }
        }

        private String CreateQuery(String filterID, String lastPosition, String tableName)
        {
            String querySelFilterCol;
            String queryGetLogTableValues;
            DataTable dataTable;
            try
            {
                WriteLogFile("CreateQuery() -->> ", "is STARTED");
                querySelFilterCol = "SELECT " +
                                    Rsc.Columns_Constants_ColumnName + "," +
                                    Rsc.Columns_Constants_Constant +
                                    " FROM " +
                                    Rsc.Columns_ConstantsTbl +
                                    " WHERE " +
                                    Rsc.Columns_Constants_FilterID + "=" + filterID;


                dataTable = GetDataTable(dataBaseNameforMainServer, querySelFilterCol);

                queryGetLogTableValues = " SELECT * " +
                                         " FROM " +
                                         tableName;
                if (lastPosition == "")
                {
                    lastPosition = "0";
                }

                if (dataTable != null)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        queryGetLogTableValues += " WHERE ";
                        foreach (DataRow row in dataTable.Rows)
                        {
                            queryGetLogTableValues += row.ItemArray[0].ToString() + "='" + row.ItemArray[1].ToString() + "' AND ";
                        }
                        queryGetLogTableValues += " '" + lastPosition + "'<" +
                                                  Rsc.LogTbl_ID +
                                                  " ORDER BY " +
                                                  Rsc.LogTbl_ID + " ASC";

                    }
                    else
                    {
                        queryGetLogTableValues += " WHERE '" + lastPosition + "'<" +
                                                  Rsc.LogTbl_ID +
                                                  " ORDER BY " +
                                                  Rsc.LogTbl_ID + " ASC";

                    }
                }
                else
                {
                    queryGetLogTableValues += " WHERE '" + lastPosition + "'<" +
                                              Rsc.LogTbl_ID +
                                              " ORDER BY " +
                                              Rsc.LogTbl_ID + " ASC";
                }
                WriteLogFile("CreateQuery() -->> ", " Query is : " + queryGetLogTableValues);
                WriteLogFile("CreateQuery() -->> ", "is successfully FINISHED.");
                return queryGetLogTableValues;
            }
            catch (Exception ex)
            {
                WriteLogFile("CreateQuery() -->> ", ex);
                return null;
            }
        }

        private void RefreshFilter(String filterID, String lastPosition)
        {
            String queryRefreshFilter;
            try
            {
                WriteLogFile("RefreshFilter() -->> ", "is STARTED");
                queryRefreshFilter = "UPDATE " +
                                     Rsc.FiltersTbl +
                                     " SET "
                                     + Rsc.Filters_LastRunTime + "='" + DateTime.Now.ToString(dateFormat) + "'," +
                                     Rsc.Filters_LastPos + "='" + lastPosition +
                                     "' WHERE "
                                     + Rsc.Filters_ID + "=" + filterID;

                Database.ExecuteNonQuery(dataBaseNameforMainServer, queryRefreshFilter);
                WriteLogFile("RefreshFilter() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                WriteLogFile("RefreshFilter", ex);
            }
        }

        private void ParseKey_CallSP(List<RowProperties> rowPropList, String filterName, String target, String storedProcName, String filterID)
        {
            String key;
            String[] keyPartArray;
            ReturnedValues returnValues;
            String queryInsertMsg;
            try
            {
                WriteLogFile("ParseKey_CallSP() -->> ", "is STARTED");
                returnValues = new ReturnedValues();
                foreach (RowProperties rowProp in rowPropList)
                {
                    String lastPosition = rowProp.ID;
                    RefreshFilter(filterID, lastPosition);

                    key = rowProp.primaryKeyValue;
                    WriteLogFile(storedProcName + " procedure will run this key", key);
                    key = key.Remove(key.LastIndexOf('-'));
                    keyPartArray = key.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    returnValues = RunStoredProcedure(keyPartArray, storedProcName);
                    if (returnValues != null)
                    {
                        if (returnValues.boolValue)
                        {
                            DateTime tempdate = Convert.ToDateTime(rowProp.dateTime);
                            queryInsertMsg = " INSERT INTO " +
                                             Rsc.ActionMsgTbl + "(" +
                                             Rsc.ActionMsg_FilterName + "," +
                                             Rsc.ActionMsg_DateTime + "," +
                                             Rsc.ActionMsg_Category + "," +
                                             Rsc.ActionMsg_UserName + "," +
                                             Rsc.ActionMsg_ComputerName + "," +
                                             Rsc.ActionMsg_WindowName + "," +
                                             Rsc.ActionMsg_PrimaryKeyValue + "," +
                                             Rsc.ActionMsg_DataWindowName + "," +
                                             Rsc.ActionMsg_RecordNumber + "," +
                                             Rsc.ActionMsg_Description + "," +
                                             Rsc.ActionMsg_AlertTime + ")" +
                                             " VALUES ('" +
                                             filterName + "','" +
                                             tempdate.ToString(dateFormat) + "','" +
                                             rowProp.category + "','" +
                                             rowProp.userName + "','" +
                                             rowProp.computerName + "','" +
                                             rowProp.windowName + "','" +
                                             rowProp.primaryKeyValue + "','" +
                                             rowProp.dataWindowName + "','" +
                                             rowProp.recordNumber + "','" +
                                             returnValues.description + "','" +
                                             DateTime.Now.ToString(dateFormat) + "')";
                            WriteLogFile("Insert table ", queryInsertMsg);
                            Database.ExecuteNonQuery(dataBaseNameforMainServer, queryInsertMsg);
                            WriteLogFile("ParseKey_CallSP() -->> ", "is successfully FINISHED.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogFile("ParseKey_CallSP() -->> ", ex);
            }
        }

        private ReturnedValues RunStoredProcedure(String[] keyPartArray, String storedProcedureName)
        {
            OleDbCommand dbCommand = null;
            Int32 keyIndex = 0;
            ReturnedValues returnValues;

            try
            {
                WriteLogFile("RunStoredProcedure() -->> ", "is STARTED");
                dbCommand = connectSPDatabase.CreateCommand();
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbCommand.CommandText = storedProcedureName;
                OleDbCommandBuilder.DeriveParameters(dbCommand);

                foreach (DbParameter dbParameter in dbCommand.Parameters)
                {
                    if (dbParameter.Direction != ParameterDirection.ReturnValue && dbParameter.Direction != ParameterDirection.Output && keyPartArray.Length > keyIndex)
                    {
                        dbParameter.Value = keyPartArray[keyIndex];
                        keyIndex++;
                    }
                }
                foreach (DbParameter dbParameter in dbCommand.Parameters)
                {
                    if (dbParameter.Value == null)
                    {
                        dbParameter.Value = "0";
                    }
                }

                dbCommand.ExecuteNonQuery();

                returnValues = new ReturnedValues();

                returnValues.boolValue = Convert.ToBoolean(dbCommand.Parameters[Rsc.StoredPrcBoolVal].Value);

                returnValues.description = dbCommand.Parameters[Rsc.StoredPrcDescVal].Value.ToString();

                WriteLogFile("RunStoredProcedure() -->> ", "is successfully FINISHED.");
                return returnValues;

            }
            catch (Exception ex)
            {
                WriteLogFile("RunStoredProcedure() -->> ", ex);
                return null;
            }
        }

        private DataTable GetDataTable(String dataBaseName, String query)
        {
            try
            {
                Database.Fast = false;
                DbCommand dbCommand = null;
                DataSet dataSet = new DataSet();
                IDataAdapter idataAdapt = Database.GetDataAdapter(dataBaseName, query, out dbCommand);
                idataAdapt.Fill(dataSet);
                Database.Drop(ref dbCommand);
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                WriteLogFile("GetDataTable", ex);
                return null;
            }
        }

        private String GetDatabaseName(String subKey, String dalName)
        {
            try
            {
                String dataBaseName = "";
                RegistryKey regKey;
                regKey = Registry.LocalMachine.OpenSubKey(subKey);
                dataBaseName = (String)regKey.GetValue(dalName);
                return dataBaseName;
            }
            catch (Exception ex)
            {
                WriteLogFile("GetDatabaseName", ex);
                return null;
            }
        }

        private Boolean ControlSPExist(String spName)
        {
            String queryGetSpName;
            try
            {
                queryGetSpName = " SELECT COUNT(NAME) FROM SYS.OBJECTS WHERE TYPE='P' AND NAME='" +
                                 spName + "'";

                OleDbDataReader oleDbDataReader = connectionClass.GetDataReader(queryGetSpName);
                if (oleDbDataReader.HasRows)
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                WriteLogFile("ControlSPExist", ex);
                return false;
            }
        }

        private class ReturnedValues
        {
            public Boolean boolValue;
            public String description;
        }

        private class RowProperties
        {
            public String ID;
            public String dateTime; //DATE_TIME
            public String category; //EVENTCATEGORY
            public String userName; //USERNAME
            public String computerName; //COMPUTERNAME
            public String windowName; //WINDOWNAME
            public String primaryKeyValue; //CUSTOMSTR7
            public String dataWindowName; //CUSTOMSTR5
            public String recordNumber; // RECORD_NUMBER 

        }

        private class ActionMsgLog
        {
            public String id;
            public String actionType;
            public String destination;
            public String actionMessage;
            public String actionTime;
            public String stat;
        }

        private void WriteLogFile(String functionName, Exception ex)
        {
            StreamWriter LogFile = File.AppendText(ControlProcessorTypex86() + "\\Natek Alert.log");
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine(DateTime.Now.ToString(dateFormat) + " " + functionName + "  : " + ex.ToString());
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine();
            LogFileSizeControl();
            LogFile.Close();
            LogFile = null;


        }

        private void WriteLogFile(String functionName, String msg)
        {
            StreamWriter LogFile = File.AppendText(ControlProcessorTypex86() + "\\Natek Alert.log");
            LogFile.WriteLine(DateTime.Now.ToString(dateFormat) + " " + functionName + "  : " + msg);
            LogFile.Close();
            LogFileSizeControl();
            LogFile = null;

        }

        private void LogFileSizeControl()
        {
            FileInfo fileInfo = new FileInfo(ControlProcessorTypex86() + "\\Natek Alert.log");
            if (fileInfo.Length > 5242880)
            {
                fileInfo.Delete();
            }

        }

        private String ControlProcessorTypex86()
        {
            if (Directory.Exists(@"C:\Program Files (x86)"))
            {
                return @"C:\Program Files (x86)\Natek\Natek Log Alert";
            }
            else
            {
                return @"C:\Program Files\Natek\Natek Log Alert";
            }

        }
    }
}

