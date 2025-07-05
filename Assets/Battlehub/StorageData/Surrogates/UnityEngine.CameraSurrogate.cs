using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Camera), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class CameraSurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 48;
        const int _TYPE_INDEX = 109;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::System.Single nearClipPlane { get; set; }

        [ProtoMember(5)]
        public global::System.Single farClipPlane { get; set; }

        [ProtoMember(6)]
        public global::System.Single fieldOfView { get; set; }

        [ProtoMember(7)]
        public global::UnityEngine.RenderingPath renderingPath { get; set; }

        [ProtoMember(8)]
        public global::System.Boolean allowHDR { get; set; }

        [ProtoMember(9)]
        public global::System.Boolean allowMSAA { get; set; }

        [ProtoMember(10)]
        public global::System.Boolean allowDynamicResolution { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean forceIntoRenderTexture { get; set; }

        [ProtoMember(12)]
        public global::System.Single orthographicSize { get; set; }

        [ProtoMember(13)]
        public global::System.Boolean orthographic { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.Rendering.OpaqueSortMode opaqueSortMode { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.TransparencySortMode transparencySortMode { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.Vector3 transparencySortAxis { get; set; }

        [ProtoMember(17)]
        public global::System.Single depth { get; set; }

        //[ProtoMember(18)]
        public global::System.Single aspect { get; set; }

        [ProtoMember(19)]
        public global::System.Int32 cullingMask { get; set; }

        [ProtoMember(20)]
        public global::System.Int32 eventMask { get; set; }

        [ProtoMember(21)]
        public global::System.Boolean layerCullSpherical { get; set; }

        [ProtoMember(22)]
        public global::UnityEngine.CameraType cameraType { get; set; }

        //[ProtoMember(23)]
        public global::System.UInt64 overrideSceneCullingMask { get; set; }

        [ProtoMember(24)]
        public SerializableArray<global::System.Single> layerCullDistances { get; set; }

        [ProtoMember(25)]
        public global::System.Boolean useOcclusionCulling { get; set; }

        //[ProtoMember(26)]
        public global::UnityEngine.Matrix4x4 cullingMatrix { get; set; }

        [ProtoMember(27)]
        public global::UnityEngine.Color backgroundColor { get; set; }

        [ProtoMember(28)]
        public global::UnityEngine.CameraClearFlags clearFlags { get; set; }

        [ProtoMember(29)]
        public global::UnityEngine.DepthTextureMode depthTextureMode { get; set; }

        [ProtoMember(30)]
        public global::System.Boolean clearStencilAfterLightingPass { get; set; }

        [ProtoMember(31)]
        public global::System.Boolean usePhysicalProperties { get; set; }

        [ProtoMember(32)]
        public global::UnityEngine.Vector2 sensorSize { get; set; }

        [ProtoMember(33)]
        public global::UnityEngine.Vector2 lensShift { get; set; }

        [ProtoMember(34)]
        public global::System.Single focalLength { get; set; }

        [ProtoMember(35)]
        public global::UnityEngine.Camera.GateFitMode gateFit { get; set; }

        //[ProtoMember(36)]
        public global::UnityEngine.Rect rect { get; set; }

        //[ProtoMember(37)]
        public global::UnityEngine.Rect pixelRect { get; set; }

        [ProtoMember(38)]
        public TID targetTexture { get; set; }

        [ProtoMember(39)]
        public global::System.Int32 targetDisplay { get; set; }

        //[ProtoMember(40)]
        public global::UnityEngine.Matrix4x4 worldToCameraMatrix { get; set; }

        //[ProtoMember(41)]
        public global::UnityEngine.Matrix4x4 projectionMatrix { get; set; }

        //[ProtoMember(42)]
        public global::UnityEngine.Matrix4x4 nonJitteredProjectionMatrix { get; set; }

        //[ProtoMember(43)]
        public global::System.Boolean useJitteredProjectionMatrixForTransparentRendering { get; set; }

        //[ProtoMember(44)]
        // public global::UnityEngine.SceneManagement.Scene scene { get; set; }

        [ProtoMember(45)]
        public global::System.Single stereoSeparation { get; set; }

        [ProtoMember(46)]
        public global::System.Single stereoConvergence { get; set; }

        [ProtoMember(47)]
        public global::UnityEngine.StereoTargetEyeMask stereoTargetEye { get; set; }

        [ProtoMember(48)]
        public global::System.Boolean enabled { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Camera)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            nearClipPlane = o.nearClipPlane;
            farClipPlane = o.farClipPlane;
            fieldOfView = o.fieldOfView;
            renderingPath = o.renderingPath;
            allowHDR = o.allowHDR;
            allowMSAA = o.allowMSAA;
            allowDynamicResolution = o.allowDynamicResolution;
            forceIntoRenderTexture = o.forceIntoRenderTexture;
            orthographicSize = o.orthographicSize;
            orthographic = o.orthographic;
            opaqueSortMode = o.opaqueSortMode;
            transparencySortMode = o.transparencySortMode;
            transparencySortAxis = o.transparencySortAxis;
            depth = o.depth;
            //aspect = o.aspect;
            cullingMask = o.cullingMask;
            eventMask = o.eventMask;
            //layerCullSpherical = o.layerCullSpherical;
            cameraType = o.cameraType;
            //overrideSceneCullingMask = o.overrideSceneCullingMask;
            layerCullDistances = o.layerCullDistances;
            useOcclusionCulling = o.useOcclusionCulling;
            //cullingMatrix = o.cullingMatrix;
            backgroundColor = o.backgroundColor;
            clearFlags = o.clearFlags;
            depthTextureMode = o.depthTextureMode;
            clearStencilAfterLightingPass = o.clearStencilAfterLightingPass;
            usePhysicalProperties = o.usePhysicalProperties;
            sensorSize = o.sensorSize;
            lensShift = o.lensShift;
            focalLength = o.focalLength;
            gateFit = o.gateFit;
            //rect = o.rect;
            //pixelRect = o.pixelRect;
            targetTexture = idmap.GetOrCreateID(o.targetTexture);
            targetDisplay = o.targetDisplay;
            //worldToCameraMatrix = o.worldToCameraMatrix;
            //projectionMatrix = o.projectionMatrix;
            //nonJitteredProjectionMatrix = o.nonJitteredProjectionMatrix;
            //useJitteredProjectionMatrixForTransparentRendering = o.useJitteredProjectionMatrixForTransparentRendering;
            //scene = o.scene;
            stereoSeparation = o.stereoSeparation;
            stereoConvergence = o.stereoConvergence;
            //stereoTargetEye = o.stereoTargetEye;
            enabled = o.enabled;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.Camera, TID>(id, gameObjectId);
            o.nearClipPlane = nearClipPlane;
            o.farClipPlane = farClipPlane;
            o.fieldOfView = fieldOfView;
            o.renderingPath = renderingPath;
            o.allowHDR = allowHDR;
            o.allowMSAA = allowMSAA;
            o.allowDynamicResolution = allowDynamicResolution;
            o.forceIntoRenderTexture = forceIntoRenderTexture;
            o.orthographicSize = orthographicSize;
            o.orthographic = orthographic;
            o.opaqueSortMode = opaqueSortMode;
            o.transparencySortMode = transparencySortMode;
            o.transparencySortAxis = transparencySortAxis;
            //o.depth = depth; //don't restore depth, breaks GameViewCamera
            //o.aspect = aspect;
            o.cullingMask = cullingMask;
            o.eventMask = eventMask;
            //o.layerCullSpherical = layerCullSpherical;
            o.cameraType = cameraType;
            //o.overrideSceneCullingMask = overrideSceneCullingMask;
            o.layerCullDistances = layerCullDistances;
            o.useOcclusionCulling = useOcclusionCulling;
            //o.cullingMatrix = cullingMatrix;
            o.backgroundColor = backgroundColor;
            o.clearFlags = clearFlags;
            o.depthTextureMode = depthTextureMode;
            o.clearStencilAfterLightingPass = clearStencilAfterLightingPass;
            o.usePhysicalProperties = usePhysicalProperties;
            o.sensorSize = sensorSize;
            o.lensShift = lensShift;
            o.focalLength = focalLength;
            o.gateFit = gateFit;
            //o.rect = rect;
            //o.pixelRect = pixelRect;
            o.targetTexture = idmap.GetObject<global::UnityEngine.RenderTexture>(targetTexture);
            o.targetDisplay = targetDisplay;
            //o.worldToCameraMatrix = worldToCameraMatrix;
            //o.projectionMatrix = projectionMatrix;
            //o.nonJitteredProjectionMatrix = nonJitteredProjectionMatrix;
            //o.useJitteredProjectionMatrixForTransparentRendering = useJitteredProjectionMatrixForTransparentRendering;
            //o.scene = scene;
            o.stereoSeparation = stereoSeparation;
            o.stereoConvergence = stereoConvergence;
            //o.stereoTargetEye = stereoTargetEye;
            o.enabled = enabled;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
