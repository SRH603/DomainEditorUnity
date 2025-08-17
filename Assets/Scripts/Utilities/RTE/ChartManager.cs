using System;
using UnityEngine;
using System;
using System.Collections.Generic;
using Battlehub.RTCommon;
using System.Reflection;


/// <summary>负责保存 GameData、音乐、BPM 信息，并广播“判定线切换”事件。</summary>
    public class ChartManager : MonoBehaviour
    {
        private static readonly MemberInfo kNotesMember =
            typeof(JudgmentLine).GetField("notes");
        
        public static ChartManager Instance { get; private set; }

        [Header("资源")]
        public GameData gameData;          // ScriptableObject
        public AudioClip levelMusic;     // AudioSource 内含 clip
        public GameUtilities.BPMTimingList bpmTimingList;

        [Header("全局设置")]
        public int laneCount = 8;          // 横向格子数
        public int currentLineIndex = 0;   // 默认第 0 条判定线

        public event Action<int> OnLineChanged;    // 参数 = 新的判定线索引
        
        /// <summary>bpmList 被修改后触发，NoteEditor 监听该事件重建网格</summary>
        public event Action OnBpmListChanged;
        
        public event Action OnOffsetChanged;
        
        // —— 新增：内容变更事件 —— 
        /// <summary>当 Note 列表（content）改变时触发</summary>
        public event Action OnContentChanged;
        
        // —— 增量音符事件 —— 
        public event Action<int, Note, int> OnNoteAdded;
        public event Action<int, Note, int> OnNoteRemoved;
        public event Action<int, Note, int> OnNoteUpdated;
        public event Action<int> OnJudgmentLineChanged;
        
        // —— 监视 bpmList 的引用变化，配合撤销/重做自动广播刷新 —— 
        private BPMList[] _bpmListSnapshot;
        
        // —— offset 变化快照 —— 
        private float _offsetSnapshot;
        private bool  _offsetSnapInited;

        
        /// <summary>
        /// 在第 lineIndex 条判定线上，index 位置插入一个 note
        /// </summary>
        public void AddNote(int lineIndex, Note note, int insertIndex)
        {
            var rte  = IOC.Resolve<IRTE>();                             //  [oai_citation:0‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)
            var undo = rte.Undo;                                        //  [oai_citation:1‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)

            var line = gameData.content.judgmentLines[lineIndex];

            undo.BeginRecord();                                         // 开始打包一步   [oai_citation:2‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)
            undo.BeginRecordValue(line, kNotesMember);                  // 记录修改前值   [oai_citation:3‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)
            {
                var list = new List<Note>(line.notes);
                if (insertIndex < 0 || insertIndex > list.Count) insertIndex = list.Count;
                list.Insert(insertIndex, note);
                line.notes = list.ToArray();                            // 调到①里的属性 -> 自动刷新 UI
            }
            undo.EndRecordValue(line, kNotesMember);                    // 记录修改后值   [oai_citation:4‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)
            undo.EndRecord();                                           // 压入撤销栈      [oai_citation:5‡rteditor.battlehub.net](https://rteditor.battlehub.net/v20/infrastructure/)
            
            // ✅ 更新快照，避免监视器在下一帧再次把这次“自己加的”当成外部变化
            if (_notesSnapshots != null && lineIndex < _notesSnapshots.Length)
                _notesSnapshots[lineIndex] = line.notes;

            OnNoteAdded?.Invoke(lineIndex, note, insertIndex);          // 你已有的事件
            OnContentChanged?.Invoke();
            // 不必再手动 Notify，属性 setter 已经发了；留着也无妨
        }

        /// <summary>
        /// 在第 lineIndex 条判定线上，删除 index 位置的 note
        /// </summary>
        
        public void RemoveNote(int lineIndex, int noteIndex)
        {
            var rte  = IOC.Resolve<IRTE>();
            var undo = rte.Undo;

            var line = gameData.content.judgmentLines[lineIndex];
            if (line.notes == null || noteIndex < 0 || noteIndex >= line.notes.Length) return;

            var removed = line.notes[noteIndex];

            undo.BeginRecord();
            undo.BeginRecordValue(line, kNotesMember);
            {
                var list = new List<Note>(line.notes);
                list.RemoveAt(noteIndex);
                line.notes = list.ToArray();                            // 属性 -> 自动刷新 UI
            }
            undo.EndRecordValue(line, kNotesMember);
            undo.EndRecord();
            
            // ✅ 更新快照，避免监视器在下一帧再次把这次“自己加的”当成外部变化
            if (_notesSnapshots != null && lineIndex < _notesSnapshots.Length)
                _notesSnapshots[lineIndex] = line.notes;

            OnNoteRemoved?.Invoke(lineIndex, removed, noteIndex);
            OnContentChanged?.Invoke();
        }


        /// <summary>
        /// 更新第 lineIndex 号判定线第 noteIndex 个 note
        /// </summary>
        public void UpdateNote(int lineIndex, Note note, int noteIndex)
        {
            var list = gameData.content.judgmentLines[lineIndex].notes;
            list[noteIndex] = note;
            OnNoteUpdated?.Invoke(lineIndex, note, noteIndex);
            OnContentChanged?.Invoke();
        }

        /// <summary>
        /// 整条判定线的参数（如 flowSpeed）变更
        /// </summary>
        public void NotifyJudgmentLineChanged(int lineIndex)
        {
            OnJudgmentLineChanged?.Invoke(lineIndex);
            OnContentChanged?.Invoke();
        }

        /// <summary>修改 bpmList 时调用</summary>
        public void NotifyBpmListChanged()
        {
            OnBpmListChanged?.Invoke();
        }
        
        /// <summary>
        /// 当你在 Grid 上点击新增 Note、
        /// 或者删除／移动 Note 后调用这个方法。
        /// </summary>
        public void NotifyContentChanged()
        {
            OnContentChanged?.Invoke();
        }
        
        /* ——— 生命周期 ——— */
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            gameData = DechHub.Instance.GetGameData();
            levelMusic = DechHub.Instance.GetAudioClip();
            
            InitBpmSnapshot();
            InitOffsetSnapshot();
        }
        
        public void InitBpmSnapshot()
        {
            _bpmListSnapshot = gameData != null && gameData.content != null ? gameData.content.bpmList : null;
        }

        public void InitOffsetSnapshot()
        {
            if (gameData != null && gameData.info != null)
            {
                _offsetSnapshot   = gameData.info.offset;
                _offsetSnapInited = true;
            }
        }


        /* ——— 公共 API ——— */
        public void SetLine(int idx)
        {
            if (idx == currentLineIndex) return;
            currentLineIndex = idx;
            OnLineChanged?.Invoke(idx);
        }

        public float Time2Beat(float time)
        {
            float beat = 0;
            for (int i = 0; i < bpmTimingList.Changes.Count; i++)
            {
                var cur = bpmTimingList.Changes[i];
                var next = (i < bpmTimingList.Changes.Count - 1) ? bpmTimingList.Changes[i + 1] : null;

                float segmentEnd = next != null ? next.StartTime : time;
                if (time < segmentEnd)
                {
                    beat += (time - cur.StartTime) * (cur.BPM / 60f);
                    break;
                }
                else
                {
                    beat += (segmentEnd - cur.StartTime) * (cur.BPM / 60f);
                }
            }
            return beat;
        }

        public int CalcTotalBeatsRounded()
        {
            if (levelMusic != null && levelMusic != null)
            {
                //Debug.Log(levelMusic.clip.length);
                LoadBpmList();
                float raw = Time2Beat(levelMusic.length);
                return Mathf.CeilToInt(raw) + 8;          // 向上取整再 +8
            }
            else
            {
                Debug.LogWarning("LevelMusic.clip 为空！");
                return 64;                                // 兜底
            }
        }
        
        private void LoadBpmList()
        {
            if (bpmTimingList.Changes.Count == 0)
            {
                double generatingtime = 0;

                int i = 0;
                foreach (var bpmTiming in gameData.content.bpmList)
                {

                    bpmTimingList.Changes.Add(new GameUtilities.BPMTiming(bpmTiming.bpm, (float)generatingtime));
                    if (gameData.content.bpmList.Length == i + 1)
                    {
                        break;
                    }
                    else
                    {
                        generatingtime += ((GameUtilities.FractionToDecimal(gameData.content.bpmList[i + 1].startBeat) - GameUtilities.FractionToDecimal(bpmTiming.startBeat)) / (bpmTiming.bpm / 60));
                    }
                    i++;
                }
            }
        }
        
        // ChartManager.cs 里（字段区）
        private Note[][] _notesSnapshots;
        private bool _snapInited;

        // 在合适时机（GameData/谱面就绪后）调用一次
        public void InitNotesSnapshots()
        {
            var lines = gameData.content.judgmentLines;
            _notesSnapshots = new Note[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
                _notesSnapshots[i] = lines[i].notes;
            _snapInited = true;
        }

        // 轻量监视（不分配、不遍历 Note 内容，先看数组引用/长度）
        private void LateUpdate()
        {
            if (!_snapInited || gameData == null || gameData.content == null) return;

            var lines = gameData.content.judgmentLines;
            for (int li = 0; li < lines.Length; li++)
            {
                var oldArr = _notesSnapshots[li];
                var newArr = lines[li].notes;

                if (!ReferenceEquals(oldArr, newArr))
                {
                    EmitNoteDelta(li, oldArr, newArr);
                    _notesSnapshots[li] = newArr; // 更新快照，防止重复
                }
            }
            
            // —— 监视 bpmList 变化（包括撤销/重做或其它面板保存）——
            if (gameData != null && gameData.content != null)
            {
                var cur = gameData.content.bpmList;
                if (!object.ReferenceEquals(cur, _bpmListSnapshot))
                {
                    _bpmListSnapshot = cur;
                    // 清一次缓存并按需要重建
                    if (bpmTimingList != null) bpmTimingList.Changes.Clear();
                    NotifyBpmListChanged(); // 触发 GenerateLevel / NoteEditorView 刷新
                }
            }
            
            // —— 侦测 Info.offset 是否变化（包括 Undo / Redo）——
            if (!_offsetSnapInited && gameData != null && gameData.info != null)
            {
                _offsetSnapshot   = gameData.info.offset;
                _offsetSnapInited = true;
            }
            if (_offsetSnapInited && gameData != null && gameData.info != null)
            {
                float cur = gameData.info.offset;
                if (!Mathf.Approximately(cur, _offsetSnapshot))
                {
                    _offsetSnapshot = cur;
                    // 与 bpmList 一样复用刷新事件，驱动 GenerateLevel / NoteEditorView 重建
                    //OnBpmListChanged?.Invoke();
                    // 如需区分，可另外再暴露 OnOffsetChanged?.Invoke();
                    OnOffsetChanged?.Invoke();
                }
            }


        }

        private void EmitNoteDelta(int lineIndex, Note[] oldArr, Note[] newArr)
        {
            int oldLen = oldArr?.Length ?? 0;
            int newLen = newArr?.Length ?? 0;

            if (newLen == oldLen + 1)
            {
                int idx = FirstDiffIndex(oldArr, newArr);
                if (idx < 0) idx = oldLen; // 追加在末尾
                OnNoteAdded?.Invoke(lineIndex, newArr[idx], idx);
            }
            else if (newLen + 1 == oldLen)
            {
                int idx = FirstDiffIndex(oldArr, newArr);
                if (idx < 0) idx = newLen; // 删除末尾
                OnNoteRemoved?.Invoke(lineIndex, oldArr[idx], idx);
            }
            else
            {
                // 出现重排或批量变化，兜底一次全量刷新（仍比每次增删都全量好很多）
                OnContentChanged?.Invoke();
            }
        }

        private static int FirstDiffIndex(Note[] a, Note[] b)
        {
            int n = Mathf.Min(a?.Length ?? 0, b?.Length ?? 0);
            for (int i = 0; i < n; i++)
                if (!ReferenceEquals(a[i], b[i])) return i;
            return -1;
        }
        
        private static readonly FieldInfo kBpmListField =
            typeof(Content).GetField("bpmList", BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// 覆盖 bpmList（压入 Undo 栈），并广播刷新；如果 Dech 会话已打开，则同时写盘。
        /// </summary>
        public void ApplyBpmListWithUndo(BPMList[] newList, bool saveDech = true)
        {
            if (gameData == null)
            {
                Debug.LogWarning("[ChartManager] ApplyBpmListWithUndo: gameData 为空。");
                return;
            }
            if (gameData.content == null) gameData.content = new Content();

            var rte  = IOC.IsRegistered<IRTE>() ? IOC.Resolve<IRTE>() : null;
            var undo = rte != null ? rte.Undo : null;

            if (undo != null && kBpmListField != null)
            {
                undo.BeginRecord();
                undo.BeginRecordValue(gameData.content, kBpmListField);
                gameData.content.bpmList = newList;
                undo.EndRecordValue(gameData.content, kBpmListField);
                undo.EndRecord();
            }
            else
            {
                gameData.content.bpmList = newList;
            }

            // 更新快照，避免下一帧被当作“外部变化”再次触发
            _bpmListSnapshot = newList;

            // 清 bpm 计算缓存
            if (bpmTimingList != null) bpmTimingList.Changes.Clear();

            // 广播：刷新所有依赖
            NotifyBpmListChanged();

            if (saveDech)
            {
                try
                {
                    DechHub.Instance?.Save();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[ChartManager] 写盘失败（已应用到内存）： " + ex.Message);
                }
            }

            Debug.Log("[ChartManager] 已应用 bpmList（含 Undo），并广播刷新。");
        }

    }
