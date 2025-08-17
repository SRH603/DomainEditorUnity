// Assets/Scripts/DomainEchoing/Mapping/GameDataMapper.cs
using System;
using System.Collections.Generic;
using UnityEngine;

// 依赖你已有的 GameData / Info / Content / BPMList / JudgmentLine / Note / NoteData / AnimationSpeed

public static class GameDataMapper
{
    public static GameDataDTO ToDTO(GameData so)
    {
        var dto = new GameDataDTO {
            info = new InfoDTO {
                designer = so.info?.designer,
                bpm = so.info?.bpm,
                rating = so.info?.rating ?? 0,
                offset = so.info?.offset ?? 0,
                version = so.info?.version ?? 0,
                conditionJson = (so.info?.condition != null) ? JsonUtility.ToJson(so.info.condition) : null
            },
            content = new ContentDTO {
                bpmList = new List<BPMItemDTO>(),
                judgmentLines = new List<JudgmentLineDTO>()
            }
        };

        if (so.content?.bpmList != null)
        {
            foreach (var b in so.content.bpmList)
                dto.content.bpmList.Add(new BPMItemDTO {
                    startBeat = new Vec3I{ x=b.startBeat.x, y=b.startBeat.y, z=b.startBeat.z },
                    bpm = b.bpm
                });
        }

        if (so.content?.judgmentLines != null)
        {
            foreach (var jl in so.content.judgmentLines)
            {
                dto.content.judgmentLines.Add(new JudgmentLineDTO {
                    flowSpeed = jl.flowSpeed,
                    notes = MapNotes(jl.notes),
                    positionX = FromUnityCurve(jl.positionX),
                    positionY = FromUnityCurve(jl.positionY),
                    positionZ = FromUnityCurve(jl.positionZ),
                    rotationX = FromUnityCurve(jl.rotationX),
                    rotationY = FromUnityCurve(jl.rotationY),
                    rotationZ = FromUnityCurve(jl.rotationZ),
                    transparency = FromUnityCurve(jl.transparency),
                    speed = MapSpeed(jl.speed)
                });
            }
        }

        return dto;
    }

    public static void FromDTO(GameDataDTO dto, GameData target)
    {
        if (dto == null) return;
        if (target.info == null) target.info = new Info();
        if (target.content == null) target.content = new Content();

        target.info.designer = dto.info?.designer;
        target.info.bpm      = dto.info?.bpm;
        target.info.rating   = dto.info?.rating ?? 0;
        target.info.offset   = dto.info?.offset ?? 0;
        target.info.version  = dto.info?.version ?? 0;

        // 若你有 Condition 强类型：
        // if (!string.IsNullOrEmpty(dto.info?.conditionJson))
        //     target.info.condition = JsonUtility.FromJson<Condition>(dto.info.conditionJson);

        // BPM
        if (dto.content?.bpmList != null)
        {
            target.content.bpmList = new BPMList[dto.content.bpmList.Count];
            for (int i=0;i<dto.content.bpmList.Count;i++)
            {
                var s = dto.content.bpmList[i];
                target.content.bpmList[i] = new BPMList {
                    startBeat = new Vector3Int(s.startBeat.x, s.startBeat.y, s.startBeat.z),
                    bpm = s.bpm
                };
            }
        }
        else target.content.bpmList = Array.Empty<BPMList>();

        // Lines
        if (dto.content?.judgmentLines != null)
        {
            target.content.judgmentLines = new JudgmentLine[dto.content.judgmentLines.Count];
            for (int i=0;i<dto.content.judgmentLines.Count;i++)
            {
                var s = dto.content.judgmentLines[i];
                var jl = new JudgmentLine {
                    flowSpeed = s.flowSpeed,
                    notes = MapNotesBack(s.notes),
                    positionX = ToUnityCurve(s.positionX),
                    positionY = ToUnityCurve(s.positionY),
                    positionZ = ToUnityCurve(s.positionZ),
                    rotationX = ToUnityCurve(s.rotationX),
                    rotationY = ToUnityCurve(s.rotationY),
                    rotationZ = ToUnityCurve(s.rotationZ),
                    transparency = ToUnityCurve(s.transparency),
                    speed = MapSpeedBack(s.speed)
                };
                target.content.judgmentLines[i] = jl;
            }
        }
        else target.content.judgmentLines = Array.Empty<JudgmentLine>();
    }

