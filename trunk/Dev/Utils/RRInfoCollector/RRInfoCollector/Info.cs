using System;
using System.Runtime.Serialization;
using Natek.Recorders.Remote.Helpers.Basic;

namespace RRInfoCollector
{
    [DataContract]
    public class Info:IComparable
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (Name == null)
                return -1;
            return String.Compare(Name, obj.ToString(), StringComparison.Ordinal);
        }
    }
}
