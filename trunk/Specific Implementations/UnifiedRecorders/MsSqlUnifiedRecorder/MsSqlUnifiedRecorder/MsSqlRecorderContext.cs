using System;
using System.Data.Common;
using System.Data.SqlClient;
using Natek.Recorders.Remote.Database;

namespace Natek.Recorders.Remote.Unified.Database.MsSql
{
    public class MsSqlRecorderContext : DbRecorderContext
    {
        public override DbConnection CreateConnection(ref Exception error, bool openInitially = false)
        {
            try
            {
                var conn =
                    new SqlConnection(string.Format("Data Source={0},{1}; Initial Catalog={2};User Id={3};Password={4};"
                                                       , ContextVariables[ContextKeys["SERVER"]]
                                                       , ContextVariables[ContextKeys["PORT"]]
                                                       , ContextVariables[ContextKeys["DATABASE"]]
                                                       , ContextVariables[ContextKeys["USER"]]
                                                       , ContextVariables[ContextKeys["PASSWORD"]]));
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
