using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.Storage
{
    public class GameObjectEnumerator : BaseEnumerator
    {
        private GameObject m_root;
        private readonly List<UnityObject> m_objects = new List<UnityObject>();
        private readonly List<Component> m_components = new List<Component>();
        private readonly Dictionary<Type, int> m_componentTypeToIndex = new Dictionary<Type, int>();
        
        public override int CurrentKey
        {
            get 
            {
                var componentType = CurrentType;
                if (componentType == null)
                {
                    return -1;
                }

                return m_componentTypeToIndex[componentType];
            }
        }

        public override object Object
        {
            get { return m_root; }
            set
            {
                m_root = (GameObject)value;
                if (m_root != null)
                {
                    var rootTransform = m_root.transform;
                    int childCount = rootTransform.childCount;
                    
                    m_root.GetComponents(m_components);

                    for (int i = 0; i < childCount; ++i)
                    {
                        m_objects.Add(rootTransform.GetChild(i).gameObject);
                    }

                    for (int i = 0; i < m_components.Count; ++i)
                    {
                        m_objects.Add(m_components[i]);
                    }

                    m_components.Clear();
                }
                else
                {
                    m_objects.Clear();
                    m_componentTypeToIndex.Clear();
                }
            }
        }

        public override bool MoveNext()
        {
            int count = m_objects.Count;
            if (Index < count)
            {
                Current = m_objects[Index];

                var componentType = CurrentType;
                if (componentType != null) 
                {
                    if(m_componentTypeToIndex.TryGetValue(componentType, out int index))
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }

                    m_componentTypeToIndex[componentType] = index;
                }
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
            m_componentTypeToIndex.Clear();
        }
    }
}
