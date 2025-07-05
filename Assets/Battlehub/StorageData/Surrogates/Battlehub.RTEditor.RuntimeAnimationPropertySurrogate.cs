using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.Battlehub.RTEditor
{
    [ProtoContract]
    [Surrogate(typeof(global::Battlehub.RTEditor.RuntimeAnimationProperty), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class RuntimeAnimationPropertySurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 10;
        const int _TYPE_INDEX = 4100;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        //[ProtoMember(3)]
        public TID Parent { get; set; }

        [ProtoMember(4)]
        public global::System.Collections.Generic.List<TID> Children { get; set; }

        [ProtoMember(5)]
        public TID Curve { get; set; }

        [ProtoMember(6)]
        public string ComponentTypeName { get; set; }

        [ProtoMember(7)]
        public string ComponentDisplayName { get; set; }

        [ProtoMember(8)]
        public string PropertyName { get; set; }

        [ProtoMember(9)]
        public string PropertyDisplayName { get; set; }

        [ProtoMember(10)]
        public string AnimationPropertyName { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::Battlehub.RTEditor.RuntimeAnimationProperty)obj;
            id = idmap.GetOrCreateID(o);
            Children = idmap.GetOrCreateIDs(o.Children);
            Curve = idmap.GetOrCreateID(o.Curve);
            ComponentTypeName = o.ComponentTypeName;
            ComponentDisplayName = o.ComponentDisplayName;
            PropertyName = o.PropertyName;
            PropertyDisplayName = o.PropertyDisplayName;
            AnimationPropertyName = o.AnimationPropertyName;

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::Battlehub.RTEditor.RuntimeAnimationProperty>(id);
            o.Children = idmap.GetObjects<global::Battlehub.RTEditor.RuntimeAnimationProperty, TID>(Children);
            if (o.Children != null) 
            {
                for (int i = 0; i < o.Children.Count; ++i)
                {
                    var child = o.Children[i];
                    if (child != null)
                    {
                        child.Parent = o;
                    }
                }
            }
            
            o.Curve = idmap.GetObject<global::UnityEngine.AnimationCurve>(Curve);
            o.ComponentTypeName = ComponentTypeName;
            o.ComponentDisplayName = ComponentDisplayName;
            o.PropertyName = PropertyName;
            o.PropertyDisplayName = PropertyDisplayName;
            o.AnimationPropertyName = AnimationPropertyName;

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
