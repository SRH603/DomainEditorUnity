using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine.UI
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.UI.ColorBlock), _PROPERTY_INDEX, _TYPE_INDEX, enabled:false)]
    public struct ColorBlockSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 8;
        const int _TYPE_INDEX = 4126;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public global::UnityEngine.Color normalColor { get; set; }

        [ProtoMember(3)]
        public global::UnityEngine.Color highlightedColor { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Color pressedColor { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Color selectedColor { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Color disabledColor { get; set; }

        [ProtoMember(7)]
        public global::System.Single colorMultiplier { get; set; }

        [ProtoMember(8)]
        public global::System.Single fadeDuration { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.UI.ColorBlock)obj;
            normalColor = o.normalColor;
            highlightedColor = o.highlightedColor;
            pressedColor = o.pressedColor;
            selectedColor = o.selectedColor;
            disabledColor = o.disabledColor;
            colorMultiplier = o.colorMultiplier;
            fadeDuration = o.fadeDuration;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = new global::UnityEngine.UI.ColorBlock();
            o.normalColor = normalColor;
            o.highlightedColor = highlightedColor;
            o.pressedColor = pressedColor;
            o.selectedColor = selectedColor;
            o.disabledColor = disabledColor;
            o.colorMultiplier = colorMultiplier;
            o.fadeDuration = fadeDuration;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
