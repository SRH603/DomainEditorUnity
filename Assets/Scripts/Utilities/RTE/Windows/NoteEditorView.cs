using System.Collections.Generic;
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
        protected override void Start()
        {
            base.Start();
        }
        
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
            tapToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 0; });
            dragToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 1; });
            holdToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 2; });

            // 默认选中 Tap
            tapToggle.isOn = true;
            
            ToolsInit();
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

        void ToolsInit()
        {
            // —— 工具切换 —— 
            selectToggle.onValueChanged.AddListener(isOn => { if (isOn) currentTool = 3; });
            
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
            HandleDeleting();
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
                    // 找到第一个 NoteUI，就删它
                    var noteUI = rr.gameObject.GetComponent<NoteUI>();
                    if (noteUI != null)
                    {
                        DeleteNote(noteUI.dataIndex);
                        break;
                    }
                }
            }
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

            // 横向
            float rawX = local.x / grid.laneWidth - grid.laneCount / 2f;
            int vSteps = Mathf.RoundToInt(rawX * grid.vSubdiv);
            double notePosX = (double)vSteps / grid.vSubdiv;

            // 纵向细分计算
            float rawY = local.y / grid.beatHeight;
            int hSteps = Mathf.RoundToInt(rawY * grid.hSubdiv);
            int beatInt = Mathf.FloorToInt(hSteps / (float)grid.hSubdiv);
            int beatNum = hSteps - beatInt * grid.hSubdiv;
            int beatDen = grid.hSubdiv;
            var hitBeat = new Vector3Int(beatInt, beatDen, beatNum);

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
                    new NoteData(hitBeat, notePosX)
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

        /* ───────────────── Note 刷新 ───────────────── */
        void RefreshNotes()
        {
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
                
                var noteUI = rt.gameObject.GetComponent<NoteUI>() ?? rt.gameObject.AddComponent<NoteUI>();
                noteUI.dataIndex = i; // i 是循环里的 note 索引
            }
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

        /* —— 对象池 —— */
        static void DeactivatePool(List<RectTransform> pool)
        {
            foreach (var rt in pool) rt.gameObject.SetActive(false);
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

            // 调用 ChartManager.RemoveNote
            mgr.RemoveNote(lineIdx, dataIndex);
            
            // 触发内容变更，NoteEditor 会刷新
            mgr.NotifyContentChanged();
        }
    }
}


