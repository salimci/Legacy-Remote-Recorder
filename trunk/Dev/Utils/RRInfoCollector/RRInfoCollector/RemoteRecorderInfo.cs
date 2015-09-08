using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RRInfoCollector
{
    [DataContract]
    public class RemoteRecorderInfo<T> : Info
    {
        public T Data { get; set; }
        [DataMember]
        public string SystemName { get; set; }
        [DataMember]
        public List<string> Fields { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Order { get; set; }
    }
}
