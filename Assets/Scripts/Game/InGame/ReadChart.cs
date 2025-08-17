using UnityEngine;
using UnityEngine.SceneManagement;

public class ReadChart : MonoBehaviour
{
    [HideInInspector] public GameData gameData;
    //[HideInInspector] public TrackData trackData;
    [HideInInspector] public GenerateLevel generateLevel;
    public ChartManager mgr;
    public AudioSource audioSource, hitSounds;

    void Start()
    {
        gameData = mgr.gameData;
        generateLevel = GetComponent<GenerateLevel>();
        
        if (gameData != null)
        {
            AudioClip musicClip = ChartManager.Instance.levelMusic;
            audioSource.clip = musicClip;

            audioSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
            hitSounds.volume = PlayerPrefs.GetFloat("hitSoundVolume", 1f);
            generateLevel.Init(this);

            Debug.Log("Game data loaded successfully!");            
        }
        else
        {
            Debug.LogError("Game data loaded failed!");
            SceneManager.LoadScene("HubScene");
        }
    }
}
