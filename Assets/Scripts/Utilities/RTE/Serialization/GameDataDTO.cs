// Assets/Scripts/DomainEchoing/DTO/GameDataDTO.cs
using System;
using System.Collections.Generic;

[Serializable] public struct Vec3I { public int x, y, z; }

[Serializable] public class KeyframeDTO {
    public float time, value, inTangent, outTangent, inWeight, outWeight;
    public bool weighted;
}

public enum WrapModeDTO { Default=0, ClampForever=1, Loop=2, PingPong=4 }

[Serializable] public class CurveDTO {
    public List<KeyframeDTO> keys = new();
    public WrapModeDTO preWrapMode = WrapModeDTO.ClampForever;
    public WrapModeDTO postWrapMode = WrapModeDTO.ClampForever;
}

[Serializable] public class GameDataDTO {
    public InfoDTO info;
    public ContentDTO content;
}

[Serializable] public class InfoDTO {
    public string designer;
    public string bpm;        // 字符串保留灵活性（多BPM备注等）
    public double rating;
    public float offset;
    public float version;
    public string conditionJson; // 若你有 Condition 强类型，可自行替换
}

[Serializable] public class ContentDTO {
    public List<BPMItemDTO> bpmList;
    public List<JudgmentLineDTO> judgmentLines;
}

[Serializable] public class BPMItemDTO {
    public Vec3I startBeat;
    public float bpm;
}

[Serializable] public class JudgmentLineDTO {
    public double flowSpeed;
    public List<NoteDTO> notes;

    public CurveDTO positionX, positionY, positionZ;
    public CurveDTO rotationX, rotationY, rotationZ;
    public CurveDTO transparency;

    public List<AnimationSpeedDTO> speed;
}

[Serializable] public class NoteDTO {
    public int type;
    public double speed;
    public Vec3I appearBeat;
    public List<NoteDataDTO> data;
}

[Serializable] public class NoteDataDTO {
    public Vec3I hitBeat;
    public double position;
}

[Serializable] public class AnimationSpeedDTO {
    public double start, end;
    public Vec3I startBeat, endBeat;
}