using System.Collections.Generic;
using System.Linq;
using Battlehub.RTEditor.Views;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameUtilities;

namespace NoteEditor.Views
{
    public class NoteEditorView : View
    {
        #region 引用字段
        /* ───── UI 引用 ───── */
        [Header("UI")]
        [SerializeField] private TMP_Dropdown  lineDropdown;
        [SerializeField] private RectTransform content;      // ScrollRect.Content
        [SerializeField] private GridGraphic   grid;         // 网格脚本
        [SerializeField] private RectTransform noteLayer;

        [Header("Prefabs")]
        [SerializeField] private RectTransform tapPrefab;    // type 0
        [SerializeField] private RectTransform dragPrefab;   // type 1
        [SerializeField] private RectTransform holdPrefab;   // type 2
        
        [Header("Gameplay")]
        private OnPlaying playing;
        
        [Header("工具切换")]
        [SerializeField] private ToggleGroup toggleGroup;  // 父物体 ToggleGroupHolder
        [SerializeField] private Toggle tapToggle;        // TapToggle
        [SerializeField] private Toggle dragToggle;       // DragToggle
        [SerializeField] private Toggle holdToggle;       // HoldToggle
        
        [Header("Scroll & Grid Click")]
        [SerializeField] private ScrollRect scrollRect;   // 必须拖入父级 ScrollRect
        [SerializeField] private Toggle selectToggle;     // 第四个“选择”Toggle

        // 记录当前工具类型：0=Tap,1=Drag,2=Hold
        [HideInInspector] public int currentTool = 0;

        /* ───── 对象池：三种音符一人一池 ───── */
        private readonly List<RectTransform> tapPool  = new();
        private readonly List<RectTransform> dragPool = new();
        private readonly List<RectTransform> holdPool = new();
        
        // ① 增加一个字段用来保存 OnBpmListChanged 的委托
        private System.Action _onBpmListChangedHandler;

        [HideInInspector] public int selectToolIndex = 3;
        
        // —— Hold 创建用状态字段 —— 
        private bool   _isCreatingHold;
        private List<Vector3Int> _holdBeats = new();
        private RectTransform    _holdPreviewQuad; // 预览用的 UI 四边形
        private List<double>     _holdPosXs  = new List<double>();
        
        // 新增：线段可视化
        private readonly List<RectTransform> _holdSegLines = new();
        private RectTransform _holdPreviewLine; // 鼠标跟随的预览线
        private int _pendingAttachNoteIndex = -1;
        
        // 放到字段区其它 SerializeField 附近
        [Header("Hold Line Style")]
        [SerializeField] private Color holdSegColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private float holdSegThickness = 2f;

        [SerializeField] private Color holdPreviewColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private float holdPreviewThickness = 2f;
        
        #endregion

        #region 生命周期
        /* ───────────────── 生命周期 ───────────────── */
        protected override void Awake()
        {
            base.Awake();

            InitDropdown();
            BuildGridOnce();          // 网格仅生成一次
            RefreshNotes();           // 首次显示当前判定线
            
            if (playing == null) playing = FindFirstObjectByType<OnPlaying>();

            ChartManager.Instance.OnLineChanged += HandleLineChanged;
            
            // ② 在 Awake 里创建并订阅
            _onBpmListChangedHandler = () =>
            {
                BuildGridOnce();
                RefreshNotes();
            };
            ChartManager.Instance.OnBpmListChanged += _onBpmListChangedHandler;
            
            // Note 列表变了，就重新渲染
            ChartManager.Instance.OnContentChanged += () => RefreshNotes();
            
            // 1) 切换工具回调
            /*
            tapToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 0; });
            dragToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 1; });
            holdToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 2; });
            */
            
            // 原：if (isOn) currentTool = 0/1/2;
            tapToggle.onValueChanged.AddListener(isOn => { if (isOn) OnToolChanged(0); });
            dragToggle.onValueChanged.AddListener(isOn => { if (isOn) OnToolChanged(1); });
            holdToggle.onValueChanged.AddListener(isOn => { if (isOn) OnToolChanged(2); });

            // 默认选中 Tap
            tapToggle.isOn = true;
            
            ToolsInit();
            
            // 在 Grid 的 EventTrigger 上再绑定 PointerMove，用来更新 “预览四边形”
            var gridGO = grid.gameObject;
            var moveHandler = gridGO.AddComponent<GridPointerMoveHandler>();
            moveHandler.Init(this);
            
            // NoteEditorView.Awake() 里，InitDropdown/BuildGridOnce 之后加：
            var cm = ChartManager.Instance;
            cm.OnNoteAdded   += (li, n, idx) => { if (li == cm.currentLineIndex) RefreshNotes(); };
            cm.OnNoteRemoved += (li, n, idx) => { if (li == cm.currentLineIndex) RefreshNotes(); };
            cm.OnNoteUpdated += (li, n, idx) => { if (li == cm.currentLineIndex) RefreshNotes(); }; // 若有的话
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ChartManager.Instance != null)
            {
                // ④ 用同一个 handler 字段去解绑
                ChartManager.Instance.OnBpmListChanged -= _onBpmListChangedHandler;

                // ⑤ 用方法组解绑
                ChartManager.Instance.OnLineChanged -= HandleLineChanged;
            }
        }
#endregion
        
