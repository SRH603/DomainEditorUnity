using System;
using System.Collections.Generic;
using System.Reflection;

namespace Battlehub.Storage
{
    public class DynamicEnumerator : ObjectEnumerator<object>
    {
        private readonly HashSet<object> m_visited = new HashSet<object>();
        private readonly List<object> m_values = new List<object>();
        private readonly List<int> m_keys = new List<int>();

        public override object Object 
        {
            get { return base.Object; }
            set
            {
                base.Object = value;

                m_values.Clear();
                m_keys.Clear();

                if (value != null)
                {
                    GetKeysAndValues(value);
                }

                m_visited.Clear();
            }
        }

        public static bool IsEnumerable(Type type, ITypeMap typeMap)
        {
            return IsEnumerable(type, typeMap, new HashSet<MemberInfo>());
        }

        private static bool IsEnumerable(Type type, ITypeMap typeMap, HashSet<MemberInfo> visited)
        {
            var fields = new List<SerializableFieldInfo>();

            DynamicSurrogateUtils.GetSerializableFields(type, typeMap, fields);

            for (int i = 0; i < fields.Count; ++i)
            {
                var field = fields[i];
                if (!visited.Add(field.MemberInfo))
                {
                    continue;
                }

                if (DynamicSurrogateUtils.IsEnumerable(field.FieldType, typeMap))
                {
                    return true;
                }

                if (DynamicSurrogateUtils.IsSerializableObject(field.FieldType, typeMap))
                {
                    if (IsEnumerable(field.FieldType, typeMap, visited))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void GetKeysAndValues(object value)
        {
            if (value == null)
            {
                return;
            }

            if (!m_visited.Add(value))
            {
                return;
            }

            var fields = new List<SerializableFieldInfo>();
            DynamicSurrogateUtils.GetSerializableFields(value.GetType(), m_typeMap, fields);

            for (int i = 0; i < fields.Count; ++i)
            {
                var field = fields[i];

                if (DynamicSurrogateUtils.IsEnumerable(field.FieldType, m_typeMap))
                {
                    m_keys.Add(field.Name.GetHashCode());
                    m_values.Add(field.GetValue(value));
                }
                else if (DynamicSurrogateUtils.IsSerializableObject(field.FieldType, m_typeMap))
                {
                    GetKeysAndValues(field.GetValue(value));
                }
            }
        }

        private ITypeMap m_typeMap;
        public DynamicEnumerator(ITypeMap typeMap)
        {
            m_typeMap = typeMap;
        }

        public override void Reset()
        {
            base.Reset();
            m_keys.Clear();
            m_values.Clear();
            m_visited.Clear();
        }

        protected override IEnumerator<(object Object, int Key)> GetNext()
        {
            for (int i = 0; i < m_keys.Count; ++i)
            {
                yield return (m_values[i], m_keys[i]);
            }

            yield return (Object, -1);
        }
    }
}
