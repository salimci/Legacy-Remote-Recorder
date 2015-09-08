using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditLogonEnv
    {
        private Dictionary<string, AuditHandle> _processLastAudit;
        private Dictionary<string, AuditHandle> _handles;
        private string _logonId;

        public AuditLogonEnv(string logonId)
        {
            _processLastAudit = new Dictionary<string, AuditHandle>();
            _handles = new Dictionary<string, AuditHandle>();
            _logonId = logonId;
        }

        public Dictionary<string, AuditHandle> ProcessLastAudit { get { return _processLastAudit; } }
        public Dictionary<string, AuditHandle> Handles { get { return _handles; } }
        public string LogonId { get { return _logonId; } }
    }
}
