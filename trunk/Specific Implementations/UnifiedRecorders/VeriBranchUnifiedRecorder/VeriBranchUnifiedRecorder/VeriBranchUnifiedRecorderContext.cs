using System.Text;
using System.Text.RegularExpressions;

namespace Natek.Recorders.Remote.Unified.VeriBranchUnifiedRecorder
{
    class VeriBranchUnifiedRecorderContext : FileLineRecorderContext
    {
        public Regex RegSplitForValue { get; set; }
        public string DateFormat { get; set; }

        public VeriBranchUnifiedRecorderContext() : this(null) { }

        public VeriBranchUnifiedRecorderContext(RecorderBase recorder) : base(recorder)
        {
           
        }

        public VeriBranchUnifiedRecorderContext(RecorderBase recorder, Regex regSplitForValue, string dateFormat)
            : base(recorder)
        {
            RegSplitForValue = regSplitForValue;
            DateFormat = dateFormat;
        }
    }
}
