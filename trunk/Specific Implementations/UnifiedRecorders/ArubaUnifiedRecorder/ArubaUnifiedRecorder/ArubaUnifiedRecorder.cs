using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.ArubaUnifiedRecorder
{
    public class ArubaUnifiedRecorder: SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"^[^\<]*<(?<SESSION_ID>[^>]+)>\s*<(?<SEVERITY>[^\>]*)>\s*<(?<HOST_NAME>[^\s]*)\s*(?<HOST_IP>[^\>]*)>\s*(?<ACTION>[^\:]*):\s*username=(?<USERNAME>[^\s]*)\s*MAC=(?<MAC>[^\s]*)\s*IP=(?<IP>.[^\s]*)\s*role=(?<ROLE>[^\s]*)\s*VLAN=(?<VLAN>[^\s]*)\s*AP=(?<AP>.*)\s*SSID=(?<SSID>[^\s]*)\s*AAA\sprofile=(?<AAA_PROFILE>[^\s]*)\s*auth\smethod=(?<AUTH_METHOD>[^\s]*)\s*auth\sserver=(?<AUTH_SERVER>[^\s]*)$", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"MAC"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST_IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"IP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ROLE"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AAA_PROFILE"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"VLAN"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr8")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AP"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr9")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"AUTH_SERVER"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr10")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"USER_NAME"}},
                        MappedField = typeof(RecWrapper).GetProperty("UserName")
                    },

                    new DataMapping
                    {
                        Original = new[] {new[] {"SEVERITY"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventCategory")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"HOST_NAME"}},
                        MappedField = typeof(RecWrapper).GetProperty("SourceName")
                    },
                     new DataMapping
                    {
                        Original = new[] {new[] {"Description"}},
                        MappedField = typeof(RecWrapper).GetProperty("Description")
                    }
                }
            };
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

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null)
            { return base.OnBeforeProcessRecordInput(context); }
            Exception error = null;
            var ins = GetHeaderInfo(context, ref error);
            return (ins & NextInstruction.Continue) != NextInstruction.Continue ? ins : base.OnBeforeProcessRecordInput(context);
        }

        protected override NextInstruction OnFieldMatch(RecorderContext context, string source, ref Match match)
        {
            if (!match.Success) return NextInstruction.Skip;
            var groupCollection = match.Groups;

            foreach (var key in RegSplitForAll.GetGroupNames())
            {
                int tmp;
                if (!int.TryParse(key, out tmp))
                    if (context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;

            }
            context.FieldBuffer[context.SourceHeaderInfo["Description"]] = source;
            
            return NextInstruction.Return;
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
