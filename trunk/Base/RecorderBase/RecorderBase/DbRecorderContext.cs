using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Natek.Helpers;
using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote.Database
{
    public abstract class DbRecorderContext : RecorderContext
    {
        protected Dictionary<string, string> contextKeys;
        protected Dictionary<string, string> contextVariables;
        protected Dictionary<string, DbCommand> commands;
        protected Dictionary<string, DbCommandBuilder> commandBuilders;
        protected Dictionary<string, DbConnection> connections;
        protected Dictionary<string, DbConnectionStringBuilder> connectionStringBuilders;
        protected Dictionary<string, DbDataAdapter> adapters;
        protected Dictionary<string, DbDataReader> readers;
        protected Dictionary<string, DbDataRecord> records;
        protected Dictionary<string, DbDataSourceEnumerator> dataSourceEnumerators;
        protected Dictionary<string, DbEnumerator> enumerators;
        protected Dictionary<string, DbParameter> parameters;
        protected Dictionary<string, DbParameterCollection> parameterCollections;
        protected Dictionary<string, DbProviderConfigurationHandler> providerConfigurationHandlers;
        protected Dictionary<string, DbProviderFactoriesConfigurationHandler> providerFactoriesConfigurationHandlers;
        protected Dictionary<string, DbProviderFactory> providerFactories;
        protected Dictionary<string, DbProviderSpecificTypePropertyAttribute> providerSpecificTypePropertyAttributes;
        protected Dictionary<string, DbTransaction> transactions;

        public DbRecorderContext()
        {
            contextKeys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            contextVariables = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            commands = new Dictionary<string, DbCommand>(StringComparer.InvariantCultureIgnoreCase);
            commandBuilders = new Dictionary<string, DbCommandBuilder>(StringComparer.InvariantCultureIgnoreCase);
            connections = new Dictionary<string, DbConnection>(StringComparer.InvariantCultureIgnoreCase);
            connectionStringBuilders = new Dictionary<string, DbConnectionStringBuilder>(StringComparer.InvariantCultureIgnoreCase);
            adapters = new Dictionary<string, DbDataAdapter>(StringComparer.InvariantCultureIgnoreCase);
            readers = new Dictionary<string, DbDataReader>(StringComparer.InvariantCultureIgnoreCase);
            records = new Dictionary<string, DbDataRecord>(StringComparer.InvariantCultureIgnoreCase);
            dataSourceEnumerators = new Dictionary<string, DbDataSourceEnumerator>(StringComparer.InvariantCultureIgnoreCase);
            enumerators = new Dictionary<string, DbEnumerator>(StringComparer.InvariantCultureIgnoreCase);
            parameters = new Dictionary<string, DbParameter>(StringComparer.InvariantCultureIgnoreCase);
            parameterCollections = new Dictionary<string, DbParameterCollection>(StringComparer.InvariantCultureIgnoreCase);
            providerConfigurationHandlers = new Dictionary<string, DbProviderConfigurationHandler>(StringComparer.InvariantCultureIgnoreCase);
            providerFactoriesConfigurationHandlers = new Dictionary<string, DbProviderFactoriesConfigurationHandler>(StringComparer.InvariantCultureIgnoreCase);
            providerFactories = new Dictionary<string, DbProviderFactory>(StringComparer.InvariantCultureIgnoreCase);
            providerSpecificTypePropertyAttributes = new Dictionary<string, DbProviderSpecificTypePropertyAttribute>(StringComparer.InvariantCultureIgnoreCase);
            transactions = new Dictionary<string, DbTransaction>(StringComparer.InvariantCultureIgnoreCase);
        }

        public abstract DbConnection CreateConnection(ref Exception error, bool openInitially = false);

        public Dictionary<string, string> ContextKeys { get { return contextKeys; } }

        public Dictionary<string, string> ContextVariables { get { return contextVariables; } }

        public Dictionary<string, string> ExternalVariables { get; set; }

        public Dictionary<string, DbCommand> Commands { get { return commands; } }

        public Dictionary<string, DbCommandBuilder> CommandBuilders { get { return commandBuilders; } }

        public Dictionary<string, DbConnection> Connections { get { return connections; } }

        public Dictionary<string, DbConnectionStringBuilder> ConnectionStringBuilders { get { return connectionStringBuilders; } }

        public Dictionary<string, DbDataAdapter> Adapters { get { return adapters; } }

        public Dictionary<string, DbDataReader> Readers { get { return readers; } }

        public Dictionary<string, DbDataRecord> Records { get { return records; } }

        public Dictionary<string, DbDataSourceEnumerator> DataSourceEnumerators { get { return dataSourceEnumerators; } }

        public Dictionary<string, DbEnumerator> Enumerators { get { return enumerators; } }

        public Dictionary<string, DbParameter> Parameters { get { return parameters; } }

        public Dictionary<string, DbParameterCollection> ParameterCollections { get { return parameterCollections; } }

        public Dictionary<string, DbProviderConfigurationHandler> ProviderConfigurationHandlers { get { return providerConfigurationHandlers; } }

        public Dictionary<string, DbProviderFactoriesConfigurationHandler> ProviderFactoriesConfigurationHandlers { get { return providerFactoriesConfigurationHandlers; } }

        public Dictionary<string, DbProviderFactory> ProviderFactories { get { return providerFactories; } }

        public Dictionary<string, DbProviderSpecificTypePropertyAttribute> ProviderSpecificTypePropertyAttributes { get { return providerSpecificTypePropertyAttributes; } }

        public Dictionary<string, DbTransaction> Transactions { get { return transactions; } }

        public override bool SetOffset(long offset, ref Exception error)
        {
            return true;
        }

        public override long ReadRecord(ref Exception error)
        {
            var reader = readers[contextKeys["DATA_READER"]];
            if (reader.Read())
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    ((object[])HeaderInfo.Mappings[i].FormatterData)[2] = reader.GetValue(i);
                    
                }   
                return reader.FieldCount;
            }
            return 0;
        }

        protected virtual bool PrepareConnection(out DbConnection conn, ref Exception error)
        {
            try
            {
                if (!connections.TryGetValue(contextKeys["CONNECTION"], out conn)
                    || conn == null || conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed)
                {
                    connections.Remove(contextKeys["CONNECTION"]);
                    conn = CreateConnection(ref error, true);
                }
                return conn != null;
            }
            catch (Exception e)
            {
                conn = null;
                error = e;
                return false;
            }
        }

        protected virtual bool PrepareQuery(out string query, ref Exception error)
        {
            try
            {
                if (!ContextVariables.TryGetValue(contextKeys["QUERY_STRING"], out query))
                {
                    error = new Exception("No Query specified in context variables");
                    return false;
                }
                query = ReplaceVariables(query);
                return query != null;
            }
            catch (Exception e)
            {
                query = null;
                error = e;
                return false;
            }
        }

        protected virtual bool PrepareCommand(DbConnection conn, string query, out DbCommand cmd, ref Exception error)
        {
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                string strTimeout;
                int timeout;
                cmd.CommandTimeout =
                    int.TryParse(contextVariables.TryGetValue(contextKeys["CMD_TIMEOUT"], out strTimeout)
                            ? strTimeout
                            : int.MaxValue.ToString(CultureInfo.InvariantCulture), out timeout)
                        ? timeout
                        : int.MaxValue;
                return true;
            }
            catch (Exception e)
            {
                cmd = null;
                error = e;
                return false;
            }
        }

        protected virtual bool PrepareReader(DbConnection conn, DbCommand cmd, ref Exception error)
        {
            try
            {
                var rs = cmd.ExecuteReader();
                connections[contextKeys["CONNECTION"]] = conn;
                commands[contextKeys["COMMAND"]] = cmd;
                readers[contextKeys["DATA_READER"]] = rs;
                return true;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }

        public override bool CreateReader(ref Exception error)
        {
            var incomplete = true;
            DbCommand cmd = null;
            try
            {
                DbConnection conn = null;
                if (!PrepareConnection(out conn, ref error))
                    return false;

                string query;
                if (!PrepareQuery(out query, ref error))
                    return false;

                if (!PrepareCommand(conn, query, out cmd, ref error))
                    return false;

                if (PrepareReader(conn, cmd, ref error))
                    incomplete = false;
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                if (incomplete)
                    DisposeHelper.Close(cmd);
            }
            return !incomplete;
        }

        protected virtual string ReplaceVariables(string query)
        {
            query = contextVariables.Keys.Where(v => v.StartsWith("@") && v.Length > 2 && v[1] != '!').Aggregate(query, (current, v) => current.Replace(v, contextVariables[v]));
            return ExternalVariables == null ? query : ExternalVariables.Keys.Where(v => v.StartsWith("@") && v.Length > 2 && v[1] != '!').Aggregate(query, (current, v) => current.Replace(v, ExternalVariables[v]));
        }

        public override NextInstruction FixOffsets(NextInstruction nextInstruction, long offset, long[] headerOff, ref Exception error)
        {
            return NextInstruction.Do;
        }
    }
}
