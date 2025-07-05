using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(Transform), propertyIndex: _PROPERTY_INDEX, typeIndex: _TYPE_INDEX, enableUpdates: false)]
    public class TransformSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 12;
        const int _TYPE_INDEX = 101;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID ID
        {
            get;
            set;
        }

        [ProtoMember(3)] // compatibility
        public TID[] ChildrenIDs 
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public TID GameObjectID
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public bool ActiveSelf
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public string Name
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public Vector3 LocalPosition
        {
            get;
            set;
        }

        [ProtoMember(8)]
        public Quaternion LocalRotation
        {
            get;
            set;
        }

        [ProtoMember(9)]
        public Vector3 LocalScale
        {
            get;
            set;
        }

        [ProtoMember(10)]
        public TID ParentID
        {
            get;
            set;
        }

        [ProtoMember(11)]
        public TID ParentGameObjectID
        {
            get;
            set;
        }

        [ProtoMember(12)]
        public string Tag
        {
            get;
            set;
        }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Transform transform = (Transform)obj;
            ID = idmap.GetOrCreateID(transform);
            GameObjectID = idmap.GetOrCreateID(transform.gameObject);
            ActiveSelf = transform.gameObject.activeSelf;
            Name = transform.name;
            LocalPosition = transform.localPosition;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;

            if (transform.parent != null && transform.parent != (object)ctx.TempRoot)
            {
                ParentID = idmap.GetOrCreateID(transform.parent);
                ParentGameObjectID = idmap.GetOrCreateID(transform.parent.gameObject);
            }
            else
            {
                ParentID = idmap.NullID;
                ParentGameObjectID = idmap.NullID;
            }

            if (!string.IsNullOrEmpty(transform.tag))
            {
                Tag = transform.tag;
            }
            
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Transform tempRoot = (Transform)ctx.TempRoot;
            Transform transform = idmap.GetComponent(ID, GameObjectID, tempRoot);

            if (!EqualityComparer<TID>.Default.Equals(ParentID, idmap.NullID))
            {
                Transform parentTransform = idmap.GetComponent(ParentID, ParentGameObjectID, tempRoot);
                transform.SetParent(parentTransform);
            }

            transform.name = Name;
            transform.localPosition = LocalPosition;
            transform.localRotation = LocalRotation;
            transform.localScale = LocalScale;

            // Remains here for compatibility with alpha versions
            if (ChildrenIDs != null) //for protobuf.net there is no difference between null and empty array.
            {
                for (int i = 0; i < ChildrenIDs.Length; ++i)
                {
                    Transform child = idmap.GetObject<Transform>(ChildrenIDs[i]);
                    if (child != null)
                    {
                        child.SetParent(transform, false);
                    }
                    else
                    {
                        Debug.Log($"Child transform {ChildrenIDs[i]} not found. Parent Transform {Name} {ID}");
                    }
                }
            }

            if (Tag != null)
            {
                if (Tag != string.Empty)
                {
                    try
                    {
                        transform.tag = Tag;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            transform.gameObject.SetActive(ActiveSelf);

            return new ValueTask<object>(transform);
        }
    }
}
