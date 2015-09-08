using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.XferUnifiedRecorder
{
    public class XferUnifiedRecorder : TerminalRecorder
    {

        protected static readonly Regex RegSplitForValue = new Regex(@"^(?<CURRENT_TIME>[a-zA-Z]*\s[a-zA-Z]*\s*[0-9]*\s[0-9\:]*\s[0-9]*)\s*(?<TRANSFER_TIME>.[^\s]+)\s*(?<REMOTE_HOST>[^\s]+)\s*(?<FILE_SIZE>[^\s]+)\s*(?<FILENAME>.[^\s]+)\s*(?<TRANSFER_TYPE>.[^\s]+)\s*(?<SPECIAL_ACTION_FLAG>.[^\s]+)\s*(?<DIRECTION>.[^\s]+)\s*(?<ACCESS_MODE>.[^\s]+)\s*(?<USERNAME>.[^\s]+)\s*(?<SERVICE_NAME>.[^\s]+)\s*(?<AUTHENTICATION_METHOD>.[^\s]+)\s*(?<AUTHENTICATED_USER_ID>.[^\s]+)\s*(?<COMPLETION_STATUS>[^\s]+)$", RegexOptions.Compiled);

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new XferUnifiedRecorderContext(this);
        }
        
        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        private DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"CURRENT_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"FILENAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"TRANSFER_TYPE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"REMOTE_HOST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"DIRECTION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"ACCESS_MODE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"COMPLETION_STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SPECIAL_ACTION_FLAG"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"AUTHENTICATED_USER_ID"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SERVICE_NAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"AUTHENTICATION_METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventId")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"TRANSFER_TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"FILE_SIZE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (int.TryParse(key, out tmp)) continue;
                if (context.SourceHeaderInfo.ContainsKey(key))
                    context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
            }
            return NextInstruction.Return;
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
