using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Log;
using Natek.Helpers;
using Natek.Helpers.Config;
using Natek.Helpers.IO;
using Natek.Recorders.Remote.Mapping;
using Natek.Helpers.Execution;
namespace Natek.Recorders.Remote.Database
{
    public abstract class DbRecorderBase : PeriodicRecorder
    {
        public static object Value2External(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            var fields = data as object[];
            if (fields == null || fields.Length < 3)
                return null;
            var dbContext = fields[0] as DbRecorderContext;
            if (dbContext != null && dbContext.ExternalVariables != null && fields[1] != null)
            {
                dbContext.ExternalVariables["@" + fields[1]] = fields[2] == null
                                                                        ? string.Empty
                                                                        : fields[2].ToString();
            }
            return null;
        }

        public static object Object2Property(RecWrapper rec, string field, string[] fieldValues, object data)
        {
            var fields = data as object[];
            if (fields == null || fields.Length < 3)
                return null;

            var prop = fields[1] as PropertyInfo;
            if (prop == null)
                return null;

            var value = fields[2] == null
                            ? Activator.CreateInstance(prop.PropertyType)
                            : Convert.ChangeType(fields[2], prop.PropertyType);
            var dbContext = fields[0] as DbRecorderContext;
            if (dbContext == null)
                return value;
            string varName;
            if (dbContext.ContextKeys.TryGetValue(field, out varName))
            {
                var extKey = dbContext.ContextKeys[field] + "_" + dbContext.ContextKeys["QUERY_EXT"];
                if (dbContext.ExternalVariables.ContainsKey(extKey))
                    dbContext.ExternalVariables[extKey] = value == null ? string.Empty : value.ToString();
            }
            return value;
        }

        protected static readonly Regex RegNull = new Regex("^$", RegexOptions.Compiled);
        protected Dictionary<string, string> externalVariables;

        public abstract int GetDefaultPort();

        protected override void InitializeComponent()
        {
            base.InitializeComponent();
            externalVariables = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            externalVariables[keyword] = value;
            touchCount++;
            return true;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegNull;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegNull;
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegSplitter; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegSplitter; }
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { new DataMappingInfo() };
        }

        protected virtual bool DisposeActiveData<T>(Dictionary<string, T> dictionary, string key) where T : IDisposable
        {
            T t;
            if (dictionary.TryGetValue(key, out t))
            {
                DisposeHelper.Close(t);
                dictionary.Remove(key);
            }
            return true;
        }

