using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.Storage.Enumerators.UnityEngine.SceneManagement
{
    [ObjectEnumerator(typeof(Scene))]
    public class SceneEnumerator : BaseEnumerator
    {
        private object m_root;
        private readonly List<GameObject> m_objects = new List<GameObject>();

        public override object Object
        {
            get { return m_root; }
            set
            {
                if (value != null)
                {
                    m_root = value;
                    int layer = LayerMask.NameToLayer(StorageLayers.IgnoreLayer);
                    ((Scene)m_root).GetRootGameObjects(m_objects);
                    for(int i = m_objects.Count - 1; i >= 0; i--)
                    {
                        GameObject go = m_objects[i];
                        if(go.layer == layer)
                        {
                            m_objects.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    m_root = default;
                    m_objects.Clear();
                }
            }
        }

        public override bool MoveNext()
        {
            int count = m_objects.Count;
            if (Index < count)
            {
                var go = m_objects[Index];
                Current = go;
            }
            else if(Index == count)
            {
                Current = m_root;
            }
            else
            {
                Current = null;
                return false;
            }

            Index++;
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            Index = 0;
        }
    }
}
