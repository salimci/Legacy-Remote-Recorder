using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Log;
using Natek.Helpers;
using Natek.Helpers.Execution;

namespace Natek.Recorders.Remote
{
    public abstract class FileRecorder : PeriodicRecorder
    {
        public static readonly Regex RegFilenameNumericStyle = new Regex("([^0-9]+)|([0-9]+)", RegexOptions.Compiled);
        public delegate int FileComparer(RecorderFileSystemInfo l, RecorderFileSystemInfo r);

        protected string filePattern;
        protected bool fileSelectionCaseSensitive;
        protected bool fileOrderingCaseSensitive;
        protected string dayIndMap;
        protected string cultureNlsName;
        protected string fileSortOrder;
        protected Dictionary<string, int> DayNames;
        protected List<FileComparer> FileComparers;

        protected virtual int CompareFiles(RecorderFileSystemInfo l, RecorderFileSystemInfo r)
        {
            if (FileComparers == null || FileComparers.Count == 0)
                return CompareFilesByName(l, r);

            foreach (var fileComparer in FileComparers)
            {
                if (fileComparer == null) continue;

                var result = fileComparer.Invoke(l, r);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        protected virtual int CompareFilesByName(RecorderFileSystemInfo l, RecorderFileSystemInfo r)
        {
            var mL = RegFilenameNumericStyle.Match(l.Name);
            var mR = RegFilenameNumericStyle.Match(r.Name);

            do
            {
                if (mL.Success)
                {
                    if (mR.Success)
                    {
                        var diff = mL.Groups[2].Success && mR.Groups[2].Success
                                   ? int.Parse(mL.Groups[2].Value) - int.Parse(mR.Groups[2].Value)
                                   : string.Compare(mL.Groups[1].Value, mR.Groups[1].Value, fileOrderingCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        if (diff != 0)
                            return diff;
                        mL = mL.NextMatch();
                        mR = mR.NextMatch();
                    }
                    else
                        return 1;
                }
                else if (mR.Success)
                    return -1;
                else
                    return 0;
            } while (true);
        }

        protected virtual int CompareFilesByModDate(RecorderFileSystemInfo l, RecorderFileSystemInfo r)
        {
            if (l.LastWriteTimeUtc < r.LastWriteTimeUtc)
                return -1;

            if (l.LastWriteTimeUtc > r.LastWriteTimeUtc)
                return 1;

            return 0;
        }

        protected virtual int CompareFilesByDayName(RecorderFileSystemInfo l, RecorderFileSystemInfo r)
        {
            var regFileName = new Regex(filePattern);

            var mL = regFileName.Match(l.Name);
            var mR = regFileName.Match(r.Name);

            return (mL.Groups[1].Success && mR.Groups[1].Success) ?
                DateCompareHelper.CompareDateTimeDay(mL.Groups[1].Value, mR.Groups[1].Value, DayNames, fileOrderingCaseSensitive) : CompareFilesByName(l, r);

        }

        protected virtual RecorderFileSystemInfo CreateFileSystemInfo(RecorderContext context, string fullName)
        {
            return new RecorderFileSystemInfoLocal(new FileInfo(fullName));
        }

        public override void SetConfigData(int cfgIdentity, string cfgLocation, string cfgLastLine, string cfgLastPosition, string cfgLastFile,
string cfgLastKeywords, bool cfgFromEndOnLoss, int cfgMaxLineToWait, string cfgUser, string cfgPassword,
string cfgRemoteHost, int cfgSleepTime, int cfgTraceLevel, string cfgCustomVar1, int cfgCustomVar2,
string cfgVirtualhost, string cfgDal, int cfgZone)
        {
            base.SetConfigData(cfgIdentity, cfgLocation, cfgLastLine, cfgLastPosition, cfgLastFile, cfgLastKeywords, cfgFromEndOnLoss, cfgMaxLineToWait, cfgUser, cfgPassword, cfgRemoteHost, cfgSleepTime, cfgTraceLevel, cfgCustomVar1, cfgCustomVar2, cfgVirtualhost, cfgDal, cfgZone);

            try
            {
                DayNames = DateCompareHelper.CreateDayIndex(cultureNlsName, dayIndMap);

                var regSplit = new Regex(@"\s*([\w]*)\s*", RegexOptions.Compiled);
                if (fileSortOrder == null) fileSortOrder = "byName";

                var match = regSplit.Match(fileSortOrder);

                if (!match.Success) return;

                FileComparers = new List<FileComparer>();

                while (match.Groups[1].Success)
                {
                    switch (match.Groups[1].Value)
                    {
                        case "byName":
                            FileComparers.Add(CompareFilesByName);
                            break;
                        case "byModDate":
                            FileComparers.Add(CompareFilesByModDate);
                            break;
                        case "byDayName":
                            FileComparers.Add(CompareFilesByDayName);
                            break;
                    }
                    match = match.NextMatch();
                }
            }
            catch (NotSupportedException e)
            {
                Log(LogLevel.ERROR, e.Message);
            }
        }

        protected override void InitContextInstance(RecorderContext context, params object[] ctxArgs)
        {
            base.InitContextInstance(context, ctxArgs);
            context.HeaderOffset[0] = headerOffset[0];
            context.HeaderOffset[1] = headerOffset[1];
            var ctxFile = context as FileRecorderContext;
            if (ctxFile == null)
                throw new ArgumentException("context parameter is not an FileRecorderContext");
            if (!string.IsNullOrEmpty(lastFile))
            {
                ctxFile.CurrentFile = CreateFileSystemInfo(context, location + context.DirectorySeparatorChar + lastFile);
                ctxFile.LastFile = ctxFile.CurrentFile.Name;
            }
            ctxFile.InputModifiedOn = inputModifiedOn;
            ctxFile.DayIndexMap = dayIndMap;
            ctxFile.FileSortOrder = fileSortOrder;
        }

        protected override NextInstruction DoLogic(RecorderContext context)
        {
            try
            {
                Log(LogLevel.INFORM, "Begin Processing cfgLastFile(" + lastFile + ") LastRecord(" + lastPosition + ")");
                var fileRecorderContext = context as FileRecorderContext;
                if (fileRecorderContext == null)
                {
                    Log(LogLevel.ERROR, "Invalid Context Type: Expected FileRecorderContext");
                    return NextInstruction.Abort;
                }

                if (!GetLastProcessedFile(fileRecorderContext, false))
                    return NextInstruction.Abort;

                Exception error = null;
                Log(LogLevel.DEBUG, "RecordSent:" + fileRecorderContext.RecordSent + ", MaxRecord=" + maxRecordSend);
                while (fileRecorderContext.RecordSent < maxRecordSend)
                {
                    if (fileRecorderContext.CurrentFile != null)
                    {
                        Log(LogLevel.DEBUG, "Refresh Current File:" +
                                            "" + fileRecorderContext.CurrentFile.FullName);
                        fileRecorderContext.CurrentFile.Refresh();
                        if (fileRecorderContext.CurrentFile.Exists)
                        {
                            Log(LogLevel.DEBUG, "File Exist, Open Stream");
                            var ins = fileRecorderContext.CreateStream(ref error);
                            try
                            {
                                if ((ins & NextInstruction.Continue) == NextInstruction.Continue)
                                {
                                    if ((ins & NextInstruction.Do) == NextInstruction.Do
                                        && fileRecorderContext.Stream != null)
                                    {
                                        fileRecorderContext.CurrentFile.Refresh();
                                        fileRecorderContext.InputModifiedOn =
                                            fileRecorderContext.CurrentFile.LastWriteTimeUtc.Ticks;
                                        Log(LogLevel.DEBUG, "Processing records");
                                        ins = ProcessContextRecords(fileRecorderContext, ref error);
                                        if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                                        {
                                            Log(LogLevel.DEBUG, "Process Context require exit:" + ins +
                                                                (error != null ? error.ToString() : null));
                                            return ins;
                                        }
                                        Log(LogLevel.DEBUG,
                                            "Process Context Records Completed. Completion Result:" + ins);
                                    }
                                    else if (!GetLastProcessedFile(fileRecorderContext, false))
                                        return NextInstruction.Abort;
                                }
                            }
                            finally
                            {
                                fileRecorderContext.CloseStream(ref error);
                            }
                        }
                        else
                        {
                            Log(LogLevel.DEBUG, "File Not Exist");
                        }
                    }
                    Log(LogLevel.DEBUG, "Check Is Last File:" + fileRecorderContext.IsLastFile);
                    if (fileRecorderContext.IsLastFile)
                    {
                        Log(LogLevel.DEBUG, "File is last file so return");
                        return NextInstruction.Return;
                    }
                    Log(LogLevel.DEBUG, "Get Next File");
                    if (!GetLastProcessedFile(fileRecorderContext, true))
                    {
                        Log(LogLevel.DEBUG, "Get Next File return abort");
                        return NextInstruction.Abort;
                    }
                }
                Log(LogLevel.DEBUG, "Max Record Reached..Exit success");
            }
            catch (Exception ex)
            {
                Log(LogLevel.ERROR, "Error in timer handler:" + ex);
            }
            return NextInstruction.Abort;
        }

        protected Regex GetPattern()
        {
            try
            {
                var flags = RegexOptions.Compiled | RegexOptions.CultureInvariant;
                if (!fileOrderingCaseSensitive)
                    flags |= RegexOptions.IgnoreCase;

                return new Regex(string.IsNullOrEmpty(filePattern) ? "^.*$" : CompleteFilePattern(filePattern), flags);
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while compiling file pattern [" + filePattern + "]: " + e.Message);
            }
            return null;
        }

        protected string CompleteFilePattern(string pattern)
        {
            if (pattern.EndsWith("$"))
            {
                var escape = GetPatternEscape(pattern, 1);
                if (escape % 2 == 1)
                    pattern += "$";
            }
            else
            {
                var escape = GetPatternEscape(pattern, 0);
                if (escape % 2 == 1)
                    pattern += '\\';
                pattern += "$";
            }

            return pattern.StartsWith("^") ? pattern : "^" + pattern;
        }

        protected int GetPatternEscape(string pattern, int adjustment)
        {
            var i = pattern.Length - adjustment;
            var escape = 0;
            while (--i >= 0 && pattern[i] == '\\')
                ++escape;
            return escape;
        }

        protected virtual void PrintError(RecorderContext context, string[] fields, Exception error)
        {
            try
            {
                var ctxFile = context as FileLineRecorderContext;
                if (ctxFile != null)
                    Log(LogLevel.ERROR, GetRecorderName() + " cannot process [" + ctxFile.InputRecord + "] File ["
                        + GetInputName(context)
                        + "] offset [" + ctxFile.OffsetInStream + "]: "
                                        + (error != null ? error.ToString() : "Unspecified Error"));
            }
            catch (Exception e)
            {
                Log(LogLevel.ERROR, "Error while processing record input error. Real Error:"
                    + (error != null ? error.ToString() : "Unspecified Error") + ". Current Error: " + e);
            }
        }

        protected override NextInstruction OnProcessRecordException(RecorderContext context, string[] fields, Exception e, ref Exception error)
        {
            PrintError(context, fields, e);
            return base.OnProcessRecordException(context, fields, e, ref error);
        }

        protected override NextInstruction OnProcessInputTextException(RecorderContext context, string[] fields, Exception e, ref Exception error)
        {
            PrintError(context, fields, e);
            return base.OnProcessInputTextException(context, fields, e, ref error);
        }

        protected override NextInstruction OnProcessInputTextError(RecorderContext context, string[] fields, ref Exception error)
        {
            PrintError(context, fields, error);
            return base.OnProcessInputTextError(context, fields, ref error);
        }

        protected override void PrepareKeywords(RecorderContext context, StringBuilder buffer)
        {
            var ctxFile = context as FileRecorderContext;

            if (buffer.Length > 0)
                buffer.Append(';');
            buffer.Append("HOff=")
                .Append(context.HeaderOffset[0])
                .Append('|')
                .Append(context.HeaderOffset[1])
                .Append(";FMdf=")
                .Append(context.InputModifiedOn);

            if (ctxFile != null)
                buffer.Append(";DayIndMap=")
                    .Append(ctxFile.DayIndexMap)
                    .Append(";FSO=")
                    .Append(ctxFile.FileSortOrder);
        }

        public override string GetInputName(RecorderContext context)
        {
            var ctxFile = context as FileRecorderContext;
            return ctxFile == null || ctxFile.CurrentFile == null ? "(No Input File Yet)" : ctxFile.CurrentFile.Name;
        }

        protected virtual RecorderFileSystemInfo CreateDirectoryInfo(RecorderContext context, string absoluteName)
        {
            return new RecorderFileSystemInfoLocal(new DirectoryInfo(absoluteName));
        }

        protected virtual bool GetLastProcessedFile(FileRecorderContext context, bool next)
        {
            try
            {
                var dir = context.CurrentFile == null ? CreateDirectoryInfo(context, location) : context.CurrentFile.Directory;
                if (dir == null)
                {
                    Log(LogLevel.DEBUG, "Cannot find directory to getfiles: Location(" + location + ") LastFile(" + lastFile + ")");
                    return false;
                }

                dir.Refresh();
                if (!dir.Exists)
                {
                    Log(LogLevel.DEBUG, "File Dir Not Exist: [" + dir.FullName + "]");
                    return false;
                }

                var regInputFile = GetPattern();
                if (regInputFile == null)
                {
                    Log(LogLevel.WARN, "No input file pattern found. So return without any operation");
                    return false;
                }

                var rFiles = dir.GetFileSystemInfos();
                var files = (from f in rFiles
                             where
                                 regInputFile.IsMatch(f.Name) &&
                                 (f.Attributes & FileAttributes.Directory) != FileAttributes.Directory
                             select f).ToList();

                if (files.Count == 0)
                {
                    Log(LogLevel.WARN, "No file found under [" + dir.FullName + "]");
                    return false;
                }

                files.Sort(CompareFiles);

                var i = 0;
                var cmp = 1;

                if (context.CurrentFile == null)
                    cmp = -1;
                else
                {
                    if (context.CurrentFile.Exists)
                    {
                        while (i < files.Count && (cmp = CompareFiles(context.CurrentFile, files[i])) > 0)
                            ++i;
                    }
                    else if (context.InputModifiedOn > 0)
                    {
                        while (i < files.Count && context.InputModifiedOn > files[i].LastWriteTimeUtc.Ticks)
                            ++i;
                    }

                    if (i == files.Count)
                    {
                        Log(LogLevel.ERROR, "All files seems old, but current does not exist:[" + context.CurrentFile.FullName + "]");
                        return false;
                    }
                }

                var position = 0L;
                long[] headerOff = null;
                if (cmp == 0)
                {
                    Log(LogLevel.DEBUG, "Last file exist");
                    if (next)
                    {
                        if (++i >= files.Count)
                        {
                            Log(LogLevel.DEBUG, "Cannot change to next, since this is last file");
                            return false;
                        }
                        Log(LogLevel.DEBUG, "Changing to next file");
                    }
                    else
                    {
                        if (context.InputModifiedOn > 0)
                        {
                            context.IsLastFile = i == files.Count - 1;
                            if (context.CurrentFile != null)
                                Log(LogLevel.DEBUG, "Go on with last file: Location(" + location + ") File(" + GetInputName(context) + ")");
                            return true;
                        }
                        position = context.OffsetInStream;
                        headerOff = context.HeaderOffset;
                    }
                }

                context.CurrentFile = files[i];
                context.LastFile = context.CurrentFile.Name;
                context.InputModifiedOn = context.CurrentFile.LastWriteTimeUtc.Ticks;
                context.OffsetInStream = position;
                if (headerOff != null)
                {
                    context.HeaderOffset[0] = headerOff[0];
                    context.HeaderOffset[1] = headerOff[1];
                }
                else
                {
                    context.HeaderOffset[0] = 0;
                    context.HeaderOffset[1] = 0;
                }

                context.LastKeywordBuffer.Remove(0, context.LastKeywordBuffer.Length);
                PrepareKeywords(context, context.LastKeywordBuffer);
                context.LastKeywords = context.LastKeywordBuffer.ToString();
                context.LastLine = string.Empty;
                context.LastRecordDate = string.Empty;
                context.IsLastFile = i == files.Count - 1;

                Log(LogLevel.DEBUG, "Need to set registry again");

                var ins = SetReg(context);
                if ((ins & NextInstruction.Continue) != NextInstruction.Continue)
                    return false;
                if (headerOff != null)
                {
                    headerOffset[0] = headerOff[0];
                    headerOffset[1] = headerOff[1];
                }
                else
                {
                    headerOffset[0] = 0;
                    headerOffset[1] = 0;
                }
                inputModifiedOn = context.InputModifiedOn;

                Log(LogLevel.DEBUG,
                    "Now current file Location(" + location + "), File(" + GetInputName(context) + "), Offset(" +
                    position + "), LastModif(" + inputModifiedOn + ")");
                return true;
            }
            catch (Exception e)
            {
                Log(LogLevel.DEBUG, "Error while getting last file:" + e);
            }
            return false;
        }

        protected override bool OnKeywordParsed(string keyword, bool quotedKeyword, string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnKeywordParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;

            switch (keyword)
            {
                case "DayIndMap":
                    dayIndMap = value;
                    touchCount++;
                    break;
                case "FSO":
                    fileSortOrder = value;
                    touchCount++;
                    break;
            }
            return true;
        }

        protected override bool OnArgParsed(string keyword, bool quotedKeyword,
                                            string value, bool quotedValue, ref int touchCount, ref Exception error)
        {
            if (!base.OnArgParsed(keyword, quotedKeyword, value, quotedValue, ref touchCount, ref error))
                return false;
            switch (keyword)
            {
                case "FP": //Filename pattern
                    filePattern = value;
                    touchCount++;
                    break;
                case "FSCS": //File selection case sensitive
                    fileSelectionCaseSensitive = !string.IsNullOrEmpty(value) && "0" != value;
                    touchCount++;
                    break;
                case "FOCS": //File sort/ordering case sensitive
                    fileOrderingCaseSensitive = !string.IsNullOrEmpty(value) && "0" != value;
                    touchCount++;
                    break;
                case "LC": //Culture of date
                    cultureNlsName = value;
                    touchCount++;
                    break;
            }
            return true;
        }
    }
}
