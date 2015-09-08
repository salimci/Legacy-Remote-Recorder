using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditLogonEnv2
    {
        private Dictionary<string, AuditHandle2> _processLastAudit;
        private string _logonId;

        public AuditLogonEnv2(string logonId)
        {
            _processLastAudit = new Dictionary<string, AuditHandle2>();
            _logonId = logonId;
        }

        public Dictionary<string, AuditHandle2> ProcessLastAudit { get { return _processLastAudit; } }
        public string LogonId { get { return _logonId; } }
    }
}
