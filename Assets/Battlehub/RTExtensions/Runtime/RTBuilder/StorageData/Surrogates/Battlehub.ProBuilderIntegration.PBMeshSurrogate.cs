using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.ProBuilderIntegration
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.ProBuilderIntegration.PBMesh), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class PBMeshSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 7;
        const int _TYPE_INDEX = 4108;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::Battlehub.Storage.SerializableArray<global::Battlehub.Storage.Surrogates.Battlehub.ProBuilderIntegration.PBFaceSurrogate<TID>> Faces { get; set; }

        [ProtoMember(5)]
        public global::Battlehub.Storage.SerializableArray<global::UnityEngine.Vector3> Positions { get; set; }

        [ProtoMember(6)]
        public global::Battlehub.Storage.SerializableArray<global::UnityEngine.Vector2> Textures { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.ProBuilderIntegration.PBMesh)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            Faces = Faces.Serialize(o.Faces, ctx);
            Positions = o.Positions;
            Textures = o.Textures;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::Battlehub.ProBuilderIntegration.PBMesh, TID>(id, gameObjectId);
            o.Faces = Faces.Deserialize((global::Battlehub.ProBuilderIntegration.PBFace[])null, ctx);
            o.Positions = Positions;
            o.Textures = Textures;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
