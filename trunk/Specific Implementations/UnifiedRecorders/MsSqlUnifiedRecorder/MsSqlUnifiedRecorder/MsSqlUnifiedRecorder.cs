using Natek.Recorders.Remote.Database;

namespace Natek.Recorders.Remote.Unified.Database.MsSql
{
    public class MsSqlUnifiedRecorder : DbRecorderBase
    {
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new MsSqlRecorderContext();
        }

        public override string GetInputName(RecorderContext context)
        {
            return location;
        }

        public override int GetDefaultPort()
        {
            return 1433;
        }
    }
}
