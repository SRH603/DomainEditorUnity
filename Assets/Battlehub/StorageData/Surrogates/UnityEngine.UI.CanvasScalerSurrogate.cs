using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.CanvasScaler), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class CanvasScalerSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 14;
        const int _TYPE_INDEX = 4145;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.UI.CanvasScaler.ScaleMode uiScaleMode { get; set; }

        [ProtoMember(5)]
        public global::System.Single referencePixelsPerUnit { get; set; }

        [ProtoMember(6)]
        public global::System.Single scaleFactor { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector2 referenceResolution { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.UI.CanvasScaler.ScreenMatchMode screenMatchMode { get; set; }

        [ProtoMember(9)]
        public global::System.Single matchWidthOrHeight { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.UI.CanvasScaler.Unit physicalUnit { get; set; }

        [ProtoMember(11)]
        public global::System.Single fallbackScreenDPI { get; set; }

        [ProtoMember(12)]
        public global::System.Single defaultSpriteDPI { get; set; }

        [ProtoMember(13)]
        public global::System.Single dynamicPixelsPerUnit { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.CanvasScaler)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            uiScaleMode = o.uiScaleMode;
            referencePixelsPerUnit = o.referencePixelsPerUnit;
            scaleFactor = o.scaleFactor;
            referenceResolution = o.referenceResolution;
            screenMatchMode = o.screenMatchMode;
            matchWidthOrHeight = o.matchWidthOrHeight;
            physicalUnit = o.physicalUnit;
            fallbackScreenDPI = o.fallbackScreenDPI;
            defaultSpriteDPI = o.defaultSpriteDPI;
            dynamicPixelsPerUnit = o.dynamicPixelsPerUnit;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.CanvasScaler, TID>(id, gameObjectId);
            o.uiScaleMode = uiScaleMode;
            o.referencePixelsPerUnit = referencePixelsPerUnit;
            o.scaleFactor = scaleFactor;
            o.referenceResolution = referenceResolution;
            o.screenMatchMode = screenMatchMode;
            o.matchWidthOrHeight = matchWidthOrHeight;
            o.physicalUnit = physicalUnit;
            o.fallbackScreenDPI = fallbackScreenDPI;
            o.defaultSpriteDPI = defaultSpriteDPI;
            o.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
