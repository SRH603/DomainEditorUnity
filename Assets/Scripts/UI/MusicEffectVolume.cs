using UnityEngine;

public class MusicEffectVolume : MonoBehaviour
{
    private float musicVolume;

    public AudioSource audioSource;

    public PreviewAudioPlayer previewAudioPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        audioSource.volume = musicVolume;
        if (previewAudioPlayer != null)
        {
            previewAudioPlayer.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
