using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.TerrainData), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class TerrainDataSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 26;
        const int _TYPE_INDEX = 137;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE


        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::System.Int32 heightmapResolution { get; set; }

        [ProtoMember(4)]
        public global::System.Boolean enableHolesTextureCompression { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector3 size { get; set; }

        [ProtoMember(6)]
        public global::System.Single wavingGrassStrength { get; set; }

        [ProtoMember(7)]
        public global::System.Single wavingGrassAmount { get; set; }

        [ProtoMember(8)]
        public global::System.Single wavingGrassSpeed { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.Color wavingGrassTint { get; set; }

        [ProtoMember(10)]
        public TID[] detailPrototypes { get; set; }

        [ProtoMember(11)]
        public global::Battlehub.Storage.SerializableArray<global::UnityEngine.TreeInstance> treeInstances { get; set; }

        [ProtoMember(12)]
        public global::Battlehub.Storage.SerializableArray<TreePrototypeSurrogate<TID>> treePrototypes { get; set; }

        [ProtoMember(13)]
        public global::System.Int32 alphamapResolution { get; set; }

        [ProtoMember(14)]
        public global::System.Int32 baseMapResolution { get; set; }

        [ProtoMember(15)]
        public TID[] terrainLayers { get; set; }

        [ProtoMember(16)]
        public global::System.String name { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        [ProtoMember(18)] //protobuf-net cannot serialize multidimensional arrays
        public float[] heightsArray { get; set; }

        [ProtoMember(19)]
        public int detailResolution { get; set; }

        [ProtoMember(20)]
        public int detailResolutionPerPatch { get; set; }

        [ProtoMember(21)]
        public int alphamapWidth { get; set; }

        [ProtoMember(22)]
        public int alphamapHeight { get; set; }

        [ProtoMember(23)]
        public int terrainLayersLength { get; set; }

        [ProtoMember(24)]
        public float[] alphamapsArray { get; set; }

        [ProtoMember(25)]
        public int holesResoultion { get; set; }

        [ProtoMember(26)]
        public bool[] holesArray { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.TerrainData)obj;
            id = idmap.GetOrCreateID(o);
            heightmapResolution = o.heightmapResolution;
            enableHolesTextureCompression = o.enableHolesTextureCompression;
            size = o.size;
            wavingGrassStrength = o.wavingGrassStrength;
            wavingGrassAmount = o.wavingGrassAmount;
            wavingGrassSpeed = o.wavingGrassSpeed;
            wavingGrassTint = o.wavingGrassTint;
            detailPrototypes = idmap.GetOrCreateIDs(o.detailPrototypes);
            treeInstances = o.treeInstances;


            treePrototypes = treePrototypes.Serialize(o.treePrototypes, ctx);
            alphamapResolution = o.alphamapResolution;
            baseMapResolution = o.baseMapResolution;
            terrainLayers = idmap.GetOrCreateIDs(o.terrainLayers);
            name = o.name;
            hideFlags = o.hideFlags;

            heightsArray = new float[heightmapResolution * heightmapResolution];
            Buffer.BlockCopy(o.GetHeights(0, 0, heightmapResolution, heightmapResolution), 0, heightsArray, 0, heightsArray.Length * sizeof(float));

            detailResolution = o.detailResolution;
            detailResolutionPerPatch = o.detailResolutionPerPatch;

            holesResoultion = o.holesResolution;
            bool[,] holes = o.GetHoles(0, 0, holesResoultion, holesResoultion);
            holesArray = new bool[holes.GetLength(0) * holes.GetLength(1)];
            Buffer.BlockCopy(holes, 0, holesArray, 0, holesArray.Length * sizeof(bool));

            alphamapWidth = o.alphamapWidth;
            alphamapHeight = o.alphamapHeight;
            terrainLayersLength = o.terrainLayers.Length;

            float[,,] alphamaps = o.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
            alphamapsArray = new float[alphamaps.GetLength(0) * alphamaps.GetLength(1) * alphamaps.GetLength(2)];
            Buffer.BlockCopy(alphamaps, 0, alphamapsArray, 0, alphamapsArray.Length * sizeof(float));

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetOrCreateObject<global::UnityEngine.TerrainData>(id);
            o.heightmapResolution = heightmapResolution;
            o.enableHolesTextureCompression = enableHolesTextureCompression;
            o.size = size;
            o.SetDetailResolution(detailResolution, detailResolutionPerPatch);
            o.heightmapResolution = heightmapResolution;
            o.wavingGrassStrength = wavingGrassStrength;
            o.wavingGrassAmount = wavingGrassAmount;
            o.wavingGrassSpeed = wavingGrassSpeed;
            o.wavingGrassTint = wavingGrassTint;
            o.detailPrototypes = idmap.GetObjects<global::UnityEngine.DetailPrototype, TID>(detailPrototypes);
            o.treePrototypes = treePrototypes.Deserialize((global::UnityEngine.TreePrototype[])null, ctx);
            o.treeInstances = treeInstances;
      
            o.alphamapResolution = alphamapResolution;
            o.baseMapResolution = baseMapResolution;
            o.terrainLayers = idmap.GetObjects<global::UnityEngine.TerrainLayer, TID>(terrainLayers);
            o.name = name;
            o.hideFlags = hideFlags;

            if (heightsArray != null)
            {
                var heights = new float[heightmapResolution, heightmapResolution];
                Buffer.BlockCopy(heightsArray, 0, heights, 0, heightsArray.Length * sizeof(float));
                o.SetHeights(0, 0, heights);
            }

            if (holesArray != null)
            {
                bool[,] holes = new bool[holesResoultion, holesResoultion];
                Buffer.BlockCopy(holesArray, 0, holes, 0, holesArray.Length * sizeof(bool));
                o.SetHoles(0, 0, holes);
            }

            if (alphamapsArray != null)
            {
                float[,,] alphamaps = new float[alphamapWidth, alphamapHeight, terrainLayersLength];
                Buffer.BlockCopy(alphamapsArray, 0, alphamaps, 0, alphamapsArray.Length * sizeof(float));
                o.SetAlphamaps(0, 0, alphamaps);
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
