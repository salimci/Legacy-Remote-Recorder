using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.LabrisWebUnifiedRecorder
{
    public class LabrisWebUnifiedRecorder:SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"(?<SourceName>[^\:]+).*?:.*?:\s.*?\s(?<Datetime>.*?\s.*?\s.*?)\s.*?:\s.*?\s(?<UsersID>[^\s]+)\s(?<IP>[^\s]+)\s.*?:\/\/(?<Host>[^\s]+)\s+((.*?\*.*?\*.*?\s)|())(?<EventCategory>[^\s]+)\s(?<CustomInt1>[^\s]+)\s(?<CustomInt2>[^\s]+)\s+.*[^0-9](?<CustomInt3>[^\s]+)\s(?<CustomInt4>[^\s]+)\s(?<CustomStr7>[^\s]+)\s+.*?\s.*?\s+(?<CustomInt5>[^\s]+)\s(?<EventType>[^\s]+)", RegexOptions.Compiled);

        protected string DateFormat = "MMM dd HH:mm:ss";

        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy.MM.dd-HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForAll;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForAll;
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
                        Original = new[] {new[] {"Datetime"} },
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SourceName"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"UsersID"}},
                        MappedField = typeof (RecWrapper).GetProperty("UsersId"),
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Host"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"EventCategory"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomInt1"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"CustomInt2"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CustomInt3"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CustomInt4"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"CustomInt5"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"EventType"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Directory"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"File"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"CustomStr7"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }
                }
            };
        }

        protected override RecorderBase.CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }

        protected override RecorderBase.CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                try
                {
                    if (!int.TryParse(key, out tmp))
                    {
                        int fieldBufferKey;
                        if (key == "Host")
                        {
                            var pieces = groupCollection[key].Value.Split('/');
                            context.FieldBuffer[context.SourceHeaderInfo["Host"]] = pieces[0];
                            if (pieces.Length != 1)
                            {
                                context.FieldBuffer[context.SourceHeaderInfo["File"]] = pieces[pieces.Length - 1];
                                string directory = "";
                                for (int i = 1; i < pieces.Length-1; i++)
                                {
                                    directory += pieces[i]+"/";
                                }
                                context.FieldBuffer[context.SourceHeaderInfo["Directory"]] = directory;
                            }
                         }else if (context.SourceHeaderInfo.TryGetValue(key, out fieldBufferKey))
                            context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                    }
                }
                catch (Exception exception)
                {
                    Console.Out.WriteLine(exception.Message);
                }
            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            return NextInstruction.Return;
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