        void ToolsInit()
        {
            // —— 工具切换 —— 
            //selectToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 3; });
            // 原：if (isOn) currentTool = 3;
            selectToggle.onValueChanged.AddListener(isOn => { if (isOn) OnToolChanged(3); });
            
            // —— 始终允许滚轮滚动 —— 
            // 把 GridLayer（GridGraphic 所在物体）上的 EventTrigger 注册到 scrollRect
            var gridGO = grid.gameObject;
            var trigger = gridGO.GetComponent<EventTrigger>() ?? gridGO.AddComponent<EventTrigger>();
            
            // 滚轮滚动
            var scrollEntry = new EventTrigger.Entry { eventID = EventTriggerType.Scroll };
            scrollEntry.callback.AddListener(data => scrollRect.OnScroll((PointerEventData)data));
            trigger.triggers.Add(scrollEntry);
            // 右键拖拽
            var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener(data =>
            {
                var pd = (PointerEventData)data;
                if (pd.button == PointerEventData.InputButton.Right) 
                    scrollRect.OnDrag(pd);
            });
            trigger.triggers.Add(dragEntry);
        }
        
        
        
        void Update()
        {
            grid.SetCurrentBeat(playing.currentBeat);
            
            // 按回车也能结束
            if (_isCreatingHold && Input.GetKeyDown(KeyCode.Return))
            {
                FinalizeHold();
            }
            
            HandleDeleting();
        }
        
#region Note摆放
#region Hold逻辑
#region Hold显示

        // 创建一条 UI 线（RectTransform + Image）—— 修正锚点/枢轴/缩放
        RectTransform CreateLineGO(string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(noteLayer, false);

            var rt = go.GetComponent<RectTransform>();
            // 横 center, 竖 bottom
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition3D = Vector3.zero;
            rt.localScale         = Vector3.one;
            rt.localRotation      = Quaternion.identity;
            rt.sizeDelta          = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = color;

            return rt;
        }
        
        // 把一条线摆放为点 A→B（中心、长度、旋转）
        static void SetLine(RectTransform rt, Vector2 a, Vector2 b, float thickness)
        {
            Vector2 dir      = b - a;
            float   length   = dir.magnitude;
            float   angleRad = Mathf.Atan2(dir.y, dir.x);
            float   angleDeg = angleRad * Mathf.Rad2Deg;

            Vector2 mid = (a + b) * 0.5f;
            // 底部枢轴：把底边对齐到“线的中线”，需要沿“上线方向”回退半个厚度
            Vector2 up = new Vector2(-Mathf.Sin(angleRad), Mathf.Cos(angleRad));
            Vector2 pivotOffset = up * (thickness * 0.5f);

            rt.sizeDelta        = new Vector2(length, thickness);
            rt.localRotation    = Quaternion.Euler(0, 0, angleDeg);
            rt.anchoredPosition = mid - pivotOffset;
        }

        // Beat(拍)+横向 pos → 转成 UI 坐标
        Vector2 ToUI(Vector3Int beat, double posX)
        {
            float x = LanePosToX(posX);
            float y = (beat.x + (float)beat.z / beat.y) * grid.beatHeight;
            return new Vector2(x, y);
        }

        // 清理所有可视化线段
        void ClearHoldLines()
        {
            if (_holdPreviewLine != null) Destroy(_holdPreviewLine.gameObject);
            _holdPreviewLine = null;
            foreach (var rt in _holdSegLines)
                if (rt != null) Destroy(rt.gameObject);
            _holdSegLines.Clear();
        }
        
