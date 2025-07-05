using ProtoBuf;
using System;

namespace Battlehub.Storage.Surrogates.Battlehub.ProBuilderIntegration
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.ProBuilderIntegration.PBFace), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public struct PBFaceSurrogate<TID> : IValueTypeSurrogate<global::Battlehub.ProBuilderIntegration.PBFace, TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4106;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public global::Battlehub.Storage.SerializableArray<global::System.Int32> Indexes { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 SubmeshIndex { get; set; }

        [ProtoMember(4)]
        public global::System.Int32 TextureGroup { get; set; }

        [ProtoMember(5)]
        public global::System.Int32 SmoothingGroup { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean IsManualUV { get; set; }

        [ProtoMember(8)]
        public PBAutoUnwrapSettingsSurrogate<TID> UnwrapSettings { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public void Serialize(in global::Battlehub.ProBuilderIntegration.PBFace o, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            Indexes = o.Indexes;
            SubmeshIndex = o.SubmeshIndex;
            TextureGroup = o.TextureGroup;
            SmoothingGroup = o.SmoothingGroup;
            IsManualUV = o.IsManualUV;
            UnwrapSettings = new PBAutoUnwrapSettingsSurrogate<TID>();
            UnwrapSettings.Serialize(o.UnwrapSettings, ctx);
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE
        }

        public global::Battlehub.ProBuilderIntegration.PBFace Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::Battlehub.ProBuilderIntegration.PBFace();
            o.Indexes = Indexes;
            o.SubmeshIndex = SubmeshIndex;
            o.TextureGroup = TextureGroup;
            o.SmoothingGroup = SmoothingGroup;
            o.IsManualUV = IsManualUV;
            if (UnwrapSettings != null)
            {
                o.UnwrapSettings = UnwrapSettings.Deserialize(ctx);
            }
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return o;
        }
    }
}
