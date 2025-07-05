using ProtoBuf;
using System;
using System.Threading.Tasks;


namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.SkinnedMeshRenderer), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class SkinnedMeshRendererSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 35;
        const int _TYPE_INDEX = 136;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.SkinQuality quality { get; set; }

        [ProtoMember(5)]
        public global::System.Boolean updateWhenOffscreen { get; set; }

        [ProtoMember(6)]
        public global::System.Boolean forceMatrixRecalculationPerRender { get; set; }

        [ProtoMember(7)]
        public TID rootBone { get; set; }

        [ProtoMember(8)]
        public TID[] bones { get; set; }

        [ProtoMember(9)]
        public TID sharedMesh { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean skinnedMotionVectors { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.Bounds localBounds { get; set; }

        [ProtoMember(12)]
        public global::System.Boolean enabled { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; set; }

        [ProtoMember(14)]
        public global::System.Boolean receiveShadows { get; set; }

        [ProtoMember(15)]
        public global::System.Boolean forceRenderingOff { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.MotionVectorGenerationMode motionVectorGenerationMode { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.Rendering.LightProbeUsage lightProbeUsage { get; set; }

        [ProtoMember(18)]
        public global::UnityEngine.Rendering.ReflectionProbeUsage reflectionProbeUsage { get; set; }

        [ProtoMember(19)]
        public global::System.UInt32 renderingLayerMask { get; set; }

        [ProtoMember(20)]
        public global::System.Int32 rendererPriority { get; set; }

        //[ProtoMember(21)]
        public global::UnityEngine.Experimental.Rendering.RayTracingMode rayTracingMode { get; set; }

        [ProtoMember(22)]
        public global::System.String sortingLayerName { get; set; }

        [ProtoMember(23)]
        public global::System.Int32 sortingLayerID { get; set; }

        [ProtoMember(24)]
        public global::System.Int32 sortingOrder { get; set; }

        [ProtoMember(25)]
        public global::System.Boolean allowOcclusionWhenDynamic { get; set; }

        [ProtoMember(26)]
        public TID lightProbeProxyVolumeOverride { get; set; }

        [ProtoMember(27)]
        public TID probeAnchor { get; set; }

        [ProtoMember(28)]
        public global::System.Int32 lightmapIndex { get; set; }

        [ProtoMember(29)]
        public global::System.Int32 realtimeLightmapIndex { get; set; }

        [ProtoMember(30)]
        public global::UnityEngine.Vector4 lightmapScaleOffset { get; set; }

        [ProtoMember(31)]
        public global::UnityEngine.Vector4 realtimeLightmapScaleOffset { get; set; }

        //[ProtoMember(32)]
        public TID[] materials { get; set; }

        //[ProtoMember(33)]
        public TID material { get; set; }

        //[ProtoMember(34)]
        public TID sharedMaterial { get; set; }

        [ProtoMember(35)]
        public TID[] sharedMaterials { get; set; }

        [ProtoMember(36)]
        public float[] blendShapeWeights;

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.SkinnedMeshRenderer)obj;

            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            quality = o.quality;
            updateWhenOffscreen = o.updateWhenOffscreen;
            forceMatrixRecalculationPerRender = o.forceMatrixRecalculationPerRender;
            rootBone = idmap.GetOrCreateID(o.rootBone);
            bones = idmap.GetOrCreateIDs(o.bones);
            sharedMesh = idmap.GetOrCreateID(o.sharedMesh);
            skinnedMotionVectors = o.skinnedMotionVectors;
            localBounds = o.localBounds;
            enabled = o.enabled;
            shadowCastingMode = o.shadowCastingMode;
            receiveShadows = o.receiveShadows;
            forceRenderingOff = o.forceRenderingOff;
            motionVectorGenerationMode = o.motionVectorGenerationMode;
            lightProbeUsage = o.lightProbeUsage;
            reflectionProbeUsage = o.reflectionProbeUsage;
            renderingLayerMask = o.renderingLayerMask;
            rendererPriority = o.rendererPriority;
            //rayTracingMode = o.rayTracingMode;
            sortingLayerName = o.sortingLayerName;
            sortingLayerID = o.sortingLayerID;
            sortingOrder = o.sortingOrder;
            allowOcclusionWhenDynamic = o.allowOcclusionWhenDynamic;
            lightProbeProxyVolumeOverride = idmap.GetOrCreateID(o.lightProbeProxyVolumeOverride);
            probeAnchor = idmap.GetOrCreateID(o.probeAnchor);
            lightmapIndex = o.lightmapIndex;
            realtimeLightmapIndex = o.realtimeLightmapIndex;
            lightmapScaleOffset = o.lightmapScaleOffset;
            realtimeLightmapScaleOffset = o.realtimeLightmapScaleOffset;
            //materials = idmap.GetOrCreateIDs(o.materials);
            //material = idmap.GetOrCreateID(o.material);
            //sharedMaterial = idmap.GetOrCreateID(o.sharedMaterial);
            sharedMaterials = idmap.GetOrCreateIDs(o.sharedMaterials);

            if (o.sharedMesh != null && o.sharedMesh.blendShapeCount > 0)
            {
                blendShapeWeights = new float[o.sharedMesh.blendShapeCount];

                for (int i = 0; i < blendShapeWeights.Length; i++)
                {
                    blendShapeWeights[i] = o.GetBlendShapeWeight(i);
                }
            }

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.SkinnedMeshRenderer, TID>(id, gameObjectId);
            o.quality = quality;
            o.updateWhenOffscreen = updateWhenOffscreen;
            o.forceMatrixRecalculationPerRender = forceMatrixRecalculationPerRender;
            o.rootBone = idmap.GetObject<global::UnityEngine.Transform>(rootBone);
            o.bones = idmap.GetObjects<global::UnityEngine.Transform, TID>(bones);
            o.sharedMesh = idmap.GetObject<global::UnityEngine.Mesh>(sharedMesh);
            o.skinnedMotionVectors = skinnedMotionVectors;
            o.localBounds = localBounds;
            o.enabled = enabled;
            o.shadowCastingMode = shadowCastingMode;
            o.receiveShadows = receiveShadows;
            o.forceRenderingOff = forceRenderingOff;
            o.motionVectorGenerationMode = motionVectorGenerationMode;
            o.lightProbeUsage = lightProbeUsage;
            o.reflectionProbeUsage = reflectionProbeUsage;
            o.renderingLayerMask = renderingLayerMask;
            o.rendererPriority = rendererPriority;
            //o.rayTracingMode = rayTracingMode;
            o.sortingLayerName = sortingLayerName;
            o.sortingLayerID = sortingLayerID;
            o.sortingOrder = sortingOrder;
            o.allowOcclusionWhenDynamic = allowOcclusionWhenDynamic;
            o.lightProbeProxyVolumeOverride = idmap.GetObject<global::UnityEngine.GameObject>(lightProbeProxyVolumeOverride);
            o.probeAnchor = idmap.GetObject<global::UnityEngine.Transform>(probeAnchor);
            o.lightmapIndex = lightmapIndex;
            o.realtimeLightmapIndex = realtimeLightmapIndex;
            o.lightmapScaleOffset = lightmapScaleOffset;
            o.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
            //o.materials = idmap.GetObjects<global::UnityEngine.Material, TID>(materials);
            //o.material = idmap.GetObject<global::UnityEngine.Material>(material);
            //o.sharedMaterial = idmap.GetObject<global::UnityEngine.Material>(sharedMaterial);
            o.sharedMaterials = idmap.GetObjects<global::UnityEngine.Material, TID>(sharedMaterials);

            if (blendShapeWeights != null && o.sharedMesh != null && o.sharedMesh.blendShapeCount > 0)
            {
                int count = global::UnityEngine.Mathf.Min(o.sharedMesh.blendShapeCount, blendShapeWeights.Length);

                for (int i = 0; i < count; i++)
                {
                    o.SetBlendShapeWeight(i, blendShapeWeights[i]);
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
