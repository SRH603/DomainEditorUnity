using System;
using System.Collections.Generic;

namespace Battlehub.Storage
{ 
    public static class ISurrogateExtension
    {
        public static SerializableArray<T> Serialize<T, V, TID>(this SerializableArray<T> _, V[] sourceArray, ISerializationContext<TID> context) where T : struct, IValueTypeSurrogate<V, TID>  where TID : IEquatable<TID>
        {
            if (sourceArray == null)
            {
                return null;
            }

            var targetArray = new T[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; ++i)
            {
                var obj = new T();
                obj.Serialize(sourceArray[i], context);
                targetArray[i] = obj;
            }

            return targetArray;
        }

        public static SerializableList<T> Serialize<T, V, TID>(this SerializableList<T> _, List<V> sourceList, ISerializationContext<TID> context) where T : struct, IValueTypeSurrogate<V, TID> where TID : IEquatable<TID>
        {
            if (sourceList == null)
            {
                return null;
            }

            var targetList = new List<T>(sourceList.Count);
            for (int i = 0; i < sourceList.Count; ++i)
            {
                var obj = new T();
                obj.Serialize(sourceList[i], context);
                targetList.Add(obj);
            }

            return targetList;
        }

        public static V[] Deserialize<T, V, TID>(this SerializableArray<T> serializableArray, V[] _, ISerializationContext<TID> context) where T : struct, IValueTypeSurrogate<V, TID> where TID : IEquatable<TID>
        {
            if (serializableArray.IsNull)
            {
                return null;
            }

            var sourceArray = serializableArray.Data;
            if (sourceArray == null)
            {
                return new V[0];
            }

            var targetArray = new V[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; ++i)
            {
                targetArray[i] = sourceArray[i].Deserialize(context); 
            }

            return targetArray;
        }

        public static List<V> Deserialize<T, V, TID>(this SerializableList<T> serializableList, List<V> _, ISerializationContext<TID> context) where T : struct, IValueTypeSurrogate<V, TID> where TID : IEquatable<TID>
        {
            if (serializableList.IsNull)
            {
                return null;
            }

            var sourceList = serializableList.Data;
            if (sourceList == null)
            {
                return new List<V>();
            }

            var targetList = new List<V>(sourceList.Count);
            for (int i = 0; i < sourceList.Count; ++i)
            {
                targetList.Add(sourceList[i].Deserialize(context));
            }

            return targetList;
        }
    }
}
