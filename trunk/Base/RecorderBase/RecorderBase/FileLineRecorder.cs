using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public abstract class FileLineRecorder : FileRecorder
    {
        protected override RecorderContext CreateContextInstance(params object[] ctxArgs)
        {
            return new FileLineRecorderContext(this);
        }

        protected override void InitContextInstance(RecorderContext context, params object[] ctxArgs)
        {
            base.InitContextInstance(context, ctxArgs);
            context.InputRecord = new TextRecord();
        }

        protected override NextInstruction ApplyContextMapping(RecorderContext context, string[] fields, ref System.Exception error)
        {
            if (fields == null || fields.Length == 0)
                return NextInstruction.Skip;
            return base.ApplyContextMapping(context, fields, ref error);
        }
    }
}