        // 清掉某个 Hold UI 下旧的可视化线段
        void DestroyChildHoldSegs(RectTransform holdRt)
        {
            var segs = holdRt.GetComponentsInChildren<HoldSegTag>(true);
            foreach (var seg in segs)
                if (seg != null) Destroy(seg.gameObject);
        }

// 按 Note.data 邻点生成线段并挂到 holdRt 下
        void RebuildHoldLinesFromData(RectTransform holdRt, Note holdNote)
        {
            var d = holdNote.data;
            if (d == null || d.Count < 2) return;

            for (int s = 0; s < d.Count - 1; s++)
            {
                var a = d[s];
                var b = d[s + 1];

                var seg = CreateLineGO("HoldSeg", holdSegColor);
                seg.gameObject.AddComponent<HoldSegTag>();

                // 先按全局 UI 坐标摆好
                SetLine(
                    seg,
                    ToUI(a.hitBeat, a.position),
                    ToUI(b.hitBeat, b.position),
                    holdSegThickness
                );

                // 再改父到这个 Hold 方块下，保持屏幕位置不变
                seg.SetParent(holdRt, worldPositionStays: true);
                seg.SetAsLastSibling();
            }
        }
        
#endregion Hold显示
        private void FinalizeHold()
        {
            if (!_isCreatingHold) return;
            _isCreatingHold = false;

            var mgr     = ChartManager.Instance;
            int lineIdx = mgr.currentLineIndex;
            var line    = mgr.gameData.content.judgmentLines[lineIdx];

            // 用两个列表一起构造 NoteData
            var dataList = _holdBeats
                .Select((beat, i) => new NoteData(beat, _holdPosXs[i]))
                .ToList();

            var note = new Note
            {
                type       = 2,
                appearBeat = _holdBeats[0],
                speed      = 1f,
                data       = dataList
            };

            /*
            var newNotes = new List<Note>(line.notes) { note };
            line.notes   = newNotes.ToArray();
            mgr.NotifyContentChanged();
            int insertIndex = mgr.gameData.content.judgmentLines[lineIdx].notes.Length;
            mgr.AddNote(lineIdx, note, insertIndex);
            */
            
            // 2) 只走 AddNote（不要再直接改 line.notes）
            int insertIndex = line.notes.Length;           // 追加到末尾
            mgr.AddNote(lineIdx, note, insertIndex);
            _pendingAttachNoteIndex = insertIndex;         // 记住：等刷新时把线段挂到这个 Note 的 UI 上
            mgr.NotifyContentChanged();

            // 3) 清理“预览线”，但保留已固化的段（_holdSegLines）
            if (_holdPreviewLine != null) Destroy(_holdPreviewLine.gameObject);
            _holdPreviewLine = null;

            if (_holdPreviewQuad != null) Destroy(_holdPreviewQuad.gameObject);
            _holdPreviewQuad = null;
            /*
            ClearHoldLines();
            Destroy(_holdPreviewQuad.gameObject);
            */
            _holdBeats.Clear();
            _holdPosXs.Clear();
        }

        private void CancelHold()
        {
            _isCreatingHold = false;
            _holdBeats.Clear();
            _holdPosXs.Clear();
            _pendingAttachNoteIndex = -1; // 新增：避免后续错误挂靠
            if (_holdPreviewQuad != null)
            {
                Destroy(_holdPreviewQuad.gameObject);
            }
            ClearHoldLines();
        }
        
        private void CreateHoldPreview()
        {
            // Clean up any old preview
            if (_holdPreviewQuad != null)
                Destroy(_holdPreviewQuad.gameObject);

            // New GameObject with RectTransform + CanvasRenderer + Image
            var go = new GameObject("HoldPreview",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(UnityEngine.UI.Image));

            go.transform.SetParent(noteLayer, false);
            _holdPreviewQuad = go.GetComponent<RectTransform>();

            // Tint it
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.3f);
            
