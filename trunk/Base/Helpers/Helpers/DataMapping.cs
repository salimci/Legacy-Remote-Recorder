using System.Reflection;

namespace Natek.Recorders.Remote.Mapping
{
    public class DataMapping
    {
        public delegate object SourceFormatter(RecWrapper rec, string field, string[] fieldValues, object data);

        public string[][] Original { get; set; }
        public int[] SourceIndex { get; set; }
        public string[] SourceValues { get; set; }
        public PropertyInfo MappedField { get; set; }
        public SourceFormatter MethodInfo { get; set; }
        public object FormatterData { get; set; }
    }
}
