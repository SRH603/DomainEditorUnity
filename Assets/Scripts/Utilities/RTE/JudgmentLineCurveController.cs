using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Battlehub.RTCommon;
using Blackout.UI;

[RequireComponent(typeof(Graphic))]
public class JudgmentLineCurveController : Selectable, IPointerClickHandler
{
    // 选择当前判定线上的哪一条曲线（7 选 1）
    public enum TargetCurve
    {
        PositionX, PositionY, PositionZ,
        RotationX, RotationY, RotationZ,
        Transparency
    }

    [Header("要编辑的曲线（针对当前判定线）")]
    [SerializeField] private TargetCurve target = TargetCurve.PositionX;

    // —— 按钮基础（拷贝自 AnimationCurveButton 并精简）——
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private ButtonCurveRenderer curveRenderer;
    [SerializeField] private CurveUpdateMode updateMode = CurveUpdateMode.OnEndEdit;
    [SerializeField] private AnimationCurveEditor editor;

    [SerializeField] private AnimationCurveEvent onCurveChange = new AnimationCurveEvent();
    [SerializeField] private AnimationCurveEvent onFinishedEditing = new AnimationCurveEvent();

    public UnityEvent<int> OnSaved; // 保存完成（参数 = 当前判定线 index）

    public AnimationCurve Curve
    {
        get => curve;
        set { curve = value; if (curveRenderer) curveRenderer.SetCurve(curve); }
    }

    public enum CurveUpdateMode { None, OnUpdate, OnEndEdit }
    [Serializable] public class AnimationCurveEvent : UnityEvent<AnimationCurve> { }

    private ChartManager _cm;
    private GameData _gd;

    // 反射到 JudgmentLine 的 7 条曲线字段
    private static readonly FieldInfo kPosX = typeof(JudgmentLine).GetField("positionX");
    private static readonly FieldInfo kPosY = typeof(JudgmentLine).GetField("positionY");
    private static readonly FieldInfo kPosZ = typeof(JudgmentLine).GetField("positionZ");
    private static readonly FieldInfo kRotX = typeof(JudgmentLine).GetField("rotationX");
    private static readonly FieldInfo kRotY = typeof(JudgmentLine).GetField("rotationY");
    private static readonly FieldInfo kRotZ = typeof(JudgmentLine).GetField("rotationZ");
    private static readonly FieldInfo kAlp  = typeof(JudgmentLine).GetField("transparency");

    private static FieldInfo FieldOf(TargetCurve t)
    {
        switch (t)
        {
            case TargetCurve.PositionX: return kPosX;
            case TargetCurve.PositionY: return kPosY;
            case TargetCurve.PositionZ: return kPosZ;
            case TargetCurve.RotationX: return kRotX;
            case TargetCurve.RotationY: return kRotY;
            case TargetCurve.RotationZ: return kRotZ;
            case TargetCurve.Transparency: return kAlp;
        }
        return null;
    }

    /* ---------- 生命周期 ---------- */
    protected override void OnEnable()
    {
        base.OnEnable();
        if (!curveRenderer) curveRenderer = GetComponentInChildren<ButtonCurveRenderer>();

        _cm = ChartManager.Instance;
        _gd = _cm != null ? _cm.gameData : null;

        RefreshFromManager();

        // 监听“切换判定线”和“该判定线被其它地方保存”的刷新
        if (_cm != null)
        {
            _cm.OnLineChanged           += OnLineChanged;
            _cm.OnJudgmentLineChanged   += OnJudgmentLineChanged;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_cm != null)
        {
            _cm.OnLineChanged           -= OnLineChanged;
            _cm.OnJudgmentLineChanged   -= OnJudgmentLineChanged;
        }
        if (editor != null)
        {
            editor.OnValueChanged -= OnEditorCurveChanged;
            editor.OnEndEdit      -= OnEditorEndEdit;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (curve == null) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            if (!curveRenderer) curveRenderer = GetComponentInChildren<ButtonCurveRenderer>();
            if (curveRenderer != null)
            {
                if (!curveRenderer.HasCurve) curveRenderer.SetCurve(curve);
                else curveRenderer.MarkDirty();
            }
        }
        else
        {
            if (!curveRenderer) curveRenderer = GetComponentInChildren<ButtonCurveRenderer>();
            RefreshFromManager();
        }
    }
#endif

    /* ---------- 读取当前判定线的目标曲线，展示在按钮上 ---------- */
    public void RefreshFromManager()
    {
        if (_gd == null || _gd.content == null || _gd.content.judgmentLines == null) return;

        int lineIndex = _cm != null ? _cm.currentLineIndex : 0;
        if (lineIndex < 0 || lineIndex >= _gd.content.judgmentLines.Length) return;

        var line = _gd.content.judgmentLines[lineIndex];
        if (line == null) return;

        var fi = FieldOf(target);
        if (fi == null) return;

        var src = fi.GetValue(line) as AnimationCurve;
        curve = SafeCloneMin2Keys(src);
        if (curveRenderer) curveRenderer.SetCurve(curve);
    }

