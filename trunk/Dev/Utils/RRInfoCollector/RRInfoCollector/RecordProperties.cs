using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace RRInfoCollector
{

    [DataContract]
    public class RecordProperties<T>
    {
        public T Data { get; set; }

        [DataMember]
        public string SystemName { get; set; }

        [DataMember]
        public Dictionary<string, TableDef<TreeNode>> Table { get; set; }
    }
}
