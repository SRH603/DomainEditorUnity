using UnityEngine;

public static class DefaultGameDataFactory
{
    /// <summary>创建一个带 1 条 BPM(200) 和 1 条判定线的默认 GameData（SO）。</summary>
    public static GameData CreateSO()
    {
        var so = ScriptableObject.CreateInstance<GameData>();
        so.info = new Info
        {
            designer = "",
            bpm = "200",
            rating = 0,
            offset = 0f,
            version = 1f,
            condition = null
        };
        so.content = new Content
        {
            bpmList = new BPMList[]
            {
                new BPMList { startBeat = new Vector3Int(0,0,0), bpm = 200f }
            },
            judgmentLines = new JudgmentLine[]
            {
                new JudgmentLine()
            }
        };
        return so;
    }
}