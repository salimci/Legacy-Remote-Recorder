using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.StreamBased.Terminal.Ssh.Apache
{
    public class ApacheAccessUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue =
            new Regex("([^ \t\"]*\"([^\"]*)\"[^ \t]*|[^ \t\\[]*\\[([^\\]]*)\\][^ \t]*|([^ \t]*))[ \t]?",
                      RegexOptions.Compiled);

        protected static readonly Regex RegMethodUriProtocol = new Regex("^([^ \t]+)[ \t]+(.*)[ \t]+([^ \t]+)$",
                                                                         RegexOptions.Compiled);

        protected static object Convert2MethodUriProtocol(RecWrapper rec, string field, string[] values, object data)
        {
            var m = RegMethodUriProtocol.Match(values[0]);
            if (m.Success)
            {
                rec.CustomStr4 = m.Groups[2].Value;
                rec.CustomStr5 = m.Groups[3].Value;
                return m.Groups[1].Value;
            }
            return values[0];
        }

        protected static object Convert2IpAddressesSplit(RecWrapper rec, string field, string[] values, object data)
        {
            if (!string.IsNullOrEmpty(values[0]))
            {
                var vals = values[0].Split(new[] { ',' }, 2);
                if (vals.Length > 1)
                    rec.CustomStr9 = vals[1];
                return vals[0];
            }
            return values[0];
        }

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as ApacheAccessUnifiedRecorder;
            if (DateTime.TryParseExact(values[0], "d/MMM/yyyy:H:m:s zzz", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out dt)
                ||
                DateTime.TryParseExact(values[0], "d/MMM/yyyy:H:m:s zz", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out dt)
                ||
                DateTime.TryParseExact(values[0], "d/MMM/yyyy:H:m:s z", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone)
                         .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new ApacheAccessRecorderContext(this) { ReadTimeout = this.ReadTimeout, Port = this.Port };
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected virtual DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
                {
                    Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Unk1"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr2"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Unk2"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Datetime"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Http-Header"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory"),
                                    MethodInfo = Convert2MethodUriProtocol
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Response"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Size"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Url"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Agent"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"Addresses"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr8"),
                                    MethodInfo = Convert2IpAddressesSplit
                                }
                        }
                };
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            if (MappingInfos != null)
            {
                foreach (var mappingInfo in MappingInfos)
                {
                    context.SourceHeaderInfo = MimicMappingInfo(mappingInfo.Mappings);
                    context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                    break;
                }
            }
            return NextInstruction.Do;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            context.Record.EventType = "access";
            context.Record.Description = context.InputRecord.ToString();
            context.Record.CustomStr10 = context.LastFile;
            return NextInstruction.Do;
        }
    }
}
