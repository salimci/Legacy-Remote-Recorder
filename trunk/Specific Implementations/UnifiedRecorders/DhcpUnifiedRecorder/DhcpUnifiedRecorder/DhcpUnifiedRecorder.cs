using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers.CSharp;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Dhcp
{
    public class DhcpUnifiedRecorder : FileLineRecorder
    {
        public static readonly Regex RegSplitForSeparator = new Regex(",", RegexOptions.Compiled);

        #region Converter
        protected static object Convert2Date(RecWrapper record, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as DhcpUnifiedRecorder;
            var dtStr = values[0] + " " + values[1];
            if (DateTime.TryParseExact(dtStr, "M/d/y H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(dtStr, "M/d/yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }
        #endregion

        #region MAPPING
        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                        {
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Date"}, new[] {"Time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"ID"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Description"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Host Name"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"IP Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"MAC Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                }
                        }
            };
        }

        protected static DataMappingInfo CreateMappingEnV6()
        {
            return new DataMappingInfo
            {
                Name = "v6",
                Mappings = new[]
                        {
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Date"}, new[] {"Time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"ID"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Description"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Host Name"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"IPv6 Address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Duid Bytes(Hex)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Duid Length"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Error Code"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr10")
                                }
                        }
            };
        }

        protected static DataMappingInfo CreateMappingTr()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                        {
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Tarih"}, new[] {"Saat"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Kimlik"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"Açiklama", "Açıklama"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description")
                                },
                            new DataMapping
                            {
                                    Original =
                                        new[] {new[] {"Ana Bilgisayar Adi", "Ana Bilgisayar Adı"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"IP Adresi"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                            {
                                    Original = new[] {new[] {"MAC Adresi"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                }
                        }
            };
        }
        #endregion

        #region Parser Methods

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var rec = context.InputRecord as TextRecord;
            if (rec == null || rec.RecordText == null)
                return RecordInputType.Unknown;
            if (rec.RecordText.Length == 0)
                return RecordInputType.Comment;
            if (rec.RecordText.StartsWith("#"))
            {
                if (context.InputRecord.ToString().StartsWith("#Fields: "))
                    return RecordInputType.Header;
                return RecordInputType.Comment;
            }
            return RecordInputType.Record;
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return context.InputRecord.ToString().Substring(8);
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForSeparator;
        }

        public override Regex CreateFieldSeparator()
        {
            return RegSplitForSeparator;
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegSplitter; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegSplitter; }
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] { CreateMappingEn(), CreateMappingEnV6(), CreateMappingTr() };
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if (ins != NextInstruction.Do)
                return ins;
            if (string.IsNullOrEmpty(context.Record.CustomStr1))
                context.Record.CustomStr1 = location;
            return NextInstruction.Do;
        }

        public override NextInstruction GetHeaderInfo(RecorderContext context, ref Exception error)
        {
            var offset = context.OffsetInStream;
            var headerOff = new[] { context.HeaderOffset[0], context.HeaderOffset[1] };
            try
            {
                var lineParts = new string[2][];
                var headerPos = new long[] { 0, 0 };
                var curr = 0;
                var cnt = 0;
                if (!context.SetOffset(context.HeaderOffset[0], ref error))
                    return context.FixOffsets(NextInstruction.Abort, offset, headerOff, ref error);

                headerPos[curr] = context.OffsetInStream;
                while (context.ReadRecord(ref error) > 0)
                {
                    lineParts[curr] = HeaderSplitter(context, context.InputRecord.ToString());
                    //Line must have at least 2 commas
                    if (ArrayHelper.AssureLength(lineParts[curr], 3))
                    {
                        curr ^= 1;
                        if (++cnt == 2)
                        {
                            if (lineParts[curr].Length <= lineParts[curr ^ 1].Length)
                            {
                                cnt = 0;
                                if (context.SourceHeaderInfo == null)
                                    context.SourceHeaderInfo = new Dictionary<string, int>();
                                else
                                    context.SourceHeaderInfo.Clear();
                                while (cnt < lineParts[curr].Length)
                                {
                                    context.SourceHeaderInfo[lineParts[curr][cnt].Trim()] = cnt;
                                    cnt++;
                                }
                                context.HeaderInfo = RecordFields2Info(MappingInfos, context.SourceHeaderInfo);
                                if (context.HeaderInfo != null)
                                {
                                    context.HeaderOffset[0] = headerPos[curr];
                                    context.HeaderOffset[1] = headerPos[curr ^ 1];
                                    return context.FixOffsets(NextInstruction.Do, offset, headerOff, ref error);
                                }
                            }
                            cnt = 1;
                        }
                    }
                    else
                        cnt = 0;
                    headerPos[curr] = context.OffsetInStream;
                }
                return context.FixOffsets(error == null ? NextInstruction.Return : NextInstruction.Abort,
                    offset, headerOff, ref error);
            }
            catch (Exception e)
            {
                error = e;
            }
            return context.FixOffsets(NextInstruction.Abort, offset, headerOff, ref error);
        }
        #endregion
    }
}
