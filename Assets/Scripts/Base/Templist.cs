using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = System.Object;

namespace Kernel.Lang.Collection
{
    /// <summary>
    /// 池列表，引用不能被长期持有。
    /// 建议使用 using 语法
    /// </summary>
#if UNITY_EDITOR
    public class TempList<T> : IList<T>, IList, IDisposable
#else
	public class TempList<T> : List<T>, IDisposable
#endif
    {
        private static readonly Stack<object> cache = new Stack<object>();

        public static TempList<T> Alloc()
        {
            if (cache.Count > 0)
            {
                var retval = (TempList<T>)cache.Pop();
                if (retval.disposed == false)
                {
                    Debug.LogWarning("注意，Alloc得到的对象还没有被Dispose");
                }
                retval.disposed = false;
                return retval;
            }
            else
            {
                return new TempList<T>();
            }
        }

        public void Dispose()
        {
            Clear();
            if (!disposed)
            {
                disposed = true;
                cache.Push(this);
            }
        }

        private bool disposed;

#if UNITY_EDITOR

        private readonly List<T> list;

        public TempList()
        {
            list = new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            OnVisitList();
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            OnVisitList();
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            OnVisitList();
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            OnVisitList();
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            OnVisitList();
            return list.Remove(item);
        }

        public int Count
        {
            get
            {
                OnVisitList();
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            OnVisitList();
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            OnVisitList();
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            OnVisitList();
            list.RemoveAt(index);
        }

        public TempList<T> GetRange(int index, int count)
        {
            OnVisitList();
            var range = Alloc();
            for (var i = index; i < index + count; ++i)
            {
                range.Add(list[i]);
            }
            return range;
        }

        public T this[int index]
        {
            get
            {
                OnVisitList();
                return list[index];
            }
            set
            {
                OnVisitList();
                list[index] = value;
            }
        }

        void IList.Insert(int index, Object item)
        {
            try
            {
                Insert(index, (T)item);
            }
            catch (InvalidCastException)
            {
                Debug.LogError("WrongValueTypeArgumentException");
            }
        }

        int IList.IndexOf(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return IndexOf((T)item);
            }
            return -1;
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        int IList.Add(Object item)
        {
            try
            {
                Add((T)item);
            }
            catch (InvalidCastException)
            {
                Debug.LogError("WrongValueTypeArgumentException");
            }

            return Count - 1;
        }

        void IList.Remove(Object item)
        {
            if (IsCompatibleObject(item))
            {
                Remove((T)item);
            }
        }

        bool IList.Contains(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return Contains((T)item);
            }
            return false;
        }

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }

        Object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    Debug.LogError("WrongValueTypeArgumentException");
                }
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            try
            {
                CopyTo((T[])array, arrayIndex);
            }
            catch (InvalidCastException)
            {
                Debug.LogError("WrongValueTypeArgumentException");
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        [NonSerialized]
        private Object _syncRoot;

        Object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            OnVisitList();
            list.AddRange(collection);
        }

        public void TrimExcess()
        {
            list.TrimExcess();
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void Sort(IComparer<T> comparer)
        {
            OnVisitList();
            list.Sort(comparer);
        }

        public void Sort()
        {
            OnVisitList();
            list.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            OnVisitList();
            list.Sort(comparison);
        }

        public T Find(Predicate<T> match)
        {
            OnVisitList();
            return list.Find(match);
        }

        public bool Exists(Predicate<T> match)
        {
            OnVisitList();
            return list.Exists(match);
        }

        public void RemoveRange(int index, int count)
        {
            OnVisitList();
            list.RemoveRange(index, count);
        }

        public int Capacity
        {
            get { return list.Capacity; }
            set { list.Capacity = value; }
        }

        private void OnVisitList()
        {
            if (TempList.CheckDispoed && disposed)
            {
                Debug.LogError("@程序，您访问的对象已被Dispose");
            }
        }
#endif
    }
}

namespace Kernel
{
#if UNITY_EDITOR

    public static class TempList
    {
        public static bool CheckDispoed = true;
    }

#endif
}
