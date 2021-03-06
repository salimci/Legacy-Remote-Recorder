﻿using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using System.Globalization;

namespace Natek.Recorders.Remote.Unified.KerioFirewallHttpUnifiedRecorder
{
    public class KerioFirewallHttpUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll =
            new Regex(
                "^(?<IP>[^:]+)\\:(?<PORT>\\S+)\\s:\\s\\S+\\s(?<FLAG>\\S+)\\s(?<DATE_TIME>[^+]+)\\+\\S+\\s*\\S+\\s*(?<LOG_NAME>\\S+)\\D+(?<USER_IP>\\S+)\\s(?<USER_ADDRESS>\\S+)\\s+\\\"(?<MESSAGE>[^\"]+)\\\"\\s(?<URL>\\S+)\\s*$",
                RegexOptions.Compiled);

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

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as KerioFirewallHttpUnifiedRecorder;
            var dtStr = values[0];
            if (DateTime.TryParseExact(dtStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone)
                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }


        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                     new DataMapping
                    {
                        Original = new[] {new[] {"LOG_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                    
                    new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },


                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_ADDRESS"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),
                    },

                  

                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                       new DataMapping
                    {
                        Original = new[] {new[] {"PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2"),
                    },

                       new DataMapping
                    {
                        Original = new[] {new[] {"USER_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },

                        new DataMapping
                    {
                        Original = new[] {new[] {"URL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                    }


                }
            };
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            return NextInstruction.Return;
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
        
        
    }
}
