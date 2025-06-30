using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrackData", menuName = "Custom/TrackData")]
public class TrackData : ScriptableObject
{
    public int idx;
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
    public GameData[] charts;
}