            ClearHoldLines(); // 保守起见先清掉旧的
            //_holdPreviewLine = CreateLineGO("HoldPreviewLine", new Color(1f, 1f, 1f, 0.35f));
            // 原： _holdPreviewLine = CreateLineGO("HoldPreviewLine", new Color(1f, 1f, 1f, 0.35f));
            _holdPreviewLine = CreateLineGO("HoldPreviewLine", holdPreviewColor);
        }
        
        private void AddPreviewSegment(Vector3Int a, double ax, Vector3Int b, double bx)
        {
            var seg = CreateLineGO("HoldSeg", holdSegColor);
            seg.gameObject.AddComponent<HoldSegTag>();
            SetLine(seg, ToUI(a, ax), ToUI(b, bx), holdSegThickness); // 原来用 grid.minorLineWidth
            _holdSegLines.Add(seg);
        }

        private void AddPreviewSegment(Vector3Int a, Vector3Int b)
        {
            // 在 _holdPreviewQuad 下再生成一个子 Quad，或者用 Mesh 更新顶点列表
            // 这里演示简单起见，把 _holdPreviewQuad 当成整条线：重新计算包围 a→b 的平行四边形
            UpdatePreviewSegment(a, b);
        }

        private void UpdatePreviewSegment(Vector3Int a, Vector3Int b)
        {
            if (_holdPreviewQuad == null) return;

            // 先算出 a/b 在_holdBeats里的 index
            int idxA = _holdBeats.IndexOf(a);
            int idxB = _holdBeats.IndexOf(b);
            if (idxA < 0 || idxB < 0) return;

            // 用保存在 _holdPosXs 的横向坐标
            float x0 = LanePosToX(_holdPosXs[idxA]);
            float x1 = LanePosToX(_holdPosXs[idxB]);

            // 纵向：beatInt + frac 乘高度
            // (a.x 是 beatInt, a.y 是 beatDen, a.z 是 beatNum)
            float y0 = a.x * grid.beatHeight + ((float)a.z / a.y) * grid.beatHeight;
            float y1 = b.x * grid.beatHeight + ((float)b.z / b.y) * grid.beatHeight;

            // 计算中心、宽高、旋转……
            Vector2 center = new Vector2((x0 + x1) * 0.5f, (y0 + y1) * 0.5f);
            Vector2 size   = new Vector2(Mathf.Abs(x1 - x0), grid.minorLineWidth);
            float   angle  = Mathf.Atan2(y1 - y0, x1 - x0) * Mathf.Rad2Deg;

            var rt = _holdPreviewQuad;
            rt.anchoredPosition = center;
            rt.sizeDelta        = size;
            rt.localRotation    = Quaternion.Euler(0, 0, angle);
        }
