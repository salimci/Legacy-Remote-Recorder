using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteRecorderTest
{
    public class CList<T> : IList<T>
    {
        private List<T> l = new List<T>();
        public int IndexOf(T item)
        {
            return l.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            while (l.Count <= index)
                l.Add(default(T));
            l.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            l.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return l[index]; }
            set
            {
                Insert(index, value);
            }
        }

        public void Add(T item)
        {
            l.Add(item);
        }

        public void Clear()
        {
            l.Clear();
        }

        public bool Contains(T item)
        {
            return l.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            l.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return l.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return l.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return l.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return l.GetEnumerator();
        }
    }
}
