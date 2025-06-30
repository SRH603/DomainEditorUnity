using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class PreviewAudioPlayer : MonoBehaviour
{
    [Header("淡入淡出（秒）")]
    [SerializeField] private float fadeInTime  = 1.0f;
    [SerializeField] private float fadeOutTime = 1.0f;

    [Header("目标音量")]
    [SerializeField] private float targetVolume = 0.8f;
    [SerializeField] private AudioMixerGroup mixerGroup;

    private AudioSource src;
    private Coroutine loopCo, fadeCo;
    
    public AudioClip CurrentClip => src.clip;   // 当前正在播放的 Clip
    public bool     IsPlaying   => src.isPlaying;

    public float maxVolume;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop        = false;
        src.outputAudioMixerGroup = mixerGroup;
    }

    public void Start()
    {
        maxVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
    }

    /// <summary>播放传入的 AudioClip 指定片段</summary>
    public void PlayPreview(AudioClip clip, float start, float end)
    {
        if (clip == null) { Debug.LogWarning("Clip 为 null"); return; }

        // 保护 start/end 合法
        start = Mathf.Clamp(start, 0f, clip.length - 0.01f);
        end   = Mathf.Clamp(end, start + 0.05f, clip.length);

        // 清理旧协程
        if (loopCo != null) StopCoroutine(loopCo);
        if (fadeCo != null) StopCoroutine(fadeCo);

        bool sameClip = src.clip == clip;

        // 若是同一首歌，也先停一下再播，可确保重新定位
        if (sameClip) src.Stop();

        // 切换/设置 clip
        src.clip   = clip;
        src.volume = 0f;                // 先静音，避免爆音
        src.time   = start;             // ★ 先设起始时间
        src.Play();                     // 再 Play，指针不会被复位
        // —— 若担心某些压缩格式在 Play 后仍复位，可再补一行：
        // src.time = start;

        // 开启循环与淡入
        loopCo = StartCoroutine(LoopSegment(start, end));
        fadeCo = StartCoroutine(FadeVolume(targetVolume, fadeInTime));
    }


    public void StopPreview(float extraFadeOut = -1f)
    {
        if (!src.isPlaying) return;

        // 1) 关闭旧协程
        if (loopCo != null) StopCoroutine(loopCo);
        if (fadeCo != null) StopCoroutine(fadeCo);

        /* ---------- 关键判断 ---------- */
        bool instant = !isActiveAndEnabled || extraFadeOut <= 0f;
        if (instant)
        {
            src.volume = 0f;
            src.Stop();      // 立即静音+停止
            return;          // 不再启动任何协程 -> 不会再报错
        }
        /* -------------------------------- */

        float dur = extraFadeOut > 0 ? extraFadeOut : fadeOutTime;
        fadeCo = StartCoroutine(FadeVolume(0f, dur, () => src.Stop()));
    }



    IEnumerator LoopSegment(float start, float end)
    {
        if (end <= start + 0.05f) end = src.clip.length;
        while (true)
        {
            if (src.time >= end - fadeOutTime)
            {
                if (fadeCo != null) StopCoroutine(fadeCo);
                fadeCo = StartCoroutine(FadeVolume(0f, fadeOutTime));
                yield return new WaitForSeconds(fadeOutTime);

                src.time = start;
                if (!src.isPlaying) src.Play();
                fadeCo = StartCoroutine(FadeVolume(targetVolume, fadeInTime));
            }
            yield return null;
        }
    }

    IEnumerator FadeVolume(float to, float dur, System.Action done = null)
    {
        float from = src.volume, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, t / dur) * maxVolume;
            yield return null;
        }
        src.volume = to * maxVolume;
        done?.Invoke();
    }
}
