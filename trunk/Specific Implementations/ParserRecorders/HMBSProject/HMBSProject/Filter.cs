using System;
using System.Collections.Generic;
using DAL;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;
using System.Data.Common;

namespace NatekLogService
{
    class Filter
    {
        String _dataBaseNameMain;
        Form _form;
        Panel _panelLeft;
        Panel _panelRight;
        String _dateFormat;

        public Filter()
        {
            _dateFormat = "yyyy/MM/dd HH:mm:ss";
        }

        public Filter(Form form)
        {
            _dateFormat = "yyyy/MM/dd HH:mm:ss";
            _form = form;
            _dataBaseNameMain = GetDatabaseName(Rsc.SubKey, Rsc.DalNameSql);
            _panelLeft = GetPanel(Rsc.PanelLeft);
            _panelRight = GetPanel(Rsc.PanelRight);
        }

        public Boolean AddFilter()
        {
            FilterProperties filterProp;
            String queryInsertFilter;
            String queryControlFilterName;
            DataTable dataTable;
            try
            {
                filterProp = GetFilterProperties();
                if (filterProp != null)
                {
                    queryControlFilterName = "SELECT * FROM " +
                                             Rsc.FiltersTbl +
                                             " WHERE " +
                                             Rsc.Filters_FilterName + "='" + filterProp.filterName + "'";

                    dataTable = GetDataTable(_dataBaseNameMain, queryControlFilterName);
                    if (dataTable.Rows.Count > 0)
                    {
                        MessageBox.Show("This filter name is already exists. Please entry a valid filter name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                    else
                    {
                        queryInsertFilter = "INSERT INTO " +
                                            Rsc.FiltersTbl + "(" +
                                            Rsc.Filters_FilterName + "," +
                                            Rsc.Filters_UsedFunc + "," +
                                            Rsc.Filters_ActType + "," +
                                            Rsc.Filters_Target + "," +
                                            Rsc.Filters_Desc + "," +
                                            Rsc.Filters_TableName +
                                            ") VALUES ('" +
                                            filterProp.filterName + "','" +
                                            filterProp.functionName + "'," +
                                            filterProp.actionType + ",'" +
                                            filterProp.target + "','" +
                                            filterProp.description + "','" +
                                            filterProp.tableName + "')";


                        Database.Fast = false;
                        Database.ExecuteNonQuery(_dataBaseNameMain, queryInsertFilter);

                        AddColumnConstans();
                        MessageBox.Show("Filter has added successfully", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private FilterProperties GetFilterProperties()
        {
            FilterProperties filterProp;
            String runTimeHour = "", runTimeMinute = "", actionName = "";
            try
            {
                filterProp = new FilterProperties();

                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("txtBoxTblName"))
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            filterProp.tableName = control.Text;
                        }
                        else
                        {
                            MessageBox.Show("Please write a table name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                    }
                    if (control.Name.Equals("cmBoxFilterName"))
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            filterProp.filterName = control.Text;
                        }
                        else
                        {
                            MessageBox.Show("Please write a filter name","Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                    }
                    if (control.Name.Equals("txtBoxFunctionName"))
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            filterProp.functionName = control.Text;
                        }
                        else
                        {
                            MessageBox.Show("Please write a function name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                    }
                    if (control.Name.Equals("cmBoxActionType"))
                    {
                        actionName = control.Text;
                    }
                    if (control.Name.Equals("numUpDownHour"))
                    {
                        runTimeHour = control.Text;
                    }
                    if (control.Name.Equals("numUpDownMinute"))
                    {
                        runTimeMinute = control.Text;
                    }
                    if (control.Name.Equals("txtBoxTarget"))
                    {
                        filterProp.target = control.Text;
                    }
                    if (control.Name.Equals("txtBoxDescription"))
                    {
                        filterProp.description = control.Text;
                    }
                }

                filterProp.runTime = Convert.ToDateTime(runTimeHour + ":" + runTimeMinute);
                filterProp.runTime = filterProp.runTime.AddDays(1);
                filterProp.actionType = GetActionNo(actionName);

                return filterProp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddColumnConstans()
        {
            String querySelectMaxID, maxID;
            String queryInsert, _queryInsert;
            String colName = "", constantValue = "";
            DataTable dataTable;
            try
            {
                querySelectMaxID = "SELECT MAX(ID) FROM " + Rsc.FiltersTbl;
                queryInsert = "INSERT INTO " + Rsc.Columns_ConstantsTbl + " VALUES(";
                dataTable = GetDataTable(_dataBaseNameMain, querySelectMaxID);
                maxID = dataTable.Rows[0].ItemArray[0].ToString();
                queryInsert = queryInsert + maxID + ",'";

                foreach (Control control in _panelLeft.Controls)
                {
                    if (control is ComboBox)
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            _queryInsert = queryInsert;
                            colName = control.Name.Remove(0, 4);
                            constantValue = control.Text;
                            _queryInsert = _queryInsert + colName + "','" + constantValue + "')";
                            Database.ExecuteNonQuery(_dataBaseNameMain, _queryInsert);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected Panel GetPanel(String panelName)
        {
            try
            {
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
                return null;
            }
            catch (Exception)
            {
                throw;
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
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        protected Int32 GetActionNo(String actionName)
        {
            String queryGetActionName;
            DataTable dataTable;
            try
            {
                if (String.IsNullOrEmpty(actionName))
                {
                    actionName = "MAIL";
                }
                queryGetActionName = "SELECT * FROM " +
                                     Rsc.ActionTbl +
                                     " WHERE " +
                                     Rsc.ActionTbl_ActionName + "='" + actionName + "'";

                dataTable = GetDataTable(_dataBaseNameMain, queryGetActionName);
                return Convert.ToInt32(dataTable.Rows[0].ItemArray[0].ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public Boolean DeleteFilter()
        {
            String filterID;
            String filterName;
            String querySelFilterID;
            String queryDeleteFilter;
            String queryDeleteFilterColConstans;
            DataTable dataTable;

            try
            {
                filterName = GetFilterName();
                if (!String.IsNullOrEmpty(filterName))
                {
                    querySelFilterID = "SELECT " +
                                       Rsc.Filters_ID +
                                       " FROM " +
                                       Rsc.FiltersTbl +
                                       " WHERE " +
                                       Rsc.Filters_FilterName + "='" + filterName + "'";

                    dataTable = GetDataTable(_dataBaseNameMain, querySelFilterID);
                    if (dataTable.Rows.Count > 0)
                    {
                        filterID = dataTable.Rows[0].ItemArray[0].ToString();
                        queryDeleteFilter = "DELETE FROM " +
                                            Rsc.FiltersTbl +
                                            " WHERE " +
                                            Rsc.Filters_ID + "=" + filterID;
                        queryDeleteFilterColConstans = "DELETE FROM " +
                                                       Rsc.Columns_ConstantsTbl +
                                                       " WHERE " +
                                                       Rsc.Columns_Constants_FilterID + "=" + filterID;

                        Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilter);
                        Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilterColConstans);
                        ClearForm();
                        MessageBox.Show("Selected filter has deleted successfully", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("This filter does NOT exists.Please write a valid filter name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public Boolean DeleteFilter(String filterName)
        {
            String filterID;
            String querySelFilterID;
            String queryDeleteFilter;
            String queryDeleteFilterColConstans;
            DataTable dataTable;

            try
            {
                if (!String.IsNullOrEmpty(filterName))
                {
                    querySelFilterID = "SELECT " +
                                       Rsc.Filters_ID +
                                       " FROM " +
                                       Rsc.FiltersTbl +
                                       " WHERE " +
                                       Rsc.Filters_FilterName + "='" + filterName + "'";

                    dataTable = GetDataTable(_dataBaseNameMain, querySelFilterID);
                    if (dataTable.Rows.Count > 0)
                    {
                        filterID = dataTable.Rows[0].ItemArray[0].ToString();
                        queryDeleteFilter = "DELETE FROM " +
                                            Rsc.FiltersTbl +
                                            " WHERE " +
                                            Rsc.Filters_ID + "=" + filterID;
                        queryDeleteFilterColConstans = "DELETE FROM " +
                                                       Rsc.Columns_ConstantsTbl +
                                                       " WHERE " +
                                                       Rsc.Columns_Constants_FilterID + "=" + filterID;

                        Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilter);
                        Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilterColConstans);
                        ClearForm();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("This filter does NOT exists.Please write a valid filter name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public Boolean UpdateFilter()
        {
            String filterID;
            String filterName;
            String querySelFilterID, queryControlFilterName;
            DataTable dataTable;
            try
            {
                filterName = GetFilterName();
                if (!String.IsNullOrEmpty(filterName))
                {
                    queryControlFilterName = "SELECT * FROM " +
                                              Rsc.FiltersTbl +
                                              " WHERE " +
                                              Rsc.Filters_FilterName + "='" + filterName + "'";

                    dataTable = GetDataTable(_dataBaseNameMain, queryControlFilterName);
                    if (dataTable.Rows.Count > 0)
                    {
                        querySelFilterID = " SELECT " +
                                           Rsc.Filters_ID +
                                           " FROM " +
                                           Rsc.FiltersTbl +
                                           " WHERE " +
                                           Rsc.Filters_FilterName + "='" + filterName + "'";
                        dataTable = GetDataTable(_dataBaseNameMain, querySelFilterID);

                        if (dataTable.Rows.Count > 0)
                        {
                            filterID = dataTable.Rows[0].ItemArray[0].ToString();
                            UpdateFilterProp(filterID);
                            UpdateFilterConstants(filterID);
                        }
                        MessageBox.Show("This filter has updated successfully", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("This filter name does NOT exists. Entry a valid Filter Name.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Please write a valid filter name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public Boolean UpdateFilter(String filterName)
        {
            String filterID;

            String querySelFilterID, queryControlFilterName;
            DataTable dataTable;
            try
            {
                if (!String.IsNullOrEmpty(filterName))
                {
                    queryControlFilterName = "SELECT * FROM " +
                                             Rsc.FiltersTbl +
                                             " WHERE " +
                                             Rsc.Filters_FilterName + "='" + filterName + "'";

                    dataTable = GetDataTable(_dataBaseNameMain, queryControlFilterName);
                    if (dataTable.Rows.Count > 0)
                    {
                        querySelFilterID = " SELECT " +
                                           Rsc.Filters_ID +
                                           " FROM " +
                                           Rsc.FiltersTbl +
                                           " WHERE " +
                                           Rsc.Filters_FilterName + "='" + filterName + "'";
                        dataTable = GetDataTable(_dataBaseNameMain, querySelFilterID);

                        if (dataTable.Rows.Count > 0)
                        {
                            filterID = dataTable.Rows[0].ItemArray[0].ToString();
                            UpdateFilterProp(filterID);
                            UpdateFilterConstants(filterID);
                        }
                    }
                    else
                    {
                        MessageBox.Show("This filter name does NOT exists. Entry a valid Filter Name.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                }
                else
                {
                    MessageBox.Show("Please write a valid filter name", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        protected void UpdateFilterProp(String filterID)
        {
            String queryDeleteFilter;
            FilterProperties filterProp;
            try
            {
                filterProp = GetFilterProperties();
                if (filterProp != null)
                {
                    queryDeleteFilter = "UPDATE " + Rsc.FiltersTbl + " SET " +
                                        Rsc.Filters_UsedFunc + "='" + filterProp.functionName + "'," +
                                        Rsc.Filters_ActType + "='" + filterProp.actionType + "'," +
                                        //Rsc.Filters_RunTime + "=CONVERT(DATETIME,'" + filterProp.runTime + "',102)," +
                                        Rsc.Filters_Target + "='" + filterProp.target + "'," +
                                        Rsc.Filters_Desc + "='" + filterProp.description + "'," +
                                        Rsc.Filters_TableName + "='" + filterProp.tableName + "'" +
                                        " WHERE " + Rsc.Filters_ID + "=" + filterID;
                    Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void UpdateFilterConstants(String filterID)
        {
            String queryDeleteFilterConstans;
            String colName = "", constantValue = "";
            String queryInsert, _queryInsert;
            try
            {
                queryDeleteFilterConstans = " DELETE FROM " +
                                            Rsc.Columns_ConstantsTbl +
                                            " WHERE " +
                                            Rsc.Columns_Constants_FilterID + "=" + filterID;
                Database.ExecuteNonQuery(_dataBaseNameMain, queryDeleteFilterConstans);
                queryInsert = "INSERT INTO " +
                               Rsc.Columns_ConstantsTbl +
                               " VALUES(";

                queryInsert = queryInsert + filterID + ",'";

                foreach (Control control in _panelLeft.Controls)
                {
                    if (control is ComboBox)
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            _queryInsert = queryInsert;
                            colName = control.Name.Remove(0, 4);
                            constantValue = control.Text;
                            _queryInsert = _queryInsert + colName + "','" + constantValue + "')";
                            Database.ExecuteNonQuery(_dataBaseNameMain, _queryInsert);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private String GetFilterName()
        {
            try
            {
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name.Equals("cmBoxFilterName"))
                    {
                        if (!String.IsNullOrEmpty(control.Text))
                        {
                            return control.Text;
                        }
                        else
                        {
                            MessageBox.Show("Please write a filter name");
                            return null;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void ClearForm()
        {
            try
            {
                foreach (Control control in _panelLeft.Controls)
                {
                    if (control is ComboBox)
                    {
                        control.Text = "";
                    }
                }
                foreach (Control control in _panelRight.Controls)
                {
                    if (control is ComboBox || control is TextBox)
                    {
                        control.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateRuntime()
        {
            String runTimeHour = "", runTimeMinute = "";
            DateTime runTime;
            String period = "";
            String queryInsertRunTime;
            try
            {
                foreach (Control control in _panelRight.Controls)
                {
                    if (control.Name == "numUpDownHour")
                    {
                        runTimeHour = control.Text;
                    }
                    if (control.Name == "numUpDownMinute")
                    {
                        runTimeMinute = control.Text;
                    }
                    if (control.Name == "numUpDownPeriod")
                    {
                        period = control.Text;
                    }
                }
                if (String.IsNullOrEmpty(period))
                {
                    period = "0";
                }
                runTime =Convert.ToDateTime(runTimeHour + ":" + runTimeMinute);

                queryInsertRunTime = " UPDATE " +
                                     Rsc.Configuration +
                                     " SET " +
                                     Rsc.Configuration_Runtime + "=CONVERT(DATETIME,'" + runTime.ToString(_dateFormat) + "',102)," +
                                     Rsc.Configuration_Period + "=" + period +
                                     " WHERE 1=1";

                Database.ExecuteNonQuery(_dataBaseNameMain, queryInsertRunTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class FilterProperties
        {
            public String filterName;
            public String functionName;
            public Int32 actionType;
            public DateTime runTime;
            public String target;
            public String description;
            public String tableName;
        }

    }
}
