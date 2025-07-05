using System;
using Battlehub.RTCommon;
using UnityEngine;
namespace Battlehub.RTGizmos
{
    public abstract class SphereGizmo : BaseGizmo
    {
        protected abstract Vector3 Center
        {
            get;
            set;
        }

        protected abstract float Radius
        {
            get;
            set;
        }

        protected override Matrix4x4 HandlesTransform
        {
            get
            {
                Vector3 scale = TargetTransform.lossyScale;
                scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return Matrix4x4.TRS(TargetTransform.TransformPoint(Center), TargetTransform.rotation, scale * Radius);
            }
        }

        protected override Matrix4x4 HandlesTransformInverse
        {
            get
            {
                Vector3 scale = TargetTransform.lossyScale;
                scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                return Matrix4x4.TRS(TargetTransform.position, TargetTransform.rotation, scale).inverse;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            RefreshOnCameraChanged = true;
        }

        protected override bool OnDrag(int index, Vector3 offset)
        {
            Radius += offset.magnitude * Math.Sign(Vector3.Dot(offset, HandlesNormals[index]));
            if(Radius < 0)
            {
                Radius = 0;
                return false;
            }
            return true;
        }

        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);
            if (TargetTransform == null)
            {
                return;
            }

            Vector3 scale = TargetTransform.lossyScale * Radius;
            scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            
            GizmoUtility.DrawCubeHandles(camera.RTECommandBuffer, TargetTransform.TransformPoint(Center), TargetTransform.rotation, scale, HandleProperties);
            GizmoUtility.DrawWireSphere(camera.RTECommandBuffer, camera.Camera, TargetTransform.TransformPoint(Center), TargetTransform.rotation, scale, LineProperties);

            if(IsDragging)
            {
                scale = TargetTransform.lossyScale;
                scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

                GizmoUtility.DrawSelection(camera.RTECommandBuffer, HandlesTransform.MultiplyPoint(Center + HandlesPositions[DragIndex]), TargetTransform.rotation, scale, SelectionProperties);
            }
        }

     
    }

}
