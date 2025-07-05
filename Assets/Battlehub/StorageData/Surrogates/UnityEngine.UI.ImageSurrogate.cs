using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Image), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class ImageSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 23;
        const int _TYPE_INDEX = 4110;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID sprite { get; set; }

        [ProtoMember(5)]
        public TID overrideSprite { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.UI.Image.Type type { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean preserveAspect { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean fillCenter { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.UI.Image.FillMethod fillMethod { get; set; }

        [ProtoMember(10)]
        public global::System.Single fillAmount { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean fillClockwise { get; set; }

        [ProtoMember(12)]
        public global::System.Int32 fillOrigin { get; set; }

        //[ProtoMember(13)]
        public global::System.Single alphaHitTestMinimumThreshold { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean useSpriteMesh { get; set; }

        [ProtoMember(15)]
        public global::System.Single pixelsPerUnitMultiplier { get; set; }

        [ProtoMember(16)]
        public TID material { get; set; }

        //[ProtoMember(17)]
        public TID onCullStateChanged { get; set; }

        [ProtoMember(18)]
        public global::System.Boolean maskable { get; set; }

        [ProtoMember(19)]
        public global::System.Boolean isMaskingGraphic { get; set; }

        [ProtoMember(20)]
        public global::UnityEngine.Color color { get; set; }

        [ProtoMember(21)]
        public global::System.Boolean raycastTarget { get; set; }

        [ProtoMember(22)]
        public global::UnityEngine.Vector4 raycastPadding { get; set; }

        [ProtoMember(23)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.Image)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            sprite = idmap.GetOrCreateID(o.sprite);
            overrideSprite = idmap.GetOrCreateID(o.overrideSprite);
            type = o.type;
            preserveAspect = o.preserveAspect;
            fillCenter = o.fillCenter;
            fillMethod = o.fillMethod;
            fillAmount = o.fillAmount;
            fillClockwise = o.fillClockwise;
            fillOrigin = o.fillOrigin;
            //alphaHitTestMinimumThreshold = o.alphaHitTestMinimumThreshold;
            useSpriteMesh = o.useSpriteMesh;
            pixelsPerUnitMultiplier = o.pixelsPerUnitMultiplier;
            material = idmap.GetOrCreateID(o.material);
            //onCullStateChanged = idmap.GetOrCreateID(o.onCullStateChanged);
            maskable = o.maskable;
            isMaskingGraphic = o.isMaskingGraphic;
            color = o.color;
            raycastTarget = o.raycastTarget;
            raycastPadding = o.raycastPadding;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.Image, TID>(id, gameObjectId);
            o.sprite = idmap.GetObject<global::UnityEngine.Sprite>(sprite);
            o.overrideSprite = idmap.GetObject<global::UnityEngine.Sprite>(overrideSprite);
            o.type = type;
            o.preserveAspect = preserveAspect;
            o.fillCenter = fillCenter;
            o.fillMethod = fillMethod;
            o.fillAmount = fillAmount;
            o.fillClockwise = fillClockwise;
            o.fillOrigin = fillOrigin;
            //o.alphaHitTestMinimumThreshold = alphaHitTestMinimumThreshold;
            o.useSpriteMesh = useSpriteMesh;
            o.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            o.material = idmap.GetObject<global::UnityEngine.Material>(material);
            //o.onCullStateChanged = idmap.GetObject<global::UnityEngine.UI.MaskableGraphic.CullStateChangedEvent>(onCullStateChanged);
            o.maskable = maskable;
            o.isMaskingGraphic = isMaskingGraphic;
            o.color = color;
            o.raycastTarget = raycastTarget;
            o.raycastPadding = raycastPadding;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
