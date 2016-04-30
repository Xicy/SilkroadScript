using System.Collections;
using System.Collections.Generic;

namespace Shared
{
    public class SynchronizedList<T> : IList<T>
    {
        // Fields
        private List<T> _list;
        private object _root;

        // Methods
        private SynchronizedList(List<T> list)
        {
            _list = list;
            _root = ((ICollection)list).SyncRoot;
        }

        public static implicit operator SynchronizedList<T>(List<T> list)
        {
            return new SynchronizedList<T>(list);
        }

        public T this[int index]
        {
            get
            {
                lock (_root)
                {
                    return _list[index];
                }
            }

            set
            {
                lock (_root)
                {
                    _list[index] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_root)
                {
                    return _list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (_root)
                {
                    return ((ICollection<T>)_list).IsReadOnly;
                }
            }
        }

        public void Add(T item)
        {
            lock (_root)
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            lock (_root)
            {
                 _list.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_root)
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_root)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_root)
            {
                return _list.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (_root)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_root)
            {
                _list.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            lock (_root)
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_root)
            {
                _list.RemoveAt(index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_root)
            {
                return _list.GetEnumerator();
            }
        }

        
    }
}