using System.Collections.Generic;

namespace Natek.Recorders.Remote.Unified.TrendMicroUrlUnifiedRecorder
{
    class TrendMicroUrlUnifiedRecorderContext: FileLineRecorderContext
    {
        public Dictionary<string,string> Buffer;

        public TrendMicroUrlUnifiedRecorderContext() : this(null) { }

        public TrendMicroUrlUnifiedRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
            Buffer = new Dictionary<string, string>();
        }
    }
}
