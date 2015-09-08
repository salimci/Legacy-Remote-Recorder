namespace EventLogFileAuditRecorder
{
    public class AccessRightInfo
    {
        public bool Valid { get; set; }
        public string AceType { get; set; }
        public string AceFlags { get; set; }
        public string Right { get; set; }
        public string Trustee { get; set; }

        public AccessRightInfo()
        {
            Valid = true;
        }
    }
}
