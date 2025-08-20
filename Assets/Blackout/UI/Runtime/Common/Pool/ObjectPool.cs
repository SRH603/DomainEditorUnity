using System;
using System.Collections.Generic;

namespace Blackout.Pool
{
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        internal readonly List<T> m_List;
        private readonly Func<T> m_CreateFunc;
        private readonly Action<T> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly int m_MaxSize;
        internal bool m_CollectionCheck;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInactive;

        public int CountInactive => m_List.Count;

        public ObjectPool(
          Func<T> createFunc,
          Action<T> actionOnGet = null,
          Action<T> actionOnRelease = null,
          Action<T> actionOnDestroy = null,
          bool collectionCheck = true,
          int defaultCapacity = 10,
          int maxSize = 10000)
        {
              if (createFunc == null)
                    throw new ArgumentNullException(nameof(createFunc));
              if (maxSize <= 0)
                    throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
              m_List = new List<T>(defaultCapacity);
              m_CreateFunc = createFunc;
              m_MaxSize = maxSize;
              m_ActionOnGet = actionOnGet;
              m_ActionOnRelease = actionOnRelease;
              m_ActionOnDestroy = actionOnDestroy;
              m_CollectionCheck = collectionCheck;
        }

        public T Get()
        {
              T obj;
              if (m_List.Count == 0)
              {
                    obj = m_CreateFunc();
                    ++CountAll;
              }
              else
              {
                    int index = m_List.Count - 1;
                    obj = m_List[index];
                    m_List.RemoveAt(index);
              }

              Action<T> actionOnGet = m_ActionOnGet;
              if (actionOnGet != null)
                    actionOnGet(obj);
              return obj;
        }

        public PooledObject<T> Get(out T v)
        {
              v = Get();
              return new PooledObject<T>(v, (IObjectPool<T>)this);
        }

        public void Release(T element)
        {
            if (m_CollectionCheck && m_List.Count > 0)
            {
                for (int index = 0; index < m_List.Count; ++index)
                {
                  if ((object)element == (object)m_List[index])
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }

            Action<T> actionOnRelease = m_ActionOnRelease;
            if (actionOnRelease != null)
                actionOnRelease(element);
            if (CountInactive < m_MaxSize)
            {
                m_List.Add(element);
            }
            else
            {
                Action<T> actionOnDestroy = m_ActionOnDestroy;
                if (actionOnDestroy != null)
                  actionOnDestroy(element);
            }
        }

        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
              foreach (T obj in m_List)
                m_ActionOnDestroy(obj);
            }

            m_List.Clear();
            CountAll = 0;
        }

        public void Dispose() => Clear();
    }
}