using UnityEngine;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// The class that controls manipulation of the tangent handles on the keyframe
    /// </summary>
    public class CurveTangent : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        private RectTransform rectTransform;
        
        [SerializeField]
        private CurveKeyframe keyframe;
        
        [SerializeField, Tooltip("The side of the keyframe that this tangent is on.")]
        public Side side = Side.Left;
        
        #endregion
        
        #region Private Fields

        private bool _weighted;
        
        #endregion
        
        #region Properties
        
        public CurveKeyframe Keyframe
        {
            get => keyframe;
            set => keyframe = value;
        }
        
        public RectTransform RectTransform
        {
            get => rectTransform;
            set => rectTransform = value;
        }
        
        public Side Direction
        {
            get => side;
            set => side = value;
        }
        
        public bool Weighted
        {
            get => _weighted;
            set
            {
                _weighted = value;
                if (!_weighted)
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, keyframe.editor.Settings.gridPixelsPerCell);
                else Weight = 1f / 3f;
            }
        }

        public float Weight
        {
            get
            {
                int cellsPerPixel = keyframe.editor.Settings.gridPixelsPerCell;
                float gridSize = cellsPerPixel * 6f;
                return Mathf.Clamp01((rectTransform.rect.width - cellsPerPixel) / gridSize);
            }
            set
            {
                int cellsPerPixel = keyframe.editor.Settings.gridPixelsPerCell;
                
                if (!_weighted)
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellsPerPixel);
                else
                {
                    float gridSize = cellsPerPixel * 6f;

                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (gridSize * Mathf.Clamp01(value)) + cellsPerPixel);
                }
            }
        }
        #endregion

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // 1) 选择一个统一的坐标空间：用手柄的父节点
            var parentRT = rectTransform.parent as RectTransform;
            if (parentRT == null) return;

            // 2) 拿到正确的事件相机
            var cam = ResolveEventCamera(eventData);

            // 3) 鼠标屏幕坐标 -> 父节点本地坐标
            if (!ScreenToLocal(parentRT, eventData.position, cam, out var mouseLocal)) return;

            // 4) 关键帧中心的位置也换到同一坐标系（父节点本地）
            Vector2 keyLocal = parentRT.InverseTransformPoint(keyframe.transform.position);

            // 5) 计算向量与角度（全部在 parent 本地）
            Vector2 a = (side == Side.Left)  ? mouseLocal : keyLocal;
            Vector2 b = (side == Side.Left)  ? keyLocal   : mouseLocal;

            float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, -90.001f, 90.001f);

            // 建议用 localRotation，避免父层级旋转影响
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

            // 6) Weighted：长度同样用“同一坐标系”的距离
            if (_weighted)
            {
                // 手柄自身的局部位置
                Vector2 handleLocal = (Vector2)rectTransform.localPosition;
                float length = Vector2.Distance(handleLocal, mouseLocal);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
            }

            // 7) 切线模式保持原逻辑
            int tangentMode = TangentUtility.EncodeTangents(TangentMode.Free);
            if (keyframe.BrokenTangents)
            {
                TangentMode left, right;
                TangentUtility.DecodeTangents(keyframe.Data.tangentMode, out left, out right);
                if (side == Side.Left)  left  = TangentMode.Free;
                if (side == Side.Right) right = TangentMode.Free;
                tangentMode = TangentUtility.EncodeTangents(left, right);
            }

            keyframe.UpdateTangentData(this, tangentMode);
        }

        
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            keyframe.editor.RecordState(true);
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!keyframe)
                keyframe = ComponentUtility.GetComponentInParent<CurveKeyframe>(gameObject);
            
            if (!rectTransform)
                rectTransform = GetComponent<RectTransform>();
        }
        #endif
        
        public enum Side { Left, Right, Both, None }
        
        private Camera ResolveEventCamera(PointerEventData e)
        {
            if (e != null)
            {
                if (e.pressEventCamera) return e.pressEventCamera;
                if (e.enterEventCamera) return e.enterEventCamera;
            }
            var canvas = GetComponentInParent<Canvas>();
            return canvas ? canvas.worldCamera : null; // Overlay 时可为 null
        }

        private bool ScreenToLocal(RectTransform targetSpace, Vector2 screenPos, Camera cam, out Vector2 local)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetSpace, screenPos, cam, out local);
        }

    }
}