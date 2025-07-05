using UnityEngine;

/// <summary>
/// 在局部 X = Cutoff 平面裁掉左侧体积，并保持侧边斜率。<br/>
/// Cutoff 规则：<br/>
/// • 若仅给 referenceTransform ⇒ Cutoff = −reference.x<br/>
/// • 若同时给 subReferenceTransform ⇒ Cutoff = (−reference.x) − (−subRef.x)
///   = −reference.x + subRef.x<br/>
/// • Inspector 里的 Cutoff 字段仍可手动调；运行时有 Transform 时以 Transform 为准。<br/>
/// 适用于你最初的 8/24 顶点平行六面体网格。<br/>
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class HoldPlaneBehavior : MonoBehaviour
{
    [Tooltip("局部坐标裁剪平面 X = Cutoff (米)")]
    public float Cutoff = 0f;

    [Tooltip("主参考：取其局部 -x")]
    public Transform referenceTransform;

    [Tooltip("次参考：若存在，Cutoff = -ref.x  -(-sub.x)")]
    public Transform subReferenceTransform;

    Mesh      _meshInst;
    Vector3[] _base;
    float     _maxX;
    float     _slope;   // Δy/Δx

    //────────────────────────
    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) { enabled = false; return; }

        _meshInst = Instantiate(mf.sharedMesh);
        _base     = _meshInst.vertices;
        foreach (var v in _base) _maxX = Mathf.Max(_maxX, v.x);

        _slope = (_base[2].y - _base[0].y) / (_base[2].x - _base[0].x);

        mf.sharedMesh = _meshInst;
        ApplyCutoff(Mathf.Clamp(Cutoff, 0f, _maxX));
    }

    void Update()
    {
        float newCut = Cutoff;   // 默认用 Inspector 值

        if (referenceTransform)
        {
            newCut = -referenceTransform.localPosition.x;

            if (subReferenceTransform)          // 有 sub ⇒ 再减去其反值
                newCut -= subReferenceTransform.localPosition.x;
        }

        newCut = Mathf.Clamp(newCut, 0f, _maxX);

        if (!Mathf.Approximately(newCut, Cutoff))
        {
            Cutoff = newCut;
            ApplyCutoff(Cutoff);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying || _meshInst == null) return;
        Cutoff = Mathf.Clamp(Cutoff, 0f, _maxX);
        ApplyCutoff(Cutoff);
    }
#endif

    //────────────────────────
    void ApplyCutoff(float cut)
    {
        Vector3[] vNew = new Vector3[_base.Length];

        for (int i = 0; i < _base.Length; i++)
        {
            Vector3 v = _base[i];
            if (v.x < cut)
            {
                float dx = cut - v.x;
                v.x  = cut;
                v.y += _slope * dx;   // 水平位移，保持斜率
            }
            vNew[i] = v;
        }

        _meshInst.vertices = vNew;
        _meshInst.RecalculateBounds();
    }
}
