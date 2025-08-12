using UnityEngine;
using System.Collections.Generic;
using static RuntimeCurveEditor.ContextMenuManager;

namespace RuntimeCurveEditor
{
    public struct Knot
    {
        public Vector2 point;
        public bool visible;

        public Knot(Vector2 point, bool visible) {
            this.point = point;
            this.visible = visible;
        }
    }

    public static class Curves
    {
        public static Camera camera;
        public static Material lineMaterial;
        public static RenderParams renderParams;

        public static Dictionary<AnimationCurve, List<ContextMenu>> dictCurvesContextMenus;

        public static List<Knot> activeCurveKnots = new List<Knot>();

        public static float margin;
        const float MARGIN_FACTOR = 1.33333f;
        const float INV_MARGIN_FACTOR = 1f / MARGIN_FACTOR;

        public static Vector2 tangPeakLeft;
        public static bool tangPeakLeftVisible;
        public static Vector2 tangPeakRight;
        public static bool tangPeakRightVisible;

        const float WEIGHTED_INV_RATIO = 3f;
        public const float WEIGHTED_RATIO = 1 / WEIGHTED_INV_RATIO;

        static Dictionary<AnimationCurve, Mesh> meshCurves = new Dictionary<AnimationCurve, Mesh>(ReferenceAnimationCurveEqualityComparer.Instance);
        static Dictionary<AnimationCurve, Mesh> meshCurvesKnots = new Dictionary<AnimationCurve, Mesh>(ReferenceAnimationCurveEqualityComparer.Instance);
        static Dictionary<AnimationCurve, bool> meshCurvesUpdate = new Dictionary<AnimationCurve, bool>(ReferenceAnimationCurveEqualityComparer.Instance);
        
        static Mesh meshTangents = new Mesh();
        static Mesh meshTangentsKnots = new Mesh();
        static Mesh meshTangentsOutlineKnots = new Mesh();

        static List<Vector3> verticesTangents = new List<Vector3>();
        static List<Vector3> verticesTangentsKnots = new List<Vector3>();
        static List<Vector3> verticesTangentsOutlineKnots = new List<Vector3>();

        static Dictionary<AnimationCurve, Mesh> meshCurvePaths = new Dictionary<AnimationCurve, Mesh>(ReferenceAnimationCurveEqualityComparer.Instance);
        static Dictionary<AnimationCurve, Mesh> meshBasicCurves = new Dictionary<AnimationCurve, Mesh>(ReferenceAnimationCurveEqualityComparer.Instance);
        static Dictionary<AnimationCurve, bool> meshBasicCurvesUpdate = new Dictionary<AnimationCurve, bool>(ReferenceAnimationCurveEqualityComparer.Instance);

        static Color LIGHT_GRAY = new Color(0.85f, 0.85f, 0.85f);

        public const float Z_DIFF = 0.01f;
        const float Z_POS = -Z_DIFF;
        public const float Z_POS_HALF = Z_POS * 0.5f;
        const float Z_POS_TANG = Z_POS - Z_DIFF;
        public const float Z_POS_KNOTS = Z_POS_TANG - Z_DIFF;
        const float Z_POS_PATH = Z_POS_HALF;

        static Matrix4x4 matrix4X4_Z_POS = Matrix4x4.Translate(new Vector3(0, 0, Z_POS));
        static Matrix4x4 matrix4X4_Z_POS_KNOTS = Matrix4x4.Translate(new Vector3(0, 0, Z_POS_KNOTS));
        public static Matrix4x4 matrix4X4_Z_POS_HALF = Matrix4x4.Translate(new Vector3(0, 0, Z_POS_HALF));
        static Matrix4x4 matrix4X4_Z_POS_TANG = Matrix4x4.Translate(new Vector3(0, 0, Z_POS_TANG));
        static Matrix4x4 matrix4X4_Z_POS_PATH = Matrix4x4.Translate(new Vector3(0, 0, Z_POS_PATH));

