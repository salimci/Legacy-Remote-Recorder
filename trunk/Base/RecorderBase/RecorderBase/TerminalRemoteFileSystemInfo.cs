using System;
using System.Collections.Generic;
using System.IO;
using Log;
using Natek.Helpers;
using Natek.Helpers.IO;

namespace Natek.Recorders.Remote.StreamBased.Terminal
{
    public class TerminalRemoteFileSystemInfo : RecorderFileSystemInfo
    {
        protected ObjectValue<RecorderFileSystemInfo> directory;
        protected ObjectValue<int> exists;
        protected readonly static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        protected string fullName;
        protected string name;
        protected string fileNodeId;
        protected DateTime creationTimeUtc;
        protected DateTime lastAccessTimeUtc;
        protected DateTime lastWriteTimeUtc;
        protected FileAttributes attributes;

        public TerminalRemoteFileSystemInfo(RecorderContext context)
            : this(context, null, null)
        {
        }

        public TerminalRemoteFileSystemInfo(RecorderContext context, string fullName, string name)
        {
            Context = context;
            this.fullName = fullName;
            this.name = name;
            exists = new ObjectValue<int>(-1);
            directory = new ObjectValue<RecorderFileSystemInfo>();
        }


        public RecorderContext Context { get; set; }

        protected virtual TerminalRemoteFileSystemInfo[] Refresh(string absolutePath)
        {
            var refreshInfo = new TerminalRemoteFileSystemInfo[] { null };
            var ctx = Context as TerminalRecorderContext;
            if (ctx == null)
                throw new Exception("Context is not a TerminalRecorderContext");
            ctx.Recorder.Log(LogLevel.DEBUG, "Execute Command to Refresh [" + absolutePath + "]");
            ctx.ExecuteRemoteCommand(ctx.CommandFileSystemInfo.Replace("@NODE", absolutePath), args =>
            {
                var info = Context.InputRecord.ToString().Split(new[] { ';' }, 2);
                var r = 0;
                if (info.Length >= 2 && int.TryParse(info[0], out r))
                {
                    if (r == 0)
                    {
                        if (string.IsNullOrEmpty(info[1]))
                        {
                            refreshInfo[0] = new TerminalRemoteFileSystemInfo(Context, absolutePath, absolutePath);
                            refreshInfo[0].exists.Value = 0;
                        }
                        else
                        {
                            info = info[1].Split(new[] { ';' }, 8);
                            if (info.Length == 8)
                            {
                                Context.Recorder.Log(LogLevel.DEBUG, "Refresh With [" + Context.InputRecord + "]");
                                refreshInfo[0] = new TerminalRemoteFileSystemInfo(Context, info[7],
                                                                                  FileSystemHelper.FileNameOf(info[7],
                                                                                                              info[0]))
                                    {
                                        fileNodeId = info[1],
                                        creationTimeUtc = Epoch.AddSeconds(ParseLong(info[2])),
                                        lastAccessTimeUtc = Epoch.AddSeconds(ParseLong(info[3])),
                                        lastWriteTimeUtc = Epoch.AddSeconds(ParseLong(info[4])),
                                        attributes =
                                            info[6].Equals("directory") || info[6].Equals("symbolic link")
                                                ? FileAttributes.Directory
                                                : FileAttributes.Normal
                                    };
                                refreshInfo[0].exists.Value = 1;
                            }
                            else
                                throw new Exception("Unexpected Create Directory Info command return:[" +
                                                    Context.InputRecord +
                                                    "]");
                        }
                    }
                    else
                        throw new Exception("Create Directory Info Failed Return(" + info[0] + "), Msg(" + info[1] +
                                            ")");
                }
                else
                    throw new Exception("Remote Create Directory Info command result not understood:[" +
                                        Context.InputRecord +
                                        "]");
            });
            return refreshInfo;
        }

        protected double ParseLong(string str, long defaultV = 0)
        {
            long v;
            return long.TryParse(str, out v) ? v : defaultV;
        }

        public override string Name
        {
            get { return name; }
        }

        public override string FullName
        {
            get { return fullName; }
        }

