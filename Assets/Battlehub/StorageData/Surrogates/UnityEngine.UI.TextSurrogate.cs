using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.Text), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TextSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 24;
        const int _TYPE_INDEX = 4117;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID font { get; set; }

        [ProtoMember(5)]
        public global::System.String text { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean supportRichText { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean resizeTextForBestFit { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 resizeTextMinSize { get; set; }

        [ProtoMember(9)]
        public global::System.Int32 resizeTextMaxSize { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.TextAnchor alignment { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean alignByGeometry { get; set; }

        [ProtoMember(12)]
        public global::System.Int32 fontSize { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.HorizontalWrapMode horizontalOverflow { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.VerticalWrapMode verticalOverflow { get; set; }

        [ProtoMember(15)]
        public global::System.Single lineSpacing { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.FontStyle fontStyle { get; set; }

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
        public TID material { get; set; }

        [ProtoMember(24)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.Text)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            font = idmap.GetOrCreateID(o.font);
            text = o.text;
            supportRichText = o.supportRichText;
            resizeTextForBestFit = o.resizeTextForBestFit;
            resizeTextMinSize = o.resizeTextMinSize;
            resizeTextMaxSize = o.resizeTextMaxSize;
            alignment = o.alignment;
            alignByGeometry = o.alignByGeometry;
            fontSize = o.fontSize;
            horizontalOverflow = o.horizontalOverflow;
            verticalOverflow = o.verticalOverflow;
            lineSpacing = o.lineSpacing;
            fontStyle = o.fontStyle;
            // onCullStateChanged = idmap.GetOrCreateID(o.onCullStateChanged);
            maskable = o.maskable;
            isMaskingGraphic = o.isMaskingGraphic;
            color = o.color;
            raycastTarget = o.raycastTarget;
            raycastPadding = o.raycastPadding;
            material = idmap.GetOrCreateID(o.material);
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.UI.Text, TID>(id, gameObjectId);
            o.font = idmap.GetObject<global::UnityEngine.Font>(font);
            o.text = text;
            o.supportRichText = supportRichText;
            o.resizeTextForBestFit = resizeTextForBestFit;
            o.resizeTextMinSize = resizeTextMinSize;
            o.resizeTextMaxSize = resizeTextMaxSize;
            o.alignment = alignment;
            o.alignByGeometry = alignByGeometry;
            o.fontSize = fontSize;
            o.horizontalOverflow = horizontalOverflow;
            o.verticalOverflow = verticalOverflow;
            o.lineSpacing = lineSpacing;
            o.fontStyle = fontStyle;
            // o.onCullStateChanged = idmap.GetObject<global::UnityEngine.UI.MaskableGraphic.CullStateChangedEvent>(onCullStateChanged);
            o.maskable = maskable;
            o.isMaskingGraphic = isMaskingGraphic;
            o.color = color;
            o.raycastTarget = raycastTarget;
            o.raycastPadding = raycastPadding;
            o.material = idmap.GetObject<global::UnityEngine.Material>(material);
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