        public static Vector2 SampleBezier(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            return (1 - t) * (1 - t) * (1 - t) * p1 + 3.0F * (1 - t) * (1 - t) * t * p2 + 3.0F * (1 - t) * t * t * p3 + t * t * t * p4;
        }

        public static void AddBezier(List<Vector3> vertices, AnimationCurve curve, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Rect clipRect, float clip = 1.0f) {
            Vector2 v1m = Vector2.zero;
            Vector2 v2m = Vector2.zero;
            int samples = (int)(0.5f * (p4.x - p1.x));
            float invSamples = 1f / samples;
            float t = 0;
            v1m = SampleBezier(t, p1, p2, p3, p4);
            bool on = true;

            List<Vector3> bezierVertices = new List<Vector3>();
            do {
                t += invSamples;
                if (t > clip) {
                    on = false;
                    t = clip;
                }
                v2m = SampleBezier(t, p1, p2, p3, p4);
                if (clipRect.Contains(v1m) && clipRect.Contains(v2m)) {
                    if ((bezierVertices.Count == 0) && (vertices.Count == 0)) {
                        bezierVertices.Add(camera.ScreenToWorldPoint(new Vector3(v1m.x, v1m.y, 0)));
                    }
                    bezierVertices.Add(camera.ScreenToWorldPoint(new Vector3(v2m.x, v2m.y, 0)));
                }
                v1m = v2m;
            } while (on);
            vertices.AddRange(bezierVertices);
        }

        static void AddConstantLine(List<Vector3> vertices, Vector2 p1, Vector2 p2, Rect clipRect) {
            Vector2 p = p1;
            p.x = p2.x;
            Vector2 pp = p;
            if (Utils.CohenSutherlandLineClip(clipRect, ref p1, ref p)) {
                AddConstantLine(vertices, p1, p, true);
            }
            if (Utils.CohenSutherlandLineClip(clipRect, ref p2, ref pp)) {
                AddConstantLine(vertices, p2, pp, true);
            }
        }

