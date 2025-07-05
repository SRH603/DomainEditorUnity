using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTCommon
{
    public interface IRTECamera
    {
        event Action<IRTECamera> CommandBufferRefresh;

        Camera Camera
        {
            get;
        }

        IRTECommandBuffer RTECommandBuffer
        {
            get;
        }

        IRTECommandBuffer RTECommandBufferOverride
        {
            get;
            set;
        }

        CameraEvent Event
        {
            get;
            set;
        }

        IRenderersCache RenderersCache
        {
            get;
        }

        IMeshesCache MeshesCache
        {
            get;
        }

        void RefreshCommandBuffer();
        void Destroy();
    }

    public interface IRTECommandBuffer
    {
        void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor);

        void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth);

        void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth, uint stencil);

        void ClearRenderTarget(RTClearFlags clearFlags, Color backgroundColor, float depth, uint stencil);

        void ClearRenderTarget(RTClearFlags clearFlags, Color[] backgroundColors, float depth, uint stencil);

        void SetInstanceMultiplier(uint multiplier);

#if UNITY_6000_0_OR_NEWER
        void SetFoveatedRenderingMode(FoveatedRenderingMode foveatedRenderingMode);
#endif
        void SetWireframe(bool enable);

        void ConfigureFoveatedRendering(IntPtr platformData);

        void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties);

        void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass);

        void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex);

        void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material);

        void DrawMultipleMeshes(Matrix4x4[] matrices, Mesh[] meshes, int[] subsetIndices, int count, Material material, int shaderPass, MaterialPropertyBlock properties);

        void DrawRenderer(Renderer renderer, Material material, int submeshIndex, int shaderPass);

        void DrawRenderer(Renderer renderer, Material material, int submeshIndex);

        void DrawRenderer(Renderer renderer, Material material);

#if UNITY_6000_0_OR_NEWER
        void DrawRendererList(RendererList rendererList);
#endif

        void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties);

        void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount);

        void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount);

        void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties);

        void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount);

        void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset);

        void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset);

        void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs);

        void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties);

        void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count);

        void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices);

        void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset);

        void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs);

        void DrawOcclusionMesh(RectInt normalizedCamViewport);

        void Clear();
    }

    public class RTECommandBuffer : IRTECommandBuffer, IDisposable
    {
        private CommandBuffer m_wrappedCommandBuffer;
        public CommandBuffer WrappedCommandBuffer
        {
            get { return m_wrappedCommandBuffer; }
            set { m_wrappedCommandBuffer = value; }
        }

        public string name
        {
            get { return m_wrappedCommandBuffer.name; }
            set { m_wrappedCommandBuffer.name = value; }
        }

        public RTECommandBuffer()
        {
            m_wrappedCommandBuffer = new CommandBuffer();
            m_wrappedCommandBuffer.name = "RTECommandBuffer";
        }

        public RTECommandBuffer(CommandBuffer commandBuffer)
        {
            m_wrappedCommandBuffer = commandBuffer;
        }

        public void Dispose()
        {
            if (m_wrappedCommandBuffer != null)
            {
                m_wrappedCommandBuffer.Dispose();
                m_wrappedCommandBuffer = null;
            }
        }

        public void Clear()
        {
            m_wrappedCommandBuffer.Clear();
        }

        public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor)
        {
            m_wrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor);
        }

        public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth)
        {
            m_wrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor, depth);
        }

        public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth, uint stencil)
        {
#if UNITY_6000_0_OR_NEWER
            m_wrappedCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor, depth, stencil);
