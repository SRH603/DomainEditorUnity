using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Component that renders the curve on to a Graphic. This renderer differs from the grid renderer
    /// as it scales the curve to fit inside the image rect transform.
    /// </summary>
    [ExecuteAlways]
    public class ButtonCurveRenderer : CurveRenderer
    {
        [SerializeField, Tooltip("The line thickness in pixels")]
        private float lineThickness = 2f;

        private Vector2 _curveRange = Vector2.one;

        public float LineThickness
        {
            get => lineThickness;
            set => lineThickness = value;
        }
        
        /// <summary>
        /// Mark the curve dirty so it will be recalculated
        /// </summary>
        [ContextMenu("Mark Dirty")]
        public override void MarkDirty()
        {
            isDirty = true;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (curve == null || curve.keys.Length < 2)
                return;
            
            if (isDirty)
            {
                RecalculateLinePoints(RangeStart, RangeEnd);
                CalculateMinMaxValues();

                _curveRange.x = maxRange.x - minRange.x;
                _curveRange.y = maxRange.y - minRange.y;
                
                isDirty = false;
            }

            vertex.color = color;

            Vector2 scale = content.localScale;

            Vector2 thickness = lineThickness * new Vector2(1f / scale.x, 1f / scale.y);
            
            CreateSegments(vh, rectTransform.rect.size, rectTransform.pivot, thickness);
        }

        /// <summary>
        /// Scale point on the button acts differently than the grid. We want to normalize the points
        /// so they all fit inside the buttons image
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        protected override Vector2 ScalePoint(Vector2 point, Vector2 size, Vector2 pivot)
        {
            Vector2 normalizedPoint = new Vector2((point.x - minRange.x) / _curveRange.x, (point.y - minRange.y) / _curveRange.y);
            return (normalizedPoint * size) - (size * pivot);
        }
    }
}
