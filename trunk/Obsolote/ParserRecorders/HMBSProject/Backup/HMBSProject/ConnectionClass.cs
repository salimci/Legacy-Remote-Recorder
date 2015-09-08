using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.Common;
using Microsoft.Win32;
using System.IO;
using System.Data.SqlClient;

namespace NatekLogService
{
    class ConnectionClass
    {
        String _dataSource;
        String _dataBase;
        String _userName;
        String _password;
        Int32 _type;
        OleDbConnection oleDbCon;

        private void ReadRegistry()
        {
            try
            {
                RegistryKey regKey;
                regKey = Registry.LocalMachine.OpenSubKey(Rsc.SubKey2);
                _dataSource = (String)regKey.GetValue("Host");
                _dataBase = (String)regKey.GetValue("DB");
                _userName = (String)regKey.GetValue("User");
                _password = (String)regKey.GetValue("Password");
                _type = (Int32)regKey.GetValue("Type");
            }
            catch (Exception ex)
            {
                WriteLogFile("ConnectionClass -->> ReadRegistry", ex);
            }
        }

        public OleDbConnection Connect()
        {
            try
            {
                ReadRegistry();
                String providerName;
                String conStr = "";
                if (_type == 1)
                {
                    providerName = "MSDAora";
                    conStr = "Provider=" + providerName +
                           "; Data Source=" + _dataSource +
                           ";Persist Security Info=True;User ID=" + _userName +
                           ";password=" + _password;
                }
                else
                {
                    providerName = "SQLOLEDB";
                    conStr = "Provider=" + providerName +
                               "; Data Source=" + _dataSource +
                               ";Initial Catalog=" + _dataBase +
                               ";Persist Security Info=True;User ID=" + _userName +
                               ";password=" + _password;

                }
                oleDbCon = new OleDbConnection(conStr);
                oleDbCon.Open();
                return oleDbCon;

            }
            catch (Exception ex)
            {
                WriteLogFile("ConnectionClass -->> Connect", ex);
                return null;
            }
        }

        public OleDbDataReader GetDataReader(String sqlString)
        {
            try
            {
                OleDbCommand oraCommand = oleDbCon.CreateCommand();
                oraCommand.CommandText = sqlString;
                OleDbDataReader oraDataReader = oraCommand.ExecuteReader();
                return oraDataReader;
            }
            catch (Exception ex)
            {
                WriteLogFile("ConnectionClass -->> GetData", ex);
                return null;
            }
        }

        public SqlConnection ConnectMsSql()
        {
            try
            {
                ReadRegistry();
                SqlConnection sqlCon = new SqlConnection("Data Source=" + _dataSource +
                                               ";Persist Security Info=True;User ID=" + _userName +
                                               ";password=" + _password);

                sqlCon.Open();
                return sqlCon;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void WriteLogFile(String functionName, Exception ex)
        {
            StreamWriter LogFile = File.AppendText(ControlProcessorTypex86() + "\\Natek Alert.log");
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine(DateTime.Now + " " + functionName + "  : " + ex.ToString());
            LogFile.WriteLine("###############################################################################################################");
            LogFile.WriteLine();
            LogFileSizeControl();
            LogFile.Close();
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
