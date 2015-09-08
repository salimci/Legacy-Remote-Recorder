using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.MerakMailUnifiedRecorder
{
    public class MerakMailUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"([\S\w]*)\s*\[[\w]*\]\s*([\w]*,\s*[\d]*\s*[\w]*\s*[\d]*\s*[\d]*:[\d]*:[\d]*\s*[\S\d]*)\s*(.*)", RegexOptions.Compiled);

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as MerakMailUnifiedRecorder;
            if (DateTime.TryParseExact(values[0], "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone)
                         .ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new MerakMailUnifiedRecorderContext(this);
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new []{new []{"datatime"}},
                        MappedField = typeof(RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"source"}},
                        MappedField = typeof(RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"event"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"from"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"rcpt"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"server"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"protocol"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"size"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                        
                    },
                    new DataMapping
                    {
                        Original = new []{new []{"hostname"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName")
                    }
                }
            };
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;
            if (rec.RecordText.Length == 0)
                return RecordInputType.Comment;
            if (rec.RecordText.StartsWith("#"))
            {
                if (context.InputRecord.ToString().StartsWith("#Fields: "))
                    return RecordInputType.Header;
                return RecordInputType.Comment;
            }
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return string.Empty;
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
            var localContext = (MerakMailUnifiedRecorderContext)context;
            if (!match.Success) return NextInstruction.Abort;

            if (match.Groups[3].Success && match.Groups[3].Value.StartsWith("Disconnected") && localContext.Buffer.Count > 2)
            {
                if (localContext.Buffer.ContainsKey("event") && !string.IsNullOrEmpty(localContext.Buffer["event"]))
                {
                    foreach (var info in context.SourceHeaderInfo.Keys)
                    {
                        string value;
                        localContext.Buffer.TryGetValue(info, out value);
                        context.FieldBuffer[context.SourceHeaderInfo[info]] = value;
                    }

                    localContext.Buffer = new Dictionary<string, string>();
                    return NextInstruction.Return;
                }
                context.OffsetInStream++;
                return NextInstruction.Skip;
            }

            //date and source ip

            if (match.Groups[1].Success)
            {
                if (localContext.Buffer.ContainsKey("source"))
                    localContext.Buffer["source"] = match.Groups[1].Value;
                else
                    localContext.Buffer.Add("source", match.Groups[1].Value);
            }
            if (match.Groups[2].Success)
            {
                if (localContext.Buffer.ContainsKey("datetime"))
                    localContext.Buffer["datetime"] = match.Groups[2].Value;
                else
                    localContext.Buffer.Add("datetime", match.Groups[2].Value);
            }

            //server name and protocol

            var serverRegex = new Regex(@".+220\s*([a-zA-Z\S]+)\s*([a-zA-Z]*)", RegexOptions.Compiled);
            var serverMatch = serverRegex.Match(match.Groups[3].Value);

            if (serverMatch.Success && !serverMatch.Groups[1].Value.Equals("2.0.0"))
            {
                if (localContext.Buffer.ContainsKey("server"))
                    localContext.Buffer["server"] = serverMatch.Groups[1].Value;
                else
                    localContext.Buffer.Add("server",serverMatch.Groups[1].Value);

                if (localContext.Buffer.ContainsKey("protocol"))
                    localContext.Buffer["protocol"] = serverMatch.Groups[2].Value;
                else
                    localContext.Buffer.Add("protocol", serverMatch.Groups[2].Value);
            }

            //host name
            var hostnameRegex = new Regex(@"\s*>>>\s*(.*)\s*Hello", RegexOptions.Compiled);
            var hostnameMatch = hostnameRegex.Match(match.Groups[3].Value);

            if (hostnameMatch.Success)
            {
                if (localContext.Buffer.ContainsKey("hostname"))
                    localContext.Buffer["hostname"] = hostnameMatch.Groups[1].Value;
                else
                    localContext.Buffer.Add("hostname",hostnameMatch.Groups[1].Value);
            }

            //mail from
            var mailFromRegex = new Regex(@"(\s*(MAIL FROM:|MAIL From:)\s*<(.*)>\s*SIZE=([0-9]+))|(\s*(MAIL FROM:|MAIL From:)\s*<(.*)>)", RegexOptions.Compiled);
            var mailFromMatch = mailFromRegex.Match(match.Groups[3].Value);

            if (mailFromMatch.Success)
            {
                if (mailFromMatch.Groups[7].Success)
                {
                    if (localContext.Buffer.ContainsKey("from"))
                        localContext.Buffer["from"] = mailFromMatch.Groups[7].Value;
                    else
                        localContext.Buffer.Add("from", mailFromMatch.Groups[7].Value);
                }
                else
                {
                    if (localContext.Buffer.ContainsKey("from"))
                        localContext.Buffer["from"] = mailFromMatch.Groups[3].Value;
                    else
                        localContext.Buffer.Add("from", mailFromMatch.Groups[3].Value);

                    if (localContext.Buffer.ContainsKey("size"))
                        localContext.Buffer["size"] = mailFromMatch.Groups[4].Value;
                    else
                        localContext.Buffer.Add("size", mailFromMatch.Groups[4].Value);
                }
            }

            //rcpt to
            var rcptToRegex = new Regex(@"\s*(RCPT TO:|RCPT To:)\s*<(.*)>", RegexOptions.Compiled);
            var rcptRoMatch = rcptToRegex.Match(match.Groups[3].Value);

            if (rcptRoMatch.Success)
            {
                if (localContext.Buffer.ContainsKey("rcpt"))
                    localContext.Buffer["rcpt"] = rcptRoMatch.Groups[2].Value;
                else
                    localContext.Buffer.Add("rcpt", rcptRoMatch.Groups[2].Value);
            }

            //event recipient
            var recipientRegex = new Regex(@"\s*>>>\s*([\d\s\d\.]*)\s*<.*>\.\.\.\s*(.*)", RegexOptions.Compiled);
            var recipientMatch = recipientRegex.Match(match.Groups[3].Value);

            if (recipientMatch.Success)
            {
                if (recipientMatch.Groups[1].Value.Equals("250 2.1.0 "))
                {
                    if (localContext.Buffer.ContainsKey("event"))
                        localContext.Buffer["event"] = recipientMatch.Groups[2].Value;
                    else
                        localContext.Buffer.Add("event", recipientMatch.Groups[2].Value);
                }
                else if (recipientMatch.Groups[1].Value.Equals("250 2.1.5 "))
                {
                    if (localContext.Buffer.ContainsKey("event"))
                        localContext.Buffer["event"] = recipientMatch.Groups[2].Value;
                    else
                        localContext.Buffer.Add("event", recipientMatch.Groups[2].Value);
                }
            }

            context.OffsetInStream++;
            return NextInstruction.Skip;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
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
    }
}
