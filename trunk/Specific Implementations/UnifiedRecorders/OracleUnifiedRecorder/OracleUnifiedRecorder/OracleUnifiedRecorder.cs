using Natek.Recorders.Remote.Database;

namespace Natek.Recorders.Remote.Unified.Database.Oracle
{
    public class OracleUnifiedRecorder : DbRecorderBase
    {
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new OracleRecorderContext();
        }

        public override string GetInputName(RecorderContext context)
        {
            return location;
        }

        public override int GetDefaultPort()
        {
            return 1521;
        }
    }
}
