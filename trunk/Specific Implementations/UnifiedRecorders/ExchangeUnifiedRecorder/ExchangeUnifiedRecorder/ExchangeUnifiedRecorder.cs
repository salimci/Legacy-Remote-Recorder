using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Microsoft.Exchange
{
    public class ExchangeUnifiedRecorder : FileLineRecorder
    {
        protected static readonly Regex RegSplitForValue = new Regex("([^,\"]*\"([^\"]*)\"[^,]*|([^,]*)),?",RegexOptions.Compiled);
        #region Converter

        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as ExchangeUnifiedRecorder;
            if (DateTime.TryParseExact(values[0], "yyyy-M-d'T'H:m:s.fff'Z'", CultureInfo.InvariantCulture,DateTimeStyles.None, out dt)
                || DateTime.TryParseExact(values[0], "yy-M-d'T'H:m:s.fff'Z'", CultureInfo.InvariantCulture,DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        public static object CustomStr1Splitter(RecWrapper rec, string field, string[] values, object data)
        {
            try
            {
                if (string.IsNullOrEmpty(values[0]))
                    return values[0];
                var uriStr = HttpHelper.UrlDecode(values[0]);
                if (uriStr.Length > 1800)
                {
                    rec.CustomStr8 = uriStr.Substring(1800, uriStr.Length > 2700 ? 900 : uriStr.Length - 1800);
                    rec.CustomStr5 = uriStr.Substring(900, 900);
                }
                return StringHelper.MakeSureLength(uriStr, 900);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static object DescriptionSplitter(RecWrapper rec, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
                return values[1];
            if (string.IsNullOrEmpty(values[1]))
                return values[0];
            return values[0] + "-" + values[1];
        }

        public static object SetIpAddress(RecWrapper rec, string field, string[] values, object data)
        {
            return values[string.IsNullOrEmpty(values[1]) ? 0 : 1];
        }

        #endregion

        #region Mapping

        public static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
                {
                    Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new[] {new[] {"date-time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"event-id"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventCategory")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"connector-id"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventType")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"source"}},
                                    MappedField = typeof (RecWrapper).GetProperty("SourceName"),
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"recipient-address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                                    MethodInfo = CustomStr1Splitter
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"message-subject"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"sender-address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"recipient-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"recipient-count"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                                    MethodInfo = Convert2Int32

                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"total-bytes"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                                    MethodInfo = Convert2Int64
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"client-ip"}, new[] {"original-client-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr4"),
                                    MethodInfo = SetIpAddress
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"connector-id"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr5")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"message-id"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6")

                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"recipient-address"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"recipient-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"server-ip"}, new[] {"original-server-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr9"),
                                    MethodInfo = SetIpAddress
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"internal-message-id"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventId"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"server-hostname"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                },
                            new DataMapping
                                {
                                    Original = new[] {new[] {"source-context"}, new[] {"reference"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description"),
                                    MethodInfo = DescriptionSplitter
                                }
                        }
                };
        }

        #endregion

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            if (context.InputRecord == null)
                return RecordInputType.Comment;

            var txt = context.InputRecord.ToString();
            if (txt.Length == 0)
                return RecordInputType.Comment;

            if (txt.StartsWith("#"))
            {
                if (txt.StartsWith("#Fields: "))
                    return RecordInputType.Header;
                return RecordInputType.Comment;
            }
            return RecordInputType.Record;
        }

        protected override DataMappingInfo[] CreateMappingInfos()
        {
            return new[] {CreateMappingEn()};
        }

        protected override string GetHeaderText(RecorderContext context)
        {
            return context.InputRecord.ToString().Substring(8);
        }


        public override Regex CreateFieldSeparator()
        {
            return RegSplitForValue;
        }

        public override Regex CreateHeaderSeparator()
        {
            return RegSplitForValue;
        }

        protected override CanAddMatchDelegate CanAddMatchHeader
        {
            get { return CanAddMatchRegValue; }
        }

        protected override CanAddMatchDelegate CanAddMatchField
        {
            get { return CanAddMatchRegValue; }
        }
    }
}
