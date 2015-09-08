using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace AviraRecorder
{
    class AccessConnection
    {   
        private OleDbConnection connection;

        public void OpenAccessConnection(string dbname)
        {
            //string dbconnectionstring = remotehost + "\\" + dbPath + "\\" + dbname + ";" + "User Id=" + user + ";" + "Password=" + password + ";";
            string dbconnectionstring = dbname;
            connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; data source=" + dbconnectionstring);
            connection.Open();
        }

        public OleDbDataReader ExecuteAccessQuery(string queryString)
        {
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = queryString;
            return command.ExecuteReader();
        }

        public void CloseAccessConnection()
        {
            connection.Close();
        }
    }
}
