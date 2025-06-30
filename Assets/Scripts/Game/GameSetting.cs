using UnityEngine;
using UnityEngine.UI;

public class GameSetting : MonoBehaviour
{
    public Slider flowSpeedSlider;         // 0.1 ~ 10     默认 4.5
    public Slider appearDistanceSlider;    // 0.1 ~ 10     默认 1
    public Slider noteSizeSlider;          // 0.8 ~ 1.4     默认 1
    public Slider assistDurationSlider;    // 0.5 ~ 2       默认 1
    public Slider audioOffsetSlider;       // -300 ~ 300    默认 0
    public Slider musicVolumeSlider;       // 0 ~ 1         默认 1
    public Slider effectVolumeSlider;      // 0 ~ 1         默认 1
    public Slider hitSoundVolumeSlider;    // 0 ~ 1         默认 1
    public Toggle highlightSimulNotesToggle; // true         默认 true

    public SoundEffectVolume soundEffectVolume;
    public MusicEffectVolume musicEffectVolume;

    void Start()
    {
        SetupSliders();   // 设置Slider范围
        LoadSettings();   // 读取PlayerPrefs并赋值
        SetupListeners(); // 监听改动并保存
    }

    void SetupSliders()
    {
        flowSpeedSlider.minValue = 0.2f; flowSpeedSlider.maxValue = 2f;
        appearDistanceSlider.minValue = 0.5f; appearDistanceSlider.maxValue = 2f;
        noteSizeSlider.minValue = 0.8f; noteSizeSlider.maxValue = 1.4f;
        assistDurationSlider.minValue = 0.5f; assistDurationSlider.maxValue = 2f;
        audioOffsetSlider.minValue = -300f; audioOffsetSlider.maxValue = 300f;

        musicVolumeSlider.minValue = 0f; musicVolumeSlider.maxValue = 1f;
        effectVolumeSlider.minValue = 0f; effectVolumeSlider.maxValue = 1f;
        hitSoundVolumeSlider.minValue = 0f; hitSoundVolumeSlider.maxValue = 1f;
    }

    void LoadSettings()
    {
        flowSpeedSlider.value = PlayerPrefs.GetFloat("flowSpeed", 1f);
        appearDistanceSlider.value = PlayerPrefs.GetFloat("appearDistance", 1f);
        noteSizeSlider.value = PlayerPrefs.GetFloat("noteSize", 1f);
        assistDurationSlider.value = PlayerPrefs.GetFloat("assistDuration", 1f);
        audioOffsetSlider.value = PlayerPrefs.GetFloat("audioOffset", 0f);

        musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f);
        effectVolumeSlider.value = PlayerPrefs.GetFloat("effectVolume", 1f);
        hitSoundVolumeSlider.value = PlayerPrefs.GetFloat("hitSoundVolume", 1f);

        highlightSimulNotesToggle.isOn = PlayerPrefs.GetInt("highlightSimulNotes", 1) == 1;
    }

    void SetupListeners()
    {
        flowSpeedSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("flowSpeed", val));
        appearDistanceSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("appearDistance", val));
        noteSizeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("noteSize", val));
        assistDurationSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("assistDuration", val));
        audioOffsetSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("audioOffset", val));

        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectVolumeSlider.onValueChanged.AddListener(OnEffectVolumeChanged);
        hitSoundVolumeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("hitSoundVolume", val));

        highlightSimulNotesToggle.onValueChanged.AddListener(val => PlayerPrefs.SetInt("highlightSimulNotes", val ? 1 : 0));
    }
    
    private void OnEffectVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("effectVolume", val);
        if (soundEffectVolume != null)
            soundEffectVolume.Start();
    }
    
    private void OnMusicVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat("musicVolume", val);
        if (musicEffectVolume != null)
            musicEffectVolume.Start();
    }
    
    public void ResetToDefault()
    {
        PlayerPrefs.DeleteKey("flowSpeed");
        PlayerPrefs.DeleteKey("appearDistance");
        PlayerPrefs.DeleteKey("noteSize");
        PlayerPrefs.DeleteKey("assistDuration");
        PlayerPrefs.DeleteKey("audioOffset");
        PlayerPrefs.DeleteKey("musicVolume");
        PlayerPrefs.DeleteKey("effectVolume");
        PlayerPrefs.DeleteKey("hitSoundVolume");
        PlayerPrefs.DeleteKey("highlightSimulNotes");

        LoadSettings();
    }

}
