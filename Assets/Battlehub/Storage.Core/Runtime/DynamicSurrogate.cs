using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage
{
    [ProtoContract, DataContract]
    public class SerializableField<TID>
    {
        public virtual object Value
        {
            get;
        }
    }

    [ProtoContract, DataContract]
    public class SerializableField<T, TID> : SerializableField<TID>
    {
        [ProtoMember(1), DataMember]
        public T value;

        public override object Value { get { return value; } }

        public SerializableField()
        {
        }

        public SerializableField(T value)
        {
            this.value = value;
        }
    }


    [ProtoContract, DataContract]
    public class SerializableArrayField<TID> : SerializableField<TID>
    {
    }

    [ProtoContract, DataContract]
    public class SerializableListField<TID> : SerializableField<TID>
    {
    }


    [ProtoContract, DataContract]
    public class SerializableArrayField<T, TID> : SerializableArrayField<TID>
    {
        [ProtoMember(1), DataMember]
        public bool IsNull;

        [ProtoMember(2), DataMember]
        public T[] Array;

        public override object Value
        {
            get { return Array; }
        }

        public SerializableArrayField()
        {
        }

        public SerializableArrayField(T[] value)
        {
            this.Array = value;
            this.IsNull = value == null;
        }

        [ProtoAfterDeserialization]
        public void OnAfterDeserialization()
        {
            if (!IsNull && Array == null)
            {
                Array = new T[0];
            }
        }
    }

    [ProtoContract, DataContract]
    public class SerializableListField<T, TID> : SerializableListField<TID>
    {
        [ProtoMember(1), DataMember]
        public bool IsNull;

        [ProtoMember(2), DataMember]
        public List<T> List;

        public override object Value
        {
            get { return List; }
        }

        public SerializableListField()
        {
        }

        public SerializableListField(List<T> value)
        {
            this.List = value;
            this.IsNull = value == null;
        }

        [ProtoAfterDeserialization]
        public void OnAfterDeserialization()
        {
            if (!IsNull && List == null)
            {
                List = new List<T>();
            }
        }
    }

    [ProtoContract, DataContract]
    public struct Ref<TID>
    {
        [ProtoMember(1), DataMember]
        public TID ID;

        [ProtoMember(2), DataMember]
        public int TypeID;

        [ProtoMember(3), DataMember]
        public TID GameObjectID;

        public Ref(TID id, int typeID, TID gameObjectID)
        {
            ID = id;
            TypeID = typeID;
            GameObjectID = gameObjectID;
        }
    }

    [ProtoContract, DataContract]
    public class SerializableObject<TID>
    {
        [ProtoMember(1), DataMember]
        public string[] Names;

        [ProtoMember(2), DataMember]
        public SerializableField<TID>[] Values;

        [ProtoMember(3), DataMember]
        public int TypeID;
    }

    [ProtoContract, DataContract]
    public struct SerializableObjectRoot<TID> where TID : IEquatable<TID>
    {
        [ProtoMember(1), DataMember]
        public SerializableObject<TID> Object;

        [ProtoMember(2), DataMember]
        public Dictionary<int, string> IDToType;

        public IReadOnlyDictionary<int, Type> GetIDToType()
        {
            var idToType = new Dictionary<int, Type>();
            foreach (var kvp in IDToType)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                var type = DynamicSurrogateUtils.ResolveType(kvp.Value);
                idToType.Add(kvp.Key, type);
            }
            return idToType;
        }
    }

    public struct SerializableFieldInfo
    {
        private PropertyInfo m_propertyInfo;
        private FieldInfo m_fieldInfo;

        public MemberInfo MemberInfo
        {
            get
            {
                if (m_fieldInfo != null)
                {
                    return m_fieldInfo;
                }

                return m_propertyInfo;
            }
        }

        public Type FieldType
        {
            get
            {
                if (m_fieldInfo != null)
                {
                    return m_fieldInfo.FieldType;
                }

                return m_propertyInfo.PropertyType;
            }
        }

        public string Name
        {
            get
            {
                if (m_fieldInfo != null)
                {
                    return m_fieldInfo.Name;
                }

                return m_propertyInfo.Name;
            }
        }

        public SerializableFieldInfo(PropertyInfo propertyInfo)
        {
            m_fieldInfo = null;
            m_propertyInfo = propertyInfo;
        }

        public SerializableFieldInfo(FieldInfo fieldInfo)
        {
            m_fieldInfo = fieldInfo;
            m_propertyInfo = null;
        }

        public object GetValue(object obj)
        {
            if (m_fieldInfo != null)
            {
                return m_fieldInfo.GetValue(obj);
            }

            return m_propertyInfo.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            if (m_fieldInfo != null)
            {
                m_fieldInfo.SetValue(obj, value);
                return;
            }

            m_propertyInfo.SetValue(obj, value);
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            if (m_fieldInfo != null)
            {
                return m_fieldInfo.GetCustomAttribute<T>();
            }

            return m_propertyInfo.GetCustomAttribute<T>();
        }
    }

    public class DynamicSurrogateUtils
    {
        public static int SerializationDepth = 7;

        public static Func<string, Type> ResolveType = name => Type.GetType(name);

        // don't change order of the types
        private static readonly Type[] s_primitiveTypes =
        {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
        };

        private static readonly HashSet<Type> s_primitiveTypesHs;

        static DynamicSurrogateUtils()
        {
            s_primitiveTypesHs = new HashSet<Type>(s_primitiveTypes);
        }

        public static Type[] GetPrimitiveTypes()
        {
            return s_primitiveTypes;
        }

        private static bool IsPrimitive(Type type)
        {
            return s_primitiveTypesHs.Contains(type);
        }

        private static bool IsUnityObjectType(Type type)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        private static bool IsKnownUnityObjectType(Type type, ITypeMap typeMap)
        {
            return IsUnityObjectType(type) && typeMap.TryGetID(type, out _);
        }

        private static bool IsKnownNonUnityObjectType(Type type, ITypeMap typeMap)
        {
            return !IsUnityObjectType(type) && typeMap.TryGetID(type, out _);
        }

        private static bool IsKnownValueType(Type type, ITypeMap typeMap)
        {
            return IsKnownNonUnityObjectType(type, typeMap) && type.IsValueType;
        }

        private static bool IsKnownNonUnityObjectReferenceType(Type type, ITypeMap typeMap)
        {
            return IsKnownNonUnityObjectType(type, typeMap) && !type.IsValueType;
        }

        private static bool IsGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        private static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        internal static bool IsEnumerable(Type type, ITypeMap typeMap)
        {
            if (IsArray(type))
            {
                type = type.GetElementType();
            }
            else if (IsGenericList(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return IsKnownUnityObjectType(type, typeMap) || IsKnownNonUnityObjectReferenceType(type, typeMap);
        }

        internal static bool IsSerializableObject(Type type, ITypeMap typeMap)
        {
            if (IsArray(type))
            {
                type = type.GetElementType();
            }
            else if (IsGenericList(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return !type.IsEnum && !IsPrimitive(type) && !typeMap.TryGetID(type, out _);
        }

        private static bool IsSerializable(Type type, ITypeMap typeMap)
        {
            if (IsPrimitive(type) || type.IsEnum)
            {
                return true;
            }

            if (typeMap != null)
            {
                bool isInTypeMap = typeMap.TryGetID(type, out _);
                if (!isInTypeMap)
                {
                    if (type.IsAbstract)
                    {
                        return false;
                    }

                    #if !UNITY_2020_1_OR_NEWER
                    if (type.IsGenericType)
                    {
                        return false;
                    }
                    #endif

                    if (IsUnityObjectType(type))
                    {
                        return true;
                    }

                    if (type.GetCustomAttribute<SerializableAttribute>() == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsSerializable(FieldInfo field, ITypeMap typeMap)
        {
            if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
            {
                return false;
            }

            Type fieldType = field.FieldType;
            if (IsArray(fieldType))
            {
                Type elementType = fieldType.GetElementType();
                return IsSerializable(elementType, typeMap);
            }

            if (IsGenericList(fieldType))
            {
                var elementType = fieldType.GetGenericArguments()[0];
                return IsSerializable(elementType, typeMap);
            }

            return IsSerializable(fieldType, typeMap);
        }


        internal static void GetSerializableFields(Type type, ITypeMap typeMap, List<SerializableFieldInfo> serializableFields)
        {
            var fieldNamesHs = new HashSet<string>();

            if (IsUnityObjectType(type))
            {
                serializableFields.Add(new SerializableFieldInfo(type.GetProperty(nameof(UnityEngine.Object.name))));
                serializableFields.Add(new SerializableFieldInfo(type.GetProperty(nameof(UnityEngine.Object.hideFlags))));

                if (typeof(Behaviour).IsAssignableFrom(type))
                {
                    serializableFields.Add(new SerializableFieldInfo(type.GetProperty(nameof(Behaviour.enabled))));
                }
            }

            while (type != null)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (!IsSerializable(field, typeMap))
                    {
                        continue;
                    }

                    if (!fieldNamesHs.Add(field.Name))
                    {
                        Debug.LogError($"Serialization of multiple fields with the same name is not supported. Field Name: {field.Name}");
                        continue;
                    }

                    serializableFields.Add(new SerializableFieldInfo(field));
                }

                type = type.BaseType;
            }
        }

        private class Context<TID> where TID : IEquatable<TID>
        {
            public Ref<TID> NullRef
            {
                get;
                private set;
            }

            public ITypeMap TypeMap
            {
                get;
                private set;
            }

            public IIDMap<TID> IDMap
            {
                get;
                private set;
            }

            public HashSet<object> Visited
            {
                get;
                private set;
            }

            public Dictionary<Type, int> TypeToID
            {
                get;
                private set;
            }

            public int TypeIndex
            {
                get;
                private set;
            }

            public int GetDynamicTypeID(Type type)
            {
                if (TypeToID.TryGetValue(type, out var typeID))
                {
                    return typeID;
                }

                int typeIndex = TypeIndex;
                TypeToID.Add(type, typeIndex);
                TypeIndex++;
                return typeIndex;
            }

            public Context(ITypeMap typeMap, IIDMap<TID> idMap)
            {
                TypeMap = typeMap;
                IDMap = idMap;
                Visited = new HashSet<object>();
                TypeToID = new Dictionary<Type, int>();
                NullRef = new Ref<TID>(idMap.NullID, -1, idMap.NullID);
            }
        }

        private static Ref<TID> CreateRef<TID>(object value, int typeID, IIDMap<TID> idmap) where TID : IEquatable<TID>
        {
            TID gameObjectID = default;

            if (value is Component)
            {
                gameObjectID = idmap.GetOrCreateID(((Component)value).gameObject);
            }
            else if (value is GameObject)
            {
                gameObjectID = idmap.GetOrCreateID(value);
            }

            return new Ref<TID>(idmap.GetOrCreateID(value), typeID, gameObjectID);
        }

        private static SerializableField<TID> CreateSerializableField<TID>(Type type, object value)
        {
            return (SerializableField<TID>)Activator.CreateInstance(type, value);
        }

        private static SerializableField<TID> CreateSerializableField<TID>(object obj, in SerializableFieldInfo fieldInfo, Context<TID> ctx, int depth) where TID : IEquatable<TID>
        {
            object value = fieldInfo.GetValue(obj);
            Type fieldType = fieldInfo.FieldType;
            Type fieldValueType = value != null ? value.GetType() : fieldType;
            
            bool isList = IsGenericList(fieldType);
            bool isArray = IsArray(fieldType);
            if (isList || isArray)
            {
                var elementType = isArray ? fieldType.GetElementType() : fieldType.GetGenericArguments()[0];
                if (elementType.IsEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(elementType);

                    if (isArray)
                    {
                        var type = typeof(SerializableArrayField<,>).MakeGenericType(underlyingType, typeof(TID));

                        if (value != null)
                        {
                            var enumArray = (Array)value;
                            var array = Array.CreateInstance(elementType, enumArray.Length);
                            for (int i = 0; i < enumArray.Length; ++i)
                            {
                                object enumValue = enumArray.GetValue(i);
                                object underlyingValue = Convert.ChangeType(enumValue, underlyingType);
                                array.SetValue(underlyingValue, i);
                            }
                            return CreateSerializableField<TID>(type, array);
                        }

                        return CreateSerializableField<TID>(type, null);
                    }

                    if (isList)
                    {
                        var listType = typeof(List<>).MakeGenericType(underlyingType);
                        var type = typeof(SerializableListField<,>).MakeGenericType(underlyingType, typeof(TID));

                        if (value != null)
                        {
                            var enumList = (IList)value;

                            var list = (IList)Activator.CreateInstance(listType);
                            for (int i = 0; i < enumList.Count; ++i)
                            {
                                object enumValue = enumList[i];
                                object underlyingValue = Convert.ChangeType(enumValue, underlyingType);
                                list.Add(underlyingValue);
                            }

                            return CreateSerializableField<TID>(type, list);
                        }

                        return CreateSerializableField<TID>(type, null);
                    }
                }

                bool isKnownValueType = IsKnownValueType(elementType, ctx.TypeMap);
                if (IsPrimitive(elementType) || isKnownValueType)
                {
                    if (isArray)
                    {
                        if (isKnownValueType && IsSerializationCallbackReceiver(elementType))
                        {
                            var array = (Array)value;
                            for (int i = 0; i < array.Length; ++i)
                            {
                                object arrayValue = array.GetValue(i);
                                RaiseOnBeforeSerialize(arrayValue);
                                array.SetValue(arrayValue, i);
                            }
                        }

                        var type = typeof(SerializableArrayField<,>).MakeGenericType(elementType, typeof(TID));
                        return CreateSerializableField<TID>(type, value);
                    }

                    if (isList)
                    {
                        if (isKnownValueType && IsSerializationCallbackReceiver(elementType))
                        {
                            var list = (IList)value;
                            for (int i = 0; i < list.Count; ++i)
                            {
                                object listValue = list[i];
                                RaiseOnBeforeSerialize(listValue);
                                list[i] = listValue;
                            }
                        }

                        var type = typeof(SerializableListField<,>).MakeGenericType(elementType, typeof(TID));
                        return CreateSerializableField<TID>(type, value);
                    }
                }

                if (IsUnityObjectType(elementType) || IsKnownNonUnityObjectReferenceType(elementType, ctx.TypeMap))
                {
                    if (isArray)
                    {
                        Ref<TID>[] refs = null;
                        if (value != null)
                        {
                            var array = (Array)value;
                            refs = new Ref<TID>[array.Length];

                            for (int i = 0; i < array.Length; i++)
                            {
                                var arrayValue = array.GetValue(i);
                                var arrayValueType = arrayValue != null ? arrayValue.GetType() : elementType;
                                if (IsKnownUnityObjectType(arrayValueType, ctx.TypeMap))
                                {
                                    int arrayValueTypeID = ctx.GetDynamicTypeID(arrayValueType);
                                    refs[i] = CreateRef(arrayValue, arrayValueTypeID, ctx.IDMap);
                                }
                                else
                                {
                                    refs[i] = ctx.NullRef;
                                }
                            }
                        }

                        return new SerializableArrayField<Ref<TID>, TID>(refs);
                    }

                    if (isList)
                    {
                        List<Ref<TID>> refs = null;
                        if (value != null)
                        {
                            var list = (IEnumerable)value;
                            refs = new List<Ref<TID>>();

                            foreach (var listValue in list)
                            {
                                var listValueType = listValue != null ? listValue.GetType() : elementType;
                                if (IsKnownUnityObjectType(listValueType, ctx.TypeMap))
                                {
                                    int listValueTypeID = ctx.GetDynamicTypeID(listValueType);
                                    refs.Add(CreateRef(listValue, listValueTypeID, ctx.IDMap));
                                }
                                else
                                {

                                    refs.Add(ctx.NullRef);
                                }
                            }
                        }

                        return new SerializableListField<Ref<TID>, TID>(refs);
                    }
                }

                if (isArray)
                {
                    SerializableObject<TID>[] serializableObjects = null;
                    if (value != null)
                    {
                        var array = (Array)value;
                        serializableObjects = new SerializableObject<TID>[array.Length];

                        for (int i = 0; i < array.Length; i++)
                        {
                            serializableObjects[i] = Serialize(array.GetValue(i), ctx, depth);
                        }
                    }

                    return new SerializableArrayField<SerializableObject<TID>, TID>(serializableObjects);
                }

                if (isList)
                {
                    List<SerializableObject<TID>> serializableObjects = null;
                    if (value != null)
                    {
                        var list = (IEnumerable)value;
                        serializableObjects = new List<SerializableObject<TID>>();
           
                        foreach (var item in list)
                        {
                            serializableObjects.Add(Serialize(item, ctx, depth));
                        }
                    }

                    return new SerializableListField<SerializableObject<TID>, TID>(serializableObjects);
                }
            }

            if (fieldType.IsEnum)
            {
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(fieldType));
                var type = typeof(SerializableField<,>).MakeGenericType(underlyingValue.GetType(), typeof(TID));
                return CreateSerializableField<TID>(type, underlyingValue);
            }

            if (IsPrimitive(fieldType) || IsKnownValueType(fieldType, ctx.TypeMap))
            {
                var type = typeof(SerializableField<,>).MakeGenericType(fieldType, typeof(TID));

                RaiseOnBeforeSerialize(value);

                return CreateSerializableField<TID>(type, value);
            }

            if (IsKnownUnityObjectType(fieldValueType, ctx.TypeMap) || IsKnownNonUnityObjectReferenceType(fieldType, ctx.TypeMap))
            {
                //#warning consider nonUnityObject as reference only if field has SerializeReferenceAttribute?

                var typeID = ctx.GetDynamicTypeID(fieldValueType);
                return new SerializableField<Ref<TID>, TID>(CreateRef(value, typeID, ctx.IDMap));
            }

            var serializableObject = Serialize(value, ctx, depth);
            return new SerializableField<SerializableObject<TID>, TID>(serializableObject);
        }

        private static SerializableObject<TID> Serialize<TID>(object obj, Context<TID> ctx, int depth) where TID : IEquatable<TID>
        {
            if (obj == null)
            {
                return null;
            }

            bool added = ctx.Visited.Add(obj);
            if (!added)
            {
                if (depth > SerializationDepth)
                {
                    return null;
                }
            }
            depth++;

            var objType = obj.GetType();
            if (IsSerializationCallbackReceiver(objType))
            {
                RaiseOnBeforeSerialize(obj);
            }

            var fields = new List<SerializableFieldInfo>();
            GetSerializableFields(objType, ctx.TypeMap, fields);

            var namesList = new List<string>();
            var valuesList = new List<SerializableField<TID>>();
            for (int i = 0; i < fields.Count; ++i)
            {
                var fieldInfo = fields[i];
                var serializableField = CreateSerializableField(obj, fieldInfo, ctx, depth);

                namesList.Add(fieldInfo.Name);
                valuesList.Add(serializableField);
            }

            var objectData = new SerializableObject<TID>();
            objectData.Names = namesList.ToArray();
            objectData.Values = valuesList.ToArray();
            objectData.TypeID = ctx.GetDynamicTypeID(obj.GetType());

            if (added)
            {
                ctx.Visited.Remove(obj);
            }

            return objectData;
        }

        private static bool HasDefaultConstructor(Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        internal static object CreateInstance(Type type)
        {
            if (type.IsAbstract)
            {
                return null;
            }

            if (HasDefaultConstructor(type))
            {
                return Activator.CreateInstance(type);
            }

            return FormatterServices.GetUninitializedObject(type);
        }

        private static object GetOrCreateRefObject<TID>(in Ref<TID> reference, IReadOnlyDictionary<int, Type> idToType, IIDMap<TID> idmap) where TID : IEquatable<TID>
        {
            object refObject = idmap.GetObject<object>(reference.ID);
            if (refObject == null)
            {
                if (idToType.TryGetValue(reference.TypeID, out var refObjectType))
                {
                    if (typeof(Component).IsAssignableFrom(refObjectType))
                    {
                        GameObject refGameObject = idmap.GetObject<GameObject>(reference.GameObjectID);
                        if (refGameObject == null)
                        {
                            refGameObject = new GameObject();
                            idmap.AddObject(refGameObject, reference.GameObjectID);
                        }
                        refObject = refGameObject.AddComponent(refObjectType);
                        idmap.AddObject(refObject, reference.ID);
                    }
                    else if (typeof(ScriptableObject).IsAssignableFrom(refObjectType))
                    {
                        refObject = ScriptableObject.CreateInstance(refObjectType);
                        idmap.AddObject(refObject, reference.ID);
                    }
                    else
                    {
                        //?add some method so that a developer can provide a construction function for each specific data type?
                        refObject = CreateInstance(refObjectType);
                        idmap.AddObject(refObject, reference.ID);
                    }
                }
            }
            return refObject;
        }

        internal static SerializableObjectRoot<TID> Serialize<TID>(object obj, ITypeMap typeMap, IIDMap<TID> idmap) where TID : IEquatable<TID>
        {
            var context = new Context<TID>(typeMap, idmap);
            var serializableObject = Serialize(obj, context, 0);

            var result = new SerializableObjectRoot<TID>
            {
                Object = serializableObject,
                IDToType = new Dictionary<int, string>()
            };

            foreach (var kvp in context.TypeToID)
            {
                result.IDToType.Add(kvp.Value, ReflectionHelpers.GetAssemblyQualifiedName(kvp.Key));
            }

            return result;
        }

        private static readonly Dictionary<Type, Dictionary<string, SerializableFieldInfo>> s_nameToFieldInfoCache = new Dictionary<Type, Dictionary<string, SerializableFieldInfo>>();

        internal static object Deserialize<TID>(object obj, SerializableObject<TID> objectData, IReadOnlyDictionary<int, Type> idToType, IIDMap<TID> idMap) where TID : IEquatable<TID>
        {
            var type = obj.GetType();
            var names = objectData.Names;
            if (names != null)
            {
                if (!s_nameToFieldInfoCache.TryGetValue(type, out var nameToFieldInfo))
                {
                    nameToFieldInfo = new Dictionary<string, SerializableFieldInfo>();

                    var fields = new List<SerializableFieldInfo>();
                    GetSerializableFields(type, null, fields);

                    foreach (var fieldInfo in fields)
                    {
                        nameToFieldInfo.Add(fieldInfo.Name, fieldInfo);

                        var formelySerializedAs = fieldInfo.GetCustomAttribute<UnityEngine.Serialization.FormerlySerializedAsAttribute>();
                        if (formelySerializedAs != null)
                        {
                            if (!nameToFieldInfo.ContainsKey(formelySerializedAs.oldName))
                            {
                                nameToFieldInfo.Add(formelySerializedAs.oldName, fieldInfo);
                            }
                        }
                    }

                    s_nameToFieldInfoCache[type] = nameToFieldInfo;
                }

                var values = objectData.Values;
                for (int i = 0; i < names.Length; i++)
                {
                    var fieldName = names[i];
                    if (!nameToFieldInfo.TryGetValue(fieldName, out var fieldInfo))
                    {
                        continue;
                    }

                    var fieldType = fieldInfo.FieldType;
                    var serializableField = values[i];
                    if (serializableField is SerializableField<Ref<TID>, TID>)
                    {
                        var reference = (Ref<TID>)serializableField.Value;
                        SetFieldValue(obj, fieldInfo, GetOrCreateRefObject(reference, idToType, idMap));
                    }
                    else if (serializableField is SerializableArrayField<Ref<TID>, TID>)
                    {
                        var references = (Ref<TID>[])serializableField.Value;
                        if (references != null)
                        {
                            var value = Array.CreateInstance(fieldType.GetElementType(), references.Length);
                            for (int j = 0; j < references.Length; ++j)
                            {
                                value.SetValue(GetOrCreateRefObject(references[j], idToType, idMap), j);
                            }

                            SetFieldValue(obj, fieldInfo, value);
                        }
                        else
                        {
                            SetFieldValue(obj, fieldInfo, null);
                        }
                    }
                    else if (serializableField is SerializableListField<Ref<TID>, TID>)
                    {
                        var references = (List<Ref<TID>>)serializableField.Value;
                        if (references != null)
                        {
                            var value = (IList)Activator.CreateInstance(fieldType);
                            for (int j = 0; j < references.Count; ++j)
                            {
                                value.Add(GetOrCreateRefObject(references[j], idToType, idMap));
                            }

                            SetFieldValue(obj, fieldInfo, value);
                        }
                        else
                        {
                            SetFieldValue(obj, fieldInfo, null);
                        }
                    }
                    else if (serializableField is SerializableField<SerializableObject<TID>, TID>)
                    {
                        Type valueType = null;
                        var valueData = (SerializableObject<TID>)serializableField.Value;
                        if (valueData != null)
                        {
                            idToType.TryGetValue(valueData.TypeID, out valueType);
                        }

                        if (valueType == fieldType)
                        {
                            object value = CreateInstance(fieldType);
                            value = Deserialize(value, valueData, idToType, idMap);
                            RaiseOnAfterDeserialize(value);
                            SetFieldValue(obj, fieldInfo, value);
                        }
                    }
                    else if (serializableField is SerializableArrayField<SerializableObject<TID>, TID>)
                    {
                        var valueDataArray = (SerializableObject<TID>[])serializableField.Value;
                        if (valueDataArray == null)
                        {
                            SetFieldValue(obj, fieldInfo, null);
                        }
                        else if (valueDataArray.Length == 0)
                        {
                            SetFieldValue(obj, fieldInfo, Array.CreateInstance(fieldType.GetElementType(), valueDataArray.Length));
                        }
                        else
                        {
                            var elementType = fieldType.GetElementType();
                            var isSerializationCallbackReceiver = IsSerializationCallbackReceiver(elementType);

                            var valueArray = Array.CreateInstance(elementType, valueDataArray.Length);
                            for (int j = 0; j < valueDataArray.Length; ++j)
                            {
                                if (valueDataArray[j] == null)
                                {
                                    continue;
                                }

                                Type valueType = null;
                                var valueData = valueDataArray[j];
                                if (valueData != null)
                                {
                                    idToType.TryGetValue(valueData.TypeID, out valueType);
                                }
                                if (valueType == elementType)
                                {
                                    object value = Deserialize(CreateInstance(elementType), valueData, idToType, idMap);
                                    if (isSerializationCallbackReceiver)
                                    {
                                        RaiseOnAfterDeserialize(value);
                                    }
                                    valueArray.SetValue(value, j);
                                }
                            }

                            SetFieldValue(obj, fieldInfo, valueArray);
                        }
                    }
                    else if (serializableField is SerializableListField<SerializableObject<TID>, TID>)
                    {
                        var valueDataList = (List<SerializableObject<TID>>)serializableField.Value;
                        if (valueDataList == null)
                        {
                            SetFieldValue(obj, fieldInfo, null);
                        }
                        else if (valueDataList.Count == 0)
                        {
                            SetFieldValue(obj, fieldInfo, Activator.CreateInstance(fieldType));
                        }
                        else
                        {
                            var elementType = fieldType.GetGenericArguments()[0];
                            var isSerializationCallbackReceiver = IsSerializationCallbackReceiver(elementType);

                            var valueList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                            for (int j = 0; j < valueDataList.Count; ++j)
                            {
                                if (valueDataList[j] == null)
                                {
                                    if (elementType.IsValueType)
                                    {
                                        var value = Activator.CreateInstance(elementType);
                                        if (isSerializationCallbackReceiver)
                                        {
                                            RaiseOnAfterDeserialize(value);
                                        }
                                        valueList.Add(value);
                                    }
                                    else
                                    {
                                        valueList.Add(null);
                                    }

                                    continue;
                                }

                                Type valueType = null;
                                var valueData = valueDataList[j];
                                if (valueData != null)
                                {
                                    idToType.TryGetValue(valueData.TypeID, out valueType);
                                }
                                if (valueType == elementType)
                                {
                                    object value = Deserialize(CreateInstance(elementType), valueData, idToType, idMap);
                                    if (isSerializationCallbackReceiver)
                                    {
                                        RaiseOnAfterDeserialize(value);
                                    }
                                    valueList.Add(value);
                                }
                            }

                            SetFieldValue(obj, fieldInfo, valueList);
                        }
                    }
                    else
                    {
                        if (fieldType.IsEnum)
                        {
                            if (serializableField.Value != null)
                            {
                                var value = Enum.ToObject(fieldType, serializableField.Value);
                                SetFieldValue(obj, fieldInfo, value);
                            }
                        }
                        else if (IsArray(fieldType))
                        {
                            var elementType = fieldType.GetElementType();
                            if (elementType.IsEnum)
                            {
                                var array = (Array)serializableField.Value;
                                if (array == null)
                                {
                                    SetFieldValue(obj, fieldInfo, null);
                                }
                                else
                                {
                                    var valueArray = Array.CreateInstance(elementType, array.Length);
                                    for (int j = 0; j < array.Length; ++j)
                                    {
                                        var value = array.GetValue(j);
                                        if (value != null)
                                        {
                                            valueArray.SetValue(Enum.ToObject(elementType, value), j);
                                        }
                                    }
                                    SetFieldValue(obj, fieldInfo, valueArray);
                                }
                            }
                            else
                            {
                                if (IsSerializationCallbackReceiver(elementType))
                                {
                                    Array array = (Array)serializableField.Value;
                                    if (array != null)
                                    {
                                        for (int j = 0; j < array.Length; ++j)
                                        {
                                            object value = array.GetValue(j);
                                            RaiseOnAfterDeserialize(value);
                                            array.SetValue(value, j);
                                        }
                                    }
                                }

                                SetFieldValue(obj, fieldInfo, serializableField.Value);
                            }
                        }
                        else if (IsGenericList(fieldType))
                        {
                            var elementType = fieldType.GetGenericArguments()[0];
                            if (elementType.IsEnum)
                            {
                                var list = (IList)serializableField.Value;
                                if (list == null)
                                {
                                    SetFieldValue(obj, fieldInfo, null);
                                }
                                else
                                {
                                    var valueList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                                    for (int j = 0; j < list.Count; ++j)
                                    {
                                        var value = list[j];
                                        if (value != null)
                                        {
                                            valueList.Add(Enum.ToObject(elementType, value));
                                        }
                                    }
                                    SetFieldValue(obj, fieldInfo, valueList);
                                }
                            }
                            else
                            {
                                if (IsSerializationCallbackReceiver(elementType))
                                {
                                    IList list = (IList)serializableField.Value;
                                    if (list != null)
                                    {
                                        for (int j = 0; j < list.Count; ++j)
                                        {
                                            object value = list[j];
                                            RaiseOnAfterDeserialize(value);
                                            list[j] = value;
                                        }
                                    }
                                }

                                SetFieldValue(obj, fieldInfo, serializableField.Value);
                            }
                        }
                        else
                        {
                            object value = serializableField.Value;
                            if (IsSerializationCallbackReceiver(fieldType))
                            {
                                RaiseOnAfterDeserialize(value);
                            }

                            SetFieldValue(obj, fieldInfo, value);
                        }
                    }
                }
            }

            return obj;
        }

        private static bool IsSerializationCallbackReceiver(Type type)
        {
            return typeof(ISerializationCallbackReceiver).IsAssignableFrom(type);
        }

        internal static void RaiseOnBeforeSerialize(object o)
        {
            var serializationCallbackReceiver = o as ISerializationCallbackReceiver;
            if (serializationCallbackReceiver != null)
            {
                serializationCallbackReceiver.OnBeforeSerialize();
            }
        }

        internal static void RaiseOnAfterDeserialize(object o)
        {
            var serializationCallbackReceiver = o as ISerializationCallbackReceiver;
            if (serializationCallbackReceiver != null)
            {
                serializationCallbackReceiver.OnAfterDeserialize();
            }
        }

        private static void SetFieldValue(object obj, in SerializableFieldInfo fieldInfo, object value)
        {
            if (value == null || fieldInfo.FieldType.IsAssignableFrom(value.GetType()))
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        public static void ClearCache(Type type)
        {
            s_nameToFieldInfoCache.Remove(type);
        }
    }

    [ProtoContract]
    public class DynamicSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4) /*, Obsolete*/]
        public string AssemblyQualifiedName { get; set; }

        [ProtoMember(5)]
        public SerializableObjectRoot<TID> Data { get; set; }

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            var component = obj as Component;
            if (component != null)
            {
                gameObjectId = idmap.GetOrCreateID(component.gameObject);
            }

            id = idmap.GetOrCreateID(obj);

            Data = DynamicSurrogateUtils.Serialize(obj, ctx.TypeMap, idmap);

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Type type = null;
            var root = Data;
            var idToType = root.GetIDToType();

            if (root.Object != null)
            {
                idToType.TryGetValue(Data.Object.TypeID, out type);
            }

            if (type == null)
            {
                type = AssemblyQualifiedName != null ? DynamicSurrogateUtils.ResolveType(AssemblyQualifiedName) : null; //compatibility
                if (type == null)
                {
                    return default;
                }

                if (root.IDToType == null)
                {
                    root.IDToType = new Dictionary<int, string>();
                }

                root.IDToType.Add(-1, AssemblyQualifiedName);
                idToType = root.GetIDToType();
            }

            object o;
            if (!idmap.NullID.Equals(gameObjectId))
            {
                o = idmap.GetComponent(type, id, gameObjectId);
            }
            else
            {
                if (!idmap.TryGetObject(id, out o))
                {
                    o = typeof(ScriptableObject).IsAssignableFrom(type) ?
                        ScriptableObject.CreateInstance(type) :
                        DynamicSurrogateUtils.CreateInstance(type);

                    idmap.AddObject(o, id);
                }
            }

            if (o != null)
            {
                if (Data.Object != null)
                {
                    o = DynamicSurrogateUtils.Deserialize(o, root.Object, idToType, idmap);
                    DynamicSurrogateUtils.RaiseOnAfterDeserialize(o);
                }
            }

            return new ValueTask<object>(o);
        }
    }
}