        public override void Refresh()
        {
            lock (exists)
            {
                var refreshInfo = Refresh(FullName);
                if (refreshInfo[0] != null)
                {
                    exists.Value = refreshInfo[0].exists.Value;
                    fileNodeId = refreshInfo[0].FileNodeId;
                    creationTimeUtc = refreshInfo[0].CreationTimeUtc;
                    lastAccessTimeUtc = refreshInfo[0].LastAccessTimeUtc;
                    lastWriteTimeUtc = refreshInfo[0].LastWriteTimeUtc;
                    fullName = refreshInfo[0].FullName;
                    name = refreshInfo[0].Name;
                    attributes = refreshInfo[0].Attributes;
                }
                else
                    exists.Value = -1;
            }
        }

        public override bool Exists
        {
            get
            {
                lock (exists)
                {
                    if (exists == -1)
                        Refresh();
                    return exists == 1;
                }
            }
        }

        public override DateTime CreationTimeUtc
        {
            get { return creationTimeUtc; }
        }

        public override DateTime LastAccessTimeUtc
        {
            get { return lastAccessTimeUtc; }
        }

        public override DateTime LastWriteTimeUtc
        {
            get { return lastWriteTimeUtc; }
        }

        public override RecorderFileSystemInfo Directory
        {
            get
            {
                lock (directory)
                {
                    if (directory.Value != null)
                        return directory;
                    var ctx = Context as TerminalRecorderContext;
                    if (ctx == null)
                        throw new Exception("Context is not a TerminalRecorderContext");
                    string parentDir = null;
                    string dirSep = null;
                    ctx.ExecuteRemoteCommand(ctx.CommandParentOf.Replace("@NODE", FullName), args =>
                        {
                            var r = Context.InputRecord.ToString().Split(new[] { ';' }, 3);
                            var code = 0;
                            if (r.Length != 3 || !int.TryParse(r[1], out code))
                                throw new Exception("ParentOf command return not understood:[" + Context.InputRecord + "]");
                            if (code != 0)
                                throw new Exception("Parent of returned error Code(" + r[1] + "), Msg(" + r[2] + ")");
                            dirSep = r[0];
                            parentDir = r[2];
                        });
                    if (string.IsNullOrEmpty(parentDir))
                        throw new Exception("Getting name of parent dir failed for:" + FullName);
                    var parent = Refresh(parentDir);
                    if (parent[0] == null)
                        throw new Exception("Getting Parent of '" + FullName + "' failed with unknown reason");
                    var dir = new TerminalRemoteFileSystemInfo(Context, parentDir, FileSystemHelper.FileNameOf(fullName, dirSep))
                    {
                        lastAccessTimeUtc = parent[0].LastAccessTimeUtc,
                        lastWriteTimeUtc = parent[0].LastWriteTimeUtc,
                        creationTimeUtc = parent[0].CreationTimeUtc,
                        attributes = parent[0].Attributes,
                        fileNodeId = parent[0].FileNodeId
                    };
                    dir.exists.Value = parent[0].exists.Value;
                    directory.Value = dir;
                    return directory.Value;
                }
            }
        }

        public override FileAttributes Attributes
        {
            get { return attributes; }
        }

        public override string FileNodeId
        {
            get { return fileNodeId; }
        }

        public override IEnumerable<RecorderFileSystemInfo> GetFileSystemInfos()
        {
            if ((Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                return null;
            var refreshInfo = new List<TerminalRemoteFileSystemInfo>();
            var ctx = Context as TerminalRecorderContext;
            if (ctx == null)
                throw new Exception("Context is not a TerminalRecorderContext");
            ctx.ExecuteRemoteCommand(ctx.CommandListFiles.Replace("@NODE", FullName), args =>
            {
                var info = Context.InputRecord.ToString().Split(new[] { ';' }, 8);
                if (info.Length == 8)
                {
                    refreshInfo.Add(new TerminalRemoteFileSystemInfo(Context, info[7], FileSystemHelper.FileNameOf(info[7], info[0]))
                    {
                        fileNodeId = info[1],
                        creationTimeUtc = Epoch.AddSeconds(ParseLong(info[2])),
                        lastAccessTimeUtc = Epoch.AddSeconds(ParseLong(info[3])),
                        lastWriteTimeUtc = Epoch.AddSeconds(ParseLong(info[4])),
                        attributes = info[6].Equals("directory") || info[6].Equals("symbolic link") ? FileAttributes.Directory : FileAttributes.Normal
                    });
                }
                else
                    throw new Exception("Unexpected Create Directory Info command return:[" + Context.InputRecord +
                                        "]");
            });
            return refreshInfo.ToArray();
        }
    }
}
