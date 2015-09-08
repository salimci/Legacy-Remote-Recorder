using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using DAL;
using Microsoft.Win32;

namespace NatekLogService
{
    /// <summary>
    /// Log Tablosundan Gereken alanları seçer ve bizim oluşturduğumuz
    /// "Rsc.ColumnsTable" tablosuna ekler
    /// </summary>
    class FormFeatures
    {

        private List<String> columnNamesList;

        Form _form;
        Panel _panelLeft;
        Panel _panelRight;
        String _dataBaseNameMain;

        /// <summary>
        /// formun  hiç bir özelliği yüklenmeden istenilen public fonksiyonlarının kullanılması için oluşturulmuş constructor.
        /// </summary>
        public FormFeatures()
        {
        }

        /// <summary>
        /// Database üzerinde bulunmuş olan filtername, runtime gibi özellikleri program başlaması anında set etmesi için kullanılacak constructur.
        /// </summary>
        /// <param name="form"></param>
        public FormFeatures(Form form)
        {
            if (Main(form))
            {
                DropandCreateColumnTbl();

                SelValues_InsertColTbl();

                CreateControls();

                LoadDatainForm();

                LoadFilterName();

            }
            else
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Cmbox üzerinde filtername değişmesi durumunda formun görünür 
        /// özelliklerini verilmiş olan filtername e göre ayarlamak için kullanılan constructor.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="filterName"></param>
        public FormFeatures(Form form, String filterName)
        {
            if (Main(form))
            {
                LoadDatainForm(filterName);
            }
        }

        /// <summary>
        /// formFeatures  için ana fonksiyondur. 
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private Boolean Main(Form form)
        {
            WriteLog.Write("Main -->> ", "is STARTED");
            _dataBaseNameMain = GetDatabaseName(Rsc.SubKey, Rsc.DalNameSql);

            _form = form;
            _panelLeft = GetPanel(Rsc.PanelLeft);
            _panelRight = GetPanel(Rsc.PanelRight);
            WriteLog.Write("Main -->> ", "is successfully FINISHED");
            return true;
        }

        /// <summary>
        /// Verilmiş olan panel name i söküp alır.
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        protected Panel GetPanel(String panelName)
        {
            WriteLog.Write("GetPanel() -->>", " is STARTED");
            foreach (Control control in _form.Controls)
            {
                if (control is Panel)
                {
                    if (control.Name == panelName)
                    {
                        return (Panel)control;
                    }
                }
            }
            WriteLog.Write("GetPanel() -->>", " is succesfully FINISHED.");
            return null;
        }

        /// <summary>
        /// Registryden database özelliklerini okur
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="dalName"></param>
        /// <returns></returns>
        public String GetDatabaseName(String subKey, String dalName)
        {
            try
            {
                WriteLog.Write("GetDatabaseName() -->>", " is Started");
                String dataBaseName = "";
                RegistryKey regKey;
                regKey = Registry.LocalMachine.OpenSubKey(subKey);
                dataBaseName = (String)regKey.GetValue(dalName);
                WriteLog.Write("GetDatabaseName() -->>", " database name is : " + dataBaseName);
                WriteLog.Write("GetDatabaseName() -->>", " is succesfully FINISHED.");
                return dataBaseName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// Verilen tablo ismi DataBase'de varsa "DROP" eder sonra yeniden "CREATE" eder
        /// </summary>
        /// <param name="dataBaseName">Database Adı</param>
        /// <param name="tableName">İşlem yapılacak tablo adı</param>
        public void DropandCreateColumnTbl()
        {
            try
            {
                WriteLog.Write("DropandCreateColumnTbl() -->> ", "is STARTED");
                String query = "IF  EXISTS (SELECT * FROM sys.objects WHERE " +
                                           "object_id = OBJECT_ID(N'" + Rsc.ColumnTbl + "') AND type in (N'U'))" +
                                           "DROP TABLE " + Rsc.ColumnTbl +
                                           " CREATE TABLE " + Rsc.ColumnTbl + "(" +
                                           Rsc.ColumnTbl_Name + " [varchar](900) NULL," +
                                           Rsc.ColumnTbl_Content + " [varchar](900) NULL" +
                                           ") ON [PRIMARY]";
                Database.Fast = false;
                Database.ExecuteNonQuery(_dataBaseNameMain, query);
                WriteLog.Write("DropandCreateColumnTbl() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// "logtable"'dan column isimlerini ve karşılığı olan değerleri seçer. 
        /// "ColumnTbl" tablosuna set eder
        /// </summary>
        /// <param name="dataBaseName">Database adı</param>
        /// <param name="logTable">Dataların alınacağı tablo adı</param>
        /// <param name="ColumnTbl">Dataların ekleneceği tablo adı</param>
        public void SelValues_InsertColTbl()
        {
            String querySelColVal, queryInsertValues;
            List<String> columnNames;
            DataTable dataTable;
            try
            {
                WriteLog.Write("SelValues_InsertColTbl() -->> ", "is STARTED");
                columnNames = GetColumnNames();
                if (columnNames != null)
                {
                    foreach (String columnName in columnNames)
                    {
                        querySelColVal = "SELECT DISTINCT (" + columnName + ") FROM " + Rsc.LogTbl;
                        dataTable = GetDataTable(querySelColVal);
                        if (dataTable != null)
                        {
                            if (dataTable.Rows.Count > 0)
                            {
                                foreach (DataRow columnValue in dataTable.Rows)
                                {
                                    queryInsertValues = "INSERT INTO " +
                                                        Rsc.ColumnTbl +
                                                        " VALUES( '" +
                                                        columnName + "' , '" +
                                                        columnValue.ItemArray[0].ToString() + "')";

                                    WriteLog.Write("SelValues_InsertColTbl() -->> ", queryInsertValues);
                                    Database.ExecuteNonQuery(_dataBaseNameMain, queryInsertValues);
                                }
                            }
                        }
                    }
                }
                WriteLog.Write("SelValues_InsertColTbl() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// verilmiş olan Log tablosunun tüm kolonlarını getirir.
        /// </summary>
        /// <returns></returns>
        public List<String> GetColumnNames()
        {
            String querySelColumnNames;
            List<String> columnNames;
            DataTable dataTable;

            try
            {
                WriteLog.Write("GetColumnNames() -->> ", "is STARTED");
                columnNames = new List<String>();
                querySelColumnNames = "SELECT * FROM " + Rsc.LogTbl + " WHERE 1=0";
                dataTable = GetDataTable(querySelColumnNames);
                if (dataTable != null)
                {
                    foreach (DataColumn item in dataTable.Columns)
                    {
                        if (!Rsc.NonUsedColNames.Contains("-" + item + "-"))
                        {
                            columnNames.Add(item.ColumnName);
                        }
                    }
                    columnNamesList = columnNames;
                    return columnNames;
                }
                WriteLog.Write("GetColumnNames() -->> ", "is successfully FINISHED.");
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// panel1 için combobox kontrolleri oluşturur.
        /// </summary>
        private void CreateControls()
        {
            Int32 positionX;
            Int32 positionY;
            ComboBox comboBox;
            Label label;
            try
            {
                WriteLog.Write("CreateControls() -->> ", "is STARTED.");
                positionX = 20;
                positionY = 50;
                foreach (String columnName in columnNamesList)
                {
                    label = new Label();
                    label.AutoSize = true;
                    label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
                    label.Location = new System.Drawing.Point(positionX, positionY + 7);
                    label.Name = "lbl" + columnName;
                    label.Size = new System.Drawing.Size(51, 20);
                    label.Text = UpdateLabelText(columnName);

                    _panelLeft.Controls.Add(label);

                    comboBox = new ComboBox();
                    comboBox.FormattingEnabled = true;
                    comboBox.Location = new System.Drawing.Point(positionX + 130, positionY);
                    comboBox.Name = "cmbx" + columnName;
                    comboBox.Size = new System.Drawing.Size(180, 21);

                    _panelLeft.Controls.Add(comboBox);
                    positionY = positionY + 30;
                }
                WriteLog.Write("CreateControls() -->> ", "is successfully FINISHED.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private String UpdateLabelText(String columnName)
        {
            try
            {
                switch (columnName)
                {
                    case "EVENTCATEGORY": return "CATEGORY";
                    case "EVENTTYPE": return "FUNCTIONNAME";
                    case "USERID": return "USERNAME";
                    case "CUSTOMSTR1": return "PROJECT";
                    case "CUSTOMSTR2": return "WINDOWNAME";
                    case "CUSTOMSTR3": return "PREIMAGE";
                    case "CUSTOMSTR4": return "POSTIMAGE";
                    case "CUSTOMSTR5": return "DATAWINDOWNAME";
                    case "CUSTOMSTR7": return "PRIMARYKEYVALUE";
                    case "CUSTOMSTR8": return "CUSTOM1";
                    case "CUSTOMSTR9": return "CUSTOM2";
                    case "CUSTOMSTR10": return "CUSTOM3";
                    default: return columnName;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Panel1 e kolon içeriklerini ekler(sorguda distinct çekerek)
        /// </summary>
        protected void LoadDatainForm()
        {
            String query = "";
            DataTable dataTable;

            try
            {
                WriteLog.Write("LoadDatainForm() -->> ", "is STARTED.");
                foreach (String columnName in columnNamesList)
                {
                    query = " SELECT DISTINCT (" +
                            Rsc.ColumnTbl_Content +
                            ") FROM " +
                            Rsc.ColumnTbl +
                            " WHERE " +
                            Rsc.ColumnTbl_Name + "='" + columnName + "'";
                    dataTable = GetDataTable(query);
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (Control control in _panelLeft.Controls)
                        {
                            if (control is ComboBox)
                            {
                                if (control.Name == "cmbx" + columnName)
                                {
                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        ((ComboBox)control).Items.Add(row.ItemArray[0].ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                WriteLog.Write("LoadDatainForm() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gelen filter name göre panel1 e dataları yükler
        /// </summary>
        /// <param name="filterName"></param>
        public void LoadDatainForm(String filterName)
        {
            String querySelFilterID;
            String querySelContent;
            String filterID, filterFuncName, filterActType, filterTarget, filterTblName;
            DataTable dataTable;

            try
            {
                WriteLog.Write("LoadDatainForm() -->> ", "is STARTED.");
                if (!String.IsNullOrEmpty(filterName))
                {

                    querySelFilterID = " SELECT " +
                                       Rsc.Filters_ID + "," +
                                       Rsc.Filters_UsedFunc + "," +
                                       Rsc.Filters_ActType + "," +
                                       Rsc.Filters_Target + "," +
                                       Rsc.Filters_TableName +
                                       " FROM " + Rsc.FiltersTbl +
                                       " WHERE " + Rsc.Filters_FilterName + "='" + filterName + "'";
                    dataTable = GetDataTable(querySelFilterID);
                    if (dataTable.Rows.Count > 0)
                    {
                        filterID = dataTable.Rows[0].ItemArray[0].ToString();
                        filterFuncName = dataTable.Rows[0].ItemArray[1].ToString();
                        filterActType = dataTable.Rows[0].ItemArray[2].ToString();
                        filterTarget = dataTable.Rows[0].ItemArray[3].ToString();
                        filterTblName = dataTable.Rows[0].ItemArray[4].ToString();

                        LoadFunctionName(filterFuncName);
                        LoadTarget(filterTarget);
                        LoadRuntimeandPeriod();
                        LoadTableName(filterTblName);

                        querySelContent = " SELECT " + Rsc.Columns_Constants_ColumnName + "," + Rsc.Columns_Constants_Constant +
                                          " FROM " + Rsc.Columns_ConstantsTbl + " WHERE " + Rsc.Columns_Constants_FilterID + "=" + filterID;
                        dataTable = GetDataTable(querySelContent);
                        if (dataTable != null)
                        {
                            if (dataTable.Rows.Count > 0)
                            {
                                foreach (Control control in _panelLeft.Controls)
                                {
                                    if (control is ComboBox)
                                    {
                                        foreach (DataRow row in dataTable.Rows)
                                        {
                                            if (control.Name == "cmbx" + row.ItemArray[0].ToString())
                                            {
                                                ((ComboBox)control).Text = row.ItemArray[1].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                WriteLog.Write("LoadDatainForm() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Filtername in değişmesi halinde table name i yükler
        /// </summary>
        /// <param name="filterTblName"></param>
        private void LoadTableName(String filterTblName)
        {
            try
            {
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("txtBoxTblName"))
                    {
                        ((TextBox)control).Text = filterTblName;
                        break;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Filtername lerin tamamını database den alır ve cmBoxFilterName içine yükler
        /// </summary>
        public void LoadFilterName()
        {
            String querySelFilterName;
            DataTable dataTable;
            try
            {
                WriteLog.Write("LoadFilterName() -->> ", "is STARTED.");
                querySelFilterName = "SELECT " + Rsc.Filters_FilterName + " FROM " + Rsc.FiltersTbl;
                dataTable = GetDataTable(querySelFilterName);
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("cmBoxFilterName"))
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            ((ComboBox)control).Items.Add(row.ItemArray[0].ToString());
                        }
                        break;
                    }
                }
                WriteLog.Write("LoadFilterName() -->> ", "is successfully FINISHED.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Verilmiş olan filtername için uygulanacak function name i txtBoxFunctionName içine yükler
        /// </summary>
        /// <param name="filterFuncName"></param>
        public void LoadFunctionName(String filterFuncName)
        {
            try
            {
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("txtBoxFunctionName"))
                    {
                        ((TextBox)control).Text = filterFuncName;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Verilmiş olan filtername için uygulanacak function name i txtBoxTarget  içine yükler
        /// </summary>
        /// <param name="filterTarget"></param>
        public void LoadTarget(String filterTarget)
        {
            try
            {
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("txtBoxTarget"))
                    {
                        ((TextBox)control).Text = filterTarget;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Verilmiş olan filtername için uygulanacak 
        /// runtime i numUpDownHour,numUpDownMinute,numUpDownPeriod  içine yükler
        /// </summary>
        public void LoadRuntimeandPeriod()
        {
            String querySelRuntime;
            DataTable dataTable;
            String filterRuntime;
            String period;
            try
            {
                querySelRuntime = "SELECT " +
                                  Rsc.Configuration_Runtime + "," +
                                  Rsc.Configuration_Period +
                                  " FROM " + Rsc.Configuration;
                dataTable = GetDataTable(querySelRuntime);
                if (dataTable != null)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        filterRuntime = dataTable.Rows[0].ItemArray[0].ToString();
                        period = dataTable.Rows[0].ItemArray[1].ToString();
                        foreach (Control control in _panelRight.Controls)
                        {
                            if (control.Name.Equals("numUpDownHour"))
                            {
                                ((NumericUpDown)control).Value = Convert.ToDateTime(filterRuntime).Hour;
                            }
                            if (control.Name.Equals("numUpDownMinute"))
                            {
                                ((NumericUpDown)control).Value = Convert.ToDateTime(filterRuntime).Minute;
                            }
                            if (control.Name == "numUpDownPeriod")
                            {
                                ((NumericUpDown)control).Value = Convert.ToInt32(period);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Verilmiş olan query i uygular ve datatable i döndürür.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private DataTable GetDataTable(String query)
        {
            try
            {
                WriteLog.Write("GetDataTable() -->>", " function is Started");
                Database.Fast = false;
                DbCommand dbCommand = null;
                DataSet dataSet = new DataSet();
                IDataAdapter idataAdapt = Database.GetDataAdapter(_dataBaseNameMain, query, out dbCommand);
                idataAdapt.Fill(dataSet);
                Database.Drop(ref dbCommand);
                WriteLog.Write("GetDataTable() -->>", "is successfully FINISHED.");
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                WriteLog.Write("GetDataTable() -->>", ex);
                throw;
            }
        }


    }
}
