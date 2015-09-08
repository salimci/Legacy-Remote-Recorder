using Natek.Recorders.Remote.Database;

namespace Natek.Recorders.Remote.Unified.Database.PostgreSql
{
    public class PostgreSqlUnifiedRecorder : DbRecorderBase
    {
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new PostgreSqlRecorderContext();
        }

        public override string GetInputName(RecorderContext context)
        {
            return location;
        }

        public override int GetDefaultPort()
        {
            return 5432;
        }
    }
}
