using System;
using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditHandle2
    {
        private DateTime _createdOn;

        public AuditHandle2(string id,string pid,string procName)
        {
            HandleId = id;
            _createdOn = DateTime.Now;
            Pid = pid;
            ProcessName = procName;
        }

        public string Pid { get; set; }
        public string ProcessName { get; set; }
        public AccessMask AccessType { get; set; }
        public DateTime CreatedOn { get { return _createdOn; } }
        public string HandleId { get; set; }
        public string Owner { get; set; }
        public string OwnerSid { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string LogonId { get; set; }
    }
}
