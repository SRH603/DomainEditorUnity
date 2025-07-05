using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.TMPro
{
    [ProtoContract]
    [Surrogate(typeof(global::TMPro.TextMeshProUGUI), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class TextMeshProUGUISurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 34;
        const int _TYPE_INDEX = 4147;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Boolean autoSizeTextContainer { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector4 maskOffset { get; set; }

        [ProtoMember(6)]
        public global::System.String text { get; set; }

        [ProtoMember(7)]
        public global::System.Boolean isRightToLeftText { get; set; }

        [ProtoMember(8)]
        public TID font { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.Color color { get; set; }

        [ProtoMember(10)]
        public global::System.Single alpha { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.Color32 faceColor { get; set; }

        [ProtoMember(12)]
        public global::UnityEngine.Color32 outlineColor { get; set; }

        [ProtoMember(13)]
        public global::System.Single outlineWidth { get; set; }

        [ProtoMember(14)]
        public global::System.Single fontSize { get; set; }

        [ProtoMember(15)]
        public global::TMPro.FontWeight fontWeight { get; set; }

        [ProtoMember(16)]
        public global::System.Boolean enableAutoSizing { get; set; }

        [ProtoMember(17)]
        public global::System.Single fontSizeMin { get; set; }

        [ProtoMember(18)]
        public global::System.Single fontSizeMax { get; set; }

        [ProtoMember(19)]
        public global::TMPro.FontStyles fontStyle { get; set; }

        [ProtoMember(20)]
        public global::TMPro.HorizontalAlignmentOptions horizontalAlignment { get; set; }

        [ProtoMember(21)]
        public global::TMPro.VerticalAlignmentOptions verticalAlignment { get; set; }

        [ProtoMember(22)]
        public global::TMPro.TextAlignmentOptions alignment { get; set; }

        [ProtoMember(23)]
        public global::System.Single characterSpacing { get; set; }

        [ProtoMember(24)]
        public global::System.Single wordSpacing { get; set; }

        [ProtoMember(25)]
        public global::System.Single lineSpacing { get; set; }
        [ProtoMember(26)]
        public global::System.Single paragraphSpacing { get; set; }

        [ProtoMember(27)]
        public global::System.Boolean enableWordWrapping { get; set; }

        [ProtoMember(28)]
        public global::TMPro.TextOverflowModes overflowMode { get; set; }

        [ProtoMember(29)]
        public global::System.Boolean extraPadding { get; set; }

        [ProtoMember(30)]
        public global::System.Boolean richText { get; set; }

        [ProtoMember(31)]
        public global::System.Boolean parseCtrlCharacters { get; set; }

        [ProtoMember(32)]
        public global::System.Boolean isOverlay { get; set; }

        [ProtoMember(33)]
        public global::UnityEngine.Vector4 margin { get; set; }

        [ProtoMember(34)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::TMPro.TextMeshProUGUI)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            autoSizeTextContainer = o.autoSizeTextContainer;
            maskOffset = o.maskOffset;
            text = o.text;
            isRightToLeftText = o.isRightToLeftText;
            font = idmap.GetOrCreateID(o.font);
            color = o.color;
            alpha = o.alpha;
            faceColor = o.faceColor;
            outlineColor = o.outlineColor;
            outlineWidth = o.outlineWidth;
            fontSize = o.fontSize;
            fontWeight = o.fontWeight;
            enableAutoSizing = o.enableAutoSizing;
            fontSizeMin = o.fontSizeMin;
            fontSizeMax = o.fontSizeMax;
            fontStyle = o.fontStyle;
            horizontalAlignment = o.horizontalAlignment;
            verticalAlignment = o.verticalAlignment;
            alignment = o.alignment;
            characterSpacing = o.characterSpacing;
            wordSpacing = o.wordSpacing;
            lineSpacing = o.lineSpacing;
            paragraphSpacing = o.paragraphSpacing;
#if UNITY_6000_0_OR_NEWER
            enableWordWrapping = o.textWrappingMode != global::TMPro.TextWrappingModes.NoWrap;
#else
            enableWordWrapping = o.enableWordWrapping;
#endif
            overflowMode = o.overflowMode;
            extraPadding = o.extraPadding;
            richText = o.richText;
            parseCtrlCharacters = o.parseCtrlCharacters;
            isOverlay = o.isOverlay;
            margin = o.margin;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::TMPro.TextMeshProUGUI, TID>(id, gameObjectId);
            o.autoSizeTextContainer = autoSizeTextContainer;
            o.maskOffset = maskOffset;
            o.text = text;
            o.isRightToLeftText = isRightToLeftText;
            o.font = idmap.GetObject<global::TMPro.TMP_FontAsset>(font);
            o.color = color;
            o.alpha = alpha;
            o.faceColor = faceColor;
            o.outlineColor = outlineColor;
            o.outlineWidth = outlineWidth;
            o.fontSize = fontSize;
            o.fontWeight = fontWeight;
            o.enableAutoSizing = enableAutoSizing;
            o.fontSizeMin = fontSizeMin;
            o.fontSizeMax = fontSizeMax;
            o.fontStyle = fontStyle;
            o.horizontalAlignment = horizontalAlignment;
            o.verticalAlignment = verticalAlignment;
            o.alignment = alignment;
            o.characterSpacing = characterSpacing;
            o.wordSpacing = wordSpacing;
            o.lineSpacing = lineSpacing;
            o.paragraphSpacing = paragraphSpacing;
#if UNITY_6000_0_OR_NEWER
            o.textWrappingMode = enableWordWrapping ? 
                global::TMPro.TextWrappingModes.Normal : 
                global::TMPro.TextWrappingModes.NoWrap;
#else
            o.enableWordWrapping = enableWordWrapping;
#endif
            o.overflowMode = overflowMode;
            o.extraPadding = extraPadding;
            o.richText = richText;
            o.parseCtrlCharacters = parseCtrlCharacters;
            o.isOverlay = isOverlay;
            o.margin = margin;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