#endif
        }

        public void ClearRenderTarget(RTClearFlags clearFlags, Color backgroundColor, float depth, uint stencil)
        {
            m_wrappedCommandBuffer.ClearRenderTarget(clearFlags, backgroundColor, depth, stencil);
        }

        public void ClearRenderTarget(RTClearFlags clearFlags, Color[] backgroundColors, float depth, uint stencil)
        {
#if UNITY_6000_0_OR_NEWER
            m_wrappedCommandBuffer.ClearRenderTarget(clearFlags, backgroundColors, depth, stencil);
#endif
        }

        public void ConfigureFoveatedRendering(IntPtr platformData)
        {
#if UNITY_6000_0_OR_NEWER
            m_wrappedCommandBuffer.ConfigureFoveatedRendering(platformData);
#endif
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass, properties);
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex, int shaderPass)
        {
            m_wrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex, shaderPass);
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int submeshIndex)
        {
            m_wrappedCommandBuffer.DrawMesh(mesh, matrix, material, submeshIndex);
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material)
        {
            m_wrappedCommandBuffer.DrawMesh(mesh, matrix, material);
        }

        public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count, properties);
        }

        public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices, int count)
        {
            m_wrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices, count);
        }

        public void DrawMeshInstanced(Mesh mesh, int submeshIndex, Material material, int shaderPass, Matrix4x4[] matrices)
        {
            m_wrappedCommandBuffer.DrawMeshInstanced(mesh, submeshIndex, material, shaderPass, matrices);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, properties);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, ComputeBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset, properties);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs, argsOffset);
        }

        public void DrawMeshInstancedIndirect(Mesh mesh, int submeshIndex, Material material, int shaderPass, GraphicsBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedIndirect(mesh, submeshIndex, material, shaderPass, bufferWithArgs);
        }

        public void DrawMeshInstancedProcedural(Mesh mesh, int submeshIndex, Material material, int shaderPass, int count, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawMeshInstancedProcedural(mesh, submeshIndex, material, shaderPass, count, properties);
        }

        public void DrawMultipleMeshes(Matrix4x4[] matrices, Mesh[] meshes, int[] subsetIndices, int count, Material material, int shaderPass, MaterialPropertyBlock properties)
        {
#if UNITY_6000_0_OR_NEWER
            m_wrappedCommandBuffer.DrawMultipleMeshes(matrices, meshes, subsetIndices, count, material, shaderPass, properties);
#endif
        }

        public void DrawOcclusionMesh(RectInt normalizedCamViewport)
        {
            m_wrappedCommandBuffer.DrawOcclusionMesh(normalizedCamViewport);
        }

        public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount, properties);
        }

        public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount, int instanceCount)
        {
            m_wrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount, instanceCount);
        }

        public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount)
        {
            m_wrappedCommandBuffer.DrawProcedural(matrix, material, shaderPass, topology, vertexCount);
        }

        public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount, properties);
        }

        public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount, int instanceCount)
        {
            m_wrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount, instanceCount);
        }

        public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount)
        {
            m_wrappedCommandBuffer.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, ComputeBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
        }

        public void DrawProceduralIndirect(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(matrix, material, shaderPass, topology, bufferWithArgs);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset, MaterialPropertyBlock properties)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset, properties);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs, int argsOffset)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs, argsOffset);
        }

        public void DrawProceduralIndirect(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, GraphicsBuffer bufferWithArgs)
        {
            m_wrappedCommandBuffer.DrawProceduralIndirect(indexBuffer, matrix, material, shaderPass, topology, bufferWithArgs);
        }

        public void DrawRenderer(Renderer renderer, Material material, int submeshIndex, int shaderPass)
        {
            m_wrappedCommandBuffer.DrawRenderer(renderer, material, submeshIndex, shaderPass);
        }

        public void DrawRenderer(Renderer renderer, Material material, int submeshIndex)
        {
            m_wrappedCommandBuffer.DrawRenderer(renderer, material, submeshIndex);
        }

        public void DrawRenderer(Renderer renderer, Material material)
        {
            m_wrappedCommandBuffer.DrawRenderer(renderer, material);
        }

#if UNITY_6000_0_OR_NEWER
        public void DrawRendererList(RendererList rendererList)
        {
            m_wrappedCommandBuffer.DrawRendererList(rendererList);
        }


        public void SetFoveatedRenderingMode(FoveatedRenderingMode foveatedRenderingMode)
        {
            m_wrappedCommandBuffer.SetFoveatedRenderingMode(foveatedRenderingMode);
        }
