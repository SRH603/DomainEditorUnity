using ProtoBuf.Meta;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage
{    
    public abstract class SerializerBase<TID, TFID> : ISurrogatesSerializer<TID>, ISerializer
        where TID : IEquatable<TID>
        where TFID : IEquatable<TFID>
      
    {
        private readonly ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)> m_serializationQueue = new ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)>();
        protected ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)> SerializationQueue
        {
            get { return m_serializationQueue; }
        }

        private readonly ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)> m_deserializationQueue = new ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)>();
        protected ConcurrentQueue<(ISurrogate<TID> Surrogate, int TypeIndex)> DeserializationQueue
        {
            get { return m_deserializationQueue; }
        }

        private readonly MemoryStream m_serializationMemoryStream = new MemoryStream();
        protected MemoryStream SerializationMemoryStream
        {
            get { return m_serializationMemoryStream; }
        }

        private static readonly Dictionary<int, Type> m_indexToType = new Dictionary<int, Type>();
        protected IReadOnlyDictionary<int, Type> IndexToType
        {
            get { return m_indexToType; }
        }

        private static readonly Dictionary<int, Func<ISurrogate<TID>>> m_typeIndexToSurrogateCtor = new Dictionary<int, Func<ISurrogate<TID>>>();
        private static TypeModel m_typeModel;
        public static RuntimeTypeModel RuntimeTypeModel
        {
            get { return m_typeModel as RuntimeTypeModel; } 
        }
        
        public static void ClearRuntimeTypeModel()
        {
            m_typeMap = null;
            m_typeModel = null;
            m_indexToType.Clear();
            m_typeIndexToSurrogateCtor.Clear();
        }

        public void CreateRuntimeTypeModel(ITypeMap typeMap)
        {
            if (m_typeModel != null)
            {
                ClearRuntimeTypeModel();
                m_typeModel = TypeModel.Create();
            }

            m_typeMap = typeMap;
            RegisterSerializableTypes();
        }

        public void CompileTypeModel(bool autoAddMissingTypes)
        {
            RuntimeTypeModel runtimeTypeModel = RuntimeTypeModel;
            if (runtimeTypeModel != null)
            {
                runtimeTypeModel.AutoAddMissingTypes = autoAddMissingTypes;
                runtimeTypeModel.CompileInPlace();
            }
        }

        private void RegisterSerializableTypes()
        {
            Register<LinkMap<TID, TID>>();
            Register<Meta<TID, TFID>>();
            Register<Thumbnail>();
            Register<ExternalData<TID>>();
            Register<DynamicSurrogate<TID>>();
            
            m_typeIndexToSurrogateCtor.Add(ITypeMap.k_DynamicTypeID, () => new DynamicSurrogate<TID>());
            m_indexToType.Add(ITypeMap.k_DynamicTypeID, typeof(DynamicSurrogate<TID>));

            Initialize();

            var typeModel = RuntimeTypeModel;

            if (typeModel != null)
            {
                Register<SerializableObject<TID>>();
                Register<Ref<TID>>();

                var serializableFieldType = typeof(SerializableField<TID>);
                var serializableField = typeModel.Add(serializableFieldType, true);

                var serializableArrayFieldType = typeof(SerializableArrayField<TID>);
                var serializableArrayField = typeModel.Add(serializableArrayFieldType, true);

                var serializableListFieldType = typeof(SerializableListField<TID>);
                var serializableListField = typeModel.Add(serializableListFieldType, true);

                int id = 10;
                serializableField.AddSubType(id, serializableArrayFieldType);
                id++;
                serializableField.AddSubType(id, serializableListFieldType);

                id = 20;
                foreach (Type type in new[] {typeof(SerializableObject<TID>), typeof(Ref<TID>)})
                { 
                    var serializableType = typeof(SerializableField<,>).MakeGenericType(type, typeof(TID));
                    serializableField.AddSubType(id, serializableType);

                    var serializableArrayType = typeof(SerializableArrayField<,>).MakeGenericType(type, typeof(TID));
                    serializableArrayField.AddSubType(id, serializableArrayType);

                    var serializableListType = typeof(SerializableListField<,>).MakeGenericType(type, typeof(TID));
                    serializableListField.AddSubType(id, serializableListType);

                    id++;
                }

                id = 30;
                foreach (Type type in DynamicSurrogateUtils.GetPrimitiveTypes().Union(new Type[] { typeof(Guid) }))
                {
                    var serializableType = typeof(SerializableField<,>).MakeGenericType(type, typeof(TID));
                    serializableField.AddSubType(id, serializableType);

                    var serializableArrayType = typeof(SerializableArrayField<,>).MakeGenericType(type, typeof(TID));
                    serializableArrayField.AddSubType(id, serializableArrayType);
 
                    var serializableListType = typeof(SerializableListField<,>).MakeGenericType(type, typeof(TID));
                    serializableListField.AddSubType(id, serializableListType);

                    id++;
                }

                Debug.Assert(id <= 100);

                foreach (var type in m_typeMap.Types)
                {
                    if (!m_typeMap.TryGetID(type, out int typeIndex))
                    {
                        continue;
                    }

                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (!RuntimeTypeModel.CanSerializeContractType(type))
                    {
                        continue;
                    }
         
                    var serializableType = typeof(SerializableField<,>).MakeGenericType(type, typeof(TID));
                    serializableField.AddSubType(typeIndex, serializableType);

                    var serializableArrayType = typeof(SerializableArrayField<,>).MakeGenericType(type, typeof(TID));
                    serializableArrayField.AddSubType(typeIndex, serializableArrayType);

                    var serializableListType = typeof(SerializableListField<,>).MakeGenericType(type, typeof(TID));
                    serializableListField.AddSubType(typeIndex, serializableListType);
                }

                Register<SerializableObjectRoot<TID>>();
            }      
        }

        private static ITypeMap m_typeMap;
        protected SerializerBase(ITypeMap typeMap)
        { 
            if (m_typeModel != null)
            {
                return;
            }

#if !UNITY_EDITOR
            Assembly typeModelAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName.Contains("StorageTypeModel")).FirstOrDefault();
            Type type = null;
            if (typeModelAssembly != null)
            {
                type = typeModelAssembly.GetTypes().Where(t => t.Name.Contains("StorageTypeModel")).FirstOrDefault();
            }

            m_typeModel = type != null ? Activator.CreateInstance(type) as TypeModel : null;      
