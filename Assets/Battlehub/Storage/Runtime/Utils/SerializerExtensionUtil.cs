using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Battlehub.Storage
{
    public static class SerializerExtensionUtil
    {
        public static Type[] FlattenHierarchy(params Type[] types)
        {
            HashSet<Type> hs = new HashSet<Type>();
            foreach (var type in types)
            {
                if (hs.Contains(type))
                {
                    continue;
                }

                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    var t = type;
                    while (t != typeof(UnityEngine.Object))
                    {
                        hs.Add(t);
                        t = t.BaseType;
                    }
                    hs.Add(t);
                }
                else if(!type.IsValueType)
                {
                    var t = type;
                    while (t != typeof(object) && t.GetCustomAttribute<SerializableAttribute>() != null)
                    {
                        hs.Add(t);
                        t = t.BaseType;
                    }
                }
            }
            return hs.ToArray();
        }
        
        public static void RegisterDynamicTypes(params Type[] types)
        {
            var deps = RuntimeAssetDatabase.Deps;
            var typeMap = deps.TypeMap;

            for (int i = 0; i < types.Length; ++i)
            {
                var type = types[i];

                if (type.IsValueType)
                {
                    UnityEngine.Debug.LogWarning("Dynamic Surrogates for value types are not supported");
                    continue;
                }

                if (!typeMap.TryGetID(type, out _))
                {
                    ClearCache(type);
                    typeMap.RegisterDynamicType(type);
                }
            }

            var enumerators = deps.ObjectEnumeratorFactory;
            for (int i = 0; i < types.Length; ++i)
            {
                var type = types[i];    
                if (IsEnumerable(type, typeMap))
                {
                    if (!enumerators.IsRegistered(type))
                    {
                        enumerators.Register(type, typeof(DynamicEnumerator));
                    }
                }
            }
        }

        public static void UnregisterDynamicTypes(params Type[] types)
        {
            var deps = RuntimeAssetDatabase.Deps;
            var typeMap = deps?.TypeMap;
            if (typeMap != null)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    Type type = types[i];

                    typeMap.Unregister(type);

                    ClearCache(type);
                }
            }
            
            var enumerators = deps?.ObjectEnumeratorFactory;
            if (enumerators != null)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    enumerators?.Unregister(types[i]);
                }
            }
        }

        private static bool IsEnumerable(Type type, ITypeMap typeMap)
        {
            return DynamicEnumerator.IsEnumerable(type, typeMap);
        }

        private static void ClearCache(Type type)
        {
            DynamicSurrogateUtils.ClearCache(type);
        }

        public static void Reset()
        {
            SerializerBase<Guid, string>.ClearRuntimeTypeModel();
            var deps = RuntimeAssetDatabase.Deps;
            if (deps != null)
            {
                var enumeratorFactory = deps.ObjectEnumeratorFactory;
                if (enumeratorFactory != null)
                {
                    enumeratorFactory.Reset();
                }
            }
        }


        [Obsolete("Use RegisterDynamicTypes")]
        public static void RegisterDynamicType(Type type)
        {
            if (type.IsValueType)
            {
                UnityEngine.Debug.LogWarning("Dynamic Surrogates for value types are not supported");
                return;
            }

            ClearCache(type);

            var deps = RuntimeAssetDatabase.Deps;
            var typeMap = deps.TypeMap;
            typeMap.RegisterDynamicType(type);

            if (IsEnumerable(type, typeMap))
            {
                var enumerators = deps.ObjectEnumeratorFactory;
                enumerators.Register(type, typeof(DynamicEnumerator));
            }
        }

        [Obsolete("Use UnregisterDynamicTypes")]
        public static void UnregisterDynamicType(Type type)
        {
            var deps = RuntimeAssetDatabase.Deps;
            var typeMap = deps?.TypeMap;
            typeMap?.Unregister(type);

            var enumerators = deps?.ObjectEnumeratorFactory;
            enumerators?.Unregister(type);

            ClearCache(type);
        }

        /*
        public static void Extend(IEnumerable<Type> surrogateTypes, IDictionary<Type, Type> typeToEnumerator)
        {
            var deps = RuntimeAssetDatabase.Deps;
            var typeMap = deps.TypeMap;
            typeMap.Clear();

            using var serializerRef = deps.AcquireSerializerRef();
            SerializerBase<Guid, string> serializer = serializerRef.Get() as SerializerBase<Guid, string>;
            serializer.CreateRuntimeTypeModel(typeMap);
            foreach (var type in surrogateTypes)
            {
                serializer.RegisterSurrogate(type);
            }
            serializer.CompileTypeModel(autoAddMissingTypes: true);

            var enumeratorFactory = deps.ObjectEnumeratorFactory;
            foreach(var kvp in typeToEnumerator)
            {
                enumeratorFactory.Register(kvp.Key, kvp.Value);
            }
        }
        */
    }

}