#endregion

        // 当用户切换到不是 Hold 的工具时，要自动取消预览
        private void OnToolChanged(int newTool)
        {
            if (_isCreatingHold && newTool != 2)
            {
                CancelHold();
            }
            currentTool = newTool;
        }

        void HandleDeleting()
        {
            // 2) 如果右键正在按住，且不是“选择”工具，就检测划过删除
            if (Input.GetMouseButton(1) && currentTool != selectToolIndex)
            {
                // 准备一个 PointerEventData 去 RaycastAll
                PointerEventData pd = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pd, results);

                foreach (var rr in results)
                {
                    // ★ 改为从点击对象向上找 NoteUI，避免点到别的 Graphic
                    var noteUI = rr.gameObject.GetComponentInParent<NoteUI>();
                    if (noteUI != null)
                    {
                        DeleteNoteByUI(noteUI);  // 用准确 UI 删除
                        break;
                    }
                }
            }
        }
        
        // ★ 新增：按“具体被点中的 UI”来删
        // 精准删除：先清可视化线，再把这个 UI 从对象池移除并销毁，最后删数据并刷新
        private void DeleteNoteByUI(NoteUI noteUI)
        {
            // 1) 清掉这颗 Note 下的线段可视化
            foreach (var seg in noteUI.GetComponentsInChildren<HoldSegTag>(true))
                Destroy(seg.gameObject);

            // 2) 从对应的对象池移除并销毁这个具体 UI（避免“删 A 复用到 B”的错觉）
            var rt = noteUI.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (holdPool.Contains(rt)) holdPool.Remove(rt);
                if (dragPool.Contains(rt)) dragPool.Remove(rt);
                if (tapPool.Contains(rt))  tapPool.Remove(rt);
                Destroy(rt.gameObject);
            }

            // 3) 删数据并刷新
            var mgr = ChartManager.Instance;
            int lineIdx = mgr.currentLineIndex;
            mgr.RemoveNote(lineIdx, noteUI.dataIndex);
            mgr.NotifyContentChanged();
        }
        
        // 在类里加上这一行
        private void HandleLineChanged(int lineIndex)
        {
            RefreshNotes();
        }
        
        // 2) 由 GridLayer 的 EventTrigger 调用
        public void OnGridClicked(BaseEventData data)
        {
            if (currentTool == 3) return; // 选择工具时不创建

            var p = (PointerEventData)data;
            
            if (p.button != PointerEventData.InputButton.Left)
                return;
            
            // 1) 计算插入到哪一行、哪一个位置（这里只示例追加到末尾）
            var mgr = ChartManager.Instance;
            int lineIdx = mgr.currentLineIndex;
            var line    = mgr.gameData.content.judgmentLines[lineIdx];
            
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content, p.position, p.pressEventCamera, out local);
            
            // 计算当前击打位置
            Vector3Int hitBeat = ComputeHitBeat(local);
            double     posX    = ComputePosX(local);
            
            // —— Hold 模式 —— 
            if (currentTool == 2)
            {
                if (!_isCreatingHold)
                {
                    // 第一次点击：启动 Hold 创建
                    _isCreatingHold = true;
                    _holdBeats.Clear();
                    _holdBeats.Add(hitBeat);
                    _holdPosXs.Add(posX);
                    CreateHoldPreview(); // 创建空的预览 Quad
                    
                    // 刚创建时，让预览线长度为 0（起点=终点）
                    //SetLine(_holdPreviewLine, ToUI(hitBeat, posX), ToUI(hitBeat, posX), grid.minorLineWidth);
                    SetLine(_holdPreviewLine, ToUI(hitBeat, posX), ToUI(hitBeat, posX), holdPreviewThickness);
                }
                else
                {
                    // 再次点击：大于上一次才算新点；否则结束
                    // （在 OnGridClicked 的 Hold 分支里）点击第二次以后：
                    var prevBeat = _holdBeats[^1];
                    if (FractionToDecimal(hitBeat) > FractionToDecimal(prevBeat))
                    {
                        var prevPosX = _holdPosXs[^1];

                        // 固化一段：prev → 当前点击
                        AddPreviewSegment(prevBeat, prevPosX, hitBeat, posX);

                        _holdBeats.Add(hitBeat);
                        _holdPosXs.Add(posX);

                        // 预览线重置为 0 长度，等待鼠标移动刷新
                        if (_holdPreviewLine != null)
                            SetLine(_holdPreviewLine, ToUI(hitBeat, posX), ToUI(hitBeat, posX), grid.minorLineWidth);
                    }
                    else
                    {
                        FinalizeHold();
                    }
                }
                return;
            }

            // 横向
            //float rawX = local.x / grid.laneWidth - grid.laneCount / 2f;
            //int vSteps = Mathf.RoundToInt(rawX * grid.vSubdiv);
            //double notePosX = (double)vSteps / grid.vSubdiv;

            // 纵向细分计算
            //float rawY = local.y / grid.beatHeight;
            //int hSteps = Mathf.RoundToInt(rawY * grid.hSubdiv);
            //int beatInt = Mathf.FloorToInt(hSteps / (float)grid.hSubdiv);
            //int beatNum = hSteps - beatInt * grid.hSubdiv;
            //int beatDen = grid.hSubdiv;
            //var hitBeat = new Vector3Int(beatInt, beatDen, beatNum);

            // 写入 GameData
            /*
            var mgr = ChartManager.Instance;
            var gd  = mgr.gameData;
            int lineIdx = mgr.currentLineIndex;
            var line    = gd.content.judgmentLines[lineIdx];
            */

            var newNote = new Note
            {
                type       = currentTool,
                appearBeat = hitBeat,
                speed      = 1f,
                data       = new List<NoteData>
                {
                    new NoteData(hitBeat, posX)
                }
            };

            int insertIndex = line.notes.Length;
            mgr.AddNote(lineIdx, newNote, insertIndex);

            /*
            var notes = new List<Note>(line.notes) { newNote };
            line.notes = notes.ToArray();
            */

            // 触发内容变更，NoteEditor 会刷新
            mgr.NotifyContentChanged();
        }
        
        /// <summary>
        /// 把 local（相对于 content 的本地坐标）转换成击打节拍 Vector3Int(beatInt, beatDen, beatNum)
        /// </summary>
        private Vector3Int ComputeHitBeat(Vector2 local)
        {
            // 纵向：local.y / 每拍高度，得到细分格数
            float rawY = local.y / grid.beatHeight;
            int   hSteps = Mathf.RoundToInt(rawY * grid.hSubdiv);
            int   beatInt = Mathf.FloorToInt(hSteps / (float)grid.hSubdiv);
            int   beatNum = hSteps - beatInt * grid.hSubdiv;
            int   beatDen = grid.hSubdiv;
            return new Vector3Int(beatInt, beatDen, beatNum);
        }

        /// <summary>
        /// 把 local（相对于 content 的本地坐标）转换成横向位置（0...laneCount 之间的分数）
        /// </summary>
        private double ComputePosX(Vector2 local)
        {
            // 横向：local.x / 每通道宽度，减去半宽度把 0 点移到中间
            float rawX = local.x / grid.laneWidth - grid.laneCount / 2f;
            // 乘以辅助细分，再四舍五入到最近的细分格
            int   vSteps = Mathf.RoundToInt(rawX * grid.vSubdiv);
            // 再除以细分数得到[–laneCount/2, +laneCount/2]范围内的小数
            return (double)vSteps / grid.vSubdiv;
        }
        
        // —— 鼠标移动时更新最后一段的预览 —— 
        public void OnGridPointerMove(PointerEventData pd)
        {
            if (!_isCreatingHold || currentTool != 2) return;

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content, pd.position, pd.pressEventCamera, out local);
            
            var lastBeat = _holdBeats[^1];
            var currBeat = ComputeHitBeat(local);
            UpdatePreviewSegment(lastBeat, currBeat);
            
            // 起点：最后一个已确定的点
            var aBeat = _holdBeats[^1];
            var aPosX = _holdPosXs[^1];

            // 终点：鼠标所在“临时点”
            var bBeat = ComputeHitBeat(local);
            var bPosX = ComputePosX(local);
            
            if (_holdPreviewLine == null)
                _holdPreviewLine = CreateLineGO("HoldPreviewLine", new Color(1f,1f,1f,0.35f));

            //SetLine(_holdPreviewLine, ToUI(aBeat, aPosX), ToUI(bBeat, bPosX), grid.minorLineWidth);
            SetLine(_holdPreviewLine, ToUI(aBeat, aPosX), ToUI(bBeat, bPosX), holdPreviewThickness);
        }
        
        /* ───────────────── Note 刷新 ───────────────── */
        void RefreshNotes()
        {
            // ★ 新增：确保布局完成，Rect 正确
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            LayoutRebuilder.ForceRebuildLayoutImmediate(noteLayer);
            
            // 1. 先全部隐藏
            DeactivatePool(tapPool);
            DeactivatePool(dragPool);
            DeactivatePool(holdPool);

            // 2. 当前判定线
            var lines = ChartManager.Instance.gameData.content.judgmentLines;
            int idx   = ChartManager.Instance.currentLineIndex;
            if (idx < 0 || idx >= lines.Length) return;

            var line     = lines[idx];
            int tapIdx   = 0;
            int dragIdx  = 0;
            int holdIdx  = 0;

            for(int i = 0; i < line.notes.Length; i++)
            {
                var n = line.notes[i];
                if (n.data == null || n.data.Count == 0) continue;

                double beat  = FractionToDecimal(n.data[0].hitBeat);
                double posX  = n.data[0].position;

                float x = LanePosToX(posX);
                float y = (float)beat * grid.beatHeight;

                RectTransform rt = null;
                switch (n.type)
                {
                    case 0: // Tap
                        rt = Spawn(tapPool, tapPrefab, tapIdx++);
                        break;
                    case 1: // Drag 起点
                        rt = Spawn(dragPool, dragPrefab, dragIdx++);
                        break;
                    case 2: // Hold 起点
                        rt = Spawn(holdPool, holdPrefab, holdIdx++);
                        break;
                    default:
                        continue;   // 其他类型忽略
                }
                rt.anchoredPosition = new Vector2(x, y);
                // ★ 新增：现在父物体位置已确定，再挂线就不会二次偏移
                if (n.type == 2)
                {
                    if (_pendingAttachNoteIndex == i)
                    {
                        DestroyChildHoldSegs(rt);
                        foreach (var seg in _holdSegLines)
                        {
                            if (seg == null) continue;
                            seg.SetParent(rt, worldPositionStays: true);
                            seg.SetAsLastSibling();
                        }
                        _holdSegLines.Clear();
                        _pendingAttachNoteIndex = -1;
                    }
                    else
                    {
                        DestroyChildHoldSegs(rt);
                        RebuildHoldLinesFromData(rt, n); // 用 Note.data 邻点重建线
                    }
                }
                
                var noteUI = rt.gameObject.GetComponent<NoteUI>() ?? rt.gameObject.AddComponent<NoteUI>();
                noteUI.dataIndex = i; // i 是循环里的 note 索引
            }
        }
        
        /* —— 对象池 —— */
        static void DeactivatePool(List<RectTransform> pool)
        {
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                var rt = pool[i];
                if (rt == null) { pool.RemoveAt(i); continue; }
                rt.gameObject.SetActive(false);
            }
        }

        RectTransform Spawn(List<RectTransform> pool, RectTransform prefab, int idx)
        {
            if (idx >= pool.Count)
                pool.Add(Instantiate(prefab, noteLayer));
            var rt = pool[idx];
            rt.gameObject.SetActive(true);
            return rt;
        }
        
        /// <summary>
        /// 删除当前判定线 notes 中索引为 dataIndex 的 Note，然后刷新
        /// </summary>
        public void DeleteNote(int dataIndex)
        {
            /*
            var cm = ChartManager.Instance;
            var line = cm.gameData.content.judgmentLines[cm.currentLineIndex];
            var list = new List<Note>(line.notes);
            if (dataIndex >= 0 && dataIndex < list.Count)
            {
                list.RemoveAt(dataIndex);
                line.notes = list.ToArray();

                // 同步数据变更，刷新 UI
                cm.NotifyContentChanged();
            }
            */
            
            var mgr = ChartManager.Instance;
            int lineIdx = mgr.currentLineIndex;
            
            // ★ 新增：找到这个 dataIndex 对应的 Hold UI，并清它下面的 HoldSeg
            RectTransform targetUI = null;
            foreach (var rt in holdPool)
            {
                if (rt == null || !rt.gameObject.activeSelf) continue;
                var ui = rt.GetComponent<NoteUI>();
                if (ui != null && ui.dataIndex == dataIndex)
                {
                    targetUI = rt;
                    break;
                }
            }
            if (targetUI != null)
            {
                var segs = targetUI.GetComponentsInChildren<HoldSegTag>(true);
                foreach (var seg in segs)
                    Destroy(seg.gameObject);
            }

            // 调用 ChartManager.RemoveNote
            mgr.RemoveNote(lineIdx, dataIndex);
            
            // 触发内容变更，NoteEditor 会刷新
            mgr.NotifyContentChanged();
        }
