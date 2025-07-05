using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.DetailPrototype), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class DetailPrototypeSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 14;
        const int _TYPE_INDEX = 118;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID prototype { get; set; }

        [ProtoMember(4)]
        public TID prototypeTexture { get; set; }

        [ProtoMember(5)]
        public global::System.Single minWidth { get; set; }

        [ProtoMember(6)]
        public global::System.Single maxWidth { get; set; }

        [ProtoMember(7)]
        public global::System.Single minHeight { get; set; }

        [ProtoMember(8)]
        public global::System.Single maxHeight { get; set; }

        [ProtoMember(9)]
        public global::System.Single noiseSpread { get; set; }

        [ProtoMember(10)]
        public global::System.Single holeEdgePadding { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.Color healthyColor { get; set; }

        [ProtoMember(12)]
        public global::UnityEngine.Color dryColor { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.DetailRenderMode renderMode { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean usePrototypeMesh { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.DetailPrototype)obj;
            id = idmap.GetOrCreateID(o);
            prototype = idmap.GetOrCreateID(o.prototype);
            prototypeTexture = idmap.GetOrCreateID(o.prototypeTexture);
            minWidth = o.minWidth;
            maxWidth = o.maxWidth;
            minHeight = o.minHeight;
            maxHeight = o.maxHeight;
            noiseSpread = o.noiseSpread;
            holeEdgePadding = o.holeEdgePadding;
            healthyColor = o.healthyColor;
            dryColor = o.dryColor;
            renderMode = o.renderMode;
            usePrototypeMesh = o.usePrototypeMesh;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.DetailPrototype>(id);
            o.prototype = idmap.GetObject<global::UnityEngine.GameObject>(prototype);
            o.prototypeTexture = idmap.GetObject<global::UnityEngine.Texture2D>(prototypeTexture);
            o.minWidth = minWidth;
            o.maxWidth = maxWidth;
            o.minHeight = minHeight;
            o.maxHeight = maxHeight;
            o.noiseSpread = noiseSpread;
            o.holeEdgePadding = holeEdgePadding;
            o.healthyColor = healthyColor;
            o.dryColor = dryColor;
            o.renderMode = renderMode;
            o.usePrototypeMesh = usePrototypeMesh;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
