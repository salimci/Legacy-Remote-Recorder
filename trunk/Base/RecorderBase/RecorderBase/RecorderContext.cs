using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CustomTools;
using Natek.Helpers;
using Natek.Helpers.Execution;
using Natek.Helpers.Patterns;
using Natek.Recorders.Remote.Mapping;

namespace Natek.Recorders.Remote
{
    public abstract class RecorderContext : DisposablePattern
    {
        private readonly long[] _headerOffset = new long[] { 0, 0 };
        protected List<string> headerBuffer;
        protected AutoExtendList<string> fieldBuffer;
        protected StringBuilder lastKeywordBuffer;

        public RecorderContext()
            : this(null)
        {
        }

        public RecorderContext(RecorderBase recorder)
        {
            Recorder = recorder;
            headerBuffer = new List<string>();
            fieldBuffer = new AutoExtendList<string>();
            DirectorySeparatorChar = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            lastKeywordBuffer = new StringBuilder();
        }

        public string DirectorySeparatorChar { get; set; }

        public List<string> HeaderBuffer { get { return headerBuffer; } }
        public Match LastHeaderMatch { get; set; }

        public AutoExtendList<string> FieldBuffer { get { return fieldBuffer; } }
        public Match LastFieldMatch { get; set; }

        public RecorderBase Recorder { get; set; }

        public long[] HeaderOffset
        {
            get { return _headerOffset; }
        }

        public long InputModifiedOn { get; set; }
        public long OffsetInStream { get; set; }
        public CustomServiceBase Service { get; set; }
        public DataMappingInfo HeaderInfo { get; set; }
        public RecWrapper Record { get; set; }
        public long RecordSizeInBytes { get; set; }
        public int RecordSent { get; set; }
        public Dictionary<string, int> SourceHeaderInfo { get; set; }
        public Encoding InputEncoding { get; set; }
        public Record InputRecord { get; set; }
        public string LastRecordDate { get; set; }
        public string LastKeywords { get; set; }
        public string LastFile { get; set; }
        public string LastLine { get; set; }
        public Dictionary<string, int> FieldMappingIndexLookup { get; set; }

        public StringBuilder LastKeywordBuffer
        {
            get { return lastKeywordBuffer; }
        }

        public abstract bool SetOffset(long offset, ref Exception error);
        public abstract long ReadRecord(ref Exception error);
        public abstract bool CreateReader(ref Exception error);

        public abstract NextInstruction FixOffsets(NextInstruction nextInstruction, long offset, long[] headerOff,
                                                   ref Exception error);
    }
}
