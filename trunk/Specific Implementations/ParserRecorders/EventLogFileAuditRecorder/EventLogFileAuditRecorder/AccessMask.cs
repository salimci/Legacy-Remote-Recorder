using System;

namespace EventLogFileAuditRecorder
{
    [Flags]
    public enum AccessMask : uint
    {
        None=0,
        ReadOrListDirectory = 0x1,
        WriteOrAddFile = 0x2,
        AppendOrAddSubDir = 0x4,
        ReadExtendedAtt = 0x8,
        WriteExtendedAtt = 0x10,
        ExecuteOrTraverse = 0x20,
        DeleteChild = 0x40,
        ReadAttributes = 0x80,
        WriteAttributes = 0x100,
        Delete = 0x10000,
        ReadControl = 0x20000,
        WriteDac = 0x40000,
        WriteOwner = 0x80000,
        Synchronize = 0x100000,
        AccessSacl = 0x1000000,
        GeneralAll = 0x10000000,
        GeneralExecute = 0x20000000,
        GeneralWrite = 0x40000000,
        GeneralRead = 0x80000000
    }
}
