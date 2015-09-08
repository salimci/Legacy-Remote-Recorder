using System.Collections.Generic;

namespace EventLogFileAuditRecorder
{
    public class AuditInfo
    {
        private List<AccessReason> _reasons;
        private Dictionary<string, AccessRightInfo> _originalRights;
        private Dictionary<string, AccessRightInfo> _newRights;

        public AuditInfo()
        {
            _reasons = new List<AccessReason>();
            _originalRights = new Dictionary<string, AccessRightInfo>();
            _newRights = new Dictionary<string, AccessRightInfo>();
        }

        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public string Sid { get; set; }
        public string Username { get; set; }
        public string Process { get; set; }
        public int ProcessId { get; set; }
        public string AccessMask { get; set; }
        public string OriginalDaclFlags { get; set; }
        public string NewDaclFlags { get; set; }

        public List<AccessReason> Reasons
        {
            get { return _reasons; }
        }

        public Dictionary<string, AccessRightInfo> OriginalRights
        {
            get
            {
                return _originalRights;
            }
        }

        public Dictionary<string, AccessRightInfo> NewRights
        {
            get
            {
                return _newRights;
            }
        }

        public void Reset()
        {
            ObjectName = string.Empty;
            ObjectType = string.Empty;
            Sid = string.Empty;
            Username = string.Empty;
            Process = string.Empty;
            ProcessId = 0;
            AccessMask = string.Empty;
            Reasons.Clear();
            OriginalRights.Clear();
            NewRights.Clear();
        }
    }
}
