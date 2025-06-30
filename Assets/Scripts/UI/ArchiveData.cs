using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizedString
{
    public string en;
    //public string ja;
    //public string ko;
    //public string zh_Hant;
    public string zhHans;
}

[System.Serializable]
public class ArchiveChart
{
    public int ratingClass; // ????????
    public double rating; // ????
    public string designer;
    //public string version;
    public Condition condition;
    

    // ????
    [SerializeField]
    private int score;
    [SerializeField]
    private int optimism;
    [SerializeField]
    private bool unlocked;

    public void SetScore(int newScore)
    {
        score = newScore;
    }

    public void SetOptimism(int newOptimism)
    {
        optimism = newOptimism;
    }

    public void SetUnlocked(bool isUnlocked)
    {
        unlocked = isUnlocked;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetOptimism()
    {
        return optimism;
    }

    public bool GetUnlocked()
    {
        return unlocked;
    }
    // 构造函数：用于直接初始化 ArchiveChart 实例
    public ArchiveChart(int ratingClass, double rating, string designer)
    {
        this.ratingClass = ratingClass;
        this.rating = rating;
        this.designer = designer;
    }
}

[System.Serializable]
public class ArchiveTrack
{
    public int idx;
    public string id;
    public string title;
    public string artist;
    public Sprite illustration;
    public Sprite previewIllustration;
    public string illustrator;
    public string bpm;
    public string duration;
    public string background;
    public string songInfo; // ??????????
    public int audioPreviewStart;
    public int audioPreviewEnd;
    public string version;
    public List<ArchiveChart> charts;
    public string packId;
    [SerializeField]
    private bool unlocked;
    public void SetUnlocked(bool isUnlocked)
    {
        unlocked = isUnlocked;
    }

    public bool GetUnlocked()
    {
        return unlocked;
    }
    public Condition condition;
    
    // 构造函数：用于直接初始化 ArchiveTrack 实例
    public ArchiveTrack(int idx, string id, string title, string artist, Sprite illustration, Sprite previewIllustration, string illustrator, string bpm, string background, string songInfo, int audioPreviewStart, int audioPreviewEnd, string version)
    {
        this.idx = idx;
        this.id = id;
        this.title = title;
        this.artist = artist;
        this.illustration = illustration;
        this.previewIllustration = previewIllustration;
        this.illustrator = illustrator;
        this.bpm = bpm;
        this.background = background;
        this.songInfo = songInfo;
        this.audioPreviewStart = audioPreviewStart;
        this.audioPreviewEnd = audioPreviewEnd;
        this.version = version;
    }
}

[System.Serializable]
public class ArchivePack
{
    public int idx;
    public string id;
    public string section;
    public int character;
    public LocalizedString name;
    public LocalizedString description;
    public List<ArchiveTrack> tracks;
    public Condition condition;

    [SerializeField]
    private bool unlocked;
    public void SetUnlocked(bool isUnlocked)
    {
        unlocked = isUnlocked;
    }

    public bool GetUnlocked()
    {
        return unlocked;
    }
    public ArchivePack(int idx, string id, string section, int character, LocalizedString name, LocalizedString description)
    {
        this.idx = idx;
        this.id = id;
        this.section = section;
        this.character = character;
        this.name = name;
        this.description = description;
        this.tracks = new List<ArchiveTrack>();
    }
}

[System.Serializable]
public class ArchivePacksContainer
{
    public List<ArchivePack> packs;
}

[System.Serializable]
public class Pack
{
    public string idx;
    public string id;
    public string section;
    public int character;
    public LocalizedString name;
    public LocalizedString description;
    public List<TrackData> tracks;
    public Condition condition;
    public Sprite illustration;
}

[System.Serializable]
public class PacksContainer
{
    public List<Pack> packs;
}

[System.Serializable]
public class TrackUnlockConditions
{
    public string trackId;
    public Condition condition;
}

[System.Serializable]
public class ChartUnlockConditions
{
    public string trackId;
    public int ratingClass;
    public Condition condition;
}

[System.Serializable]
public class PackUnlockConditions
{
    public string packId;
    public Condition condition;
}

[System.Serializable]
public class UnlocksContainer
{
    public List<TrackUnlockConditions> tracks;
    public List<ChartUnlockConditions> charts;
    public List<PackUnlockConditions> packs;
}

[System.Serializable]
public class Condition
{
    public ConditionType type;
    public int amount;
    public string otherTrackId;
    public int ratingClass;
    public int targetScore;
    public string packId;
    public string otherPackId;
}

[System.Serializable]
public enum ConditionType
{
    Null, None, Currency, OtherTrack, OtherChart, Pack, Track, Ranking, GeneralRatingClass2
}