using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

namespace EsetNod32Recorder
{
    class AccessConnection
    {
        private OleDbConnection connection;

        public void OpenAccessConnection(string remotehost, string dbPath, string dbname)
        {
            //string dbconnectionstring = remotehost + "\\" + dbPath + "\\" + dbname + ";" + "User Id=" + user + ";" + "Password=" + password + ";";
            string dbconnectionstring = remotehost + "\\" + dbPath + "\\" + dbname + ";";

            connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; data source=" + dbconnectionstring);
            connection.Open();
        }

        public OleDbDataReader ExecuteAccessQuery(string queryString)
        {
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = queryString;
            return command.ExecuteReader();
        }

        public Int64 ExecuteScalarQuery(string queryString)
        {
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;
            command.CommandText = queryString;
            return Convert.ToInt64(command.ExecuteScalar());
        }

        public void CloseAccessConnection()
        {
            connection.Close();
        }
    }
}
