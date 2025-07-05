using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.GridLayoutGroup), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class GridLayoutGroupSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 12;
        const int _TYPE_INDEX = 4116;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.UI.GridLayoutGroup.Corner startCorner { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.UI.GridLayoutGroup.Axis startAxis { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector2 cellSize { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector2 spacing { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.UI.GridLayoutGroup.Constraint constraint { get; set; }

        [ProtoMember(9)]
        public global::System.Int32 constraintCount { get; set; }

        [ProtoMember(10)]
        public TID padding { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.TextAnchor childAlignment { get; set; }

        [ProtoMember(12)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.GridLayoutGroup)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            startCorner = o.startCorner;
            startAxis = o.startAxis;
            cellSize = o.cellSize;
            spacing = o.spacing;
            constraint = o.constraint;
            constraintCount = o.constraintCount;
            padding = idmap.GetOrCreateID(o.padding);
            childAlignment = o.childAlignment;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.GridLayoutGroup, TID>(id, gameObjectId);
            o.startCorner = startCorner;
            o.startAxis = startAxis;
            o.cellSize = cellSize;
            o.spacing = spacing;
            o.constraint = constraint;
            o.constraintCount = constraintCount;
            o.padding = idmap.GetObject<global::UnityEngine.RectOffset>(padding);
            o.childAlignment = childAlignment;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
