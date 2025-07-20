using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayControlUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public Slider progressSlider;
    [SerializeField] public Button playButton;
    [SerializeField] public Button stopButton;

    [Header("Gameplay")]
    [SerializeField] private OnPlaying playing;

    private bool isDragging;
    private float bookmark;

    public void Init()
    {
        if (playing == null) playing = FindFirstObjectByType<OnPlaying>();

        // Slider 范围
        progressSlider.minValue = 0f;
        progressSlider.maxValue = playing.LevelMusic.clip.length;
        progressSlider.value    = 0f;

        // onValueChanged 里只在拖动时同步
        progressSlider.onValueChanged.AddListener(val => {
            if (isDragging) Seek(val);
        });

        // 按钮
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);

        // 给 slider 加 EventTrigger（或取到已有的）
        var trigger = progressSlider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = progressSlider.gameObject.AddComponent<EventTrigger>();

        // PointerDown -> 开始拖
        var entryDown = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerDown
        };
        entryDown.callback.AddListener((data) => {
            isDragging = true;
            PauseOnly();
        });
        trigger.triggers.Add(entryDown);

        // PointerUp -> 结束拖
        var entryUp = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerUp
        };
        entryUp.callback.AddListener((data) => {
            Seek(progressSlider.value);
            isDragging = false;
        });
        trigger.triggers.Add(entryUp);
    }

    void Update()
    {
        // 播放中、未拖动，则跟新 slider
        if (playing.isStart && !isDragging)
        {
            progressSlider.value = playing.LevelMusic.time;
        }
    }

    private void OnPlayClicked()
    {
        if (!playing.isStart)
        {
            // 从当前位置播放并记录 bookmark
            bookmark = playing.LevelMusic.time;
            Resume();
        }
        else
        {
            // 播放中再次点 Play → 跳回 bookmark 并继续播放
            Seek(bookmark);
        }
    }

    private void OnStopClicked()
    {
        if (playing.isStart)
            PauseOnly();
    }

    private void Seek(float t)
    {
        t = Mathf.Clamp(t, 0f, playing.LevelMusic.clip.length);
        playing.LevelMusic.time = t;
        playing.currentTime     = t;  // 如果字段名不同请替换
        progressSlider.value    = t;
    }

    private void PauseOnly()
    {
        playing.BetaPause();
        playing.isStart = false;
    }

    private void Resume()
    {
        playing.BetaClick();
        playing.isStart = true;
    }
}
