using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTTerrain
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTTerrain.TerrainToolState), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TerrainToolStateSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 13;
        const int _TYPE_INDEX = 4103;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::Battlehub.RTTerrain.TerrainGridTool.Interpolation Interpolation { get; set; }

        [ProtoMember(5)]
        public global::System.Single Height { get; set; }

        [ProtoMember(6)]
        public global::System.Single ZSize { get; set; }

        [ProtoMember(7)]
        public global::System.Single XSize { get; set; }

        [ProtoMember(8)]
        public global::System.Single ZSpacing { get; set; }

        [ProtoMember(9)]
        public global::System.Single XSpacing { get; set; }

        [ProtoMember(10)]
        public global::Battlehub.Storage.SerializableArray<global::System.Single> Grid { get; set; }

        [ProtoMember(11)]
        public global::Battlehub.Storage.SerializableArray<global::System.Single> HeightMap { get; set; }

        [ProtoMember(12)]
        public TID CutoutTexture { get; set; }

        [ProtoMember(13)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTTerrain.TerrainToolState)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            Interpolation = o.Interpolation;
            Height = o.Height;
            ZSize = o.ZSize;
            XSize = o.XSize;
            ZSpacing = o.ZSpacing;
            XSpacing = o.XSpacing;
            Grid = o.Grid;
            HeightMap = o.HeightMap;
            CutoutTexture = idmap.GetOrCreateID(o.CutoutTexture);
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::Battlehub.RTTerrain.TerrainToolState, TID>(id, gameObjectId);
            o.Interpolation = Interpolation;
            o.Height = Height;
            o.ZSize = ZSize;
            o.XSize = XSize;
            o.ZSpacing = ZSpacing;
            o.XSpacing = XSpacing;
            o.Grid = Grid;
            o.HeightMap = HeightMap;
            o.CutoutTexture = idmap.GetObject<global::UnityEngine.Texture2D>(CutoutTexture);
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
