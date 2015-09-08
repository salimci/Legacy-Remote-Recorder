using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;
using Natek.Recorders.Remote.StreamBased.Terminal;

namespace Natek.Recorders.Remote.Unified.RadiusUnifiedRecorder
{
    public class RadiusUnifiedRecorder : TerminalRecorder
    {

        protected Regex RegSplitForValue = new Regex("^(?<Datetime>\\S+\\s+\\S+\\s+\\S+\\s+\\S+\\s+\\S+)|\\s+(\\S+)\\s+\\=\\s+\"?([^\"\\s]+)\"?", RegexOptions.Compiled);

        protected string DateFormat = "ddd MMM  d HH:mm:ss yyyy";
        private readonly Regex _state0=new Regex(@"^\S+",RegexOptions.Compiled);
        private readonly Regex _state1 = new Regex(@"^\t", RegexOptions.Compiled);


        protected object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            return DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? dt.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : string.Empty;
        }

        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new RadiusUnifiedRecorderContext(this);
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
                        Original = new[] {new []{"Datetime"}},
                        MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                        MethodInfo = Convert2Date
                    },
                         new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Terminate-Cause"}},
                        MappedField = typeof (RecWrapper).GetProperty("SourceName")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Authentic"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                    },
                      new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Status-Type"}},
                        MappedField = typeof (RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"User-Name"}},
                        MappedField = typeof (RecWrapper).GetProperty("UserName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NAS-IP-Address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Framed-IP-Address"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NAS-Identifier"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Session-Id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Stripped-User-Name"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Calling-Station-Id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Called-Station-Id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Unique-Session-Id"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Request-Authenticator"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                    },
        
                    new DataMapping
                    {
                        Original = new[] {new[] {"Cisco-AVPair"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"NAS-Port"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Input-Octets"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Output-Octets"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt3"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Input-Packets"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Output-Packets"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                        MethodInfo = Convert2Int32
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Acct-Session-Time"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int64
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Timestamp"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int64
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Tunnel-Private-Group-Id:0"}},
                        MappedField = typeof (RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int64
                    },
                        new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof (RecWrapper).GetProperty("Description"),    
                    }   
                }
            };
        }

       
        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            try
            {
                    if (!match.Success) return NextInstruction.Skip;
                    var datetime = false;
                    while (match.Success)
                    {
                        if (!datetime) context.FieldBuffer[context.SourceHeaderInfo["Datetime"]] = match.Groups["Datetime"].Value;
                        datetime = true;
                        if (context.SourceHeaderInfo.ContainsKey(match.Groups[1].Value))
                            context.FieldBuffer[context.SourceHeaderInfo[match.Groups[1].Value]] = match.Groups[2].Value;
                        match = match.NextMatch();
                    }
                    context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
                return NextInstruction.Return;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while processing veribranch record: " + e);
                return NextInstruction.Abort;
            }
        }

        protected override NextInstruction OnProcessInputTextRecord(RecorderContext _context, string[] fields, ref Exception error)
        {
            var context = _context as RadiusUnifiedRecorderContext;
            var lineRecord = context.InputRecord.ToString();
            
           
            if (context.State == 0)
            {
                var matchState0 = _state0.Match(lineRecord);
                if (matchState0.Success)
                {
                    context.LastBuffer.AppendLine(lineRecord);
                    context.State = 1;
                }
            }
            else if(context.State==1)
            {
                var matchState0 = _state0.Match(lineRecord);
                
                if (matchState0.Success)
                {
                    context.InputRecord.SetValue(context.LastBuffer.ToString());
                    context.LastBuffer.Remove(0, context.LastBuffer.Length);
                    context.LastBuffer.AppendLine(lineRecord);
                    return NextInstruction.Do;
                }
                context.LastBuffer.AppendLine(lineRecord);
            }
            return NextInstruction.Skip;
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
