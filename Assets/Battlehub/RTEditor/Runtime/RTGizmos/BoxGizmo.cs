using UnityEngine;
using Battlehub.RTCommon;

namespace Battlehub.RTGizmos
{
    public class BoxGizmo : BaseGizmo
    {
        protected virtual Bounds Bounds
        {
            get { return new Bounds(Vector3.zero, Vector3.one); }
            set { }
        }

        protected override Matrix4x4 HandlesTransform
        {
            get { return Matrix4x4.TRS(TargetTransform.TransformPoint(Bounds.center), TargetTransform.rotation, Vector3.Scale(Bounds.extents, TargetTransform.lossyScale)); }
        }

        protected override bool OnDrag(int index, Vector3 offset)
        {
            Bounds bounds = Bounds;
            bounds.center += offset / 2;
            bounds.extents += Vector3.Scale(offset / 2, HandlesPositions[index]);
            Bounds = bounds;
            return true;
        }

        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);

            Bounds bounds = Bounds;
            Vector3 parentScale = TargetTransform.parent == null ? Vector3.one : TargetTransform.parent.lossyScale;
            Vector3 scale = Vector3.Scale(Vector3.Scale(bounds.extents, TargetTransform.localScale), parentScale);

            GizmoUtility.DrawCubeHandles(camera.RTECommandBuffer, TargetTransform.TransformPoint(bounds.center), TargetTransform.rotation, scale, HandleProperties);
            GizmoUtility.DrawWireCube(camera.RTECommandBuffer, bounds, TargetTransform.TransformPoint(bounds.center), TargetTransform.rotation, TargetTransform.lossyScale, LineProperties);

            if(IsDragging)
            {
                GizmoUtility.DrawSelection(camera.RTECommandBuffer, TargetTransform.TransformPoint(bounds.center + Vector3.Scale(HandlesPositions[DragIndex], bounds.extents)), TargetTransform.rotation, TargetTransform.lossyScale, SelectionProperties);
            }
        }
    }
}
