using Battlehub.RTEditor;
using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTEditor
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTEditor.RuntimeAnimationClip), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class RuntimeAnimationClipSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 4;
        const int _TYPE_INDEX = 4099;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        [ProtoMember(4)]
        public TID[] Properties { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTEditor.RuntimeAnimationClip)obj;
            id = idmap.GetOrCreateID(o);
            hideFlags = o.hideFlags;
            var properties = o.Properties;
            if (properties != null)
            {
                int index = 0;
                Properties = new TID[properties.Count];
                foreach (var property in properties)
                {
                    Properties[index] = idmap.GetOrCreateID(property);
                    index++;
                }
            }

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::Battlehub.RTEditor.RuntimeAnimationClip>(id);
            
            o.hideFlags = hideFlags;
            o.Clear();

            if (Properties != null)
            {
                for (int i = 0; i < Properties.Length; ++i)
                {
                    var property = idmap.GetObject<RuntimeAnimationProperty>(Properties[i]);
                    if (property != null)
                    {
                        o.Add(property);
                    }
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
