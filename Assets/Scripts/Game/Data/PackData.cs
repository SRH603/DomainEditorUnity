using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PackData", menuName = "GameData/PackData")]
public class PackData : ScriptableObject
{
    public string id;
    public string section;
    public int character;
    public LocalizedString name;
    public LocalizedString description;
    public List<TrackData> tracks;
    public Condition condition;
    public Sprite illustration;
}