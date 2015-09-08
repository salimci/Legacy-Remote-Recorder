using System;
using System.Collections.Generic;
using System.IO;

namespace Natek.Recorders.Remote
{
    public abstract class RecorderFileSystemInfo
    {
        public abstract string Name { get; }
        public abstract string FullName { get; }

        public abstract void Refresh();

        public abstract bool Exists { get; }

        public abstract DateTime CreationTimeUtc { get; }
        public abstract DateTime LastAccessTimeUtc { get; }
        public abstract DateTime LastWriteTimeUtc { get; }

        public abstract RecorderFileSystemInfo Directory { get; }

        public abstract FileAttributes Attributes { get; }

        public abstract string FileNodeId { get; }

        public abstract IEnumerable<RecorderFileSystemInfo> GetFileSystemInfos();
    }
}