#endif
            if (m_typeModel == null)
            {
                m_typeModel = TypeModel.Create();
                CreateRuntimeTypeModel(typeMap);
                CompileTypeModel(false);
            }
            else
            {
                m_typeMap = typeMap;
                RegisterSerializableTypes();
            }
        }

        protected virtual void Initialize() { }

        public void RegisterSurrogate<TSurrogate>() where TSurrogate : ISurrogate<TID>, new()
        {
            RegisterSurrogate(typeof(TSurrogate), () => new TSurrogate());
        }

        public void RegisterSurrogate(Type surrogateType)
        {
            RegisterSurrogate(surrogateType, () => (ISurrogate<TID>)Activator.CreateInstance(surrogateType));
        }

        private void RegisterSurrogate(Type surrogateType, Func<ISurrogate<TID>> ctor)
        {
            SurrogateAttribute surrogateAttribute = surrogateType.GetCustomAttribute<SurrogateAttribute>();
            if (surrogateAttribute == null)
            {
                return;
            }

            m_typeMap.Register(surrogateAttribute.Type, surrogateAttribute.TypeIndex);
            m_indexToType.Add(surrogateAttribute.TypeIndex, surrogateType);
            m_typeIndexToSurrogateCtor.Add(surrogateAttribute.TypeIndex, ctor);

            if (m_typeModel is RuntimeTypeModel)
            {
                Add((RuntimeTypeModel)m_typeModel, surrogateType);
            }
        }

        public void Register<T>()
        {
            if (m_typeModel is RuntimeTypeModel)
            {
                Add((RuntimeTypeModel)m_typeModel, typeof(T));
            }
        }

        private static void Add(RuntimeTypeModel model, Type type)
        { 
            SurrogateAttribute surrogateAttribute = type.GetCustomAttribute<SurrogateAttribute>();
            if (surrogateAttribute == null)
            {
                model.Add(type, true);
                return;
            }

            if (surrogateAttribute.Enabled)
            {
                model.Add(type, true);
            }
            else
            {
                var serializablePropertyNames = type.GetProperties().Where(p => p.CanWrite && p.CanRead).Select(p => (p.GetCustomAttribute<global::ProtoBuf.ProtoMemberAttribute>(), p.Name));
                var serializableFieldNames = type.GetFields().Select(f => (f.GetCustomAttribute<global::ProtoBuf.ProtoMemberAttribute>(), f.Name));
                var metaType = model.Add(surrogateAttribute.Type, true);

                foreach (var (attribute, name) in serializablePropertyNames.Union(serializableFieldNames))
                {
                    if (attribute != null)
                    {
                        metaType.Add(attribute.Tag, name);
                    }
                }

                model.Add(typeof(SerializableArray<>).MakeGenericType(surrogateAttribute.Type), true);
            }
        }

        public ISurrogate<TID> CreateSurrogate(Type type)
        {
            if (!m_typeMap.TryGetID(type, out int typeIndex))
            {
                return null;
            }

            if (!m_typeIndexToSurrogateCtor.TryGetValue(typeIndex, out var surrogateCtor))
            {
                return null;
            }

            return surrogateCtor.Invoke();
        }

        public async ValueTask<bool> Enqueue(object obj, ISerializationContext<TID> context)
        {
            if (obj == null)
            {
                m_serializationQueue.Enqueue((null, -1));
                return false;
            }

            Type type = obj.GetType();
            if (!m_typeMap.TryGetID(type, out int typeIndex))
            {
                return false;
            }

            if (!m_typeIndexToSurrogateCtor.TryGetValue(typeIndex, out var surrogateCtor))
            {
                return false;
            }

            ISurrogate<TID> surrogate = surrogateCtor.Invoke();
            await surrogate.Serialize(obj, context);
            m_serializationQueue.Enqueue((surrogate, typeIndex));
            return true;
        }

        public virtual Task SerializeToStream(Stream stream)
        {
            while(true)
            {
                if (!m_serializationQueue.TryDequeue(out var item))
                {
                    continue;
                }

                ISurrogate<TID> surrogate = item.Surrogate;
                if (surrogate == null)
                {
                    //Done
                    break;
                }

                int typeIndex = item.TypeIndex;
                try
                {
                    m_serializationMemoryStream.Position = 0;
                    m_typeModel.Serialize(m_serializationMemoryStream, surrogate);

                    int length = (int)m_serializationMemoryStream.Position;
                    m_serializationMemoryStream.Position = 0;

                    byte[] data = ReadExactly(m_serializationMemoryStream, length);
                    byte[] header1 = BitConverter.GetBytes(length);
                    byte[] header2 = BitConverter.GetBytes(typeIndex);

                    stream.Write(header1, 0, header1.Length);
                    stream.Write(header2, 0, header2.Length);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    break; 
                }
            }

            return Task.CompletedTask;
        }

        public bool CopyToDeserializationQueue()
        {
            if(!m_serializationQueue.TryDequeue(out var item))
            {
                return true;
            }

            ISurrogate<TID> surrogate = item.Surrogate;
            if (surrogate == null)
            {
                m_deserializationQueue.Enqueue((null, -1));
                return false;
            }

            int typeIndex = item.TypeIndex;
            try
            {
                m_deserializationQueue.Enqueue((surrogate, typeIndex));
                return true;
            }
            catch (Exception e)
            {
                m_deserializationQueue.Enqueue((null, -1));
                Debug.LogException(e);
                return false;
            }
        }

        public virtual Task DeserializeFromStream(Stream stream)
        {
            byte[] header = new byte[sizeof(int)];

            try
            {
                while (stream.Position < stream.Length)
                {
                    stream.Read(header, 0, header.Length);
                    int length = BitConverter.ToInt32(header, 0);

                    stream.Read(header, 0, header.Length);
                    int typeIndex = BitConverter.ToInt32(header, 0);

                    if (m_indexToType.TryGetValue(typeIndex, out var type))
                    {
                        ISurrogate<TID> surrogate = (ISurrogate<TID>)m_typeModel.Deserialize(stream, null, type, length);
                        m_deserializationQueue.Enqueue((surrogate, typeIndex));
                    }
                    else
                    {
                        stream.Seek(length, SeekOrigin.Current);
                    }
                }

                m_deserializationQueue.Enqueue((null, -1));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_deserializationQueue.Enqueue((null, -1));
            }


            return Task.CompletedTask;
        }

        public ValueTask<object> Dequeue(ISerializationContext<TID> context)
        {
            if (!m_deserializationQueue.TryDequeue(out var item))
            {
                return new ValueTask<object>(-1);
            }

            int typeIndex = item.TypeIndex;
            if (typeIndex == -1)
            {
                return new ValueTask<object>(-2);
            }

            ISurrogate<TID> surrogate = item.Surrogate;
            return surrogate.Deserialize(context);
        }

        public virtual void Reset()
        {
            while (m_serializationQueue.TryDequeue(out var _)) ;
            while (m_deserializationQueue.TryDequeue(out var _)) ;
            m_serializationMemoryStream.SetLength(0);
        }

        protected static byte[] ReadExactly(Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset, count - offset);
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }
                    
                offset += read;
            }
            return buffer;
        }

        public virtual void Serialize<T>(Stream stream, T obj)
        {
            m_typeModel.Serialize(stream, obj);
        }

        public virtual ValueTask<Pack<T>> Deserialize<T>(Stream stream)
        {
            var obj = m_typeModel.Deserialize(stream, null, typeof(T));
            if (obj is T)
            {
                return new ValueTask<Pack<T>>(new Pack<T>(false, (T)obj));
            }

            return new ValueTask<Pack<T>>(new Pack<T>(isEmpty: true));
        }
    }
}
