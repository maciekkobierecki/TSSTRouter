using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelSwtichingRouter
{
    [Serializable]
    class ThreadSafeList<T>
    {
        private List<T> _list = new List<T>();
        private object _sync = new object();

        public void Add(T value)
        {
            lock (_sync)
            {
                _list.Add(value);
            }
        }

        public bool Remove(T item)
        {
            lock (_sync)
            {
                bool didRemove;
                didRemove=_list.Remove(item);
                return didRemove;
            }
        } 

        public void RemoveAt(int index)
        {
            lock (_sync)
            {
                _list.RemoveAt(index);
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _list.Clear();
            }
        }

        public int Count()
        {
            lock(_sync)
            {
                return _list.Count;
            }
        }

        public void ForEach(Action<T> action)
        {
            lock (_sync)
            {
                _list.ForEach(action);
            }
        }

        public List<T>.Enumerator GetEnumerator()
        {
            lock (_sync)
            {
                 return _list.GetEnumerator();
            }
        }

        public T this[int key]
        {
            get
            {
                return _list.ElementAt(key);
            }
            set
            {
                
            }
        }
    }
}
