using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.ProBuilderIntegration
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class PBAutoUnwrapSettingsSurrogate<TID> : ISurrogate<TID>, IValueTypeSurrogate<global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings, TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 11;
        const int _TYPE_INDEX = 4105;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings.Anchor anchor { get; set; }

        [ProtoMember(4)]
        public global::System.Single rotation { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector2 offset { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Vector2 scale { get; set; }

        [ProtoMember(7)]
        public global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings.Fill fill { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean swapUV { get; set; }

        [ProtoMember(9)]
        public global::System.Boolean flipV { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean flipU { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean useWorldSpace { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public void Serialize(in global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings value, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = value;
            id = idmap.GetOrCreateID(o);
            anchor = o.anchor;
            rotation = o.rotation;
            offset = o.offset;
            scale = o.scale;
            fill = o.fill;
            swapUV = o.swapUV;
            flipV = o.flipV;
            flipU = o.flipU;
            useWorldSpace = o.useWorldSpace;
        }
        public global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings>(id);
            o.anchor = anchor;
            o.rotation = rotation;
            o.offset = offset;
            o.scale = scale;
            o.fill = fill;
            o.swapUV = swapUV;
            o.flipV = flipV;
            o.flipU = flipU;
            o.useWorldSpace = useWorldSpace;

            return o;
        }

        ValueTask ISurrogate<TID>.Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var value = (global::Battlehub.ProBuilderIntegration.PBAutoUnwrapSettings)obj;
            Serialize(value, ctx);
            return default;
        }

        ValueTask<object> ISurrogate<TID>.Deserialize(ISerializationContext<TID> ctx)
        {
            return new ValueTask<object>(Deserialize(ctx));
        }
    }
}
