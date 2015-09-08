namespace Natek.Helpers
{
    public class ObjectValue<T>
    {
        protected T value;

        public ObjectValue()
        {
            value = default(T);
        }

        public ObjectValue(T value)
        {
            this.value = value;
        }

        public static implicit operator T(ObjectValue<T> o)
        {
            return o.Value;
        }

        public T Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
