using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Sprite), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class SpriteSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 9;
        const int _TYPE_INDEX = 189;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.String name { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        [ProtoMember(5)]
        public TID texture { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Rect rect { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector2 pivot { get; set; }

        [ProtoMember(8)]
        public float pixelsPerUnit { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.Vector4 border { get; set; }


        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Sprite)obj;
            id = idmap.GetOrCreateID(o);
            name = o.name;
            hideFlags = o.hideFlags;
            texture = idmap.GetOrCreateID(o.texture);
            rect = o.rect;
            pivot = o.pivot;
            pixelsPerUnit = o.pixelsPerUnit;
            border = o.border;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            global::UnityEngine.Sprite o = idmap.GetObject<global::UnityEngine.Sprite>(id); 
            if (o == null)
            {
                var spriteTexture = idmap.GetObject<global::UnityEngine.Texture2D>(texture);
               
                var p = pivot;
                p.x = p.x / rect.width;
                p.y = p.y / rect.height;

                o = global::UnityEngine.Sprite.Create(spriteTexture, rect, p, pixelsPerUnit, 0, global::UnityEngine.SpriteMeshType.FullRect, border);
                if (o == null)
                {
                    return default;
                }

                idmap.AddObject(o, id);
            }

            o.name = name;
            o.hideFlags = hideFlags;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
