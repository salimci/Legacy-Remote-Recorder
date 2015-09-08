using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace NetCadPostGreRecorder
{
    class PostGreConnection
    {
        private NpgsqlConnection connection = null;

        public void OpenPostGreConnection(string remotehost, string username, string password, string dbname)
        {
            //string connstring = String.Format("Server={0};Port={1};" + "User Id={2};Password={3};Database={4};", remotehost, 5432, username, password, dbname);
            string connstring = "Server=10.10.11.31;Port=5432;User Id=postgres;Password=ntc123*;Database=ncauth;Timeout=15;CommandTimeout=15;Pooling=False;MinPoolSize=1;MaxPoolSize=20;ConnectionLifeTime=15;";
            // Making connection with Npgsql provider
            connection = new NpgsqlConnection(connstring);
            connection.Open();
        }

        public NpgsqlDataReader ExecutePostGreQuery(string queryString, ref NpgsqlCommand command)
        {
            command.Connection = connection;
            command.CommandText = queryString;
            return command.ExecuteReader();
        }

        public void ClosePostGreConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }

            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
