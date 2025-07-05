using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;

namespace Battlehub.Storage
{
    [Serializable]
    public class SurrogatesGenConfig
    {
        public static SurrogatesGenConfig Instance = new SurrogatesGenConfig();

        public int TypeIndex = 32768;
        public bool IncludeProperties = true;
        
        protected Type GetPropertyType(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo)memberInfo).PropertyType;
            }

            FieldInfo fieldInfo = (FieldInfo)memberInfo;
            return fieldInfo.FieldType;
        }

        public int GetTypeIndex(Type surrogateType)
        {
            int typeIndex;
            var surrogateAttribute = surrogateType != null ?
                surrogateType.GetCustomAttribute<SurrogateAttribute>() :
                null;

            if (surrogateAttribute != null)
            {
                typeIndex = surrogateAttribute.TypeIndex;
            }
            else
            {
                typeIndex = TypeIndex++;
            }

            return typeIndex;
        }

        public virtual IEnumerable<MemberInfo> GetEnumerableProperties(Type type)
        {
            return GetSerializableProperties(type);
        }

        private static readonly HashSet<Type> s_excludeSerializableTypes = new HashSet<Type> { typeof(object), typeof(LocalKeyword) };        
        public virtual bool IsSerializableType(Type type)
        {
            if (s_excludeSerializableTypes != null && s_excludeSerializableTypes.Contains(type))
            {
                return false;
            }

            if (typeof(string) == type)
            {
                return true;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return true;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // for now only allow arrays and lists
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    return !IsArrayOrGenericList(elementType) && IsSerializableType(elementType);
                }

                if (IsGenericList(type))
                {
                    var elementType = type.GetGenericArguments()[0];
                    return !IsArrayOrGenericList(elementType) && IsSerializableType(elementType);
                }

                return false;
            }

            
            if (type.FullName != null && (type.FullName.StartsWith("Battlehub.Storage.SerializableList") || type.FullName.StartsWith("Battlehub.Storage.SerializableArray")))
            {
                return true;
            }

            if (type.IsGenericType || type.IsAbstract || type.IsInterface)
            {
                return false;
            }

            return true;
        }

        private bool IsArrayOrGenericList(Type type)
        {
            return type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        private bool IsGenericList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        protected virtual bool IsPropertySerializable(PropertyInfo propertyInfo)
        {
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
            {
                return false;
            }

            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
            {
                return false;
            }

            if (propertyInfo.GetIndexParameters().Length != 0)
            {
                return false;
            }

            if (setMethod.GetCustomAttribute<ObsoleteAttribute>() != null)
            {
                return false;
            }

            if (getMethod.GetCustomAttribute<ObsoleteAttribute>() != null)
            {
                return false;
            }

            return true;
        }

        private bool CausesCycleInStructLayout(Type type, MemberInfo memberInfo, HashSet<Type> visitedTypes = null)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            if (visitedTypes == null)
            {
                visitedTypes = new HashSet<Type>();
            }

            if (!visitedTypes.Add(type))
            {
                return true;
            }

            var propertyType = GetPropertyType(memberInfo);
            var childProperties = GetSerializableProperties(propertyType);
            foreach (var childProperty in childProperties)
            {
                if (CausesCycleInStructLayout(propertyType, childProperty, new HashSet<Type>(visitedTypes)))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual IEnumerable<MemberInfo> GetSerializableProperties(Type type)
        {
            return GetSerializableMembers(type, includeProperties: IncludeProperties);
        }

        public virtual IEnumerable<MemberInfo> GetSerializableMembers(Type type, bool includeProperties)
        {
            var properties = includeProperties ?
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => IsPropertySerializable(p)) :
                new PropertyInfo[0];

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Cast<MemberInfo>();

            var result = fields.Union(properties)
                .Where(p => p.GetCustomAttribute<ObsoleteAttribute>() == null &&
                            p.GetCustomAttribute<NonSerializedAttribute>() == null);

            result = result.Where(p => !CausesCycleInStructLayout(type, p));

            result = result.Where(p =>
            {
                Type propertyType = GetPropertyType(p);
                return IsSerializableType(propertyType);
            });

            if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(type))
            {
#if UNITY_EDITOR
                result = result.Where(p =>
                    p.Name != nameof(UnityEngine.MonoBehaviour.runInEditMode) &&
                    p.Name != nameof(UnityEngine.MonoBehaviour.useGUILayout));
#endif
            }

            if (typeof(UnityEngine.Component).IsAssignableFrom(type))
            {
                result = result.Where(p =>
                    p.Name != nameof(UnityEngine.MonoBehaviour.name) &&
                    p.Name != nameof(UnityEngine.MonoBehaviour.hideFlags) &&
                    p.Name != nameof(UnityEngine.MonoBehaviour.tag));
            }

            result = result.Where(p => p.Name != "id" && p.Name != "gameObjectId");

            return result;
        }

    }
}
