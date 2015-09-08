using System;
using System.Globalization;

namespace RRInfoCollector
{
    public class FieldItem : IComparable
    {
        public string Text { get; set; }
        public bool Selected { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public int CompareTo(object obj)
        {
            var o = obj as FieldItem;
            if (o == null)
                return -1;
            if (Selected)
            {
                if (!o.Selected)
                    return -1;
            }
            else if (o.Selected)
                return 1;

            if (string.IsNullOrEmpty(Text))
            {
                return string.IsNullOrEmpty(o.Text) ? 0 : -1;
            }
            return string.IsNullOrEmpty(o.Text) ? 1 : String.Compare(Text, o.Text, System.StringComparison.Ordinal);
        }
    }
}
