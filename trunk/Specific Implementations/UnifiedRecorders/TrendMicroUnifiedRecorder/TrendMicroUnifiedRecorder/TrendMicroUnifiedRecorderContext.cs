using System.Collections.Generic;

namespace Natek.Recorders.Remote.Unified.TrendMicroUnifiedRecorder
{
    class TrendMicroUnifiedRecorderContext : FileLineRecorderContext
    {
        public Dictionary<string,string> Buffer;

        public TrendMicroUnifiedRecorderContext() : this(null) { }

        public TrendMicroUnifiedRecorderContext(RecorderBase recorder): base(recorder)
        {
            Buffer = new Dictionary<string, string>();
        }
    }
}
