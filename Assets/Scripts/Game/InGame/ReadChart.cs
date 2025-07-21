using UnityEngine;
using UnityEngine.SceneManagement;

public class ReadChart : MonoBehaviour
{
    public GameData gameData;
    public TrackData trackData;
    [HideInInspector] public GenerateLevel generateLevel;
    public AudioSource audioSource, hitSounds;

    void Start()
    {
        generateLevel = GetComponent<GenerateLevel>();
        
        if (gameData != null)
        {
            AudioClip musicClip = trackData.track;
            audioSource.clip = musicClip;

            audioSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
            hitSounds.volume = PlayerPrefs.GetFloat("hitSoundVolume", 1f);
            generateLevel.Init(this);

            Debug.Log(trackData.title + " " + gameData.info.rating);            
        }
        else
        {
            Debug.LogError("JSON file is not assigned in the inspector.");
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
