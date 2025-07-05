using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.Storage
{
    public static class ITypeMapExtension
    {
        public static void RegisterDynamicType(this ITypeMap typeMap, Type type)
        {
            typeMap.Register(type, ITypeMap.k_DynamicTypeID);
        }

        public static int GetID(this ITypeMap typeMap, Type type)
        {
            return typeMap.TryGetID(type, out var id) ? id : 0;
        }

        public static Type GetType(this ITypeMap typeMap, int id)
        {
            return typeMap.TryGetType(id, out var type) ? type : null;
        }
    }

    public interface ITypeMap
    {
        const int k_GameObjectTypeID = -2;
        const int k_IListTypeID = -3;
        const int k_BinaryDataTypeID = -10;
        const int k_DynamicTypeID = -11;

        IReadOnlyCollection<Type> Types { get; }

        bool TryGetID(Type type, out int id);

        bool TryGetType(int id, out Type type);

        void Register(Type type, int id);

        void Unregister(Type type);

        void Clear();
    }

    public class TypeMap : ITypeMap
    {
        private readonly Dictionary<int, Type> m_idToType = new Dictionary<int, Type>();
        private readonly Dictionary<Type, int> m_typeToId = new Dictionary<Type, int>();

        public IReadOnlyCollection<Type> Types
        {
            get { return m_typeToId.Keys; }
        }

        public TypeMap()
        {
            Clear();
        }

        public bool TryGetID(Type type, out int id)
        {
            if(type == null)
            {
                id = 0;
                return false;
            }

            if(m_typeToId.TryGetValue(type, out id))
            {
                return true;
            }

            if (typeof(IList).IsAssignableFrom(type)) 
            {
                id = ITypeMap.k_IListTypeID;
                return true;
            }

            return false;
        }

        public bool TryGetType(int id, out Type type)
        {
            if(m_idToType.TryGetValue(id, out type))
            {
                return true;
            }

            if (id == ITypeMap.k_IListTypeID)
            {
                type = typeof(IList);
            }

            return false;
        }

        public void Register(Type type, int id)
        {
            if (m_typeToId.ContainsKey(type))
            {
                Debug.LogWarning($"Type {type} already registered");
                return;
            }

            m_typeToId.Add(type, id);

            if (id != ITypeMap.k_DynamicTypeID)
            {
                m_idToType.Add(id, type);
            }
        }

        public void Unregister(Type type)
        {
            if (m_typeToId.TryGetValue(type, out int id))
            {
                m_idToType.Remove(id);
            }
            m_typeToId.Remove(type);
        }

        public void Clear()
        {
            m_typeToId.Clear();
            m_idToType.Clear();

            // register core types
            m_typeToId.Add(typeof(GameObject), ITypeMap.k_GameObjectTypeID);
            m_typeToId.Add(typeof(BinaryData), ITypeMap.k_BinaryDataTypeID);
            foreach (var kvp in m_typeToId)
            {
                m_idToType.Add(kvp.Value, kvp.Key);
            }
        }
    }
}
