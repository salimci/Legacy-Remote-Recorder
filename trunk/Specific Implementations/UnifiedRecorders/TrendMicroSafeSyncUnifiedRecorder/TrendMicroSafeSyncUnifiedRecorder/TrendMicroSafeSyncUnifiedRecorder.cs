using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.TrendMicroSafeSync
{
    public class TrendMicroSafeSyncUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex("time=(?<TIME>[^\\s]*)\\s+((detail_code=\"(?<DETAIL_CODE>[^\"]*)\".*?user_name=\"(?<USER_NAME>[^\"]*)\".*?file_name=\"(?<FILE_NAME>[^\"]*)\"\\s*log_type=\"(?<LOG_TYPE>[^\"]*)\"\\s*client_ip=\"(?<CLIENT_IP>[^\"]*)\".*?action=\"(?<ACTION>[^\"]*)\".*?(owner_name=\"(?<OWNER_NAME>[^\"]*)\")?)|(log_type=\"(?<ERROR_LOG_TYPE>[^\"]*)\"\\s*action=\"(?<ERROR_ACTION>[^\"]*)\".*?host=\"(?<ERROR_HOST>[^\"]*)\"\\s*detail_code=\"(?<ERROR_DETAIL_CODE>[^\"]*)\"))", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("DateTime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DETAIL_CODE"}, new []{"ERROR_DETAIL_CODE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType"),
                        MethodInfo = Concatinate
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"FILE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"LOG_TYPE"}, new []{"ERROR_LOG_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                        MethodInfo = Concatinate
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CLIENT_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ACTION"}, new [] {"ERROR_ACTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                        MethodInfo = Concatinate
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"OWNER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"ERROR_HOST"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName") 
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        private object Concatinate(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            var temp = string.Empty;

            foreach (var fieldvalue in fieldvalues)
            {
                if (!string.IsNullOrEmpty(fieldvalue))
                {
                    temp += fieldvalue;
                }
            }
            return temp;
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            var recorder = data as TrendMicroSafeSyncUnifiedRecorder;

            if (DateTime.TryParseExact(fieldvalues[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone)
                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo == null)
            {
                Exception error = null;
                var ins = GetHeaderInfo(context, ref error);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    return ins;
            }
            return base.OnBeforeProcessRecordInput(context);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (match.Success)
            {
                var groupCollection = match.Groups;

                foreach (var key in RegSplitForAll.GetGroupNames())
                {
                    int tmp;
                    if (!int.TryParse(key, out tmp))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }

                context.FieldBuffer.Add(source);
                return NextInstruction.Return;
            }
            return NextInstruction.Skip;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }
    }
}
