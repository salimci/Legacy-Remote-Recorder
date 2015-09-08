using System;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.WebsenseMailSecurityUnifiedRecoder
{
    public class WebsenseMailSecurityUnifiedRecorder : SyslogRecorderBase
    {
        protected static readonly Regex RegSplitForAll = new Regex(@"\|(?<DEVICE>[^\|]*)\|(?<MOD>[^\|]*)\|(?<VERSION>[^\|]*)\|(?<EVENT>[^\|]*)\|(?<SUB_EVENT>[^\|]*)\|(?<EVENT_ID>[^\|]*)\|\s*dvc=(?<DVC>[0-9\.]*[^\s]*)\s*dvchost=(?<DVC_HOST>[^\s]*)\s*rt=(?<RT>[0-9][^\s]*)\s*(externalId=(?<EXTERNAL_ID>[0-9][^\s]*))?\s*(messageId=(?<MESSAGE_ID>[0-9][^\s]*))?\s*(suser=(?<SUSER>[^\s]*)\s*)?\s*(duser=(?<DUSER>[^\s]*))?\s*((msg=(?<MSG>.*)\s*in=(?<MESSAGE_IN>[0-9][^\n]*))|(src=(?<SRC>[0-9\.][^\s]*)\s*dst=(?<DST>[0-9\.][^\s]*)\s*encryptedDelivery=(?<ENCRYPTED_DELIVERY>.*?)\s*deliveryCode=(?<DELIVERY_CODE>[0-9][^\s]*)\s*deliveryCodeInfo=(?<DELIVERY_CODE_INFO>.*?)\s*app=(?<APP>.*?)\s*act=(?<DELIVERY_ACT>[^\n]*))|(in=(?<POLICY_IN>[0-9][^\s]*)\s*deviceDirection=(?<DEVICE_DIRECTION>[^\s]*)\s*deviceFacility=(?<DEVICE_FACILITY>.*?)\s*deviceProcessName=(?<DEVICE_PROCESS_NAME>.*?)\s*act=(?<POLICY_ACT>.*?)\s*cat=(?<CAT>.*?)\s*cs1=(?<CS1>.*?)\s*exceptionReason=(?<EXCEPTION_REASON>.*)))", RegexOptions.Compiled);

        protected DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                {
                    new DataMapping
                    {
                        Original = new[] {new[] {"DUSER"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr1")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MSG"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr2")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SUSER"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr3")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DST"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr4")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"SRC"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr5")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE_DIRECTION"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr6")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE_FACILITY"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomStr7")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"POLICY_ACT"}},
                        MappedField = typeof(RecWrapper).GetProperty("EventType")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DEVICE"}},
                        MappedField = typeof(RecWrapper).GetProperty("ComputerName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"DVC_HOST"}},
                        MappedField = typeof(RecWrapper).GetProperty("SourceName")
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"ENTERNAL_ID"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt6"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"MESSAGE_ID"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt7"),
                        MethodInfo = Convert2Int32
                    },
                    new DataMapping
                    {
                        Original = new[] {new[] {"RT"}},
                        MappedField = typeof(RecWrapper).GetProperty("CustomInt8"),
                        MethodInfo = Convert2Int32
                    }
                } 
            };
        }

       

        protected override NextInstruction OnBeforeProcessRecordInput(RecorderContext context)
        {
            if (context.HeaderInfo != null) 
            {return base.OnBeforeProcessRecordInput(context);}
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
                    if(context.SourceHeaderInfo.ContainsKey(key))
                        context.FieldBuffer[context.SourceHeaderInfo[key]] = groupCollection[key].Value;
                
            }

            context.FieldBuffer.Add(source);
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
