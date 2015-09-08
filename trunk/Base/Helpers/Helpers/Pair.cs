namespace Natek.Helpers
{
    public class Pair<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public Pair()
        { }

        public Pair(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public struct StructPair<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
    }
}
