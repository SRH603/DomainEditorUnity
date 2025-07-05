using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTCommon
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTCommon.ExposeToEditor), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class ExposeToEditorSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 4096;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID BoundsObject { get; set; }

        [ProtoMember(5)]
        public global::Battlehub.RTCommon.BoundsType BoundsType { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Bounds CustomBounds { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean AddColliders { get; set; }

        [ProtoMember(9)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTCommon.ExposeToEditor)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            BoundsObject = idmap.GetOrCreateID(o.BoundsObject);
            BoundsType = o.BoundsType;
            CustomBounds = o.CustomBounds;
            AddColliders = o.AddColliders;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::Battlehub.RTCommon.ExposeToEditor, TID>(id, gameObjectId);
            o.BoundsObject = idmap.GetObject<global::UnityEngine.GameObject>(BoundsObject);
            o.BoundsType = BoundsType;
            o.CustomBounds = CustomBounds;
            o.AddColliders = AddColliders;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
