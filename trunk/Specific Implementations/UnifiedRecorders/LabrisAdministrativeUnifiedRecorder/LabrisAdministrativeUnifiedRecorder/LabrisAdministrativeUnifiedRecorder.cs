using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.LabrisAdministrativeUnifiedRecorder
{
    public class LabrisAdministrativeUnifiedRecorder : TerminalRecorder
    {
        protected static readonly Regex RegSplitForAll = new Regex("(?<DATE_TIME>.*?)\\s*sshd\\[(?<UNIQUE>[^\\]]+)\\]:\\s*(((?<EVENT_CATEGORY_0>.*?)\\s+for\\s+(?<USER_0>.*?)\\s+from\\s+(?<SOURCE_IP>[^\\s]+)\\s+port\\s+(?<PORT>[^\\s]+)\\s+(?<DESCRIPTION_0>[^\\s]+))|(pam_unix\\((?<DESCRIPTION_1>[^\\)]+)\\):\\s+(?<EVENT_CATEGORY_1>.*?)\\s+for\\s+user\\s+(?<USER_1>[^\\s]+)(\\s+by\\s+\\((?<UID_0>[^\\)]+)\\))?)|(pam_unix\\(sshd:auth\\):\\s*(?<DESCRIPTION_2>[^\\;]+)\\;\\s*logname=(?<LOGNAME>()|.[^\\s]*)\\s+uid=(?<UID_1>[0-9]+)\\s+euid=(?<EUID>[0-9]+)\\s+tty=(?<TTY>[\\w]+)\\s+ruser=(?<RUSER>()|.[^\\s]*)\\s+rhost=(?<RHOST>[0-9\\.]+)\\s+user=(?<USER_2>[\\w]+))|(?<DESCRIPTION_3>.[^\\n]+))", RegexOptions.Compiled);

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new LabrisAdministrativeUnifiedRecorderContext(this);
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
                        Original = new[] {new[] {"EVENT_CATEGORY_0", "EVENT_CATEGORY_1"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_0", "USER_1", "USER_2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SOURCE_IP", "RHOST"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"DESCRIPTION_0", "DESCRIPTION_1", "DESCRIPTION_2", "DESCRIPTION_3"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                try
                {
                    int tmp;
                    if (int.TryParse(key, out tmp)) continue;
                    int fieldBufferKey;
                    if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            return NextInstruction.Return;
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
