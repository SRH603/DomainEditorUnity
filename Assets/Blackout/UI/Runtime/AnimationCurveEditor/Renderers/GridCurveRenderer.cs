using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Component that renders the curve on to a Graphic. This renderer differs from the button renderer
    /// as it scales the image to fit the entire curve whilst maintaining its position on the grid.
    /// </summary>
    [ExecuteAlways]
    public class GridCurveRenderer : CurveRenderer
    {
        private AnimationCurveEditor _editor;
        
        private readonly Vector2 _pivot = Vector2.one * 0.5f;
        
        protected override void Awake()
        {
            if (!_editor)
                _editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
            
            base.Awake();
        }

        /// <summary>
        /// Mark the curve dirty so it will be recalculated
        /// </summary>
        [ContextMenu("Mark Dirty")]
        public override void MarkDirty()
        {
            if (!_editor)
                _editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);

            if (!_editor)
                return;
            
            // Update the calculated points
            RecalculateLinePoints(RangeStart, RangeEnd);
            
            // Check if the image needs to be resized to fit all the calculated line points
            CalculateApproximateSizeAndPivot(_editor.Settings.gridPixelsPerCell * 10f);
            SetVerticesDirty();
        }
       
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (curve == null || curve.keys.Length < 2)
                return;
            
            Vector2 size = (Vector2.one * _editor.Settings.gridPixelsPerCell) * 10f;
            Vector2 scale = content.localScale;
            
            vertex.color = _editor.Settings.curveColor;
            Vector2 lineThickness = _editor.Settings.curveThickness * new Vector2(1f / scale.x, 1f / scale.y);
            
            CreateSegments(vh, size, _pivot, lineThickness);
        }
       
        /// <summary>
        /// Adjusts the size and pivot of the curve image to fit the curve
        /// and keep the center of the curve (0.5, 0.5) centered to the grid
        /// </summary>
        /// <param name="baseSize"></param>
        private void CalculateApproximateSizeAndPivot(float baseSize)
        {
            CalculateMinMaxValues();
            
            float timeMin = minRange.x;
            float timeMax = maxRange.x;
            float valueMin = minRange.y;
            float valueMax = maxRange.y;
            
            // Calculate the midpoint of the keyframes' time span
            float midpointTime = (timeMin + timeMax) * 0.5f;
            float midpointValue = (valueMin + valueMax) * 0.5f;

            // Calculate pivot for both directions
            float pivotX = (0.5f - midpointTime) / (timeMax - timeMin) + 0.5f;
            float pivotY = (0.5f - midpointValue) / (valueMax - valueMin) + 0.5f;

            float timeRange = timeMax - timeMin;

            // Change our size and pivot so the size of the image covers the entirety of the rendered curve
            Vector2 pivot = new Vector2(timeRange < 1f ? 1f - pivotX : pivotX, pivotY);
            Vector2 sizeDelta = new Vector2((maxRange.x - minRange.x), (maxRange.y - minRange.y)) * baseSize;
            
            // Only apply the changes if they are different then what already exists
            if (pivot != rectTransform.pivot || sizeDelta != rectTransform.sizeDelta)
            {
                rectTransform.pivot = pivot;
                rectTransform.sizeDelta = sizeDelta;
            }
        }
        
        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (!_editor)
                _editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
        }
        #endif
    }
}
