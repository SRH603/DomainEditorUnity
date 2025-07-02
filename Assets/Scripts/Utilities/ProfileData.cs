using UnityEngine;

public class ProfileData : MonoBehaviour
{
    
}

[System.Serializable]
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public class TitleData
{
    public string id;
    public string zhText;
    public string enText;
    public Rarity rarity;
}
[System.Serializable]
public class AvatarData
{
    public string id;
    public Sprite sprite;
    public Rarity rarity;
}
