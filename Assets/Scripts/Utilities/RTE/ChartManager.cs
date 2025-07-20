using System;
using UnityEngine;

/// <summary>负责保存 GameData、音乐、BPM 信息，并广播“判定线切换”事件。</summary>
    public class ChartManager : MonoBehaviour
    {
        public static ChartManager Instance { get; private set; }

        [Header("资源")]
        public GameData gameData;          // ScriptableObject
        public AudioSource levelMusic;     // AudioSource 内含 clip
        public GameUtilities.BPMTimingList bpmTimingList;

        [Header("全局设置")]
        public int laneCount = 8;          // 横向格子数
        public int currentLineIndex = 0;   // 默认第 0 条判定线

        public event Action<int> OnLineChanged;    // 参数 = 新的判定线索引
        
        /// <summary>bpmList 被修改后触发，NoteEditor 监听该事件重建网格</summary>
        public event Action OnBpmListChanged;
        
        // —— 新增：内容变更事件 —— 
        /// <summary>当 Note 列表（content）改变时触发</summary>
        public event Action OnContentChanged;

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
            if (levelMusic != null && levelMusic.clip != null)
            {
                //Debug.Log(levelMusic.clip.length);
                LoadBpmList();
                float raw = Time2Beat(levelMusic.clip.length);
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
    }
