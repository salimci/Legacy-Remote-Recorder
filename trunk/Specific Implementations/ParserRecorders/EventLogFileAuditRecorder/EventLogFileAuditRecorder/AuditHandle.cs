using System;
using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditHandle
    {
        private DateTime _createdOn;
        private Dictionary<string, string> _ownerProcess;

        public string Handle { get; set; }

        public AuditObject Object { get; set; }

        public AuditHandle()
        {
            _createdOn = DateTime.Now;
            _ownerProcess = new Dictionary<string, string>();
        }

        public AccessMask AccessType { get; set; }
        public DateTime CreatedOn { get { return _createdOn; } }
        public Dictionary<string, string> OwnerProcess { get { return _ownerProcess; } }
    }
}
