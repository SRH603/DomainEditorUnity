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
            Vector2 pointA = side == Side.Left ? eventData.position : (Vector2)keyframe.transform.position;
            Vector2 pointB = side == Side.Left ? (Vector2)keyframe.transform.position : eventData.position;
            
            float angle = Mathf.Rad2Deg * (Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x));
            
            rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(angle, -90.001f, 90.001f));
            
            if (_weighted)
            {
                float length = Vector2.Distance(rectTransform.position, eventData.position);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
            }

            int tangentMode = TangentUtility.EncodeTangents(TangentMode.Free);
            if (keyframe.BrokenTangents)
            {
                TangentMode left, right;
                TangentUtility.DecodeTangents(keyframe.Data.tangentMode, out left, out right);

                if (side == Side.Left)
                    left = TangentMode.Free;

                if (side == Side.Right)
                    right = TangentMode.Free;
                
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
    }
}