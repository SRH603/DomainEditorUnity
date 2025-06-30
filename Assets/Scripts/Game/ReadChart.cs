using UnityEngine;
using UnityEngine.SceneManagement;

public class ReadChart : MonoBehaviour
{
    [HideInInspector] public GameData gameData;
    [HideInInspector] public TrackData trackData;
    [HideInInspector] public GenerateLevel generateLevel;
    public AudioSource audioSource, hitSounds;

    void Start()
    {
        generateLevel = GetComponent<GenerateLevel>();

        string levelname = PlayerPrefs.GetString("PlayingID", "");
        trackData = Resources.Load<TrackData>("level/" + levelname + "/track");
        //gameData = Resources.Load<GameData>("level/" + levelname + "/" + PlayerPrefs.GetInt("DifficultyIndex", -1).ToString());
        gameData = trackData.charts[PlayerPrefs.GetInt("DifficultyIndex", -1)];
        
        if (gameData != null)
        {
            AudioClip musicClip = trackData.track;
            audioSource.clip = musicClip;

            audioSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
            hitSounds.volume = PlayerPrefs.GetFloat("hitSoundVolume", 1f);
            generateLevel.Generate(this);

            Debug.Log(trackData.title + " " + gameData.info.rating);            
        }
        else
        {
            Debug.LogError("JSON file is not assigned in the inspector.");
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