        protected virtual bool DisposeActiveData(DbRecorderContext dbContext)
        {
            try
            {
                DisposeActiveData(dbContext.Readers, dbContext.ContextKeys["DATA_READER"]);
                DisposeActiveData(dbContext.Commands, dbContext.ContextKeys["COMMAND"]);
                DisposeActiveData(dbContext.Connections, dbContext.ContextKeys["CONNECTION"]);
                return true;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Exception occured while disposing active command and streamExpect:" + e);
                return false;
            }
            finally
            {
                try
                {
                    dbContext.Readers.Remove(dbContext.ContextKeys["DATA_READER"]);
                }
                catch
                {
                }
                try
                {
                    dbContext.Readers.Remove(dbContext.ContextKeys["COMMAND"]);
                }
                catch
                {
                }
            }
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            try
            {
                if (context.HeaderInfo != null)
                    return NextInstruction.Do;
                var dbContext = context as DbRecorderContext;
                if (dbContext == null)
                {
                    error = new Exception("Invalid recorder context. Db Recorder Context expected");
                    return NextInstruction.Abort;
                }

                DbDataReader reader;

                if (!dbContext.Readers.TryGetValue(dbContext.ContextKeys["DATA_READER"], out reader)
                    || reader == null)
                    return NextInstruction.Do;

                var mappings = new List<DataMapping>();
                var propDict = GetRecordPropertyDictionary(typeof(RecWrapper));

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    PropertyInfo prop;
                    if (!propDict.TryGetValue(reader.GetName(i).ToLowerInvariant(), out prop))
                        prop = null;

                    var originalName = new string[1][];
                    originalName[0] = new[] { reader.GetName(i) };

                    mappings.Add(prop != null
                                     ? new DataMapping
                                         {
                                             MappedField = prop,
                                             MethodInfo = Object2Property,
                                             FormatterData = new object[] { context, prop, null },
                                             Original = originalName,
                                             SourceIndex = new[] { i }
                                         }
                                     : new DataMapping
                                         {
                                             MethodInfo = Value2External,
                                             Original = originalName,
                                             FormatterData = new object[] { context, reader.GetName(i), null },
                                             SourceIndex = new[] { i }
                                         });
                }
                if (mappings.Count > 0)
                {
                    context.HeaderInfo = new DataMappingInfo { Name = "Mapping", Mappings = mappings.ToArray() };
                    return NextInstruction.Do;
                }
                return NextInstruction.Abort;
            }
            catch (Exception e)
            {
                error = e;
                return NextInstruction.Abort;
            }
        }

        protected virtual Dictionary<string, PropertyInfo> GetRecordPropertyDictionary(Type type)
        {
            var propDict = new Dictionary<string, PropertyInfo>();
            foreach (var prop in type.GetProperties())
                propDict[prop.Name.ToLowerInvariant()] = prop;

            return propDict;
        }

        protected override NextInstruction DoLogic(RecorderContext context)
        {
            try
            {
                var dbContext = context as DbRecorderContext;
                if (dbContext == null)
                {
                    Log(LogLevel.ERROR, "DbRecorderContext expected but not found");
                    return NextInstruction.Abort;
                }

                int port;
                int index;
                string host;

                if (!string.IsNullOrEmpty(RemoteHost) && (index = RemoteHost.IndexOf(':')) >= 0 && ++index < remoteHost.Length)
                {
                    if (int.TryParse(remoteHost.Substring(index), out port))
                        host = RemoteHost.Substring(0, index - 1);
                    else
                    {
                        Log(LogLevel.ERROR, "Invalid Port for remote host:[" + RemoteHost + "]");
                        return NextInstruction.Abort;
                    }
                }
                else
                {
                    host = RemoteHost;
                    port = GetDefaultPort();
                }

                InitContextGlobals(dbContext, host, port);

                var queries = externalVariables.Keys.Where(query => query.StartsWith("QUERY_")).ToList();
                queries.Sort(FileSystemHelper.CompareFilesIgnoreCase);
                foreach (var query in queries)
                {
                    var ext = query.Substring(6);
                    InitActiveParameters(dbContext, query, ext, externalVariables[query]);
                    Exception error = null;
                    try
                    {
                        var ins = ProcessContextRecords(context, ref error);
                        if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                        {
                            Log(LogLevel.DEBUG, "Process Context require exit:" + ins + (error != null ? error.ToString() : null));
                            return ins;
                        }
                    }
                    finally
                    {
                        DisposeActiveData(dbContext);
                    }
                }
                return NextInstruction.Do;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while executing queries:" + e);
                return NextInstruction.Abort;
            }
        }

        protected override NextInstruction OnAfterSetData(RecorderContext context)
        {
            var ins = base.OnAfterSetData(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            var dbContext = context as DbRecorderContext;
            if (dbContext == null)
            {
                Log(LogLevel.ERROR, "OnAfterSetData expected database recorder context");
                return NextInstruction.Abort;
            }
            externalVariables["@RECORDNUM_" + dbContext.ContextVariables[dbContext.ContextKeys["QUERY_EXT"]]] = context.Record.Recordnum.ToString(CultureInfo.InvariantCulture);
            externalVariables["@RECORDDATE_" + dbContext.ContextVariables[dbContext.ContextKeys["QUERY_EXT"]]] = context.Record.Datetime;
            return NextInstruction.Do;
        }

        protected override NextInstruction OnBeforeSetReg(RecorderContext context)
        {
            var ins = base.OnBeforeSetReg(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            context.LastKeywordBuffer.Remove(0, context.LastKeywordBuffer.Length);
            PrepareKeywords(context, context.LastKeywordBuffer);
            context.LastKeywords = context.LastKeywordBuffer.ToString();
            return NextInstruction.Do;
        }

        protected virtual void InitActiveParameters(DbRecorderContext dbContext, string query, string queryExtension, string queryString)
        {
            dbContext.ContextVariables[dbContext.ContextKeys["QUERY"]] = query;
            dbContext.ContextVariables[dbContext.ContextKeys["QUERY_STRING"]] = queryString;
            dbContext.ContextVariables[dbContext.ContextKeys["QUERY_EXT"]] = queryExtension;
            dbContext.ContextKeys["RECORDNUM"] = "@RECORDNUM_" + queryExtension;
            dbContext.ContextKeys["DATETIME"] = "@RECORDDATE_" + queryExtension;

            GetExternal(dbContext, "SERVER", queryExtension);
            GetExternal(dbContext, "DATABASE", queryExtension);
            GetExternal(dbContext, "USER", queryExtension);
            GetExternal(dbContext, "PASSWORD", queryExtension, "!");
            GetExternal(dbContext, "PORT", queryExtension);
            GetExternal(dbContext, "CMD_TIMEOUT", queryExtension);

            if (!dbContext.ExternalVariables.ContainsKey(dbContext.ContextKeys["RECORDNUM"]))
                dbContext.ExternalVariables[dbContext.ContextKeys["RECORDNUM"]] = "0";
            if (!dbContext.ExternalVariables.ContainsKey(dbContext.ContextKeys["DATETIME"]))
                dbContext.ExternalVariables[dbContext.ContextKeys["DATETIME"]] = "1970/1/1 00:00:00";
        }

        protected virtual void GetExternal(DbRecorderContext dbContext, string varName, string extension, string varExtension = "@")
        {
            string v;
            var key = "@" + varName + "_" + extension;
            if (externalVariables.TryGetValue(key, out v))
                dbContext.ContextVariables["@" + varExtension + varName] = v;
        }

        protected virtual void InitContextGlobals(DbRecorderContext dbContext, string host, int port)
        {
            dbContext.ExternalVariables = externalVariables;

            dbContext.ContextVariables["@@SERVER"] = host;
            dbContext.ContextVariables["@@DATABASE"] = Location;
            dbContext.ContextVariables["@@USER"] = User;
            dbContext.ContextVariables["@!PASSWORD"] = Password;
            dbContext.ContextVariables["@@PORT"] = port.ToString(CultureInfo.InvariantCulture);

            dbContext.ContextKeys["CONNECTION"] = "@!CONNECTION";
            dbContext.ContextKeys["SERVER"] = "@@SERVER";
            dbContext.ContextKeys["DATABASE"] = "@@DATABASE";
            dbContext.ContextKeys["USER"] = "@@USER";
            dbContext.ContextKeys["PASSWORD"] = "@!PASSWORD";
            dbContext.ContextKeys["PORT"] = "@@PORT";
            dbContext.ContextKeys["COMMAND"] = "@!COMMAND";
            dbContext.ContextKeys["DATA_READER"] = "@!DATA_READER";
            dbContext.ContextKeys["CMD_TIMEOUT"] = "@@CMD_TIMEOUT";
            dbContext.ContextKeys["QUERY"] = "@@QUERY";
            dbContext.ContextKeys["QUERY_STRING"] = "@@QUERY_STRING";
            dbContext.ContextKeys["QUERY_EXT"] = "@@QUERY_EXT";
        }

        protected override void PrepareKeywords(RecorderContext context, StringBuilder keywordBuffer)
        {
            foreach (var key in externalVariables.Keys)
            {
                keywordBuffer.Append(ConfigHelper.Escape(key))
                             .Append("=\"")
                             .Append(ConfigHelper.Escape(externalVariables[key]))
                             .Append("\";");
            }
        }

        protected override NextInstruction InputText2RecordField(RecorderContext context, ref string[] fields)
        {
            return NextInstruction.Do;
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return location;
        }
    }
}
