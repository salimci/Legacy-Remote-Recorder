using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OracleClient;
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
using System.Data.OracleClient;

namespace NatekLogService
{
    public class FilterService
    {
        private System.Timers.Timer filterTimer;
        private String dataBaseNameforMainServer;
        private String dateFormat;
        private OleDbConnection connectSPDatabase;
        private ConnectionClass connectionClass;

        DataTable dataTable;
        public String Id;
        public String FilterName;
        public String FunctionName;
        public String LastPosition;
        public String TableName;
        String DbName = "HMDBSql";
        private string OraTNS;
        private string OraUser;
        private string OraPass;
        public String Date_Time;
        public String EVENTCATEGORY;
        public String USERNAME;
        public String COMPUTERNAME;
        public String WINDOWNAME;
        public String DATAWINDOWNAME;
        public String RECORDNUMBER;
        public String PRIMARYKEYVALUE;
        public String DESCRIPTION;
        private double IntervalTime;

        public FilterService()
        {
            try
            {
                ReadRegistry();
                dateFormat = "yyyy/MM/dd HH:mm:ss";
                //dataBaseNameforMainServer = GetDatabaseName(Rsc.SubKey, Rsc.DalNameSql);
                WriteLogFile("FilterService: ", "dataBaseNameforMainServer");
                filterTimer = new System.Timers.Timer();
                filterTimer.Elapsed += filterTimerTick;
                filterTimer.Interval = IntervalTime;
                filterTimer.Enabled = true;
                filterTimer.Start();
            }
            catch (Exception ex)
            {
                WriteLogFile("FilterService ERROR", ex.Message);
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
            WriteLogFile("MainFunc: ", "Goto Step1.");
            WriteLogFile("MainFunc: ", "Test2");
            WriteLogFile("MainFunc: ", "Test3");
            WriteLogFile("MainFunc: Test 4", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 5", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 6", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 7", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 9", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            WriteLogFile("MainFunc: Test 10", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //ReadRegistry();
            Step1();
            //DataTable dataTable;
            //List<RowProperties> rowPropList;
            //String querySelFilters;
            //String queryGetLogTableValues;
            //String filterID, usedFunction;
            //String lastPosition, tableName;
            //String filterName, target;
            //try
            //{
            //    WriteLogFile("MainFunc() -->> ", "is STARTED");
            //    //Fonksiyonun çalışıp çalışmayacağı kontrol ediliyor.
            //    if (RunControl())
            //    {
            //        connectionClass = new ConnectionClass();
            //        connectSPDatabase = connectionClass.Connect();
            //        if (connectSPDatabase != null)
            //        {
            //            if (connectSPDatabase.State == ConnectionState.Open)
            //            {
            //                querySelFilters = " SELECT " +
            //                                  Rsc.Filters_ID + "," +
            //                                  Rsc.Filters_FilterName + "," +
            //                                  Rsc.Filters_UsedFunc + "," +
            //                                  Rsc.Filters_LastPos + "," +
            //                                  Rsc.Filters_Target + "," +
            //                                  Rsc.Filters_TableName +
            //                                  " FROM " +
            //                                  Rsc.FiltersTbl;

            //                WriteLogFile("MainFunc() -->> Server log files stored: ", Rsc.DalNameSql);
            //                WriteLogFile("MainFunc() -->> Server stored procedure stored :", Rsc.DalNameOra);
            //                dataTable = GetDataTable(dataBaseNameforMainServer, querySelFilters);
            //                if (dataTable != null)
            //                {
            //                    foreach (DataRow row in dataTable.Rows)
            //                    {
            //                        filterID = row[Rsc.Filters_ID].ToString();
            //                        filterName = row[Rsc.Filters_FilterName].ToString();
            //                        usedFunction = row[Rsc.Filters_UsedFunc].ToString();
            //                        lastPosition = row[Rsc.Filters_LastPos].ToString();
            //                        target = row[Rsc.Filters_Target].ToString();
            //                        tableName = row[Rsc.Filters_TableName].ToString();

            //                        queryGetLogTableValues = CreateQuery(filterID, lastPosition, tableName);
            //                        rowPropList = RunFilter_GetValues(queryGetLogTableValues, lastPosition);

            //                        if (rowPropList != null && rowPropList.Count > 0)
            //                        {
            //                            ParseKey_CallSP(rowPropList, filterName, target, usedFunction, filterID);
            //                        }
            //                    }
            //                }
            //                connectSPDatabase.Close();
            //            }
            //            RefreshRuntime();
            //        }
            //    }
            //    WriteLogFile("MainFunc() -->> ", " is successfully FINISHED");
            //}
            //catch (Exception ex)
            //{
            //    WriteLogFile("MainFunc() -->> ", ex);
            //}
        } // MainFunc

        public void ReadRegistry()
        {
            try
            {
                RegistryKey regKey;
                regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NATEK\DAL\HMDBSql");
                WriteLogFile("ReadRegistry--> OraUser ", regKey.GetValue("OraUser").ToString());
                WriteLogFile("ReadRegistry--> OraTNS ", regKey.GetValue("OraTNS").ToString());
                WriteLogFile("ReadRegistry--> OraPass ", regKey.GetValue("OraPass").ToString());
                WriteLogFile("ReadRegistry--> AlertInterval ", regKey.GetValue("AlertInterval").ToString());
                //AlertInterval

                OraUser = regKey.GetValue("OraUser").ToString();
                OraTNS = regKey.GetValue("OraTNS").ToString();
                OraPass = regKey.GetValue("OraPass").ToString();
                IntervalTime = Convert.ToDouble(regKey.GetValue("AlertInterval"));
            }
            catch (Exception ex)
            {
                WriteLogFile("ReadRegistry--> ERROR :", ex.Message);
                WriteLogFile("ReadRegistry--> ERROR :", ex.Source);
            }
        } // ReadRegistry

        private void Step1()
        {
            String SQL = "Select ID, FILTERNAME, USEDFUNCTION, LASTPOSITION,TABLENAME FROM EXTERNAL_ALERT_FILTERS WHERE STATUS = 1 ORDER BY ID ASC";
            try
            {
                dataTable = getDataTable(DbName, SQL);
                foreach (DataRow row in dataTable.Rows)
                {
                    Id = row["ID"].ToString();
                    FilterName = row["FILTERNAME"].ToString();
                    FunctionName = row["USEDFUNCTION"].ToString();
                    if (!String.IsNullOrEmpty(row["LASTPOSITION"].ToString()))
                    {
                        LastPosition = row["LASTPOSITION"].ToString();    
                    }
                    else
                    {
                        LastPosition = "0";
                    }
                    
                    TableName = row["TABLENAME"].ToString();
                    Step2(Id);

                    WriteLogFile("Step1, Id : ", Id.ToString());
                    WriteLogFile("Step1, FilterName: ", FilterName);
                    WriteLogFile("Step1, FunctionName: ", FunctionName);
                    WriteLogFile("Step1, LastPosition : ", LastPosition);
                    WriteLogFile("Step1, TableName : ", TableName);
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                //MessageBox.Show(exception.StackTrace);

                WriteLogFile("Step1()--> ERROR: ", exception.Message);
                WriteLogFile("Step1()--> ERROR: ", exception.StackTrace);
            }
            //MessageBox.Show(dataTable.Rows.Count + "");
        } // Step1

        private void Step2(String FilterId)
        {
            ArrayList ColumnArray = new ArrayList();
            ArrayList ConstantArray = new ArrayList();
            String SQL = "SELECT * FROM EXTERNAL_ALERT_COLUMNS_CONSTANTS where FILTERID = " + FilterId + "";

            try
            {
                dataTable = getDataTable(DbName, SQL);
                foreach (DataRow row in dataTable.Rows)
                {
                    ColumnArray.Add(row["COLUMNNAME"].ToString());
                    ConstantArray.Add(row["CONSTANT"].ToString());
                }
                Step3(ColumnArray, ConstantArray, TableName, LastPosition, dataTable.Rows.Count);
            }
            catch (Exception exception)
            {
                WriteLogFile("Step2--> ERROR : ", exception.Message);
                WriteLogFile("Step2--> ERROR : ", exception.StackTrace);
            }
        } // Step2

        private void Step3(ArrayList ColumnName, ArrayList ConstantName, String TableName, String LastPosition, int RowCount)
        {
            String MainSQL = "SELECT Top 1 * FROM " + TableName;
            String WhereSQL = "";
            String IdSQL = " Id ";
            String FullSQl = "";

            ArrayList IdValue;

            if (RowCount == 1)
            {
                WhereSQL = " Where " + ColumnName[0] + " = '" + ConstantName[0] + "' And ";
            }
            else if (RowCount > 1)
            {
                WhereSQL += " Where ";
                for (int i = 0; i < RowCount; i++)
                {
                    WhereSQL += ColumnName[i] + " = '" + ConstantName[i] + "'" + " And ";
                }
            }

            FullSQl = MainSQL + " " + WhereSQL + IdSQL + " > " + LastPosition;
            WriteLogFile("Step3: ", "FullSQl : " + FullSQl);
            try
            {
                dataTable = getDataTable(DbName, FullSQl);
                //dataGridView3.DataSource = dataTable;

                IdValue = new ArrayList();
                if (dataTable != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        PRIMARYKEYVALUE = row["CUSTOMSTR7"].ToString();
                        IdValue.Add(row["Id"].ToString());
                        EVENTCATEGORY = row["EVENTCATEGORY"].ToString();
                        Date_Time = row["DATE_TIME"].ToString();
                        USERNAME = row["USERSID"].ToString();
                        COMPUTERNAME = row["COMPUTERNAME"].ToString();
                        WINDOWNAME = row["CUSTOMSTR2"].ToString();
                        DATAWINDOWNAME = row["CUSTOMSTR5"].ToString();
                        RECORDNUMBER = row["RECORD_NUMBER"].ToString();

                        WriteLogFile("Step3 : ", "--------------------------------------------------");
                        WriteLogFile("Step3, PRIMARYKEYVALUE : ", PRIMARYKEYVALUE);
                        //WriteLogFile("Step3, IdValue : ", IdValue.ToString());
                        for (int i = 0; i < IdValue.Count; i++)
                        {
                            WriteLogFile("Step3, IdValue : ", IdValue[i].ToString());
                        }
                        WriteLogFile("Step3, EVENTCATEGORY : ", EVENTCATEGORY);
                        WriteLogFile("Step3, Date_Time : ", Date_Time);
                        WriteLogFile("Step3, USERNAME: ", USERNAME);
                        WriteLogFile("Step3, COMPUTERNAME: ", COMPUTERNAME);
                        WriteLogFile("Step3, WINDOWNAME: ", WINDOWNAME);
                        WriteLogFile("Step3, DATAWINDOWNAME: ", DATAWINDOWNAME);
                        WriteLogFile("Step3, RECORDNUMBER: ", RECORDNUMBER);
                        WriteLogFile("Step3 : ", "--------------------------------------------------");
                    }
                    

                    if (dataTable.Rows.Count > 0)
                    {
                        if (RunStoredProcedure(PRIMARYKEYVALUE, FunctionName) == 1)
                        {
                            WriteLogFile("Step3--> ", "Proc Calisti if ici");
                            InsertValues();
                        }
                    }
                    for (int i = 0; i < IdValue.Count; i++)
                    {
                        UpdateTable(IdValue[i].ToString());
                    }
                }
                else
                {
                    WriteLogFile("Step3: ", "İşlenecek yeni log yok.");
                }
            }
            catch (Exception exception)
            {
                WriteLogFile("Step3--> ERROR stored function çalıştırılamadı : ", exception.Message);
                WriteLogFile("Step3--> ERROR stored function çalıştırılamadı : ", exception.StackTrace);
            }
        } // Step3

        public void UpdateTable(String IdValue)
        {
            //string queryInsertMsg = " INSERT INTO EXTERNAL_ALERT_CONFIGURATION VALUES (1000,'Natekdb','HMDBOra')";
            string queryInsertMsg = " UPDATE EXTERNAL_ALERT_FILTERS SET LASTPOSITION = " + IdValue + " WHERE ID = " + Id;
            Database.Fast = false;
            WriteLogFile("UpdateTable : queryInsertMsg", queryInsertMsg);
            try
            {
                Database.ExecuteNonQuery("HMDBSql", queryInsertMsg);
                WriteLogFile("UpdateTable : queryInserted.", "");
            }
            catch (Exception exception)
            {
                WriteLogFile("UpdateTable--> ERROR : ", exception.Message);
                WriteLogFile("UpdateTable--> ERROR : ", exception.StackTrace);
            }
        }// UpdateTable

        public void InsertValues()
        {
            WriteLogFile("InsertValues : ", "Insert function is started.");
            WriteLogFile("InsertValues : ", Date_Time);
            RECORDNUMBER = LastPosition;

            DateTime dt;

            dt = Convert.ToDateTime(Date_Time);
 
            try
            {
                string queryInsertMsg =
                    "INSERT INTO EXTERNAL_ALERT_ACTION_MSG (FILTERNAME, DATE_TIME, CATEGORY, USERNAME, COMPUTERNAME, WINDOWNAME, PRIMARYKEYVALUE, DATAWINDOWNAME, RECORDNUMBER, DESCRIPTION, ALERT_TIME) VALUES (" +
                    "'" + FilterName + "', '" + dt.ToString("yyyy/MM/dd HH:mm:ss") + "', '" + EVENTCATEGORY + "', '" + USERNAME + "', '" +
                    COMPUTERNAME + "', '" + WINDOWNAME + "', '" + PRIMARYKEYVALUE + "', '" + DATAWINDOWNAME + "', '" +
                    RECORDNUMBER + "', '" + DESCRIPTION + "', '" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "')";
                Database.Fast = false;
                try
                {
                    WriteLogFile("InsertValues : ", queryInsertMsg);
                    Database.ExecuteNonQuery("HMDBSql", queryInsertMsg);
                }
                catch (Exception exception)
                {
                    WriteLogFile("InsertValues--> ExecuteNonequery ERROR : ", exception.Message);
                    WriteLogFile("InsertValues--> ExecuteNonequery ERROR : ", exception.StackTrace);
                }
            }
            catch (Exception exception)
            {
                WriteLogFile("InsertValues--> ERROR : ", exception.Message);
                WriteLogFile("InsertValues--> ERROR : ", exception.StackTrace);
            }
        } // InsertValues

        public decimal RunStoredProcedure(string PrimaryKeyValue, string FunctionName)
        {
            ArrayList primaryKeyvalueArr = new ArrayList();
            if (PrimaryKeyValue.EndsWith("-"))
            {
                PrimaryKeyValue += "0";
            }
            string[] arr = PrimaryKeyValue.Split('-');
            for (int i = 0; i < arr.Length; i++)
            {
                primaryKeyvalueArr.Add(arr[i]);
            }

            int count = primaryKeyvalueArr.Count;
            int a = 10 - count;
            object o = 0;
            if (a < 10)
            {
                for (int i = 0; i < a; i++)
                {
                    primaryKeyvalueArr.Add(o);
                }
            }

            string OracleConnectionStr = "Data Source=" + OraTNS + "; User ID=" + OraUser + "; Password=" + OraPass;

            WriteLogFile("RunStoredProcedure--> ", OracleConnectionStr);
            WriteLogFile("RunStoredProcedure--> ", " OracleConnection Oncesi");
            OracleConnection objConn = null;
            OracleCommand objCmd = null;
            try
            {
                objConn = new OracleConnection(OracleConnectionStr);

                WriteLogFile("RunStoredProcedure--> ", " OracleConnection Sonrası");
                WriteLogFile("RunStoredProcedure--> ", " OracleCommand Oncesi");
                objCmd = new OracleCommand();
                WriteLogFile("RunStoredProcedure--> ", " OracleCommand Sonrası");
                objCmd.Connection = objConn;
                objCmd.CommandText = FunctionName;
                objCmd.CommandType = CommandType.StoredProcedure;

                objCmd.Parameters.Add("PK1", OracleType.VarChar).Value = primaryKeyvalueArr[0];
                objCmd.Parameters.Add("PK2", OracleType.VarChar).Value = primaryKeyvalueArr[1];
                objCmd.Parameters.Add("PK3", OracleType.VarChar).Value = primaryKeyvalueArr[2];
                objCmd.Parameters.Add("PK4", OracleType.VarChar).Value = primaryKeyvalueArr[3];
                objCmd.Parameters.Add("PK5", OracleType.VarChar).Value = primaryKeyvalueArr[4];
                objCmd.Parameters.Add("PK6", OracleType.VarChar).Value = primaryKeyvalueArr[5];
                objCmd.Parameters.Add("PK7", OracleType.VarChar).Value = primaryKeyvalueArr[6];
                objCmd.Parameters.Add("PK8", OracleType.VarChar).Value = primaryKeyvalueArr[7];
                objCmd.Parameters.Add("PK9", OracleType.VarChar).Value = primaryKeyvalueArr[8];
                objCmd.Parameters.Add("PK10", OracleType.VarChar).Value = primaryKeyvalueArr[9];

                objCmd.Parameters.Add("RETURNVALUE", OracleType.Number, 1);
                objCmd.Parameters["RETURNVALUE"].Direction = ParameterDirection.Output;

                objCmd.Parameters.Add("DESCRIPTION", OracleType.VarChar, 1000);
                objCmd.Parameters["DESCRIPTION"].Direction = ParameterDirection.Output;

                objCmd.Parameters.Add("ITEMID", OracleType.VarChar, 1000);
                objCmd.Parameters["ITEMID"].Direction = ParameterDirection.Output;
            }
            catch (Exception exception)
            {
                WriteLogFile("Oracle Conneciton string error.", exception);
            }
            try
            {
                objConn.Open();
                WriteLogFile("RunStoredProcedure: ", "Oracle Connection Open");
                objCmd.ExecuteNonQuery();
                decimal returnValue = (decimal) objCmd.Parameters["RETURNVALUE"].Value;
                DESCRIPTION = objCmd.Parameters["DESCRIPTION"].Value.ToString();
                WriteLogFile("RunStoredProcedure-->,", "Return Value = " + returnValue + " Description = " +
                             DESCRIPTION);
                objConn.Close();
                return returnValue;
            }
            catch (Exception exception)
            {
                WriteLogFile("RunStoredProcedure--> ERROR : ", exception.Message);
                WriteLogFile("RunStoredProcedure--> ERROR : ", exception.StackTrace);
                WriteLogFile("RunStoredProcedure--> ERROR, GetType : ", exception.GetType().ToString());
                WriteLogFile("RunStoredProcedure--> ERROR, MemberType : ", exception.GetType().MemberType.ToString());
                WriteLogFile("RunStoredProcedure--> ERROR, Namespace : ", exception.GetType().Namespace);
                WriteLogFile("RunStoredProcedure--> ERROR, Source : ", exception.Source);
                throw;
            }
        } //RunStoredProcedure

        private DataTable getDataTable(string dbName, string query)
        {
            Database.Fast = false;
            DbCommand cmd = null;
            DataSet ds = new DataSet();
            IDataAdapter iAdapter = Database.GetDataAdapter(dbName, query, out cmd);
            iAdapter.Fill(ds);
            Database.Drop(ref cmd);
            return ds.Tables[0];
        } // getDataTable

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

        //private void ParseKey_CallSP(List<RowProperties> rowPropList, String filterName, String target, String storedProcName, String filterID)
        //{
        //    String key;
        //    String[] keyPartArray;
        //    ReturnedValues returnValues;
        //    String queryInsertMsg;
        //    try
        //    {
        //        WriteLogFile("ParseKey_CallSP() -->> ", "is STARTED");
        //        returnValues = new ReturnedValues();
        //        foreach (RowProperties rowProp in rowPropList)
        //        {
        //            String lastPosition = rowProp.ID;
        //            RefreshFilter(filterID, lastPosition);

        //            key = rowProp.primaryKeyValue;
        //            WriteLogFile(storedProcName + " procedure will run this key", key);
        //            key = key.Remove(key.LastIndexOf('-'));
        //            keyPartArray = key.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        //            returnValues = RunStoredProcedure(keyPartArray, storedProcName);
        //            if (returnValues != null)
        //            {
        //                if (returnValues.boolValue)
        //                {
        //                    DateTime tempdate = Convert.ToDateTime(rowProp.dateTime);
        //                    queryInsertMsg = " INSERT INTO " +
        //                                     Rsc.ActionMsgTbl + "(" +
        //                                     Rsc.ActionMsg_FilterName + "," +
        //                                     Rsc.ActionMsg_DateTime + "," +
        //                                     Rsc.ActionMsg_Category + "," +
        //                                     Rsc.ActionMsg_UserName + "," +
        //                                     Rsc.ActionMsg_ComputerName + "," +
        //                                     Rsc.ActionMsg_WindowName + "," +
        //                                     Rsc.ActionMsg_PrimaryKeyValue + "," +
        //                                     Rsc.ActionMsg_DataWindowName + "," +
        //                                     Rsc.ActionMsg_RecordNumber + "," +
        //                                     Rsc.ActionMsg_Description + "," +
        //                                     Rsc.ActionMsg_AlertTime + ")" +
        //                                     " VALUES ('" +
        //                                     filterName + "','" +
        //                                     tempdate.ToString(dateFormat) + "','" +
        //                                     rowProp.category + "','" +
        //                                     rowProp.userName + "','" +
        //                                     rowProp.computerName + "','" +
        //                                     rowProp.windowName + "','" +
        //                                     rowProp.primaryKeyValue + "','" +
        //                                     rowProp.dataWindowName + "','" +
        //                                     rowProp.recordNumber + "','" +
        //                                     returnValues.description + "','" +
        //                                     DateTime.Now.ToString(dateFormat) + "')";
        //                    WriteLogFile("Insert table ", queryInsertMsg);
        //                    Database.ExecuteNonQuery(dataBaseNameforMainServer, queryInsertMsg);
        //                    WriteLogFile("ParseKey_CallSP() -->> ", "is successfully FINISHED.");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLogFile("ParseKey_CallSP() -->> ", ex);
        //    }
        //}

        //private ReturnedValues RunStoredProcedure(String[] keyPartArray, String storedProcedureName)
        //{
        //    OleDbCommand dbCommand = null;
        //    Int32 keyIndex = 0;
        //    ReturnedValues returnValues;

        //    try
        //    {
        //        WriteLogFile("RunStoredProcedure() -->> ", "is STARTED");
        //        dbCommand = connectSPDatabase.CreateCommand();
        //        dbCommand.CommandType = CommandType.StoredProcedure;
        //        dbCommand.CommandText = storedProcedureName;
        //        OleDbCommandBuilder.DeriveParameters(dbCommand);
        //        foreach (DbParameter dbParameter in dbCommand.Parameters)
        //        {
        //            if (dbParameter.Direction != ParameterDirection.ReturnValue && dbParameter.Direction != ParameterDirection.Output && keyPartArray.Length > keyIndex)
        //            {
        //                dbParameter.Value = keyPartArray[keyIndex];
        //                keyIndex++;
        //            }
        //        }
        //        foreach (DbParameter dbParameter in dbCommand.Parameters)
        //        {
        //            if (dbParameter.Value == null)
        //            {
        //                dbParameter.Value = "0";
        //            }
        //        }
        //        dbCommand.ExecuteNonQuery();
        //        returnValues = new ReturnedValues();
        //        returnValues.boolValue = Convert.ToBoolean(dbCommand.Parameters[Rsc.StoredPrcBoolVal].Value);
        //        returnValues.description = dbCommand.Parameters[Rsc.StoredPrcDescVal].Value.ToString();
        //        WriteLogFile("RunStoredProcedure() -->> ", "is successfully FINISHED.");
        //        return returnValues;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLogFile("RunStoredProcedure() -->> ", ex);
        //        return null;
        //    }
        //}

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

