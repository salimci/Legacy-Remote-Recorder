using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Ftp2012UnifiedRecorder
{
    public class Ftp2012UnifiedRecorder: FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("(?<DATE>[\\w-\\/]*)\\s*(?<TIME>[0-9:]+)\\s*(?<C_IP>[0-9\\.]+)\\s*(?<CS_USERNAME>.[^\\s]+)?\\s*(?<S_IP>[0-9\\.]+)?\\s*(?<S_PORT>[0-9]+)?\\s*(?<CS_METHOD>[\\w]+)\\s*(?<CS_URI_STEM>.[^\\s]+)\\s*(?<SC_STATUS>.[^\\s]+)\\s*(?<SC_WIN32_STATUS>.[^\\s]+)?\\s*(?<SC_SUBSTATUS>.[^\\s]+)?\\s*(?<X_SESSION>.[^\\s]+)?\\s*(?<X_FULLPATH>.[^\\s]+)", RegexOptions.Compiled);
        protected string DateFormat = "yyyy-MM-dd HH:mm:ss";
        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo()
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DATE"}, new []{"TIME"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SC_STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"S_PORT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")

                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"C_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"S_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_URI_STEM"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"X_SESSION"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_URI_STEM"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_METHOD"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"CS_USERNAME"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SC_SUBSTATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SC_WIN32_STATUS"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        private object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            if (DateTime.TryParseExact(fieldvalues[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForValue.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
        }

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null)
            { return base.OnBeforeProcessRecordInput(context); }
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
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

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn() };
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;

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
    }
}
