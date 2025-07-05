using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Canvas), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class CanvasSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 18;
        const int _TYPE_INDEX = 150;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.RenderMode renderMode { get; set; }

        [ProtoMember(5)]
        public global::System.Single scaleFactor { get; set; }

        [ProtoMember(6)]
        public global::System.Single referencePixelsPerUnit { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean overridePixelPerfect { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean pixelPerfect { get; set; }

        [ProtoMember(9)]
        public global::System.Single planeDistance { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean overrideSorting { get; set; }

        [ProtoMember(11)]
        public global::System.Int32 sortingOrder { get; set; }

        [ProtoMember(12)]
        public global::System.Int32 targetDisplay { get; set; }

        [ProtoMember(13)]
        public global::System.Int32 sortingLayerID { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.AdditionalCanvasShaderChannels additionalShaderChannels { get; set; }

        [ProtoMember(15)]
        public global::System.String sortingLayerName { get; set; }

        [ProtoMember(16)]
        public TID worldCamera { get; set; }

        [ProtoMember(17)]
        public global::System.Single normalizedSortingGridSize { get; set; }

        [ProtoMember(18)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Canvas)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            renderMode = o.renderMode;
            scaleFactor = o.scaleFactor;
            referencePixelsPerUnit = o.referencePixelsPerUnit;
            overridePixelPerfect = o.overridePixelPerfect;
            pixelPerfect = o.pixelPerfect;
            planeDistance = o.planeDistance;
            overrideSorting = o.overrideSorting;
            sortingOrder = o.sortingOrder;
            targetDisplay = o.targetDisplay;
            sortingLayerID = o.sortingLayerID;
            additionalShaderChannels = o.additionalShaderChannels;
            sortingLayerName = o.sortingLayerName;
            worldCamera = idmap.GetOrCreateID(o.worldCamera);
            normalizedSortingGridSize = o.normalizedSortingGridSize;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.Canvas, TID>(id, gameObjectId);
            o.renderMode = renderMode;
            o.scaleFactor = scaleFactor;
            o.referencePixelsPerUnit = referencePixelsPerUnit;
            o.overridePixelPerfect = overridePixelPerfect;
            o.pixelPerfect = pixelPerfect;
            o.planeDistance = planeDistance;
            o.overrideSorting = overrideSorting;
            o.sortingOrder = sortingOrder;
            o.targetDisplay = targetDisplay;
            o.sortingLayerID = sortingLayerID;
            o.additionalShaderChannels = additionalShaderChannels;
            o.sortingLayerName = sortingLayerName;
            o.worldCamera = idmap.GetObject<global::UnityEngine.Camera>(worldCamera);
            o.normalizedSortingGridSize = normalizedSortingGridSize;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
