using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在 Prefab 被 Instantiate 后，将 Inspector 里拖好的引用，
/// 依次赋给同一对象上的 PlayControlUI 组件。
/// </summary>
public class PlayControlBinder : MonoBehaviour
{
    [Header("目标脚本")]
    [Tooltip("要接收引用的脚本")]
    [SerializeField] private PlayControlUI targetControl;

    [Header("UI 元件 (运行时生成后会在此 Prefab 下)")]
    [Tooltip("进度条 Slider")]
    [SerializeField] private Slider slider;
    [Tooltip("播放按钮")]
    [SerializeField] private Button playButton;
    [Tooltip("停止按钮")]
    [SerializeField] private Button stopButton;

    [Header("游戏逻辑")]
    [Tooltip("场景中唯一的 OnPlaying 实例")]
    [SerializeField] private OnPlaying playing;

    void Awake()
    {
        if (targetControl == null)
        {
            //Debug.LogError("PlayControlBinder: 需要在 Inspector 里拖入 PlayControlUI 实例", this);
            //return;
            if (targetControl == null) targetControl = FindFirstObjectByType<PlayControlUI>();
        }

        // 依次赋值
        targetControl.progressSlider = slider;
        targetControl.playButton     = playButton;
        targetControl.stopButton     = stopButton;

        targetControl.Init();
    }
}