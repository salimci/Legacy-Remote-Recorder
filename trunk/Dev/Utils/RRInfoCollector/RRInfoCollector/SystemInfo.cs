using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RRInfoCollector
{
    [DataContract]
    public class SystemInfo
    {
        [DataMember]
        public List<string> ShortNotations { get; set; }
        [DataMember]
        public Dictionary<string, RemoteRecorderSystem> SystemLookup { get; set; }
    }
}
