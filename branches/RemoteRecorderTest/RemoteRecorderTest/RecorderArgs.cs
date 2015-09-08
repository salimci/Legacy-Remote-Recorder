using System;

namespace RemoteRecorderTest
{
    public class RecorderArgs
    {
  
        public string ServiceName { get; set; }
        public string RecorderName { get; set; }
        public string Location { get; set; }
        public string LastLine { get; set; }
        public string LastPosition { get; set; }
        public string LastKeywords { get; set; }
        public bool FromEndOnLoss { get; set; }
        public int MaxRecordSend { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string RemoteHost { get; set; }
        public int SleepTime { get; set; }
        public int TraceLevel { get; set; }
        public string VirtualHost { get; set; }
        public DateTime LastUpdated { get; set; }
        public int MaxRespondTime { get; set; }
        public string Email { get; set; }
        public string CustomVar1 { get; set; }
        public int CustomVar2 { get; set; }
        public string Dal { get; set; }
        public int Status { get; set; }
        public int Reload { get; set; }
        public DateTime LastRecDate { get; set; }
        public string LastFile { get; set; }
        public int TimeGap { get; set; }
        public int CheckTimeSync { get; set; }
        public DateTime LastReload { get; set; }
        public int TimeZone { get; set; }
        public string TimeRange { get; set; }
        public DateTime LastMailDate { get; set; }
        public int MailSupress { get; set; }
  
   
        public int MaxLineToWait { get; set; }        
        public string OutputLocation { get; set; }
    }
}
