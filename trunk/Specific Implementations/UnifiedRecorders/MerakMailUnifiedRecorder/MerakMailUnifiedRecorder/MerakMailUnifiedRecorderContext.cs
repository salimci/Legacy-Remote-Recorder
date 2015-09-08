using System.Collections.Generic;

namespace Natek.Recorders.Remote.Unified.MerakMailUnifiedRecorder
{
    class MerakMailUnifiedRecorderContext : FileLineRecorderContext
    {
        public Dictionary<string, string> Buffer; 
        
        public MerakMailUnifiedRecorderContext() : this(null) { }

        public MerakMailUnifiedRecorderContext(RecorderBase recorder)
            : base(recorder)
        {
            Buffer = new Dictionary<string, string>();
        }
    }
}
