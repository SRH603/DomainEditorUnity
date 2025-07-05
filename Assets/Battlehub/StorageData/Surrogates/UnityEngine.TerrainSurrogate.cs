using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Terrain), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class TerrainSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 35;
        const int _TYPE_INDEX = 140;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public TID terrainData { get; set; }

        [ProtoMember(5)]
        public global::System.Single treeDistance { get; set; }

        [ProtoMember(6)]
        public global::System.Single treeBillboardDistance { get; set; }

        [ProtoMember(7)]
        public global::System.Single treeCrossFadeLength { get; set; }

        [ProtoMember(8)]
        public global::System.Int32 treeMaximumFullLODCount { get; set; }

        [ProtoMember(9)]
        public global::System.Single detailObjectDistance { get; set; }

        [ProtoMember(10)]
        public global::System.Single detailObjectDensity { get; set; }

        [ProtoMember(11)]
        public global::System.Single heightmapPixelError { get; set; }

        [ProtoMember(12)]
        public global::System.Int32 heightmapMaximumLOD { get; set; }

        [ProtoMember(13)]
        public global::System.Single basemapDistance { get; set; }

        [ProtoMember(14)]
        public global::System.Int32 lightmapIndex { get; set; }

        [ProtoMember(15)]
        public global::System.Int32 realtimeLightmapIndex { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.Vector4 lightmapScaleOffset { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.Vector4 realtimeLightmapScaleOffset { get; set; }

        [ProtoMember(19)]
        public global::UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; set; }

        [ProtoMember(20)]
        public global::UnityEngine.Rendering.ReflectionProbeUsage reflectionProbeUsage { get; set; }

        [ProtoMember(21)]
        public TID materialTemplate { get; set; }

        [ProtoMember(22)]
        public global::System.Boolean drawHeightmap { get; set; }

        [ProtoMember(23)]
        public global::System.Boolean allowAutoConnect { get; set; }

        [ProtoMember(24)]
        public global::System.Int32 groupingID { get; set; }

        [ProtoMember(25)]
        public global::System.Boolean drawInstanced { get; set; }

        [ProtoMember(26)]
        public global::System.Boolean drawTreesAndFoliage { get; set; }

        [ProtoMember(27)]
        public global::UnityEngine.Vector3 patchBoundsMultiplier { get; set; }

        [ProtoMember(28)]
        public global::System.Single treeLODBiasMultiplier { get; set; }

        [ProtoMember(29)]
        public global::System.Boolean collectDetailPatches { get; set; }

        [ProtoMember(30)]
        public global::UnityEngine.TerrainRenderFlags editorRenderFlags { get; set; }

        //[ProtoMember(31)]
        public global::System.Boolean bakeLightProbesForTrees { get; set; }

        //[ProtoMember(32)]
        public global::System.Boolean deringLightProbesForTrees { get; set; }

        [ProtoMember(33)]
        public global::System.Boolean preserveTreePrototypeLayers { get; set; }

        [ProtoMember(34)]
        public global::System.UInt32 renderingLayerMask { get; set; }

        [ProtoMember(35)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Terrain)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            terrainData = idmap.GetOrCreateID(o.terrainData);
            treeDistance = o.treeDistance;
            treeBillboardDistance = o.treeBillboardDistance;
            treeCrossFadeLength = o.treeCrossFadeLength;
            treeMaximumFullLODCount = o.treeMaximumFullLODCount;
            detailObjectDistance = o.detailObjectDistance;
            detailObjectDensity = o.detailObjectDensity;
            heightmapPixelError = o.heightmapPixelError;
            heightmapMaximumLOD = o.heightmapMaximumLOD;
            basemapDistance = o.basemapDistance;
            lightmapIndex = o.lightmapIndex;
            realtimeLightmapIndex = o.realtimeLightmapIndex;
            lightmapScaleOffset = o.lightmapScaleOffset;
            realtimeLightmapScaleOffset = o.realtimeLightmapScaleOffset;
            shadowCastingMode = o.shadowCastingMode;
            reflectionProbeUsage = o.reflectionProbeUsage;
            materialTemplate = idmap.GetOrCreateID(o.materialTemplate);
            drawHeightmap = o.drawHeightmap;
            allowAutoConnect = o.allowAutoConnect;
            groupingID = o.groupingID;
            drawInstanced = o.drawInstanced;
            drawTreesAndFoliage = o.drawTreesAndFoliage;
            patchBoundsMultiplier = o.patchBoundsMultiplier;
            treeLODBiasMultiplier = o.treeLODBiasMultiplier;
            collectDetailPatches = o.collectDetailPatches;
            editorRenderFlags = o.editorRenderFlags;
            preserveTreePrototypeLayers = o.preserveTreePrototypeLayers;
            renderingLayerMask = o.renderingLayerMask;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.Terrain, TID>(id, gameObjectId);
            o.terrainData = idmap.GetObject<global::UnityEngine.TerrainData>(terrainData);
            o.treeDistance = treeDistance;
            o.treeBillboardDistance = treeBillboardDistance;
            o.treeCrossFadeLength = treeCrossFadeLength;
            o.treeMaximumFullLODCount = treeMaximumFullLODCount;
            o.detailObjectDistance = detailObjectDistance;
            o.detailObjectDensity = detailObjectDensity;
            o.heightmapPixelError = heightmapPixelError;
            o.heightmapMaximumLOD = heightmapMaximumLOD;
            o.basemapDistance = basemapDistance;
            o.lightmapIndex = lightmapIndex;
            o.realtimeLightmapIndex = realtimeLightmapIndex;
            o.lightmapScaleOffset = lightmapScaleOffset;
            o.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
            o.shadowCastingMode = shadowCastingMode;
            o.reflectionProbeUsage = reflectionProbeUsage;
            o.materialTemplate = idmap.GetObject<global::UnityEngine.Material>(materialTemplate);
            o.drawHeightmap = drawHeightmap;
            o.allowAutoConnect = allowAutoConnect;
            o.groupingID = groupingID;
            o.drawInstanced = drawInstanced;
            o.drawTreesAndFoliage = drawTreesAndFoliage;
            o.patchBoundsMultiplier = patchBoundsMultiplier;
            o.treeLODBiasMultiplier = treeLODBiasMultiplier;
            o.collectDetailPatches = collectDetailPatches;
            o.editorRenderFlags = editorRenderFlags;
            o.preserveTreePrototypeLayers = preserveTreePrototypeLayers;
            o.renderingLayerMask = renderingLayerMask;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
