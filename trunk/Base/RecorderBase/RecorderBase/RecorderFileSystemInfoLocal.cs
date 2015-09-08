using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Natek.Recorders.Remote
{
    public class RecorderFileSystemInfoLocal : RecorderFileSystemInfo
    {
        protected FileSystemInfo instance;
        protected RecorderFileSystemInfoLocal directory;

        public RecorderFileSystemInfoLocal(FileSystemInfo instance)
        {
            this.instance = instance;
        }

        public override string Name
        {
            get { return instance.Name; }
        }

        public override string FullName
        {
            get { return instance.FullName; }
        }

        public override void Refresh()
        {
            instance.Refresh();
        }

        public override bool Exists
        {
            get { return instance.Exists; }
        }

        public override DateTime CreationTimeUtc
        {
            get { return instance.CreationTimeUtc; }
        }

        public override DateTime LastAccessTimeUtc
        {
            get { return instance.LastAccessTimeUtc; }
        }

        public override DateTime LastWriteTimeUtc
        {
            get { return instance.LastWriteTimeUtc; }
        }

        public override RecorderFileSystemInfo Directory
        {
            get { return directory ?? (directory = new RecorderFileSystemInfoLocal(instance is FileInfo ? (instance as FileInfo).Directory : (instance is DirectoryInfo ? (instance as DirectoryInfo).Parent : null))); }
        }

        public override FileAttributes Attributes
        {
            get { return instance.Attributes; }
        }

        public override string FileNodeId
        {
            get { return instance.FullName; }
        }

        public override IEnumerable<RecorderFileSystemInfo> GetFileSystemInfos()
        {
            if (instance is DirectoryInfo)
            {
                return (instance as DirectoryInfo).GetFileSystemInfos().Select(f => new RecorderFileSystemInfoLocal(f)).Cast<RecorderFileSystemInfo>().ToArray();
            }
            return null;
        }
    }
}
