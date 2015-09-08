using System;
using System.Data.Common;
using System.Data.OracleClient;
using Natek.Recorders.Remote.Database;

namespace Natek.Recorders.Remote.Unified.Database.Oracle
{
    public class OracleRecorderContext : DbRecorderContext
    {
        public override DbConnection CreateConnection(ref Exception error, bool openInitially = false)
        {
            try
            {
                var conn =
                    new OracleConnection(string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2})));User Id={3};Password={4};"
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
