using System;
using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditObject
    {
        private Dictionary<string, AuditHandle> _handles;

        public AuditObject()
        {
            _handles = new Dictionary<string, AuditHandle>();
        }

        public Dictionary<string, AuditHandle> Handles
        {
            get { return _handles; }
        }

        public string Owner { get; set; }
        public string OwnerSid { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string LogonId { get; set; }
    }
}
