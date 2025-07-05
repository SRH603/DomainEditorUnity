using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.RectTransform), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates: false)]
    public class RectTransformSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {
        const int _PROPERTY_INDEX = 27;
        const int _TYPE_INDEX = 152;

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


        [ProtoMember(20)]
        public Vector2 anchorMin
        {
            get;
            set;
        }

        [ProtoMember(21)]
        public Vector2 anchorMax
        {
            get;
            set;
        }

        [ProtoMember(22)]
        public Vector2 anchoredPosition
        {
            get;
            set;
        }

        [ProtoMember(23)]
        public Vector2 sizeDelta
        {
            get;
            set;
        }

        [ProtoMember(24)]
        public Vector2 pivot
        {
            get;
            set;
        }

        [ProtoMember(25)]
        public Vector3 anchoredPosition3D
        {
            get;
            set;
        }

        [ProtoMember(26)]
        public Vector2 offsetMin
        {
            get;
            set;
        }

        [ProtoMember(27)]
        public Vector2 offsetMax
        {
            get;
            set;
        }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var transform = (global::UnityEngine.RectTransform)obj;
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

            Tag = transform.tag;

            anchorMin = transform.anchorMin;
            anchorMax = transform.anchorMax;
            anchoredPosition = transform.anchoredPosition;
            sizeDelta = transform.sizeDelta;
            pivot = transform.pivot;
            anchoredPosition3D = transform.anchoredPosition3D;
            offsetMin = transform.offsetMin;
            offsetMax = transform.offsetMax;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Transform tempRoot = (Transform)ctx.TempRoot;
            var transform = idmap.GetTransform<global::UnityEngine.RectTransform, TID>(ID, GameObjectID, tempRoot);

            if (!EqualityComparer<TID>.Default.Equals(ParentID, idmap.NullID))
            {
                Transform parentTransform = idmap.GetComponent(ParentID, ParentGameObjectID, tempRoot);
                transform.SetParent(parentTransform);
            }


            transform.name = Name;
            transform.localPosition = LocalPosition;
            transform.localRotation = LocalRotation;
            transform.localScale = LocalScale;

            if (!string.IsNullOrEmpty(Tag))
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

            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.anchoredPosition = anchoredPosition;
            transform.sizeDelta = sizeDelta;
            transform.pivot = pivot;
            transform.anchoredPosition3D = anchoredPosition3D;
            transform.offsetMin = offsetMin;
            transform.offsetMax = offsetMax;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            transform.gameObject.SetActive(ActiveSelf);

            return new ValueTask<object>(transform);
        }
    }
}
