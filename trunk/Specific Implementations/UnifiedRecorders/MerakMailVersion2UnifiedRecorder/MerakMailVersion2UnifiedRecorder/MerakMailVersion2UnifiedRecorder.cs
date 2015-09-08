using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.MerakMailVersion2UnifiedRecorder
{
    public class MerakMailVersion2UnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"^.*?Connected.*\s(?<SOURCENAME>[^\s]+).*?\s+.*?>>>\s.*?\s.*?\s(?<PROTOCOL>[^\s]+)\s.*\,\s(?<DATETIME>[^\+]+)\+.*\s.*\s.*?\>>>\s(?<COMPUTERNAME>.*?)Hello\s(?<TARGETNAME>[^\s]+)(.|\n)*?>>>\s.*?\s.*?(.*MAIL FROM:<(?<SENDERMAIL>([^\>]+)|()))\>(.|\n)+\.\.\.\s(?<EVENTTYPE>[^\s]+)(.|\n)*RCPT TO:<(?<RECIPIENT>[^\>]+)((.|\n)*250\s2\.6\.0\s(?<RECEIVEDBYTES>[^\s]+)|())(.|\n)*QUIT$", RegexOptions.Compiled);
        protected static readonly Regex RegforConnection = new Regex(@"^.*?Connected.*?$", RegexOptions.Compiled);
        protected static readonly Regex RegforQuit = new Regex(@"^.*?QUIT.*?$", RegexOptions.Compiled);
        protected static readonly Regex RegforDisconnected = new Regex(@"^.*?Disconnected$", RegexOptions.Compiled);
        protected static readonly Regex RegforClientSession = new Regex(@"^.*?Client\ssession.*$", RegexOptions.Compiled);

        private StringBuilder _lastBuffer = new StringBuilder();

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCENAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOCOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATETIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"COMPUTERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TARGETNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SENDERMAIL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENTTYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType"),
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RECIPIENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"RECEIVEDBYTES"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        //it will return false if MappedField is null
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),
                    }
                }
            };
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new MerakMailVersion2UnifiedRecorderContext(this);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                var getConnect = RegforConnection.Match(source);
                var getQuit = RegforQuit.Match(source);
                var getDisconnect = RegforDisconnected.Match(source);
                var getClientSession = RegforClientSession.Match(source);

                if (string.IsNullOrEmpty(_lastBuffer.ToString()) && getClientSession.Success)
                {
                    return NextInstruction.Skip;
                }

                if (string.IsNullOrEmpty(_lastBuffer.ToString()))
                {
                    if (!getConnect.Success) return NextInstruction.Skip;
                    _lastBuffer.Append(source.TrimEnd());
                    return NextInstruction.Abort;
                }
                if (getDisconnect.Success)
                {
                    _lastBuffer = new StringBuilder();
                    return NextInstruction.Skip;
                }
                if (getQuit.Success || getConnect.Success)
                {
                    _lastBuffer.Append("\n" + source.TrimEnd());
                    var regmatch = RegSplitForValue.Match(_lastBuffer.ToString());

                    if (!regmatch.Success)
                    {
                        _lastBuffer = new StringBuilder();
                        _lastBuffer.Append(source.TrimEnd());
                        return NextInstruction.Skip;
                    }

                    var groupCollection = regmatch.Groups;

                    foreach (var key in RegSplitForValue.GetGroupNames())
                    {
                        int tmp;
                        if (int.TryParse(key, out tmp)) continue;
                        if (!context.SourceHeaderInfo.ContainsKey(key)) continue;
                        if (groupCollection[key].Value.Length > 0)
                            context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                    }
                    context.FieldBuffer[context.SourceHeaderInfo["Description"]] = _lastBuffer.ToString();
                    _lastBuffer = new StringBuilder();
                    return NextInstruction.Return;
                }
                _lastBuffer.Append("\n" + source.TrimEnd());
                return NextInstruction.Abort;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing veribranch record: " + e);
                return NextInstruction.Abort;
            }
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
            if (MappingInfos == null) return NextInstruction.Do;
            foreach (var mappingInfo in MappingInfos)
            {
                context.SourceHeaderInfo = MimicMappingInfo(mappingInfo.Mappings);
                context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                break;
            }
            return NextInstruction.Do;
        }

        protected override Dictionary<string, int> MimicMappingInfo(IEnumerable<DataMapping> dataMapping)
        {
            var info = new Dictionary<string, int>();
            var index = 0;
            foreach (var mapping in dataMapping)
            {
                if (mapping.Original.Length > 1)
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index;
                    }
                    index++;
                }
                else
                {
                    foreach (var orig in mapping.Original)
                    {
                        info[orig[0]] = index++;
                    }
                }
            }
            return info;
        }
    }
}

