using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.Storage
{
    public class ObjectEnumerator<T> : BaseEnumerator
    {
        private T m_typedObject;
        protected T TypedObject
        {
            get { return m_typedObject; }
        }

        public override object Object
        {
            get { return m_typedObject; }
            set
            {
                if (value == null)
                {
                    m_typedObject = default;
                }
                else
                {
                    m_typedObject = (T)value;
                }
            }
        }

        private int m_currentKey = -1;

        public override int CurrentKey
        {
            get { return m_currentKey; }
        }

        private IEnumerator<(object Object, int Key)> m_objectKeyEnumerator;
        public override bool MoveNext()
        {
            if (Index == 0)
            {
                m_objectKeyEnumerator = GetNext();
            }

            while (true)
            {
                if (!m_objectKeyEnumerator.MoveNext())
                {
                    return false;
                }

                if (m_objectKeyEnumerator.Current.Object == null)
                {
                    continue;
                }

                return MoveNext(m_objectKeyEnumerator.Current.Object, m_objectKeyEnumerator.Current.Key);
            }
        }

        protected virtual IEnumerator<(object Object, int Key)> GetNext()
        {
            yield return (Object, -1);
        }

        protected bool MoveNext(object obj, int key /* key field from surrogate (persistent property identifier */)
        {
            Current = obj;
            Index++;
            m_currentKey = key;
            return Current != null;
        }

        protected bool MoveNext(UnityEngine.Object obj, int key)
        {
            Current = obj;
            Index++;
            m_currentKey = key;
            return obj != null;
        }

        protected bool MoveNext(Component component, int key /*  key field from surrogate (persistent property identifier) */)
        {
            GameObject go = null;
            if (component != null)
            {
                go = component.gameObject;
            }

            return MoveNext(go, key);
        }

        public override void Reset()
        {
            base.Reset();
            Index = 0;
            m_currentKey = -1;
            m_objectKeyEnumerator = null;
        }
    }
}
