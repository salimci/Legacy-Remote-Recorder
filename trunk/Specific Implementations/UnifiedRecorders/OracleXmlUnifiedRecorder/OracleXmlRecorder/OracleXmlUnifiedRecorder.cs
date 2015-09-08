using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.OracleXmlUnifiedRecorder
{
    public class OracleXmlUnifiedRecorder:FileLineRecorder
    {
        protected static Regex RegSplitForAll = new Regex("^\\<\\/?(?<root>[^\\>\\s]+)\\>(?=\\<)|\\<(?<tag>.*?)\\>\\s*(.*?)\\<\\/(.*?)\\>", RegexOptions.Compiled);
        private readonly Regex _state = new Regex("^\\<\\/?(?<root>[^\\>\\s]+)\\>", RegexOptions.Compiled);
        private readonly StringBuilder _lastBuffer = new StringBuilder();
        private String _rootName;
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
                        Original = new[] {new[] {"Extended_Timestamp"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo =Convert2Date
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"Audit_Type"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo=Convert2Int32
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"StatementId"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo=Convert2Int32
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"EntryId"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo=Convert2Int32
                    },
                         new DataMapping
                    {
                        Original = new[] {new[] {"OS_Process"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9"),
                        
                    },
                         new DataMapping
                    {
                        Original = new[] {new[] {"Instance_Number"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo=Convert2Int64
                    },
                         new DataMapping
                    {
                        Original = new[] {new[] {"Returncode"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo=Convert2Int64
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"DBID"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo=Convert2Int64
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"Session_Id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt9"),
                        MethodInfo=Convert2Int64
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"DB_User"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Ext_Name"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"OS_User"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Userhost"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Terminal"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"OSPrivilege"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                       new DataMapping
                    {
                        Original = new[] {new[] {"Sql_Text"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    }
                }
            };
        }

        protected object Convert2Date(RecWrapper rec, string field, string[] fieldvalues, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(fieldvalues[0].Split('Z')[0], "yyyy-MM-ddTHH:mm:ss.ffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }
  
        protected override NextInstruction OnProcessInputTextRecord(RecorderContext context, string[] fields, ref Exception error)
        {
            
            var lineRecord = context.InputRecord.ToString();
            var matchState = _state.Match(lineRecord);
            if (matchState.Success)
            {
                if (matchState.Groups[1].Value == _rootName)
                {
                    _rootName = null;
                    context.InputRecord.SetValue(_lastBuffer.ToString());
                    _lastBuffer.Remove(0, _lastBuffer.Length);
                    return NextInstruction.Do;
                }
                if (String.IsNullOrEmpty(_lastBuffer.ToString()))
                {
                    _rootName = matchState.Groups[1].Value;
                    _lastBuffer.Append(lineRecord);

                }
                else
                {
                    _lastBuffer.Append(lineRecord);
                }

            }
             return NextInstruction.Skip;
        }
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
                if (!match.Success) return NextInstruction.Skip;
                while (match.Success)
                {
                    if (context.SourceHeaderInfo.ContainsKey(match.Groups[2].Value))
                        context.FieldBuffer[context.SourceHeaderInfo[match.Groups[2].Value]] = match.Groups[1].Value;
                    match = match.NextMatch();
                }
                return NextInstruction.Return;
           
      
        }
        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (string.IsNullOrEmpty(context.InputRecord.ToString()))
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

    }
}
