using System;
using System.Text;
using System.Text.RegularExpressions;
using Natek.Helpers.Execution;
using Natek.Helpers.IO;
using Natek.Recorders.Remote.Helpers.Basic;

namespace Natek.Recorders.Remote.StreamBased.Terminal
{
    public abstract class TerminalRecorder : FileRecorder
    {
        public int Port { get; set; }
        public int ReadTimeout { get; set; }
        public Regex Pattern { get; set; }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;

            var vInt = 0;
            switch (keyword)
            {
                case "Port":
                    touchCount++;
                    if (int.TryParse(value, out vInt))
                    {
                        Port = vInt;
                    }
                    break;
                case "ReadTimeout":
                    touchCount++;
                    if (int.TryParse(value, out vInt))
                        ReadTimeout = vInt;
                    break;
                case "Pattern":
                    touchCount++;
                    try
                    {
                        Pattern = new Regex(value);
                    }
                    catch (Exception e)
                    {
                        error = e;
                        return false;
                    }
                    break;
            }
            return true;
        }

        protected override int CompareFiles(RecorderFileSystemInfo l, RecorderFileSystemInfo r)
        {
            if (l != null && r != null)
            {
                if (StringHelper.NullEmptyEquals(l.FileNodeId, r.FileNodeId))
                    return 0;
                var diff = l.LastWriteTimeUtc.CompareTo(r.LastWriteTimeUtc);
                if (diff < 0)
                    return -1;
                if (diff > 0)
                    return 1;
            }
            return base.CompareFiles(l, r);
        }

        protected override NextInstruction OnBeforeSetData(RecorderContext context)
        {
            var ins = base.OnBeforeSetData(context);
            if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                return ins;
            context.OffsetInStream++;
            return NextInstruction.Do;
        }

        public override RecordInputType InputTextType(RecorderContext context, ref Exception error)
        {
            var ctx = context as TerminalRecorderContext;
            if (ctx == null)
            {
                error = new Exception("Context is not TerminalRecorderContext or null");
                return RecordInputType.Error;
            }
            var line = context.InputRecord.ToString();
            if (line == ctx.Keyword)
            {
                if (ctx.WaitBegin)
                {
                    ctx.Keyword = "END" + ctx.Keyword.Substring(5);
                    ctx.WaitBegin = false;
                    return RecordInputType.Comment;
                }
                return RecordInputType.EndOfStream;
            }
            if (ctx.WaitBegin)
                return RecordInputType.Comment;
            if (line.StartsWith("0;"))
            {
                var index = line.IndexOf(';', 2);
                if (++index < line.Length)
                {
                    context.InputRecord.SetValue(line.Substring(index));
                    return RecordInputType.Record;
                }
                error = new Exception("Unexpected record line. No Record order after 0;");
            }
            else
            {
                var regErr = new Regex("^[0-9]+;[0-9]+;.", RegexOptions.Compiled);
                var sb = new StringBuilder();
                do
                {
                    var m = regErr.Match(line);
                    sb.Append(m.Success ? line.Substring(m.Length) : line);
                    if (context.ReadRecord(ref error) <= 0)
                        break;
                    line = context.InputRecord.ToString();
                    if (line == ctx.Keyword)
                        break;
                } while (true);
                context.InputRecord.SetValue(sb.ToString());
            }
            return RecordInputType.Error;
        }

        protected override RecorderFileSystemInfo CreateDirectoryInfo(RecorderContext context, string absoluteName)
        {
            return new TerminalRemoteFileSystemInfo(context, absoluteName, FileSystemHelper.FileNameOf(absoluteName, context.DirectorySeparatorChar)) { Context = context as TerminalRecorderContext };
        }

        protected override RecorderFileSystemInfo CreateFileSystemInfo(RecorderContext context, string fullName)
        {
            return new TerminalRemoteFileSystemInfo(context, fullName, FileSystemHelper.FileNameOf(fullName, context.DirectorySeparatorChar));
        }
    }
}
