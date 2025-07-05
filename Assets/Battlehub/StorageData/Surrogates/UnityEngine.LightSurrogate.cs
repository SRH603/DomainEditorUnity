using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Light), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class LightSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 39;
        const int _TYPE_INDEX = 122;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.LightType type { get; set; }

#if !UNITY_6000_0_OR_NEWER
        [ProtoMember(5)]
        public global::UnityEngine.LightShape shape { get; set; }
#endif

        [ProtoMember(6)]
        public global::System.Single spotAngle { get; set; }

        [ProtoMember(7)]
        public global::System.Single innerSpotAngle { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.Color color { get; set; }

        [ProtoMember(9)]
        public global::System.Single colorTemperature { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean useColorTemperature { get; set; }

        [ProtoMember(11)]
        public global::System.Single intensity { get; set; }

        [ProtoMember(12)]
        public global::System.Single bounceIntensity { get; set; }

        [ProtoMember(13)]
        public global::System.Boolean useBoundingSphereOverride { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.Vector4 boundingSphereOverride { get; set; }

        [ProtoMember(15)]
        public global::System.Boolean useViewFrustumForShadowCasterCull { get; set; }

        [ProtoMember(16)]
        public global::System.Int32 shadowCustomResolution { get; set; }

        [ProtoMember(17)]
        public global::System.Single shadowBias { get; set; }

        [ProtoMember(18)]
        public global::System.Single shadowNormalBias { get; set; }

        [ProtoMember(19)]
        public global::System.Single shadowNearPlane { get; set; }

        [ProtoMember(20)]
        public global::System.Boolean useShadowMatrixOverride { get; set; }

        [ProtoMember(21)]
        public global::UnityEngine.Matrix4x4 shadowMatrixOverride { get; set; }

        [ProtoMember(22)]
        public global::System.Single range { get; set; }

        [ProtoMember(23)]
        public TID flare { get; set; }

        [ProtoMember(24)]
        public global::UnityEngine.LightBakingOutput bakingOutput { get; set; }

        [ProtoMember(25)]
        public global::System.Int32 cullingMask { get; set; }

        [ProtoMember(26)]
        public global::System.Int32 renderingLayerMask { get; set; }

        [ProtoMember(27)]
        public global::UnityEngine.LightShadowCasterMode lightShadowCasterMode { get; set; }

        //[ProtoMember(28)]
        public global::System.Single shadowRadius { get; set; }

        //[ProtoMember(29)]
        public global::System.Single shadowAngle { get; set; }

        [ProtoMember(30)]
        public global::UnityEngine.LightShadows shadows { get; set; }

        [ProtoMember(31)]
        public global::System.Single shadowStrength { get; set; }

        [ProtoMember(32)]
        public global::UnityEngine.Rendering.LightShadowResolution shadowResolution { get; set; }

        [ProtoMember(33)]
        public global::Battlehub.Storage.SerializableArray<global::System.Single> layerShadowCullDistances { get; set; }

        [ProtoMember(34)]
        public global::System.Single cookieSize { get; set; }

        [ProtoMember(35)]
        public TID cookie { get; set; }

        [ProtoMember(36)]
        public global::UnityEngine.LightRenderMode renderMode { get; set; }

        //[ProtoMember(37)]
        public global::UnityEngine.Vector2 areaSize { get; set; }

        //[ProtoMember(38)]
        public global::UnityEngine.LightmapBakeType lightmapBakeType { get; set; }

        [ProtoMember(39)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Light)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            type = o.type;
#if !UNITY_6000_0_OR_NEWER
            shape = o.shape;
#endif
            spotAngle = o.spotAngle;
            innerSpotAngle = o.innerSpotAngle;
            color = o.color;
            colorTemperature = o.colorTemperature;
            useColorTemperature = o.useColorTemperature;
            intensity = o.intensity;
            bounceIntensity = o.bounceIntensity;
            useBoundingSphereOverride = o.useBoundingSphereOverride;
            boundingSphereOverride = o.boundingSphereOverride;
            useViewFrustumForShadowCasterCull = o.useViewFrustumForShadowCasterCull;
            shadowCustomResolution = o.shadowCustomResolution;
            shadowBias = o.shadowBias;
            shadowNormalBias = o.shadowNormalBias;
            shadowNearPlane = o.shadowNearPlane;
            useShadowMatrixOverride = o.useShadowMatrixOverride;
            shadowMatrixOverride = o.shadowMatrixOverride;
            range = o.range;
            flare = idmap.GetOrCreateID(o.flare);
            bakingOutput = o.bakingOutput;
            cullingMask = o.cullingMask;
            renderingLayerMask = o.renderingLayerMask;
            lightShadowCasterMode = o.lightShadowCasterMode;
            //shadowRadius = o.shadowRadius;
            //shadowAngle = o.shadowAngle;
            shadows = o.shadows;
            shadowStrength = o.shadowStrength;
            shadowResolution = o.shadowResolution;
            layerShadowCullDistances = o.layerShadowCullDistances;
            cookieSize = o.cookieSize;
            cookie = idmap.GetOrCreateID(o.cookie);
            renderMode = o.renderMode;
            //areaSize = o.areaSize;
            //lightmapBakeType = o.lightmapBakeType;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.Light, TID>(id, gameObjectId);
            o.type = type;
#if !UNITY_6000_0_OR_NEWER
            o.shape = shape;
#endif
            o.spotAngle = spotAngle;
            o.innerSpotAngle = innerSpotAngle;
            o.color = color;
            o.colorTemperature = colorTemperature;
            o.useColorTemperature = useColorTemperature;
            o.intensity = intensity;
            o.bounceIntensity = bounceIntensity;
            o.useBoundingSphereOverride = useBoundingSphereOverride;
            o.boundingSphereOverride = boundingSphereOverride;
            o.useViewFrustumForShadowCasterCull = useViewFrustumForShadowCasterCull;
            o.shadowCustomResolution = shadowCustomResolution;
            o.shadowBias = shadowBias;
            o.shadowNormalBias = shadowNormalBias;
            o.shadowNearPlane = shadowNearPlane;
            o.useShadowMatrixOverride = useShadowMatrixOverride;
            o.shadowMatrixOverride = shadowMatrixOverride;
            o.range = range;
            o.flare = idmap.GetObject<global::UnityEngine.Flare>(flare);
            o.bakingOutput = bakingOutput;
            o.cullingMask = cullingMask;
            o.renderingLayerMask = renderingLayerMask;
            o.lightShadowCasterMode = lightShadowCasterMode;
            //o.shadowRadius = shadowRadius;
            //o.shadowAngle = shadowAngle;
            o.shadows = shadows;
            o.shadowStrength = shadowStrength;
            o.shadowResolution = shadowResolution;
            o.layerShadowCullDistances = layerShadowCullDistances;
            o.cookieSize = cookieSize;
            o.cookie = idmap.GetObject<global::UnityEngine.Texture>(cookie);
            o.renderMode = renderMode;
            //o.areaSize = areaSize;
            //o.lightmapBakeType = lightmapBakeType;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
