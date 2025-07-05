using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrackData", menuName = "GameData/TrackData")]
public class TrackData : ScriptableObject
{
    public string id;
    public string title;
    public string artist;
    public Texture illustration;
    public Texture previewIllustration;
    public AudioClip track;
    public string illustrator;
    public string bpm;
    public string background;
    public string songInfo; // ??????????
    public int audioPreviewStart;
    public int audioPreviewEnd;
    public string version;
    public Condition condition;
    public GameData[] charts;
}
