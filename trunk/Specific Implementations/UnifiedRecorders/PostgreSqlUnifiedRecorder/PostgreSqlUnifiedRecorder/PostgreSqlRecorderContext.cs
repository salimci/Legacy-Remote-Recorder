using System;
using System.Data.Common;
using Natek.Recorders.Remote.Database;
using Npgsql;

namespace Natek.Recorders.Remote.Unified.Database.PostgreSql
{
    public class PostgreSqlRecorderContext : DbRecorderContext
    {
        public override DbConnection CreateConnection(ref Exception error, bool openInitially = false)
        {
            try
            {
                var conn =
                    new NpgsqlConnection(string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4}"
                                                       , ContextVariables[ContextKeys["SERVER"]]
                                                       , ContextVariables[ContextKeys["PORT"]]
                                                       , ContextVariables[ContextKeys["USER"]]
                                                       , ContextVariables[ContextKeys["PASSWORD"]]
                                                       , ContextVariables[ContextKeys["DATABASE"]]));
                if (openInitially)
                    conn.Open();
                return conn;
            }
            catch (Exception e)
            {
                error = e;
                return null;
            }
        }
    }
}
