using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTTerrain
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTTerrain.TerrainBrushSource), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TerrainBrushSourceSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 6;
        const int _TYPE_INDEX = 4104;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        //[ProtoMember(4)]
        public global::System.Collections.Generic.List<TID> UserBrushes { get; set; }

        [ProtoMember(5)]
        public TID[] UserTextures { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTTerrain.TerrainBrushSource)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            UserTextures = idmap.GetOrCreateIDs(o.UserTextures);
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::Battlehub.RTTerrain.TerrainBrushSource, TID>(id, gameObjectId);
            o.UserTextures = idmap.GetObjects<global::UnityEngine.Texture2D, TID>(UserTextures);
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