        static void AddConstantLine(List<Vector3> vertices, Vector2 p1, Vector2 p2, bool oneLine = false) {
            if (oneLine || (p1.y == p2.y)) {
                if (vertices.Count == 0)
                {
                    vertices.Add(camera.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 0)));
                }
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p2.y, 0)));
            } else {
                if (vertices.Count == 0)
                {
                    vertices.Add(camera.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 0)));
                }
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p1.y, 0)));
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p2.y, 0)));
            }
        }

        static Vector2 GetTangLength(Vector2 p1, Vector2 p2) {
            Vector2 tangLength = Vector2.zero;
            tangLength.x = Mathf.Abs(p1.x - p2.x) * WEIGHTED_RATIO;
            tangLength.y = tangLength.x;
            return tangLength;
        }

        public static void GetControlPoints(Vector2 p1, Vector2 p2, float tangOut, float tangIn, out Vector2 c1, out Vector2 c2, float tangWeightOut = WEIGHTED_RATIO, float tangWeightIn = WEIGHTED_RATIO) {
            Vector2 tangLength1 = GetTangLength(p1, p2); 
            Vector2 tangLength2 = tangLength1;
            if (tangWeightOut != WEIGHTED_RATIO)
            {
                tangLength1 *= tangWeightOut * WEIGHTED_INV_RATIO;
            }
            if (tangWeightIn != WEIGHTED_RATIO)
            {
                tangLength2 *= tangWeightIn * WEIGHTED_INV_RATIO;
            }

            c1 = p1;
            c2 = p2;
            c1.x += tangLength1.x;
            c1.y += tangLength1.y * tangOut;
            c2.x -= tangLength2.x;
            c2.y -= tangLength2.y * tangIn;
        }

        public static void GetTangents(Vector2 p1, Vector2 p2, Vector2 c1, Vector2 c2, out float tangOut, out float tangIn) {
            Vector2 tangLength = GetTangLength(p1, p2);
            tangOut = (c1.y - p1.y) / tangLength.y;
            tangIn = (c2.y - p2.y) / tangLength.y;
        }

        public static void TriggerUpdateCurves()
        {
            List<AnimationCurve> curves = new List<AnimationCurve>(meshCurvesUpdate.Keys);
            foreach (AnimationCurve curve in curves)
            {
                meshCurvesUpdate[curve] = true;
            }
        }

        public static void TriggerUpdateCurve(AnimationCurve curve)
        {
            if (curve != null) {
                meshCurvesUpdate[curve] = true;
            }
        }

        public static void TriggerUpdateBasicCurves()
        {
            List<AnimationCurve> curves = new List<AnimationCurve>(meshBasicCurvesUpdate.Keys);
            foreach (AnimationCurve curve in curves)
            {
                meshBasicCurvesUpdate[curve] = true;
            }
        }

        public static bool BasicCurvesUpdate(AnimationCurve curve)
        {
            return (meshBasicCurvesUpdate.Count == 0) || meshBasicCurvesUpdate[curve];
        }

        static void AddLine(List<Vector3> vertices, float x1, float y1, float x2, float y2) {
            if (vertices.Count == 0)
            {
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(x1, y1, 0)));
            }
            vertices.Add(camera.ScreenToWorldPoint(new Vector3(x2, y2, 0)));
        }

        static void AddQuadToTangPeak(AnimationCurve curve, int selectedKey, bool left, Vector2 knot, List<Vector3>[] verticesTangentsArray, float tangScaled, Vector2 opossedKnot, Rect gridClipRect)
        {
            ContextMenu contextMenu = dictCurvesContextMenus[curve][selectedKey];
            bool leftCondition = left && contextMenu.leftTangent.free;
            bool rightCondition = !left && contextMenu.rightTangent.free;
            if (!contextMenu.broken || leftCondition || rightCondition)
            {
                var verticesTangents = verticesTangentsArray[0];
                var verticesTangentsKnots = verticesTangentsArray[1];
                float sign = left ? -1 : 1;
                float magnitude = CurveLines.tangFloat;
                float m = margin;
                bool isWeightedTangent = (leftCondition && contextMenu.leftTangent.weighted) || (rightCondition && contextMenu.rightTangent.weighted);
                if (isWeightedTangent)
                {
                    m *= INV_MARGIN_FACTOR;
                    magnitude = (knot - opossedKnot).magnitude;

                    if (leftCondition)
                    {
                        magnitude *= curve[selectedKey].inWeight / WEIGHTED_RATIO;
                    }
                    else
                    {
                        magnitude *= curve[selectedKey].outWeight / WEIGHTED_RATIO;
                    }

                    verticesTangentsKnots = verticesTangentsArray[2];
                }

                Vector2 tangPeak = new Vector2(knot.x + sign * magnitude * Mathf.Cos(tangScaled), knot.y + sign * magnitude * Mathf.Sin(tangScaled));
                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(knot.x, knot.y, 0)));

                float adj = CurveLines.tangFloat * 0.3333f;
                Rect gridAdjClipRect = new Rect(gridClipRect.x - adj, gridClipRect.y - adj, gridClipRect.width + 2 * adj, gridClipRect.height + 2 * adj);
                bool peakVisible = gridAdjClipRect.Contains(tangPeak);

                if (left)
                {
                    tangPeakLeftVisible = peakVisible;
                }
                else
                {
                    tangPeakRightVisible = peakVisible; 
                }

                if (!peakVisible)
                {
                    Utils.CohenSutherlandLineClip(gridAdjClipRect, ref knot, ref tangPeak);
                }

                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(tangPeak.x, tangPeak.y, 0)));

                if (peakVisible)
                {
                    AddQuad(verticesTangentsKnots, tangPeak, m, !isWeightedTangent, sign * tangScaled, sign);

                    if (isWeightedTangent)
                    {
                        tangPeak += (Vector2)(verticesTangentsKnots[0] + verticesTangentsKnots[2]) * 0.5f;
                        if (left)
                        {
                            tangPeakLeft = tangPeak;
                        }
                        else
                        {
                            tangPeakRight = tangPeak;
                        }
                    }
                    else
                    {
                        if (left)
                        {
                            tangPeakLeft = tangPeak;
                        }
                        else
                        {
                            tangPeakRight = tangPeak;
                        }
                    }
                }

            }
        }

        static void SetupMesh(Mesh mesh, List<Vector3> vertices, Color color, MeshTopology meshTopology)
        {
            var indices = new int[vertices.Count];
            var colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                indices[i] = i;
                colors[i] = color;
            }

            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetIndices(indices, meshTopology, 0);
        }

        static void SetupMeshLineLoop(Mesh mesh, List<Vector3> vertices, Color color)
        {
            var indices = new int[2 * vertices.Count];
            var colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                colors[i] = color;
            }

            for (int ii = 0; ii < vertices.Count; ii += 4)
            {
                int i = 2 * ii;
                for (int j = 0; j < 4; ++j)
                {
                    indices[i + j] = ii + j;
                }
                indices[i + 4] = ii;
                indices[i + 5] = ii + 3;
                indices[i + 6] = ii + 1;
                indices[i + 7] = ii + 2;
            }

            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
        }

        static void UpdateCurve(Color color, AnimationCurve curve, bool activeCurve, int selectedKey, Rect entireGridRect, Rect gridClipRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> verticesKnots = new List<Vector3>();
            List<Color> colorsKnots = new List<Color>();

            float ratio = entireGridRect.height * gradRect.width / (entireGridRect.width * gradRect.height);

            if (activeCurve)
            {
                activeCurveKnots.Clear();

                meshTangents.Clear();
                meshTangentsOutlineKnots.Clear();
                meshTangentsKnots.Clear();
            
                verticesTangents.Clear();
                verticesTangentsKnots.Clear();
                verticesTangentsOutlineKnots.Clear();
            }

            for (int i = 0; i < curve.length; ++i)
            {
                Vector2 knot = new Vector2(curve[i].time, curve[i].value);
                knot = Utils.Convert(knot, entireGridRect, gradRect);
                bool knotIn = !isIcon && gridClipRect.Contains(knot);

                //outside of the interval, just draw straigt lines outside from the 1st and last key respectively
                if (i == 0)
                {
                    var xMin = Mathf.Max(entireGridRect.xMin, gridClipRect.xMin);
                    if (knotIn)
                    {
                        AddLine(vertices, xMin, knot.y, knot.x, knot.y);
                    }
                    else if (!isIcon && (gridClipRect.yMin <= knot.y) && (knot.y <= gridClipRect.yMax) && (gridClipRect.xMax <= knot.x))
                    {
                        AddLine(vertices, xMin, knot.y, Mathf.Min(entireGridRect.xMax, gridClipRect.xMax), knot.y);
                    }
                }

                if (i == curve.length - 1)
                {
                    var xMax = Mathf.Min(entireGridRect.xMax, gridClipRect.xMax);
                    if (knotIn)
                    {
                        AddLine(vertices, knot.x, knot.y, xMax, knot.y);
                    }
                    else if (!isIcon && (gridClipRect.yMin <= knot.y) && (knot.y <= gridClipRect.yMax) && (knot.x <= gridClipRect.xMin))
                    {
                        AddLine(vertices, Mathf.Max(entireGridRect.xMin, gridClipRect.xMin), knot.y, xMax, knot.y);
                    }
                }

                if (isIcon && (curve.length == 1))
                {
                    AddLine(vertices, gridClipRect.xMin, knot.y, gridClipRect.xMax, knot.y);
                }

                if (curve.length > i + 1)
                {//draw bezier between consecutive keys
                    Vector2 knot2 = new Vector2(curve[i + 1].time, curve[i + 1].value);
                    knot2 = Utils.Convert(knot2, entireGridRect, gradRect);
                    bool knotIn2 = gridClipRect.Contains(knot2);

                    var keyOut = curve[i];
                    var keyIn = curve[i + 1];
                    float tangOut = keyOut.outTangent;
                    float tangIn = keyIn.inTangent;
                    var tangOutWeightMode = keyOut.weightedMode;
                    var tangInWeightMode = keyIn.weightedMode;

                    float tangWeightOut = (tangOutWeightMode == WeightedMode.Out) || (tangOutWeightMode == WeightedMode.Both) ? keyOut.outWeight : WEIGHTED_RATIO;
                    float tangWeightIn = (tangInWeightMode == WeightedMode.In) || (tangInWeightMode == WeightedMode.Both) ? keyIn.inWeight : WEIGHTED_RATIO;

                    if ((tangOut != float.PositiveInfinity) && (tangIn != float.PositiveInfinity))
                    {
                        Vector2 c1;
                        Vector2 c2;
                        GetControlPoints(knot, knot2, tangOut * ratio, tangIn * ratio, out c1, out c2, tangWeightOut, tangWeightIn);
                        AddBezier(vertices, curve, knot, c1, c2, knot2, gridClipRect, clip);
                    }
                    else
                    {
                        if (knotIn && knotIn2)
                        {
                            AddConstantLine(vertices, knot, knot2);
                        }
                        else
                        {
                            AddConstantLine(vertices, knot, knot2, gridClipRect);
                        }
                    }

                    if (activeCurve)
                    {
                        List<Vector3>[] verticesTangentsArray = new List<Vector3>[3] { verticesTangents, verticesTangentsKnots, verticesTangentsOutlineKnots};

                        if (knotIn && (selectedKey == i))
                        {
                            AddQuadToTangPeak(curve, selectedKey, false, knot, verticesTangentsArray, Mathf.Atan(tangOut * ratio), knot2, gridClipRect);
                        }
                        else if (knotIn2 && (selectedKey == i + 1))
                        {
                            AddQuadToTangPeak(curve, selectedKey, true, knot2, verticesTangentsArray, Mathf.Atan(tangIn * ratio), knot, gridClipRect);
                        }

                    }
                }

                if (activeCurve)
                {
                    activeCurveKnots.Add(new Knot(knot, knotIn));
                }

                if (knotIn)
                {
                    if (activeCurve)
                    {
                        if (selectedKey == i)
                        {
                            AddQuad(verticesKnots, colorsKnots, LIGHT_GRAY, knot, MARGIN_FACTOR * margin);
                        }
                    }
                    if (!isIcon)
                    {
                        AddQuad(verticesKnots, colorsKnots, color, knot, margin);
                    }
                }
            }

            Mesh meshCurve = (isIcon ? meshBasicCurves : meshCurves)[curve];
            //setup curve mesh
            meshCurve.Clear();
            meshCurve.SetVertices(vertices);
            Color[] colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                colors[i] = color;
            }
            meshCurve.SetColors(colors);
            int[] indices = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                indices[i] = i;
            }
            meshCurve.SetIndices(indices, MeshTopology.LineStrip, 0);

            if (!isIcon) {                
                Mesh meshCurveKnots = meshCurvesKnots[curve];

                //setup curve knots
                meshCurveKnots.Clear();
                meshCurveKnots.SetVertices(verticesKnots);
                meshCurveKnots.SetColors(colorsKnots);
                int[] indicesKnots = new int[verticesKnots.Count];
                for (int i = 0; i < verticesKnots.Count; ++i)
                {
                    indicesKnots[i] = i;
                }
                meshCurveKnots.SetIndices(indicesKnots, MeshTopology.Quads, 0);

                if (activeCurve)
                {
                    //setup tangent
                    if (verticesTangents.Count > 0)
                    {
                        SetupMesh(meshTangents, verticesTangents, Color.gray, MeshTopology.Lines);
                        if (verticesTangentsKnots.Count > 0)
                        {
                            SetupMesh(meshTangentsKnots, verticesTangentsKnots, Color.gray, MeshTopology.Quads);
                        }
                        if (verticesTangentsOutlineKnots.Count > 0)
                        {
                            SetupMeshLineLoop(meshTangentsOutlineKnots, verticesTangentsOutlineKnots, Color.gray);
                        }
                    }
                }
            }
        }

        static void DrawCurve(Color color, AnimationCurve curve, bool activeCurve, int selectedKey, Rect entireGridRect, Rect gridClipRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {           
            if (isIcon) {//basic shape
                if (CurveLines.deltaTime > CurveLines.PREFERRED_DELTA_TIME)
                {
                    if (!meshBasicCurves.ContainsKey(curve))
                    {
                        meshBasicCurves.Add(curve, new Mesh());
                        meshBasicCurvesUpdate.Add(curve, true);
                    }

                    if (meshBasicCurvesUpdate[curve])
                    {
                        meshBasicCurvesUpdate[curve] = false;
                        UpdateCurve(color, curve, activeCurve, selectedKey, entireGridRect, gridClipRect, gradRect, true, clip);
                    }
                }
                Graphics.RenderMesh(renderParams, meshBasicCurves[curve], 0, matrix4X4_Z_POS);
            } else {
                if (!meshCurves.ContainsKey(curve))
                {
                    meshCurves.Add(curve, new Mesh());
                    meshCurvesKnots.Add(curve, new Mesh());
                    meshCurvesUpdate.Add(curve, true);
                }
                Mesh meshCurve = meshCurves[curve];
                Mesh meshCurveKnots = meshCurvesKnots[curve];

                if (CurveLines.deltaTime > CurveLines.PREFERRED_DELTA_TIME)
                {
                    if (meshCurvesUpdate[curve])
                    {
                        meshCurvesUpdate[curve] = false;
                        UpdateCurve(color, curve, activeCurve, selectedKey, entireGridRect, gridClipRect, gradRect);
                    }
                }

                Graphics.RenderMesh(renderParams, meshCurve, 0, matrix4X4_Z_POS);
                Graphics.RenderMesh(renderParams, meshCurveKnots, 0, matrix4X4_Z_POS_KNOTS);
                Graphics.RenderMesh(renderParams, meshTangents, 0, matrix4X4_Z_POS_TANG);
                Graphics.RenderMesh(renderParams, meshTangentsKnots, 0, matrix4X4_Z_POS_TANG);
                Graphics.RenderMesh(renderParams, meshTangentsOutlineKnots, 0, matrix4X4_Z_POS_TANG);
            }
        }

        public static void DrawCurveForm(Color color, AnimationCurve curve1, AnimationCurve curve2, bool activeCurve1, bool activeCurve2, int selectedKey, Rect entireGridRect, Rect gridRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {
            if (curve2 != null) {
                if (!meshCurvePaths.ContainsKey(curve1))
                {
                    meshCurvePaths.Add(curve1, new Mesh());
                }
                Mesh meshPath = meshCurvePaths[curve1];
                if (CurveLines.deltaTime > CurveLines.PREFERRED_DELTA_TIME) {
                    if (!meshCurvesUpdate.ContainsKey(curve2) || meshCurvesUpdate[curve2] || (meshCurvesUpdate.ContainsKey(curve1) && meshCurvesUpdate[curve1])) {
                        int samples = (int)entireGridRect.width;
                        Color colorTransp = color;
                        colorTransp.a *= 0.35f;
                        Vector2 v1;
                        Vector2 v2;
                        Vector2 v1prev;
                        Vector2 v2prev;
                        float invSamples = 1f / samples;
                        float t = 0;

                        bool lineIn = GetValues(out v1, out v2, curve1, curve2, entireGridRect, gridRect, gradRect, t);
                        bool prevLineIn;

                        List<Vector3> vertices = new List<Vector3>();
                        for (int i = 0; i <= samples; ++i)
                        {
                            v1prev = v1;
                            v2prev = v2;
                            prevLineIn = lineIn;
                            lineIn = GetValues(out v1, out v2, curve1, curve2, entireGridRect, gridRect, gradRect, t);

                            if (prevLineIn && lineIn) {
                                vertices.Add(camera.ScreenToWorldPoint(new Vector3(v1prev.x, v1prev.y, 0)));
                                vertices.Add(camera.ScreenToWorldPoint(new Vector3(v2prev.x, v2prev.y, 0)));
                                vertices.Add(camera.ScreenToWorldPoint(new Vector3(v2.x, v2.y, 0)));
                                vertices.Add(camera.ScreenToWorldPoint(new Vector3(v1.x, v1.y, 0)));
                            }
                            t += invSamples;
                        }
                        int[] indices = new int[vertices.Count];
                        Color[] colors = new Color[vertices.Count];
                        for (int i = 0; i < vertices.Count; ++i)
                        {
                            indices[i] = i;
                            colors[i] = colorTransp;
                        }

                        meshPath.Clear();
                        meshPath.SetVertices(vertices);
                        meshPath.SetColors(colors);
                        meshPath.SetIndices(indices, MeshTopology.Quads, 0);
                    }
                }
                Graphics.RenderMesh(renderParams, meshPath, 0, matrix4X4_Z_POS_PATH);
                DrawCurve(color, curve2, activeCurve2, selectedKey, entireGridRect, gridRect, gradRect, isIcon, clip);
            }
            DrawCurve(color, curve1, activeCurve1, selectedKey, entireGridRect, gridRect, gradRect, isIcon, clip);
        }

        static bool GetValues(out Vector2 v1, out Vector2 v2, AnimationCurve curve1, AnimationCurve curve2, Rect entireGridRect, Rect clipRect, Rect gradRect, float t) {
            v1.x = gradRect.xMin + t * (gradRect.xMax - gradRect.xMin);
            v2.x = v1.x;
            v1.y = curve1.Evaluate(v1.x);
            v1 = Utils.Convert(v1, entireGridRect, gradRect);
            v2.y = curve2.Evaluate(v2.x);
            v2 = Utils.Convert(v2, entireGridRect, gradRect);
            return Utils.CohenSutherlandLineClip(clipRect, ref v1, ref v2);
        }

        static void AddQuad(List<Vector3> vertices, Vector2 pos, float m, bool diamond = true, float tangScaledAngle = 0, float sign = 1f)
        {
            Vector2[] offsets;
            if (diamond)
            {
                offsets = new Vector2[] { new Vector2(0, -m), new Vector2(m, 0), new Vector2(0, m), new Vector2(-m, 0) };
            }
            else
            {
                if (0.25f * Mathf.PI <= tangScaledAngle && tangScaledAngle < 0.75f * Mathf.PI)
                {
                    offsets = new Vector2[] { new Vector2(-m, 0), new Vector2(m, 0), new Vector2(m, 2f * m), new Vector2(-m, 2f * m) };
                }
                else if (-0.75f * Mathf.PI <= tangScaledAngle && tangScaledAngle < -0.25f * Mathf.PI)
                {

                    offsets = new Vector2[] { new Vector2(-m, 0), new Vector2(m, 0), new Vector2(m, -2f * m), new Vector2(-m, -2f * m) };
                }
                else 
                {
                    offsets = new Vector2[] { new Vector2(2f * m * sign, -m), new Vector2(0, -m), new Vector2(0, m), new Vector2(2f * m * sign, m) };
                }
            }

            Vector3[] positions = new Vector3[4];
            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = camera.ScreenToWorldPoint(new Vector3(pos.x + offsets[i].x, pos.y + offsets[i].y, 0));
            }

            vertices.AddRange(positions);
        }

        static void AddQuad(List<Vector3> vertices, List<Color> colors, Color color, Vector2 pos, float m) {
            for (int i = 0; i < 4; ++i)
            {
                colors.Add(color);                
            }
            AddQuad(vertices, pos, m);
        }
    }
}
