using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// The curve renderer base class holds all of the common logic used by the different curve renderers
    /// </summary>
    [ExecuteAlways]
    public abstract class CurveRenderer : MaskableGraphic
    {
        #region Serialized Fields
        [SerializeField]
        protected RectTransform content;
        
        [SerializeField]
        protected AnimationCurve curve;

        #endregion
        
        #region Fields
        
        protected bool isDirty = true;
        protected UIVertex vertex = UIVertex.simpleVert;
        
        protected Vector2 minRange = Vector2.zero;
        protected Vector2 maxRange = Vector2.one;
        
        private readonly List<Vector2> points = new List<Vector2>(1024);
        private readonly List<Vector2> vertices = new List<Vector2>();
        
        private const int MaximumSampleCount = 50;
        private const float SegmentWindowResolution = 1000;
        
        private const float MinMitreAngle = 15 * Mathf.Deg2Rad;
        private const float MinBevelAngle = 30 * Mathf.Deg2Rad;
        
        private static readonly float[,] OneTwo = new float[1, 2];
        private static readonly float[,] TwoTwo = new float[2, 2];
        
        #endregion
        
        #region Properties
        
        protected float RangeStart => curve[0].time;
        
        protected float RangeEnd => curve[curve.length - 1].time;

        public Vector2 MinRange => minRange;
        
        public Vector2 MaxRange => maxRange;
        
        public bool HasCurve => curve != null;

        public RectTransform Content
        {
            get => content;
            set => content = value;
        }

        public AnimationCurve Curve
        {
            get => curve;
            set => curve = value;
        }
        
        #endregion

        /// <summary>
        /// Mark the curve dirty so it will be recalculated
        /// </summary>
        public abstract void MarkDirty();

        /// <summary>
        /// Redraws the curve without recalculating the curve itself
        /// </summary>
        public void Redraw()
            => SetVerticesDirty();

        /// <summary>
        /// Set the curve to be rendered
        /// </summary>
        /// <param name="animationCurve"></param>
        public void SetCurve(AnimationCurve animationCurve)
        {
            curve = animationCurve;
            MarkDirty();
        }
        
        /// <summary>
        /// Create each segment of the line
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="size"></param>
        /// <param name="pivot"></param>
        /// <param name="lineThickness"></param>
        protected void CreateSegments(VertexHelper vh, Vector2 size, Vector2 pivot, Vector2 lineThickness)
        {
            vertices.Clear();
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 start = ScalePoint(points[i - 1], size, pivot);
                Vector2 end = ScalePoint(points[i], size, pivot);

                if (i == 1)
                {
                    Vector2 capStart = start - ((end - start).normalized * lineThickness * 0.5f);
                    CreateSegment(capStart, start, lineThickness);
                }
                
                CreateSegment(start, end, lineThickness);

                if (i == points.Count - 1)
                {
                    Vector2 capEnd = end + ((end - start).normalized * lineThickness * 0.5f);
                    CreateSegment(end, capEnd, lineThickness);
                }
            }

            CreateQuads(vh);
            vertices.Clear();
        }

        /// <summary>
        /// Build out the quads of our line
        /// </summary>
        /// <param name="vh"></param>
        private void CreateQuads(VertexHelper vh)
        {
            for (int i = 0; i < vertices.Count; i += 4)
            {
                // Get the 4 points of this quad
                Vector2 startMin = vertices[i];
                Vector2 startMax = vertices[i + 1];
                Vector2 endMax = vertices[i + 2];
                Vector2 endMin = vertices[i + 3];

                if (i < vertices.Count - 5)
                {
                    // Get the direction of this quad and the next
                    Vector2 dirQuad = startMax - endMax;
                    Vector2 dirNext = vertices[i + 6] - vertices[i + 5];
                    
                    float angle = Vector2.Angle(dirQuad, dirNext) * Mathf.Deg2Rad;
                    float sign = Mathf.Sign(Vector3.Cross(dirQuad.normalized, dirNext.normalized).z);
                    float dist = Mathf.Abs(Vector3.Distance(endMin, endMax)) / (2f * Mathf.Tan(angle * 0.5f));
                    
                    // Calculate central points between the end of this quad and the start of the next
                    Vector2 centerMax = endMax - dirQuad.normalized * dist * sign;
                    Vector2 centerMin = endMin + dirQuad.normalized * dist * sign;

                    // Adjust the points on the joint to match up nicely
                    if (dist < dirQuad.magnitude * 0.5f && dist < dirNext.magnitude * 0.5f && angle > MinMitreAngle)
                    {
                        endMax = centerMax;
                        endMin = centerMin;
                        vertices[i + 4] = centerMin;
                        vertices[i + 5] = centerMax;
                    }
                    else
                    {
                        if (dist < dirQuad.magnitude * 0.5f && dist < dirNext.magnitude * 0.5f && angle > MinBevelAngle)
                        {
                            if (sign < 0)
                            {
                                endMax = centerMax;
                                vertices[i + 5] = centerMax;
                            }
                            else
                            {
                                endMin = centerMin;
                                vertices[i + 4] = centerMin;
                            }
                        }
                    }
                }

                CreateQuad(vh, startMin, startMax, endMax, endMin);
            }
        }

        /// <summary>
        /// Add the quad to the VertexHelper
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        private void CreateQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int startIndex = vh.currentVertCount;

            vertex.position = a;
            vh.AddVert(vertex);
            
            vertex.position = b;
            vh.AddVert(vertex);
            
            vertex.position = c;
            vh.AddVert(vertex);
            
            vertex.position = d;
            vh.AddVert(vertex);
            
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
        
        /// <summary>
        /// Calculate the 4 points of that make up a segment of the curve
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineThickness"></param>
        protected void CreateSegment(Vector2 start, Vector2 end, Vector2 lineThickness)
        {
            Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * (lineThickness * 0.5f);

            vertices.Add(start - offset);
            vertices.Add(start + offset);
            vertices.Add(end + offset);
            vertices.Add(end - offset);
        }
        
        /// <summary>
        /// Scale a normalized curve point to fit our image
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        protected virtual Vector2 ScalePoint(Vector2 point, Vector2 size, Vector2 pivot) 
            => (point * size) - (size * pivot);

        /// <summary>
        /// Calculate the highest and lowest time/value of the curve
        /// </summary>
        protected void CalculateMinMaxValues()
        {
            float valueMin = float.MaxValue;
            float valueMax = float.MinValue;
            
            // Get the highest and lowest values of the curve
            foreach (Vector2 point in points)
            {
                valueMin = Mathf.Min(valueMin, point.y);
                valueMax = Mathf.Max(valueMax, point.y);
            }
           
            // Get the highest and lowest times of the curve, then clamp all of them
            // so the minimum size of our curve image is 1 full grid square. 
            minRange.x = Mathf.Min(points[0].x, 0f);
            maxRange.x = Mathf.Max(points[points.Count - 1].x, 1f);
            minRange.y = Mathf.Min(valueMin, 0f);
            maxRange.y = Mathf.Max(valueMax, 1f);
        }
        
        protected void RecalculateLinePoints(float minTime, float maxTime)
        {
            points.Clear();

            if (curve.length == 0)
                return;

            float[,] ranges = CalculateRanges(minTime, maxTime, RangeStart, RangeEnd);
            
            for (int i = 0; i < ranges.GetLength(0); i++)
                AddPoints(ranges[i, 0], ranges[i, 1], minTime, maxTime);
            
            if (points.Count > 0)
            {
                for (int i = 1; i < points.Count; i++)
                {
                    if (points[i].x < points[i - 1].x)
                    {
                        points.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private static float[,] CalculateRanges(float minTime, float maxTime, float rangeStart, float rangeEnd)
        {
            if (maxTime - minTime > rangeEnd - rangeStart)
            {
                OneTwo[0, 0] = rangeStart;
                OneTwo[0, 1] = rangeEnd;
                return OneTwo;
            }
            
            minTime = Mathf.Repeat(minTime - rangeStart, rangeEnd - rangeStart) + rangeStart;
            maxTime = Mathf.Repeat(maxTime - rangeStart, rangeEnd - rangeStart) + rangeStart;

            if (minTime < maxTime)
            {
                OneTwo[0, 0] = minTime;
                OneTwo[0, 1] = maxTime;
                return OneTwo;
            }
            
            TwoTwo[0, 0] = rangeStart;
            TwoTwo[0, 1] = maxTime;
            TwoTwo[1, 0] = minTime;
            TwoTwo[1, 1] = rangeEnd;
            return TwoTwo;
        }
        
        private static int GetSegmentResolution(float minTime, float maxTime, float keyTime, float nextKeyTime)
        {
            float fullTimeRange = maxTime - minTime;
            float keyTimeRange = nextKeyTime - keyTime;
            int count = Mathf.RoundToInt(SegmentWindowResolution * (keyTimeRange / fullTimeRange));
            return Mathf.Clamp(count, 1, MaximumSampleCount);
        }

        private void AddPoints(float minTime, float maxTime, float visibleMinTime, float visibleMaxTime)
        {
            if (curve[0].time >= minTime)
            {
                points.Add(new Vector2(RangeStart, curve[0].value));
                points.Add(new Vector2(curve[0].time, curve[0].value));
            }

            for (int i = 0; i < curve.length - 1; i++)
            {
                Keyframe key = curve[i];
                Keyframe nextKey = curve[i + 1];

                // Ignore segments that are outside of the range from minTime to maxTime
                if (nextKey.time < minTime || key.time > maxTime)
                    continue;

                // Get first value from actual key rather than evaluating curve (to correctly handle stepped interpolation)
                points.Add(new Vector2(key.time, key.value));

                // Place second sample very close to first one (to correctly handle stepped interpolation)
                int segmentResolution = GetSegmentResolution(visibleMinTime, visibleMaxTime, key.time, nextKey.time);
                float newTime = Mathf.Lerp(key.time, nextKey.time, 0.001f / segmentResolution);
               
                float value = curve.Evaluate(newTime);
                points.Add(new Vector2(newTime, value));

                // Iterate through curve segment
                for (float j = 1; j < segmentResolution; j++)
                {
                    newTime = Mathf.Lerp(key.time, nextKey.time, j / segmentResolution);
                    value = curve.Evaluate(newTime);
                    points.Add(new Vector2(newTime, value));
                }

                // Place second last sample very close to last one (to correctly handle stepped interpolation)
                newTime = Mathf.Lerp(key.time, nextKey.time, 1 - 0.001f / segmentResolution);
                value = curve.Evaluate(newTime);
                points.Add(new Vector2(newTime, value));
                
                // Get last value from actual key rather than evaluating curve (to correctly handle stepped interpolation)
                newTime = nextKey.time;
                points.Add(new Vector2(newTime, value));
            }

            if (curve[curve.length - 1].time <= maxTime)
            {
                float clampedValue = curve[curve.length - 1].value;
                points.Add(new Vector2(curve[curve.length - 1].time, clampedValue));
                points.Add(new Vector2(RangeEnd, clampedValue));
            }
        }
    }
}