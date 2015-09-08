using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Log;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Srx.Mobile
{
    public class Srx3600MobileFwRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSessionInfo = new Regex("^([^ \t]+([ \t]+[^ \t:]+([ \t:]+[^ \t:]+)))", RegexOptions.Compiled);
        protected static readonly Regex RegSplitForAll = new Regex("^([^ \t]*)[ \t]*:[ \t]*([^ ]+)[ \t]+([^ \t]+)[ \t]+([[0-9]+)[ \t]+([0-9]+:[0-9]+:[0-9]+)[ \t]+([^ ]+)[ \t]+([^ ]+)[ \t]*:[ \t]*([^ ]+)[ \t]*:[ \t]*(.*)[ \t]+([^/]+)/([0-9]+)->([^/]+)/([0-9]+)[ \t]+([^ \t]+)[ \t]+([^/]+)/([0-9]+)->([^/]+)/([0-9]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)[ \t]+([^ \t]+)(([ \t]+[^ \t]*)*)", RegexOptions.Compiled);

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
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
            var recorder = data as Srx3600MobileFwRecorder;

            if (DateTime.TryParseExact(values[0], "yyyy MMM d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.Zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
                {
                    Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new[] {new[] {"dateTime"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo=Convert2Date
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"fwName"}},
                                    MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"eventType"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("EventType"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"protocol"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr1"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"ip1"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr3"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"port1"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomInt3"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"name1"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr6"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"ip2"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr4"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"port2"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomInt4"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"name2"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr7"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"ip3"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr5"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"port3"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomInt5"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"name3"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr8"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"network"}},
                                    MappedField =  typeof (RecWrapper).GetProperty("CustomStr9"),
                                }
                        }
                };
        }

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

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                if (match.Success)
                {
                    context.FieldBuffer.Add(DateTime.Now.Year + " " + match.Groups[3].Value + " " + match.Groups[4].Value + " " + match.Groups[5].Value);
                    context.FieldBuffer.Add(match.Groups[6].Value);
                    var m = RegSessionInfo.Match(match.Groups[9].Value);
                    context.FieldBuffer.Add(m.Success ? m.Value : match.Groups[9].Value);
                    context.FieldBuffer.Add(match.Groups[14].Value);
                    context.FieldBuffer.Add(match.Groups[10].Value);
                    context.FieldBuffer.Add(match.Groups[11].Value);
                    context.FieldBuffer.Add(match.Groups[20].Value);
                    context.FieldBuffer.Add(match.Groups[12].Value);
                    context.FieldBuffer.Add(match.Groups[13].Value);
                    context.FieldBuffer.Add(match.Groups[22].Value);
                    context.FieldBuffer.Add(match.Groups[17].Value);
                    context.FieldBuffer.Add(match.Groups[18].Value);
                    context.FieldBuffer.Add(match.Groups[23].Value);
                    context.FieldBuffer.Add(match.Groups[24].Value);

                    return NextInstruction.Return;
                }
                Log(LogLevel.WARN, "Skip No Match [" + source + "]");
                return NextInstruction.Skip;
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing syslog record:" + e);
                return NextInstruction.Abort;
            }
        }
    }
}