    static List<NoteDTO> MapNotes(Note[] arr)
    {
        var list = new List<NoteDTO>();
        if (arr == null) return list;
        foreach (var n in arr)
        {
            var nd = new NoteDTO {
                type = n.type,
                speed = n.speed,
                appearBeat = new Vec3I{ x=n.appearBeat.x,y=n.appearBeat.y,z=n.appearBeat.z },
                data = new List<NoteDataDTO>()
            };
            if (n.data != null)
                foreach (var d in n.data)
                    nd.data.Add(new NoteDataDTO{
                        hitBeat = new Vec3I{ x=d.hitBeat.x,y=d.hitBeat.y,z=d.hitBeat.z },
                        position = d.position
                    });
            list.Add(nd);
        }
        return list;
    }

    static Note[] MapNotesBack(List<NoteDTO> list)
    {
        if (list == null) return Array.Empty<Note>();
        var arr = new Note[list.Count];
        for (int i=0;i<list.Count;i++)
        {
            var s = list[i];
            var n = new Note {
                type = s.type,
                speed = s.speed,
                appearBeat = new Vector3Int(s.appearBeat.x, s.appearBeat.y, s.appearBeat.z),
                data = new List<NoteData>()
            };
            if (s.data != null)
                foreach (var d in s.data)
                    n.data.Add(new NoteData(new Vector3Int(d.hitBeat.x, d.hitBeat.y, d.hitBeat.z), d.position));
            arr[i] = n;
        }
        return arr;
    }

    static List<AnimationSpeedDTO> MapSpeed(AnimationSpeed[] arr)
    {
        var list = new List<AnimationSpeedDTO>();
        if (arr != null)
            foreach (var s in arr)
                list.Add(new AnimationSpeedDTO{
                    start=s.start, end=s.end,
                    startBeat=new Vec3I{ x=s.startBeat.x,y=s.startBeat.y,z=s.startBeat.z },
                    endBeat=new Vec3I{ x=s.endBeat.x,y=s.endBeat.y,z=s.endBeat.z }
                });
        return list;
    }

    static AnimationSpeed[] MapSpeedBack(List<AnimationSpeedDTO> list)
    {
        if (list == null) return Array.Empty<AnimationSpeed>();
        var arr = new AnimationSpeed[list.Count];
        for (int i=0;i<list.Count;i++)
        {
            var s = list[i];
            arr[i] = new AnimationSpeed{
                start=s.start, end=s.end,
                startBeat=new Vector3Int(s.startBeat.x,s.startBeat.y,s.startBeat.z),
                endBeat=new Vector3Int(s.endBeat.x,s.endBeat.y,s.endBeat.z)
            };
        }
        return arr;
    }

    static CurveDTO FromUnityCurve(AnimationCurve c)
    {
        var dto = new CurveDTO();
        if (c == null) return dto;
        foreach (var k in c.keys)
            dto.keys.Add(new KeyframeDTO{
                time=k.time, value=k.value, inTangent=k.inTangent, outTangent=k.outTangent,
                inWeight=k.inWeight, outWeight=k.outWeight, weighted=(k.weightedMode!=WeightedMode.None)
            });
        dto.preWrapMode = (WrapModeDTO)(int)c.preWrapMode;
        dto.postWrapMode = (WrapModeDTO)(int)c.postWrapMode;
        return dto;
    }

    static AnimationCurve ToUnityCurve(CurveDTO dto)
    {
        if (dto == null || dto.keys == null) return new AnimationCurve();
        var keys = new Keyframe[dto.keys.Count];
        for (int i=0;i<dto.keys.Count;i++)
        {
            var k = dto.keys[i];
            keys[i] = new Keyframe(k.time, k.value, k.inTangent, k.outTangent, k.inWeight, k.outWeight);
        }
        var ac = new AnimationCurve(keys);
        ac.preWrapMode = (WrapMode)(int)dto.preWrapMode;
        ac.postWrapMode = (WrapMode)(int)dto.postWrapMode;
        return ac;
    }
}
