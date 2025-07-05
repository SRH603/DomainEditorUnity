using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.Storage
{
    public class ObjectEnumeratorFactoryBase : IObjectEnumeratorFactory
    {
        private readonly Dictionary<Type, Type> m_typeToEnumerator = new Dictionary<Type, Type>();
        private ITypeMap m_typeMap;
        
        public ObjectEnumeratorFactoryBase(ITypeMap typeMap)
        {
            m_typeMap = typeMap;
        }

        public bool IsRegistered(Type type)
        {
            return m_typeToEnumerator.ContainsKey(type);
        }

        public void Register(Type type, Type enumeratorType)
        {
            m_typeToEnumerator[type] = enumeratorType;
        }

        public void Unregister(Type type)
        {
            m_typeToEnumerator.Remove(type);
        }

        public virtual void Reset()
        {
            m_typeToEnumerator.Clear();
        }

        public virtual IObjectEnumerator Create(object obj, Type type)
        {
            if (m_typeToEnumerator.TryGetValue(type, out Type enumeratorType))
            {
                if (enumeratorType == typeof(DynamicEnumerator))
                {
                    return new DynamicEnumerator(m_typeMap);
                }
                return (IObjectEnumerator)Activator.CreateInstance(enumeratorType);
            }
            else if (obj is GameObject)
            {
                return new GameObjectEnumerator();
            }
            else if (obj is IList)
            {
                return new IListEnumerator();
            }
            return null;
        }
    }
}