#endif

        public void SetInstanceMultiplier(uint multiplier)
        {
            m_wrappedCommandBuffer.SetInstanceMultiplier(multiplier);
        }

        public void SetWireframe(bool enable)
        {
#if UNITY_6000_0_OR_NEWER
            m_wrappedCommandBuffer.SetWireframe(enable);
#endif
        }
    }

    public class RTECamera : MonoBehaviour, IRTECamera
    {
        public static event Action<IRTECamera> Created;
        public static event Action<IRTECamera> Destroyed;

        public event Action<IRTECamera> CommandBufferRefresh;

        private Camera m_camera;
        public Camera Camera
        {
            get { return m_camera; }
        }

        private RTECommandBuffer m_rteCommandBuffer;
        public IRTECommandBuffer RTECommandBuffer
        {
            get { return m_rteCommandBufferOverride != null ? m_rteCommandBufferOverride : m_rteCommandBuffer; }
        }

        private IRTECommandBuffer m_rteCommandBufferOverride;
        public IRTECommandBuffer RTECommandBufferOverride
        {
            get { return m_rteCommandBufferOverride; }
            set
            {
                m_rteCommandBufferOverride = value;
                if (m_rteCommandBufferOverride != null)
                {
                    RemoveCommandBuffer();
                }
                else
                {
                    CreateCommandBuffer();
                }
            }
        }

        [SerializeField]
        private CameraEvent m_cameraEvent = CameraEvent.BeforeImageEffects;
        public CameraEvent Event
        {
            get { return m_cameraEvent; }
            set
            {
                if (m_rteCommandBufferOverride == null)
                {
                    RemoveCommandBuffer();
                    m_cameraEvent = value;
                    CreateCommandBuffer();
                }
                else
                {
                    m_cameraEvent = value;
                }
            }
        }

        private IRenderersCache m_renderersCache;
        private bool m_destroyRenderersCache;

        private IMeshesCache m_meshesCache;
        private bool m_destroyMeshesCache;

        public IRenderersCache RenderersCache
        {
            get
            {
                if (m_initialized && m_renderersCache == null)
                {
                    CreateRenderersCache();
                }
                return m_renderersCache;
            }
            set
            {
                DestroyRenderersCache();
                m_renderersCache = value;
            }
        }

        public IMeshesCache MeshesCache
        {
            get
            {
                if (m_initialized && m_meshesCache == null)
                {
                    CreateMeshesCache();
                }
                return m_meshesCache;
            }
            set
            {
                DestroyMeshesCache();
                m_meshesCache = value;
            }
        }

        private bool m_initialized;
        private bool IsInitialized
        {
            get { return m_initialized && m_camera != null; }
        }

        private void Awake()
        {
            m_initialized = true;
            m_camera = GetComponent<Camera>();

            if (m_rteCommandBufferOverride == null)
            {
                CreateCommandBuffer();
            }

            RefreshCommandBuffer();

            if (m_renderersCache != null)
            {
                m_renderersCache.Refreshed -= OnCacheRefresh;
                m_renderersCache.Refreshed += OnCacheRefresh;
            }

            if (m_meshesCache != null)
            {
                m_meshesCache.Refreshing -= OnCacheRefresh;
                m_meshesCache.Refreshing += OnCacheRefresh;
            }

            if (Created != null)
            {
                Created(this);
            }
        }

        private void OnDestroy()
        {
            m_initialized = false;

            if (m_renderersCache != null)
            {
                m_renderersCache.Refreshed -= OnCacheRefresh;
                DestroyRenderersCache();
            }

            if (m_meshesCache != null)
            {
                m_meshesCache.Refreshing -= OnCacheRefresh;
                DestroyMeshesCache();
            }

            if (m_camera != null)
            {
                RemoveCommandBuffer();
            }

            if (Destroyed != null)
            {
                Destroyed(this);
            }
        }

        public void CreateRenderersCache()
        {
            DestroyRenderersCache();
            m_destroyRenderersCache = true;
            m_renderersCache = gameObject.AddComponent<RenderersCache>();
            if (IsInitialized)
            {
                m_renderersCache.Refreshed += OnCacheRefresh;
            }
        }

        public void CreateMeshesCache()
        {
            DestroyMeshesCache();
            m_destroyMeshesCache = true;
            m_meshesCache = gameObject.AddComponent<MeshesCache>();
            if (IsInitialized)
            {
                m_meshesCache.Refreshing += OnCacheRefresh;
            }
        }

        public void DestroyRenderersCache()
        {
            if (m_destroyRenderersCache && m_renderersCache != null)
            {
                m_renderersCache.Refreshed -= OnCacheRefresh;
                m_renderersCache.Destroy();
                m_renderersCache = null;
            }
        }

        public void DestroyMeshesCache()
        {
            if (m_destroyMeshesCache && m_meshesCache != null)
            {
                m_meshesCache.Refreshing -= OnCacheRefresh;
                m_meshesCache.Destroy();
                m_meshesCache = null;
            }
        }

        public void Destroy()
        {
            DestroyMeshesCache();
            DestroyRenderersCache();
            Destroy(this);
        }

        private void OnCacheRefresh()
        {
            RefreshCommandBuffer();
        }

        private void CreateCommandBuffer()
        {
            if (m_rteCommandBuffer != null || m_camera == null)
            {
                return;
            }

            if (RenderPipelineInfo.Type == RPType.HDRP || RenderPipelineInfo.Type == RPType.URP && RenderPipelineInfo.UseRenderGraph)
            {
                return;
            }

            m_rteCommandBuffer = new RTECommandBuffer();
            m_rteCommandBuffer.name = "RTECameraCommandBuffer";

            m_camera.AddCmdBuffer(m_cameraEvent, m_rteCommandBuffer.WrappedCommandBuffer);
        }

        private void RemoveCommandBuffer()
        {
            if (m_rteCommandBuffer == null)
            {
                return;
            }

            if (RenderPipelineInfo.Type == RPType.HDRP || RenderPipelineInfo.Type == RPType.URP && RenderPipelineInfo.UseRenderGraph)
            {
                return;
            }

            m_camera.RemoveCmdBuffer(m_cameraEvent, m_rteCommandBuffer.WrappedCommandBuffer);
            m_rteCommandBuffer.Dispose();
            m_rteCommandBuffer = null;
        }

        public void RefreshCommandBuffer()
        {
            if (!IsInitialized)
            {
                return;
            }

            IRTECommandBuffer commandBuffer;
            if (m_rteCommandBufferOverride == null)
            {
                if (m_rteCommandBuffer == null)
                {
                    return;
                }

                m_rteCommandBuffer.Clear();
                if (m_cameraEvent == CameraEvent.AfterImageEffects || m_cameraEvent == CameraEvent.AfterImageEffectsOpaque)
                {
                    m_rteCommandBuffer.ClearRenderTarget(true, false, Color.black);
                }

                commandBuffer = m_rteCommandBuffer;
            }
            else
            {
                commandBuffer = m_rteCommandBufferOverride;
            }

            if (m_meshesCache != null)
            {
                IList<RenderMeshesBatch> batches = m_meshesCache.Batches;
                for (int i = 0; i < batches.Count; ++i)
                {
                    RenderMeshesBatch batch = batches[i];
                    if (batch.Material == null)
                    {
                        continue;
                    }

                    if (batch.Material.enableInstancing)
                    {
                        for (int j = 0; j < batch.Mesh.subMeshCount; ++j)
                        {
                            if (batch.Mesh != null)
                            {
                                commandBuffer.DrawMeshInstanced(batch.Mesh, j, batch.Material, -1, batch.Matrices, batch.Matrices.Length);
                            }
                        }
                    }
                    else
                    {
                        Matrix4x4[] matrices = batch.Matrices;
                        for (int m = 0; m < matrices.Length; ++m)
                        {
                            for (int j = 0; j < batch.Mesh.subMeshCount; ++j)
                            {
                                if (batch.Mesh != null)
                                {
                                    commandBuffer.DrawMesh(batch.Mesh, matrices[m], batch.Material, j, -1);
                                }
                            }
                        }
                    }
                }
            }

            if (m_renderersCache != null)
            {
                IList<Renderer> renderers = m_renderersCache.Renderers;
                for (int i = 0; i < renderers.Count; ++i)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }
                    Material[] materials = renderer.sharedMaterials;
                    for (int j = 0; j < materials.Length; ++j)
                    {
                        if (m_renderersCache.MaterialOverride != null)
                        {
                            commandBuffer.DrawRenderer(renderer, m_renderersCache.MaterialOverride, j, -1);
                        }
                        else
                        {
                            Material material = materials[j];
                            if (material != null)
                            {
                                commandBuffer.DrawRenderer(renderer, material, j, -1);
                            }
                        }
                    }
                }
            }

            if (CommandBufferRefresh != null)
            {
                CommandBufferRefresh(this);
            }
        }
    }
}
