using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Natek.Recorders.Remote.Helpers.Basic;

namespace RRInfoCollector
{
    [DataContract]
    public class TableDef<T> : IComparable
    {
        public T Data { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string TableName { get; set; }

        [DataMember]
        public List<string> DefaultAnalysisQuery { get; set; }

        public int CompareTo(object obj)
        {
            var o = obj as TableDef<T>;
            if (o == null)
                return -1;
            if (string.IsNullOrEmpty(Description))
            {
                return string.IsNullOrEmpty(o.Description) ? 0 : -1;
            }
            return string.IsNullOrEmpty(o.Description) ? 1 : String.Compare(Description, o.Description, StringComparison.Ordinal);
        }
    }
}
