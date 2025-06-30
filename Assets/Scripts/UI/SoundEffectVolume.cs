using UnityEngine;

public class SoundEffectVolume : MonoBehaviour
{
    private float effectVolume;

    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        effectVolume = PlayerPrefs.GetFloat("effectVolume", 1f);
        audioSource.volume = effectVolume;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
