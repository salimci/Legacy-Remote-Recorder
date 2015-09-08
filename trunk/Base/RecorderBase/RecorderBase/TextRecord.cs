namespace Natek.Recorders.Remote
{
    public class TextRecord : Record
    {
        public string RecordText { get; set; }

        public override string ToString()
        {
            return RecordText;
        }

        public override void SetValue(object value)
        {
            RecordText = value == null ? null : value.ToString();
        }
    }
}
