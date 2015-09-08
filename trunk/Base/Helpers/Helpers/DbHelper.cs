using System;
using System.Data.Common;
using System.Data;

namespace Natek.Helpers.Database
{
    public static class DbHelper
    {
        public static DbTransaction BeginTransaction(DbConnection con, out int dirty)
        {
            dirty = 0;
            return con.BeginTransaction();
        }

        public static T GetField<T>(DbDataReader reader, int index, T nullValue)
        {
            if (reader.IsDBNull(index))
            {
                return nullValue;
            }
            return (T)reader.GetValue(index);
        }

        public static object IfDBNull<T>(T value, T nullCompare)
        {
            if (value == null)
            {
                if (nullCompare == null)
                {
                    return DBNull.Value;
                }
                return value;
            }
            if (value.Equals(nullCompare))
            {
                return DBNull.Value;
            }
            return value;
        }

        public static object IfDBNull<T>(T value) where T : class
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            return value;
        }

        public static DbParameter AddParameter(DbCommand cmd, string name, DbType type, ParameterDirection direction)
        {
            DbParameter param;

            param = cmd.CreateParameter();
            param.DbType = type;
            param.ParameterName = name;
            param.Direction = direction;
            cmd.Parameters.Add(param);
            return param;
        }
    }
}
