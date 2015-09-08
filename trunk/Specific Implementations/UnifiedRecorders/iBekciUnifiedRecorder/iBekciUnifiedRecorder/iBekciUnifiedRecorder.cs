using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.iBekciUnifiedRecorder
{
// ReSharper disable InconsistentNaming
    public class iBekciUnifiedRecorder : FileLineRecorder
// ReSharper restore InconsistentNaming
    {
        protected static readonly Regex RegSplitForAll = new Regex("^(?<ZAMAN>[^:]*):(?<PROTOKOL>[^:]*):(?<KAYNAK_IP>[^:]*):(?<KAYNAK_KAPI>[^:]*):(?<ADRES_DONUSUMU_IP>[^:]*):(?<ADRES_DONUSUMU_KAPI>[^:]*):(?<HEDEF_IP>[^:]*):(?<HEDEF_KAPI>[^:]*):(?<TCP_DURUMLARI>[^:]*):(?<PAKET_YONU>[^:]*):(?<GIDEN_BAYT>[^:]*):(?<GELEN_BAYT>[^:]*):(?<GIDEN_PAKET>[^:]*):(?<GELEN_PAKET>[^:]*):(?<SURE>[^((:)|(\\n))]*)$", RegexOptions.Compiled);

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
                        Original = new[] {new []{"ZAMAN"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOKOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"KAYNAK_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ADRES_DONUSUMU_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HEDEF_IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"TCP_DURUMLARI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"PAKET_YONU"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"TCP_DURUMLARI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"PROTOKOL"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"KAYNAK_KAPI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ADRES_DONUSUMU_KAPI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HEDEF_KAPI"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GIDEN_BAYT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GELEN_BAYT"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GIDEN_PAKET"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"GELEN_PAKET"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SURE"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int32
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;

            return DateTime.TryParseExact(fieldvalues[0], "dd/MMM/yyyy:HH:mm:ss zzzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
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

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null || string.IsNullOrEmpty(context.InputRecord.ToString()))
                return RecordInputType.Comment;
            return RecordInputType.Record;
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
