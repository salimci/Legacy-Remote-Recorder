using System;
using System.Text.RegularExpressions;
using System.Globalization;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.FreeRadiusUnifiedRecorder
{
    public class FreeRadiusUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex(@"^(?<DATE_TIME>.*?)\s*:\s*(?<EVENT_CATEGORY>[a-zA-Z]+)\s*:\s*(((?<EVENT_TYPE>()|[^:]+)\s*(:\s*)?(()|(\s*(?<USER_NAME>[^\s]+)\s*))?(()|(\((?<ERROR_DESCRIPTION>[^\)]+)\):\s*))\([\w]+\s*[\w]+\s*(?<FROM_CLIENT>[^\s]+)\s*port\s*(?<NAS_PORT>[0-9]+)\s+(()|(cli\s*(?<CALLING_STATION_ID_>[^\)]+)))\))|((?<DESCRIPTION>.+)))$", RegexOptions.Compiled);

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new FreeRadiusUnifiedRecorderContext(this);
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[] { 
                    new DataMapping
                    {
                        Original = new[] {new []{"DATE_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EVENT_CATEGORY"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ERROR_DESCRIPTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"FROM_CLIENT"}},
                        MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                    }
                }
            };
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                try
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    int fieldBufferKey;
                    if(context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }

            return NextInstruction.Return;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return String.Empty;
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
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

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }
    }
}