    private void OnLineChanged(int newIndex)                => RefreshFromManager();
    private void OnJudgmentLineChanged(int changedIndex)
    {
        if (_cm != null && changedIndex == _cm.currentLineIndex)
            RefreshFromManager();
    }

    /* ---------- 点击打开编辑器 ---------- */
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Press();
    }

    private void Press()
    {
        if (!IsActive() || !IsInteractable()) return;

        if (!editor)
        {
            Debug.LogError("JudgmentLineCurveController 需要绑定 AnimationCurveEditor");
            return;
        }

        // 避免重复订阅
        editor.OnValueChanged -= OnEditorCurveChanged;
        editor.OnEndEdit      -= OnEditorEndEdit;

        // 防止第三方渲染器对 0/1 个 key 越界
        curve = SafeCloneMin2Keys(curve);
        if (curveRenderer) curveRenderer.SetCurve(curve);

        editor.EditCurve(curve);
        editor.OnValueChanged += OnEditorCurveChanged;
        editor.OnEndEdit      += OnEditorEndEdit;
    }

    private void OnEditorCurveChanged()
    {
        if (updateMode == CurveUpdateMode.OnUpdate && curveRenderer != null)
            curveRenderer.MarkDirty();
        onCurveChange.Invoke(curve);
    }

    private void OnEditorEndEdit()
    {
        if (updateMode == CurveUpdateMode.OnEndEdit && curveRenderer != null)
            curveRenderer.MarkDirty();
        onFinishedEditing.Invoke(curve);

        // 结束就解绑，避免累计
        if (editor != null)
        {
            editor.OnValueChanged -= OnEditorCurveChanged;
            editor.OnEndEdit      -= OnEditorEndEdit;
        }
    }

    /* ---------- 只把当前按钮里的曲线保存到 GameData（含 RTE 撤销/重做） ---------- */
    public void SaveToGameData()
    {
        if (_gd == null || _gd.content == null || _gd.content.judgmentLines == null)
        {
            Debug.LogWarning("[JudgmentLineCurveController] GameData 不可用，保存取消。");
            return;
        }

        int lineIndex = _cm != null ? _cm.currentLineIndex : 0;
        if (lineIndex < 0 || lineIndex >= _gd.content.judgmentLines.Length)
        {
            Debug.LogWarning("[JudgmentLineCurveController] 当前判定线越界，保存取消。");
            return;
        }

        var line = _gd.content.judgmentLines[lineIndex];
        if (line == null) { Debug.LogWarning("[JudgmentLineCurveController] 当前判定线为空，保存取消。"); return; }

        var fi = FieldOf(target);
        if (fi == null) { Debug.LogWarning("[JudgmentLineCurveController] 目标曲线字段解析失败。"); return; }

        var rte  = IOC.IsRegistered<IRTE>() ? IOC.Resolve<IRTE>() : null;
        var undo = rte != null ? rte.Undo : null;

        var newCurve = SafeCloneMin2Keys(curve); // 保存前也兜底

        if (undo != null)
        {
            undo.BeginRecord();              // ← 一次保存 = 一步撤销
            undo.BeginRecordValue(line, fi);
            fi.SetValue(line, newCurve);
            undo.EndRecordValue(line, fi);
            undo.EndRecord();
        }
        else
        {
            fi.SetValue(line, newCurve);
        }

        // 通知：带判定线 index 的事件（供下游刷新）
        ChartManager.Instance?.NotifyJudgmentLineChanged(lineIndex);
        OnSaved?.Invoke(lineIndex);

        //try { DechHub.Instance?.Save(); } catch { }

        Debug.Log($"[JudgmentLineCurveController] 已保存 {target} 到判定线 {lineIndex}（含 Undo/Redo）。");
    }

    /* ---------- 工具：克隆并保证至少 2 个 key，避免渲染器越界 ---------- */
    private static AnimationCurve SafeCloneMin2Keys(AnimationCurve src)
    {
        if (src == null || src.length == 0)
            return AnimationCurve.Linear(0f, 0f, 1f, 1f);
        if (src.length == 1)
        {
            float v = src.keys[0].value;
            return AnimationCurve.Linear(0f, v, 1f, v);
        }
        var c = new AnimationCurve(src.keys) { preWrapMode = src.preWrapMode, postWrapMode = src.postWrapMode };
        return c;
    }
}
