using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.Storage
{
    public static class IIDMapExtensions
    {
        public static List<TID> GetOrCreateIDs<TID>(this IIDMap<TID> idmap, IList list) where TID : IEquatable<TID>
        {
            if (list == null)
            {
                return null;
            }

            List<TID> idsList = new List<TID>(list.Count);
            foreach (object obj in list)
            {
                idsList.Add(idmap.GetOrCreateID(obj));
            }
            return idsList;
        }

        public static TID[] GetOrCreateIDs<TID>(this IIDMap<TID> idmap, Array array) where TID : IEquatable<TID>
        {
            if (array == null)
            {
                return null;
            }

            TID[] ids = new TID[array.Length];
            for (int i = 0; i < array.Length; ++i)
            {
                ids[i] = idmap.GetOrCreateID(array.GetValue(i));
            }
            return ids;
        }

        public static T[] GetObjects<T, TID>(this IIDMap<TID> idmap, TID[] ids, bool updateRefCounter = true) where TID : IEquatable<TID> 
        {
            T[] objects = null;
            if(ids != null)
            {
                objects = new T[ids.Length];
                for (int i = 0; i < ids.Length; ++i)
                {
                    T obj = idmap.GetObject<T>(ids[i]);
                    objects[i] = obj;
                }
            }
            return objects;
        }

        public static List<T> GetObjects<T, TID>(this IIDMap<TID> idmap, List<TID> ids, bool updateRefCounter = true) where TID : IEquatable<TID>
        {
            List<T> objects = null;
            if (ids != null)
            {
                objects = new List<T>(ids.Count);
                for (int i = 0; i < ids.Count; ++i)
                {
                    T obj = idmap.GetObject<T>(ids[i]);
                    objects.Add(obj);
                }
            }
            return objects;
        }

        public static List<T> GetOrCreateObjects<T, TID>(this IIDMap<TID> idmap, List<TID> ids, bool updateRefCounter = true) where TID : IEquatable<TID> where T : new()
        {
            List<T> objects = null;
            if (ids != null)
            {
                objects = new List<T>(ids.Count);
                for (int i = 0; i < ids.Count; ++i)
                {
                    if (idmap.NullID.Equals(ids[i]))
                    {
                        continue;
                    }

                    T obj = idmap.GetOrCreateObject<T>(ids[i]);
                    objects.Add(obj);
                }
            }
            return objects;
        }

        public static T GetTransform<T, TID>(this IIDMap<TID> idmap, TID componentID, TID gameObjectID, Transform tempRoot = null) where TID : IEquatable<TID> where T : Transform
        {
            Debug.Assert(!idmap.NullID.Equals(componentID));
            T component = idmap.GetObject<T>(componentID);
            if (component == null)
            {
                Debug.Assert(!idmap.NullID.Equals(gameObjectID));
                GameObject gameObject = idmap.GetObject<GameObject>(gameObjectID);
                if (gameObject == null)
                {
                    gameObject = new GameObject();
                    gameObject.transform.SetParent(tempRoot);
                    idmap.AddObject(gameObject, gameObjectID);                    
                }
                
                component = gameObject.GetComponent<T>();
                if (component == null)
                {
                    component = gameObject.AddComponent<T>();
                }
                
                
                idmap.AddObject(component, componentID);
            }
            return component;
        }

        public static Transform GetComponent<TID>(this IIDMap<TID> idmap, TID componentID, TID gameObjectID, Transform tempRoot) where TID : IEquatable<TID>
        {
            return GetTransform<Transform, TID>(idmap, componentID, gameObjectID, tempRoot);
        }

        public static T GetComponent<T, TID>(this IIDMap<TID> idmap, TID componentID, TID gameObjectID, Transform tempRoot = null) where TID : IEquatable<TID> where T : Component
        {
            if(idmap.NullID.Equals(componentID))
            {
                return default;
            }

            T component = idmap.GetObject<T>(componentID);
            if (component == null)
            {
                Debug.Assert(!idmap.NullID.Equals(gameObjectID));
                GameObject gameObject = idmap.GetObject<GameObject>(gameObjectID);
                if (gameObject == null)
                {
                    gameObject = new GameObject();
                    gameObject.transform.SetParent(tempRoot);
                    idmap.AddObject(gameObject, gameObjectID);
                }
                component = gameObject.AddComponent<T>();
                if (component == null)
                {
                    // The propagation mechanism is designed in such a way that additions are propagated first, followed by deletions.
                    // This is a fix for the case where components like AudioListener could not be added twice when applied.
                    // and propagating changes to an asset.
                    // https://github.com/Battlehub0x/AssetPack/commit/d9d0004023470af366b6f4c96530f872aa782aef
                    // STR: 1. Create an Empty prefab, add it to scene
                    //      2. Add Audio Listener component
                    //      3. Open prefab, Add Audio Listener component, and apply changes
                    component = gameObject.GetComponent<T>();
                    UnityEngine.Object.DestroyImmediate(component);
                    component = gameObject.AddComponent<T>();
                }
                idmap.AddObject(component, componentID);
            }
            return component;
        }

        public static Component GetComponent<TID>(this IIDMap<TID> idmap, Type componentType, TID componentID, TID gameObjectID, Transform tempRoot = null) where TID : IEquatable<TID> 
        {
            if (idmap.NullID.Equals(componentID) || componentType == null)
            {
                return default;
            }
            var component = idmap.GetObject<Component>(componentID);
            if (component == null)
            {
                Debug.Assert(!idmap.NullID.Equals(gameObjectID));
                GameObject gameObject = idmap.GetObject<GameObject>(gameObjectID);
                if (gameObject == null)
                {
                    gameObject = new GameObject();
                    gameObject.transform.SetParent(tempRoot);
                    idmap.AddObject(gameObject, gameObjectID);
                }
                component = gameObject.AddComponent(componentType);
                idmap.AddObject(component, componentID);
            }
            return component;
        }


        public static Ref<TID> GetOrCreateRef<TID>(this IIDMap<TID> idmap, Component component, ITypeMap typeMap) where TID : IEquatable<TID>
        {
            if (component == null)
            {
                return new Ref<TID>(idmap.NullID, 0, idmap.NullID);
            }

            int typeId = typeMap.GetID(component.GetType());
            var id = idmap.GetOrCreateID(component);
            var gameObjectId = idmap.GetOrCreateID(component.gameObject);
            return new Ref<TID>(id, typeId, gameObjectId);
        }

        public static Component GetOrCreateObject<TID>(this IIDMap<TID> idmap, Ref<TID> componentRef, ITypeMap typeMap, Transform tempRoot = null) where TID : IEquatable<TID>
        {
            return GetComponent(idmap, typeMap.GetType(componentRef.TypeID), componentRef.ID, componentRef.GameObjectID, tempRoot);
        }
        public static T GetOrCreateObject<T, TID>(this IIDMap<TID> idmap, Ref<TID> componentRef, ITypeMap typeMap, Transform tempRoot = null) where TID : IEquatable<TID> where T : Component
        {
            return GetComponent(idmap, typeMap.GetType(componentRef.TypeID), componentRef.ID, componentRef.GameObjectID, tempRoot) as T;
        }

        public static List<Ref<TID>> GetOrCreateRefs<TID>(this IIDMap<TID> idmap, IList list, ITypeMap typeMap) where TID : IEquatable<TID>
        {
            if (list == null)
            {
                return null;
            }

            List<Ref<TID>> refList = new List<Ref<TID>>(list.Count);
            foreach (var obj in list)
            {
                var component = obj as Component;
                if (component == null)
                {
                    continue;
                }

                refList.Add(idmap.GetOrCreateRef(component, typeMap));
            }
            return refList;
        }

        public static Ref<TID>[] GetOrCreateRefs<TID>(this IIDMap<TID> idmap, Component[] array, ITypeMap typeMap) where TID : IEquatable<TID>
        {
            if (array == null)
            {
                return null;
            }

            Ref<TID>[] refs = new Ref<TID>[array.Length];
            for (int i = 0; i < array.Length; ++i)
            {
                refs[i] = idmap.GetOrCreateRef(array[i], typeMap);
            }
            return refs;
        }

        public static List<T> GetOrCreateObjects<T, TID>(this IIDMap<TID> idmap, List<Ref<TID>> componentRefs, ITypeMap typeMap, Transform tempRoot = null) where TID : IEquatable<TID> where T : Component
        {
            List<T> objects = null;
            if (componentRefs != null)
            {
                objects = new List<T>(componentRefs.Count);
                for (int i = 0; i < componentRefs.Count; ++i)
                {
                    T obj = GetOrCreateObject<T, TID>(idmap, componentRefs[i], typeMap, tempRoot);
                    objects.Add(obj);
                }
            }
            return objects;
        }

        public static T[] GetOrCreateObjects<T, TID>(this IIDMap<TID> idmap, Ref<TID>[] componentRefs, ITypeMap typeMap, Transform tempRoot = null) where TID : IEquatable<TID> where T : Component
        {
            T[] objects = null;
            if (componentRefs != null)
            {
                objects = new T[componentRefs.Length];
                for (int i = 0; i < componentRefs.Length; ++i)
                {
                    T obj = GetOrCreateObject<T, TID>(idmap, componentRefs[i], typeMap, tempRoot);
                    objects[i] = obj;
                }
            }
            return objects;
        }
    }


    [Serializable]
    public class IDMapCommitException : Exception
    {
        public IDMapCommitException() { }
        public IDMapCommitException(string message) : base(message) { }
        public IDMapCommitException(string message, Exception inner) : base(message, inner) { }
        protected IDMapCommitException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IIDMap<TID> where TID : IEquatable<TID>
    {
        bool IsReadOnly
        {
            get;
            set;
        }

        IIDMap<TID> ParentMap
        {
            get;
            set;
        }

        IReadOnlyDictionary<object, TID> ObjectToID
        {
            get;
        }

        IReadOnlyDictionary<TID, object> IDToObject
        {
            get;
        }

        TID NullID { get; }

        TID CreateID();

        bool TryGetID(object obj, out TID id);

        bool TryGetObject<T>(TID id, out T obj);

        /// <summary>
        /// This method will create id for the object and add it to the map (or return NullID) 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public TID GetOrCreateID(object obj);

        void AddObject(object obj, TID id);
        
        T GetObject<T>(TID id);

        T GetOrCreateObject<T>(TID id) where T : new();

        bool Remove(TID id);

        //Commit to parent
        void Commit();

        //Remove from parent
        void Rollback();

        void Reset();
    }
}