#endregion

        /* ───────────────── 网格：只建一次 ───────────────── */
        void BuildGridOnce()
        {
            grid.laneCount = ChartManager.Instance.laneCount;
            grid.beatCount = ChartManager.Instance.CalcTotalBeatsRounded();
            grid.RebuildGrid();                       // 已同步 Content 尺寸
        }

        /* 若 BPM/乐曲变动，可手动调用 */
        public void RebuildGrid()
        {
            BuildGridOnce();
            RefreshNotes();
        }

        /* ───────────────── Dropdown 初始化 ───────────────── */
        void InitDropdown()
        {
            var lines = ChartManager.Instance.gameData.content.judgmentLines;
            var opts  = new List<TMP_Dropdown.OptionData>(lines.Length);
            for (int i = 0; i < lines.Length; i++)
                opts.Add(new TMP_Dropdown.OptionData($"{i}"));
            lineDropdown.options = opts;
            lineDropdown.value   = ChartManager.Instance.currentLineIndex;
            lineDropdown.onValueChanged.AddListener(ChartManager.Instance.SetLine);
        }

        /* ───────────────── 工具方法 ───────────────── */

        float LanePosToX(double pos)
        {
            int lanes = ChartManager.Instance.laneCount;
            float half = lanes / 2f;
            return (float)((pos) * grid.laneWidth);
        }
    }
    
    public class GridPointerMoveHandler : MonoBehaviour, IPointerMoveHandler
    {
        private NoteEditorView _view;
        public void Init(NoteEditorView view) { _view = view; }

        public void OnPointerMove(PointerEventData eventData)
        {
            _view.OnGridPointerMove(eventData);
        }
    }
}


