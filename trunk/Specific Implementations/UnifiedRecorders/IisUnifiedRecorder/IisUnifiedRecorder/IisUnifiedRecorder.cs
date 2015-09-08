using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Recorders.Remote.Helpers.Basic;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote.Unified.Microsoft.Iis
{
    public class IisUnifiedRecorder : FileLineRecorder
    {
        public static readonly Regex RegSplitForSeparator = new Regex(" ", RegexOptions.Compiled);

        #region Converters
        protected static object CustomStr5Splitter(RecWrapper record, string field, string[] values, object data)
        {
            return StringHelper.MakeSureLength(values[0], 900);
        }

        protected static object VersionConcat(RecWrapper record, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
                return HttpHelper.UrlDecode(values[1]);
            if (string.IsNullOrEmpty(values[1]))
                return HttpHelper.UrlDecode(values[0]);
            return HttpHelper.UrlDecode(values[0] + " " + values[1]);
        }

        protected static object CustomStr1Splitter(RecWrapper record, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
                return values[0];

            var uriStr = HttpHelper.UrlDecode(values[0]);
            if (uriStr.Length > 1800)
                record.CustomStr10 = uriStr.Substring(900, 900);
            else if (uriStr.Length > 900)
                record.CustomStr10 = uriStr.Substring(900, uriStr.Length - 900);
            return StringHelper.MakeSureLength(uriStr, 900);
        }

        protected static object DescriptionSplitter(RecWrapper rec, string field, string[] values, object data)
        {
            if (string.IsNullOrEmpty(values[0]))
            {
                rec.CustomStr1 = string.Empty;
                return string.Empty;
            }

            var uriStr = HttpHelper.UrlDecode(values[0]);
            var parts = uriStr.Split(new[] { ';' }, 2);
            if (parts.Length == 2)
                rec.CustomStr1 = StringHelper.MakeSureLength(parts[1], 900);

            return StringHelper.MakeSureLength(parts[0], 900);
        }


        protected static object Convert2Date(RecWrapper rec, string field, string[] values, object data)
        {
            DateTime dt;
            var recorder = data as IisUnifiedRecorder;
            var dtStr = values[0] + " " + values[1];
            if (DateTime.TryParseExact(dtStr, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
            || DateTime.TryParseExact(dtStr, "y-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.AddSeconds(recorder == null ? 0 : recorder.zone).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            return string.Empty;
        }

        #endregion

        #region Mapping
        protected static DataMappingInfo CreateMappingEn()
        {
            return new DataMappingInfo
            {
                Mappings = new[]
                        {
                            new DataMapping
                                {
                                    Original = new [] {new [] {"date"}, new [] {"time"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Datetime"),
                                    MethodInfo = Convert2Date
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-sitename"}},
                                    MappedField = typeof (RecWrapper).GetProperty("SourceName")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-method"}},
                                    MappedField = typeof (RecWrapper).GetProperty("EventType")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-uri-stem"}},
                                    MappedField = typeof (RecWrapper).GetProperty("Description"),
                                    MethodInfo = DescriptionSplitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-uri-query"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr1"),
                                    MethodInfo = CustomStr1Splitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-username"}},
                                    MappedField = typeof (RecWrapper).GetProperty("UserName")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"c-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr3")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt1"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-substatus"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt2"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-win32-status"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt4"),
                                    MethodInfo = Convert2Int32
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-ip"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr4")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"s-port"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr2")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-version"},new [] {"cs(User-Agent)"} },
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr6"),
                                    MethodInfo = VersionConcat,
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs(Referer)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr5"),
                                    MethodInfo = CustomStr5Splitter
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"sc-bytes"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr7")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs(Cookie)"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr8")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-host"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomStr9")
                                },
                            new DataMapping
                                {
                                    Original = new [] {new [] {"cs-bytes"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt6"),
                                    MethodInfo = Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"time-taken"}},
                                    MappedField = typeof (RecWrapper).GetProperty("CustomInt5"),
                                    MethodInfo = Convert2Int32
                                },
                                new DataMapping
                                {
                                    Original = new [] {new [] {"s-computername"}},
                                    MappedField = typeof (RecWrapper).GetProperty("ComputerName")
                                }
                        }
            };
        }


        #endregion

        protected string IisType;

        protected override bool OnArgParsed(string keyword, bool quotedKeyword,string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            if (keyword == "T")
            {
                IisType = value;
                touchCount++;
            }
            return true;
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
            if (rec.RecordText.Length == 0)
                return RecordInputType.Comment;
            if (context.InputRecord.ToString().StartsWith("#"))
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

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if (ins != NextInstruction.Do)
                return ins;
            if (string.IsNullOrEmpty(context.Record.ComputerName))
                context.Record.ComputerName = remoteHost;
            if (string.IsNullOrEmpty(context.Record.CustomStr10))
                context.Record.CustomStr10 = IisType;
            return NextInstruction.Do;
        }
    }
}
