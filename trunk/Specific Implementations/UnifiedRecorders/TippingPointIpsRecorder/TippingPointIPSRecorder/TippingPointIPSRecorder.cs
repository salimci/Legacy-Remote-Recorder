using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Log;

namespace Natek.Recorders.Remote.Unified.TippingPointIps
{
    public class TippingPointIpsRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll =
            new Regex(
                @"^(([\d]+)[\-]([\d]+)[\-]([\d]+)[ \t]([\d\:]+)[ \t]([\w\.]+)[ \t]([\d\.]+)[^\|]+[^\:]+[^ A-Z]+[ A-Z]+[ \:]+([ A-Za-z\.\(\)]+)[\d\|]+)|[ \t]*([^ =]*)=",
                RegexOptions.Compiled);

        protected static readonly Regex RegSplitForEquality = new Regex(@"[ \t]*([^ =]*)=", RegexOptions.Compiled);

        protected List<string> mappingFields = new List<string>
        {
            "app","cnt","dst","dpt","act","cn3Label","cs1","src","spt","externalId","rt","deviceInboundInterface"
        };

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
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

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"date-time"}},
                        MappedField = typeof (RecWrapper).GetProperty("DateTime"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"sourcename"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"computername"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"customStr1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"app"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"cnt"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dst"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"dpt"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"act"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"cn3Label"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"cs1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"src"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"spt"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },                  
                    new DataMapping
                    {
                        Original = new[] {new[] {"externalId"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"rt"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int64
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"deviceInboundInterface"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
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

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as TippingPointIpsRecorder;

            if (DateTime.TryParseExact(values[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone)
                    .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                if (!match.Groups[1].Success)
                {
                    Log(LogLevel.WARN, "Skip No Match [" + source + "]");
                    return NextInstruction.Skip;
                }


                context.FieldBuffer[0] = match.Groups[4].Value + " " + match.Groups[3].Value + " " +
                                        match.Groups[2].Value + " " + match.Groups[5].Value;
                //year month day and time //datetime
                context.FieldBuffer[1] = match.Groups[6].Value; //sourcename
                context.FieldBuffer[2] = match.Groups[7].Value; //computername
                context.FieldBuffer[3] = match.Groups[8].Value; //customstr1

                var currentMatch = match.NextMatch();

                while (currentMatch.Success)
                {
                    var nextMatch = currentMatch.NextMatch();

                    var currentFieldEnd = currentMatch.Index + currentMatch.Length;

                    var currentMatchLength = (nextMatch.Index != 0 ? nextMatch.Index : source.Length) - currentFieldEnd;

                    var fieldValue = source.Substring(currentFieldEnd, currentMatchLength);

                    int fieldIndex;
                    if (context.FieldMappingIndexLookup.TryGetValue(currentMatch.Groups[9].Value, out fieldIndex))
                    {
                        context.FieldBuffer[fieldIndex] = fieldValue;
                    }

                    currentMatch = nextMatch;
                }

                return NextInstruction.Return;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing tipping point record:" + e);
                return NextInstruction.Abort;
            }
        }

        protected override IEnumerable<string> GetFieldMappingsFields()
        {
            return mappingFields;
        }


        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            var ins = base.GetHeaderInfo(context, ref error);

            if ((ins & NextInstruction.Do) == NextInstruction.Do)
            {
                context.FieldMappingIndexLookup = CreateFieldMappingIndexLookup(context.HeaderInfo, context, GetFieldMappingsFields());
            }

            return ins;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
        }

    }
}
