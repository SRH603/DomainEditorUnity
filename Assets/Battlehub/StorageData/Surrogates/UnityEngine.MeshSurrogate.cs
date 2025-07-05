using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    public struct MeshIndexGroup
    {
        [ProtoMember(1)]
       // [MessagePackFormatter(typeof(MessagePack.Unity.Extension.IntArrayBlitFormatter))]
        public int[] Indices
        {
            get;
            set;
        }

    }

    [ProtoContract]
    public struct BlendShapeFrame
    {
        [ProtoMember(1)]
        public float weight;

        [ProtoMember(2)]
        public Vector3[] deltaVertices;

        [ProtoMember(3)]
        public Vector3[] deltaNormals;

        [ProtoMember(4)]
        public Vector3[] deltaTangents;

        public BlendShapeFrame(float weight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            this.weight = weight;
            this.deltaVertices = deltaVertices;
            this.deltaNormals = deltaNormals;
            this.deltaTangents = deltaTangents;
        }
    }
    
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Mesh), _PROPERTY_INDEX, _TYPE_INDEX, enableUpdates:false)]
    public class MeshSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 25;
        const int _TYPE_INDEX = 104;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public global::UnityEngine.Rendering.IndexFormat indexFormat { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Matrix4x4[] bindposes { get; set; }

        [ProtoMember(5)]
        public global::System.Int32 subMeshCount { get; set; }

        [ProtoMember(6)]
        public global::UnityEngine.Bounds bounds { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.Vector3[] vertices { get; set; }

        [ProtoMember(8)]
        public global::UnityEngine.Vector3[] normals { get; set; }

        [ProtoMember(9)]
        public global::UnityEngine.Vector4[] tangents { get; set; }

        [ProtoMember(10)]
        public global::UnityEngine.Vector2[] uv { get; set; }

        [ProtoMember(11)]
        public global::UnityEngine.Vector2[] uv2 { get; set; }

        [ProtoMember(12)]
        public global::UnityEngine.Vector2[] uv3 { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.Vector2[] uv4 { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.Vector2[] uv5 { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.Vector2[] uv6 { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.Vector2[] uv7 { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.Vector2[] uv8 { get; set; }

        [ProtoMember(18)]
        public global::UnityEngine.Color[] colors { get; set; }

        [ProtoMember(19)]
        public global::UnityEngine.Color32[] colors32 { get; set; }

        //[ProtoMember(20)]
        public global::System.Int32[] triangles { get; set; }

        [ProtoMember(21)]
        public global::UnityEngine.BoneWeight[] boneWeights { get; set; }

        [ProtoMember(22)]
        public global::System.String name { get; set; }

        [ProtoMember(23)]
        public global::UnityEngine.HideFlags hideFlags { get; set; }

        [ProtoMember(24)]
        public MeshIndexGroup[] indices { get; set; }

        [ProtoMember(25)]
        public MeshTopology[] topology { get; set; }

        [ProtoMember(26)]
        public int blendShapeCount;

        [ProtoMember(27)]
        public string[] blendShapeNames;

        [ProtoMember(28)]
        public int[] blendShapeFrameCount;

        [ProtoMember(29)]
        public List<BlendShapeFrame> blendShapeFrames;

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Mesh)obj;

            id = idmap.GetOrCreateID(o);
            indexFormat = o.indexFormat;
            subMeshCount = o.subMeshCount;
            bindposes = o.bindposes;
            bounds = o.bounds;
            vertices = o.vertices;
            normals = o.normals;
            tangents = o.tangents;
            uv = o.uv;
            uv2 = o.uv2;
            uv3 = o.uv3;
            uv4 = o.uv4;
            uv5 = o.uv5;
            uv6 = o.uv6;
            uv7 = o.uv7;
            uv8 = o.uv8;
            colors = o.colors;
            colors32 = o.colors32;
            boneWeights = o.boneWeights;
            name = o.name;
            hideFlags = o.hideFlags;
            topology = new MeshTopology[subMeshCount];
            indices = new MeshIndexGroup[subMeshCount];
            for (int i = 0; i < subMeshCount; ++i)
            {
                topology[i] = o.GetTopology(i);
                switch (topology[i])
                {
                    case MeshTopology.Points:
                    case MeshTopology.Lines:
                        indices[i] = new MeshIndexGroup
                        {
                            Indices = o.GetIndices(i),
                        };
                        break;
                    case MeshTopology.Triangles:
                        indices[i] = new MeshIndexGroup
                        {
                            Indices = o.GetTriangles(i),
                        };
                        break;
                }
            }

            blendShapeCount = o.blendShapeCount;
            if (blendShapeCount > 0)
            {
                blendShapeNames = new string[blendShapeCount];
                blendShapeFrameCount = new int[blendShapeCount];
                blendShapeFrames = new List<BlendShapeFrame>();
                int vertexCount = o.vertexCount;
                for (int shapeIndex = 0; shapeIndex < blendShapeCount; ++shapeIndex)
                {
                    int frameCount = o.GetBlendShapeFrameCount(shapeIndex);
                    blendShapeFrameCount[shapeIndex] = frameCount;

                    string blendShapeName = o.GetBlendShapeName(shapeIndex);
                    blendShapeNames[shapeIndex] = blendShapeName;

                    for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                    {
                        float weight = o.GetBlendShapeFrameWeight(shapeIndex, frameIndex);

                        Vector3[] deltaVertices = new Vector3[vertexCount];
                        Vector3[] deltaNormals = new Vector3[vertexCount];
                        Vector3[] deltaTangents = new Vector3[vertexCount];
                        o.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                        BlendShapeFrame frame = new BlendShapeFrame(weight, deltaVertices, deltaNormals, deltaTangents);
                        blendShapeFrames.Add(frame);
                    }
                }
            }

            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;
            var o = idmap.GetOrCreateObject<global::UnityEngine.Mesh>(id);
            o.vertices = vertices;
            o.indexFormat = indexFormat;
            o.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; ++i)
            {
                switch (topology[i])
                {
                    case MeshTopology.Points:
                    case MeshTopology.Lines:
                        o.SetIndices(indices[i].Indices, topology[i], i);
                        break;
                    case MeshTopology.Triangles:
                        o.SetTriangles(indices[i].Indices, i);
                        break;
                }
            }
            o.bindposes = bindposes;
            o.bounds = bounds;
            o.normals = normals;
            o.tangents = tangents;
            o.uv = uv;
            o.uv2 = uv2;
            o.uv3 = uv3;
            o.uv4 = uv4;
            o.uv5 = uv5;
            o.uv6 = uv6;
            o.uv7 = uv7;
            o.uv8 = uv8;
            o.colors = colors;
            o.colors32 = colors32;
            //o.triangles = triangles;
            o.boneWeights = boneWeights;
            o.name = name;
            o.hideFlags = hideFlags;

            if (blendShapeCount > 0)
            {
                o.ClearBlendShapes();

                int index = 0;
                for (int shapeIndex = 0; shapeIndex < blendShapeCount; ++shapeIndex)
                {
                    int frameCount = blendShapeFrameCount[shapeIndex];
                    string blendShapeName = blendShapeNames[shapeIndex];
                    for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                    {
                        BlendShapeFrame frame = blendShapeFrames[index];
                        o.AddBlendShapeFrame(blendShapeName, frame.weight, frame.deltaVertices, frame.deltaNormals, frame.deltaTangents);
                        index++;
                    }
                }
            }

            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
