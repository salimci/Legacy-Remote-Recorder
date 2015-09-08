using System.Collections;
using System.Collections.Generic;

namespace Natek.Helpers
{
    public class AutoExtendList<T> : IList<T>
    {
        private List<T> list = new List<T>(); 

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Clear(bool emptyList)
        {
            if (emptyList)
            {
                Clear();
            }
            else
            {
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = default(T);
                }
            }
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public int Count
        {
            get { return list.Count; } 
        }

        public bool IsReadOnly { get { return false; } }
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            while(list.Count <= index)
                list.Add(default(T));

            list[index] = item;
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { Insert(index, value);}
        }
    }
}
