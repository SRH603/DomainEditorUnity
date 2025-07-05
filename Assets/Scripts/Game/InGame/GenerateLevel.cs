using System.Collections.Generic;
using UnityEngine;
using static OnPlaying;
using static GameUtilities.InGameUtilities;
using static GameUtilities;

public class GenerateLevel : MonoBehaviour
{
    public GameObject linePrefab; // 判定线的预制体
    public GameObject TapPrefab; // Tap预制体
    public GameObject DragPrefab; // Drag预制体
    public GameObject HoldPrefab; // Hold预制体
    public GameObject HoldComponentPrefab; // Hold组成部分
    public Material Holdmaterial; // Hold材质
    public Material lineMaterial;
    [HideInInspector] public int NotesNum;
    public Transform judgmentLinesParent;
    [HideInInspector] public List<JudgmentLineData> AllJudgementLines = new List<JudgmentLineData>(); // 存储所有判定线及其notes
    [HideInInspector] public GameData gameData;
    [HideInInspector] public TrackData trackData;
    public AudioSource LevelMusic;
    [HideInInspector] public BPMTimingList bpmTimingList = new BPMTimingList();
    [HideInInspector] public float totalBeats;
    public GameObject background;

    public void Generate(ReadChart readChart)
    {
        gameData = readChart.gameData;
        trackData = readChart.trackData;
        LoadBPMList();
        GenerateJudgmentLines();
        float appearDistance = PlayerPrefs.GetFloat("appearDistance", 1f);
        RenderSettings.fogStartDistance = appearDistance * 20f;
        RenderSettings.fogEndDistance = appearDistance * 40f;
        GetComponent<OnPlaying>().offsetTime = gameData.info.offset;
        GetComponent<OnPlaying>().LoadLevel();
        // 获取当前对象的Renderer组件
        Renderer renderer = background.GetComponent<Renderer>();
        background.transform.localPosition = new Vector3(0, 0, appearDistance * 37f + 30f);
        background.transform.localScale = new Vector3(appearDistance * 61.62846f, appearDistance * 61.62846f, 0.06162845f);
        if(renderer != null)
        {
            // 获取材质实例（避免修改所有使用相同材质的对象）
            Material mat = renderer.material;

            //Texture newtext = gameData.info.illustration;
            mat.SetTexture("_MainTex", readChart.trackData.illustration);
        }
        else
        {
            Debug.LogError("当前对象没有Renderer组件！");
        }
    }
    void LoadBPMList()
    {
        if (bpmTimingList.Changes.Count == 0)
        {
            double generatingtime = 0;

            int i = 0;
            foreach (var bpmTiming in gameData.content.bpmList)
            {

                bpmTimingList.Changes.Add(new BPMTiming(bpmTiming.bpm, (float)generatingtime));
                if (gameData.content.bpmList.Length == i + 1)
                {
                    break;
                }
                else
                {
                    generatingtime += ((GameUtilities.FractionToDecimal(gameData.content.bpmList[i + 1].startBeat) - GameUtilities.FractionToDecimal(bpmTiming.startBeat)) / (bpmTiming.bpm / 60));
                }
                i++;
            }
            totalBeats = CalculateTotalBeats();
        }
    }
    void GenerateJudgmentLines()
    {

        if (gameData != null && gameData.content != null)
        {
            
            foreach (var judgmentLine in gameData.content.judgmentLines)
            {

                Vector3 StartPosition = new Vector3(judgmentLine.positionX.Evaluate(0), judgmentLine.positionY.Evaluate(0), judgmentLine.positionZ.Evaluate(0));
                Quaternion StartRotation = Quaternion.Euler(judgmentLine.rotationX.Evaluate(0), judgmentLine.rotationY.Evaluate(0), judgmentLine.rotationZ.Evaluate(0));

                GameObject lineInstance = Instantiate(linePrefab, StartPosition, StartRotation, judgmentLinesParent);
                // 设置材质
                MeshRenderer meshRenderer = lineInstance.transform.Find("JudgementLine").GetComponent<MeshRenderer>();
                Material instanceMaterial = new Material(lineMaterial);
                meshRenderer.sharedMaterial = instanceMaterial;

                JudgmentLineData lineData = new JudgmentLineData(lineInstance, judgmentLine.flowSpeed); // 创建判定线数据时包含速度信息

                lineData.positionX = judgmentLine.positionX;
                lineData.positionY = judgmentLine.positionY;
                lineData.positionZ = judgmentLine.positionZ;
                lineData.rotationX = judgmentLine.rotationX;
                lineData.rotationY = judgmentLine.rotationY;
                lineData.rotationZ = judgmentLine.rotationZ;
                lineData.transparency = judgmentLine.transparency;
                lineData.speed = judgmentLine.speed;

                GenerateNotes(judgmentLine, lineInstance.transform, lineData);
                AllJudgementLines.Add(lineData);

                Renderer renderer = FindDeepChild(lineInstance.transform, "JudgementLine").GetComponent<Renderer>();

                // 检查渲染器和材质是否存在
                if (renderer != null && renderer.material != null)
                {
                    // 获取当前材质的颜色
                    Color color = renderer.material.color;

                    // 修改颜色的Alpha值来改变透明度
                    color.a = judgmentLine.transparency.Evaluate(0);

                    // 将修改后的颜色赋回给材质
                    renderer.material.color = color;
                }

            }
        }
        if (PlayerPrefs.GetInt("highlightSimulNotes", 1) == 1)
            NotesHighLighter();
    }
    void GenerateNotes(JudgmentLine judgmentLine, Transform lineTransform, JudgmentLineData lineData)
    {
        float flowSpeed = PlayerPrefs.GetFloat("flowSpeed", 1f);
        float noteSize = PlayerPrefs.GetFloat("noteSize", 1f);
        foreach (var note in judgmentLine.notes)
        {
            if (note == null)
            {
                Debug.LogWarning("Note is null");
                continue; // 跳过当前迭代
            }

            if (note.type == 0 || note.type == 1)
            {
                if (note.data[0].hitBeat == null)
                {
                    Debug.LogWarning("HitTime array is not initialized or does not have enough elements");
                    continue; // 跳过当前迭代
                }

                double beatTime = GameUtilities.FractionToDecimal(note.data[0].hitBeat); // 计算note的击打时间（转换为拍数）
                
                double xPosition = CalculateIntegratedSpeed(judgmentLine.speed, 0, beatTime) * note.speed * flowSpeed; // 计算note的z坐标（假设NoteFlowSpeed是已知的）
                Vector3 noteLocalPosition = new Vector3((float)xPosition, (float)note.data[0].position, 0); // 使用局部坐标实例化note预制体，并设置其位置和父物体


                if (note.type == 0)
                {

                    GameObject noteInstance = Instantiate(TapPrefab, Vector3.zero, Quaternion.identity, lineTransform.GetComponent<JudgeLineManage>().NoteHolder); // 先实例化note，位置设为Vector3.zero
                    noteInstance.GetComponent<Tap>().Mesh.transform.localScale = new Vector3(1, noteSize, 1);
                    noteInstance.GetComponent<Tap>().HitBeat = beatTime;
                    noteInstance.GetComponent<Tap>().Speed = note.speed * flowSpeed;
                    if (FractionToDecimal(note.appearBeat) == -1)
                        noteInstance.GetComponent<Tap>().AppearTime = -1;
                    else
                        noteInstance.GetComponent<Tap>().AppearTime = NoteUtilities.CalculateIntegratedHitTime(gameData.content.bpmList, beatTime - FractionToDecimal(note.appearBeat));
                    noteInstance.transform.localPosition = noteLocalPosition; // 然后设置note的局部位置
                    noteInstance.transform.localRotation = Quaternion.identity; // 确保note的局部旋转为0（相对于判定线）
                    lineData.Notes.Add(noteInstance); // 将生成的note添加到对应判定线的数据中
                    if (beatTime < totalBeats) // 是否为有效音符
                        NotesNum++;
                }
                if (note.type == 1)
                {

                    GameObject noteInstance = Instantiate(DragPrefab, Vector3.zero, Quaternion.identity, lineTransform.GetComponent<JudgeLineManage>().NoteHolder); // 先实例化note，位置设为Vector3.zero
                    noteInstance.GetComponent<Drag>().Mesh.transform.localScale = new Vector3(1, noteSize, 1);
                    noteInstance.GetComponent<Drag>().HitBeat = beatTime;
                    noteInstance.GetComponent<Drag>().Speed = note.speed * flowSpeed;
                    if (FractionToDecimal(note.appearBeat) == -1)
                        noteInstance.GetComponent<Drag>().AppearTime = -1;
                    else
                        noteInstance.GetComponent<Drag>().AppearTime = NoteUtilities.CalculateIntegratedHitTime(gameData.content.bpmList, beatTime - FractionToDecimal(note.appearBeat));
                    noteInstance.transform.localPosition = noteLocalPosition; // 设置note局部位置
                    noteInstance.transform.localRotation = Quaternion.identity; // 确保note局部旋转为0（相对于判定线）
                    lineData.Notes.Add(noteInstance); // 将生成的note添加到对应判定线的数据中
                    if (beatTime < totalBeats)
                        NotesNum++;
                }


            }
            if (note.type == 2)
            {
                if (note.data[0].hitBeat == null)
                {
                    Debug.LogWarning("HitTime array is not initialized or does not have enough elements");
                    continue; // 跳过当前迭代
                }
                // 计算note的击打时间（转换为拍数）
                double beatTimeParent = FractionToDecimal(note.data[0].hitBeat); // 计算note的击打时间（转换为拍数）

                // 计算note的z坐标（假设NoteFlowSpeed是已知的）
                double xPositionFather = CalculateIntegratedSpeed(judgmentLine.speed, 0, beatTimeParent) * note.speed * flowSpeed;
                // 实例化note预制体，并设置其位置和父物体
                Vector3 noteLocalPositionParent = new Vector3((float)xPositionFather, (float)note.data[0].position, 0); // 使用局部坐标

                // 先实例化note，位置设为Vector3.zero
                GameObject noteInstanceParent = Instantiate(HoldPrefab, Vector3.zero, Quaternion.identity, lineTransform.GetComponent<JudgeLineManage>().NoteHolder);
                noteInstanceParent.GetComponent<Hold>().baseMesh.transform.localScale = new Vector3(1, noteSize, 1);
                // 然后设置note的局部位置
                noteInstanceParent.transform.localPosition = noteLocalPositionParent;
                // 确保note的局部旋转为0（相对于判定线）
                noteInstanceParent.transform.localRotation = Quaternion.identity;
                // 将生成的note添加到对应判定线的数据中
                lineData.Notes.Add(noteInstanceParent);
                noteInstanceParent.GetComponent<Hold>().HitBeat = beatTimeParent;
                noteInstanceParent.GetComponent<Hold>().Speed = note.speed * flowSpeed;
                if (FractionToDecimal(note.appearBeat) == -1)
                    noteInstanceParent.GetComponent<Hold>().AppearTime = -1;
                else
                    noteInstanceParent.GetComponent<Hold>().AppearTime = NoteUtilities.CalculateIntegratedHitTime(gameData.content.bpmList, beatTimeParent - FractionToDecimal(note.appearBeat));

                foreach (var data in note.data)
                {
                    if (data.hitBeat == null)
                    {
                        Debug.LogWarning("HitTime array is not initialized or does not have enough elements");
                        continue; // 跳过当前迭代
                    }
                    // 计算note的击打时间（转换为拍数）
                    double beatTime = FractionToDecimal(data.hitBeat);

                    // 计算note的z坐标（假设NoteFlowSpeed是已知的）
                    double xPosition = CalculateIntegratedSpeed(judgmentLine.speed, 0, beatTime) * note.speed * flowSpeed;
                    // 实例化note预制体，并设置其位置和父物体
                    Vector3 noteLocalPosition = new Vector3((float)xPosition, (float)data.position, 0); // 使用局部坐标

                    // 先实例化note，位置设为Vector3.zero
                    GameObject noteInstance = Instantiate(HoldComponentPrefab, Vector3.zero, Quaternion.identity, lineTransform);
                    // 然后设置note的局部位置
                    noteInstance.transform.localPosition = noteLocalPosition;
                    // 确保note的局部旋转为0（相对于判定线）
                    noteInstance.transform.localRotation = Quaternion.identity;
                    // 将生成的Component添加到对应Hold的数据中
                    //lineData.Notes.Add(noteInstance);
                    noteInstanceParent.GetComponent<Hold>().HoldComponents.Add(noteInstance);
                    noteInstance.transform.SetParent(noteInstanceParent.GetComponent<Hold>().Mesh.transform, true);
                    noteInstance.GetComponent<HoldLocationPool>().HitTime = (float)beatTime;
                    noteInstance.GetComponent<HoldLocationPool>().HitTimePos = (float)xPosition;
                    noteInstance.GetComponent<HoldLocationPool>().HitPosition = (float)data.position;

                }
                for (int i = 0; i <= note.data.Count - 2; i++)
                {
                    float PLGWidth = 0.8f * noteSize;
                    float thick = 0.05f;
                    Vector3[] PLGvertices = new Vector3[8];

                    float PLGLength = (float)((float)(CalculateIntegratedSpeed(judgmentLine.speed, 0, noteInstanceParent.GetComponent<Hold>().HoldComponents[i + 1].transform.GetComponent<HoldLocationPool>().HitTime) - CalculateIntegratedSpeed(judgmentLine.speed, 0, noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().HitTime)) * note.speed * flowSpeed);
                    float PLGDisplacement = noteInstanceParent.GetComponent<Hold>().HoldComponents[i + 1].transform.GetComponent<HoldLocationPool>().HitPosition - noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().HitPosition;
                    noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().EndPosition = noteInstanceParent.GetComponent<Hold>().HoldComponents[i + 1].transform.GetComponent<HoldLocationPool>().HitPosition;
                    noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().EndTime = noteInstanceParent.GetComponent<Hold>().HoldComponents[i + 1].transform.GetComponent<HoldLocationPool>().HitTime;
                    noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().EndTimePos = noteInstanceParent.GetComponent<Hold>().HoldComponents[i + 1].transform.GetComponent<HoldLocationPool>().HitTimePos;

                    PLGvertices[0] = new Vector3(0, -PLGWidth / 2, -thick / 2);
                    PLGvertices[1] = new Vector3(0, PLGWidth / 2, -thick / 2);
                    PLGvertices[2] = new Vector3(PLGLength, PLGDisplacement - PLGWidth / 2, -thick / 2);
                    PLGvertices[3] = new Vector3(PLGLength, PLGDisplacement + PLGWidth / 2, -thick / 2);
                    PLGvertices[4] = new Vector3(0, -PLGWidth / 2, thick / 2);
                    PLGvertices[5] = new Vector3(0, PLGWidth / 2, thick / 2);
                    PLGvertices[6] = new Vector3(PLGLength, PLGDisplacement - PLGWidth / 2, thick / 2);
                    PLGvertices[7] = new Vector3(PLGLength, PLGDisplacement + PLGWidth / 2, thick / 2);

                    noteInstanceParent.GetComponent<Hold>().HoldComponents[i].transform.GetComponent<HoldLocationPool>().LastComponent = false;

                    CreateParallelogramObject(PLGvertices, Holdmaterial, noteInstanceParent, noteInstanceParent.GetComponent<Hold>().HoldComponents[i]);

                }
                noteInstanceParent.GetComponent<Hold>().HoldComponents[note.data.Count - 1].transform.GetComponent<HoldLocationPool>().EndTime = float.PositiveInfinity;

                if (FractionToDecimal(note.data[0].hitBeat) < totalBeats)
                    NotesNum += (int)Mathf.Ceil((float)((Mathf.Min((float)FractionToDecimal(note.data[note.data.Count - 1].hitBeat), totalBeats) - FractionToDecimal(note.data[0].hitBeat)) * 4));

                noteInstanceParent.GetComponent<Hold>().InitializeHold();
            }
            
        }
    }

    void NotesHighLighter()
    {
        foreach (var generatedjudgmentLine in AllJudgementLines)
        {
            foreach (var AllGeneratedNotes in generatedjudgmentLine.Notes)
            {
                foreach (var generatedjudgmentLineit in AllJudgementLines)
                {
                    foreach (var AllGeneratedNotesit in generatedjudgmentLine.Notes)
                    {
                        if (AllGeneratedNotes == AllGeneratedNotesit)
                        {
                            continue;
                        }
                        if (AllGeneratedNotes.GetComponent<NoteEntity>().HitBeat == AllGeneratedNotesit.GetComponent<NoteEntity>().HitBeat)
                        {
                            AllGeneratedNotesit.GetComponent<NoteEntity>().HighLight();
                            AllGeneratedNotes.GetComponent<NoteEntity>().HighLight();
                        }
                    }
                }
            }
        }
    }
    void CreateParallelogramObject(Vector3[] vertices, Material material, GameObject OriginalHold, GameObject holdcomponent)
    {
        // 创建一个新的GameObject
        GameObject parallelogramObject = new GameObject("Parallelogram");

        // 添加Mesh Filter组件并设置Mesh
        MeshFilter meshFilter = parallelogramObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateParallelogramMesh(vertices);

        // 添加Mesh Renderer组件并设置材质
        MeshRenderer meshRenderer = parallelogramObject.AddComponent<MeshRenderer>();
        Material instanceMaterial = new Material(material);
        instanceMaterial.SetFloat("_Cutoff", 0);
        meshRenderer.material = instanceMaterial;

        GameObject HoldPlane = new GameObject("HoldPlane");

        HoldPlane.transform.parent = holdcomponent.transform;
        HoldPlane.transform.localPosition = Vector3.zero;
        HoldPlane.transform.localRotation = Quaternion.identity;

        parallelogramObject.transform.parent = HoldPlane.transform;
        parallelogramObject.transform.localPosition = Vector3.zero;
        parallelogramObject.transform.localRotation = Quaternion.identity;
        
        HoldPlaneBehavior behavior = parallelogramObject.AddComponent<HoldPlaneBehavior>();
        behavior.Cutoff = 0;
        behavior.referenceTransform = OriginalHold.transform;
        behavior.subReferenceTransform = holdcomponent.transform;

        holdcomponent.GetComponent<HoldLocationPool>().HoldPlane = HoldPlane;
        holdcomponent.GetComponent<HoldLocationPool>().parallelogram = parallelogramObject;

    }

    Mesh CreateParallelogramMesh(Vector3[] vertices)
    {
        /*
        Mesh mesh = new Mesh();

        // 设置顶点
        mesh.vertices = vertices;

        // 设置三角形（两个三角形构成一个四边形）
        mesh.triangles = new int[]
        {
            0, 1, 2, 
            1, 3, 2, 
            4, 6, 5, 
            5, 6, 7, 
            0, 2, 4, 
            4, 2, 6, 
            1, 5, 3,
            5, 7, 3, 
            0, 4, 1, 
            1, 4, 5, 
            2, 3, 6, 
            3, 7, 6
        };
        
        mesh.triangles = new int[]
        {
            // ────────── ① Front  (-Z) 0-1-2-3 ──────────
            0, 1, 2,   2, 1, 3,

            // ────────── ② Back   (+Z) 4-5-6-7 ──────────
            4, 6, 5,   6, 7, 5,

            // ────────── ③ Left   (-X) 0-1-5-4 ──────────
            0, 4, 1,   1, 4, 5,

            // ────────── ④ Right  (+X) 2-3-7-6 ──────────
            2, 3, 6,   6, 3, 7,

            // ────────── ⑤ Top    (+Y) 1-5-7-3 ──────────
            1, 5, 3,   3, 5, 7,

            // ────────── ⑥ Bottom (-Y) 0-2-6-4 ──────────
            0, 2, 4,   4, 2, 6
        };
        */
        
        // ① 复制 8→24：每面 4 顶点，法线互不干扰
        var v24 = new Vector3[24]
        {
            // -------- Front (-Z) --------
            vertices[0], vertices[1], vertices[2], vertices[3],
            // -------- Back  (+Z) --------
            vertices[4], vertices[5], vertices[6], vertices[7],
            // -------- Left  (-X) --------
            vertices[0], vertices[4], vertices[1], vertices[5],
            // -------- Right (+X) --------
            vertices[2], vertices[3], vertices[6], vertices[7],
            // -------- Top   (+Y) --------
            vertices[1], vertices[5], vertices[3], vertices[7],
            // -------- Bot   (-Y) --------
            vertices[0], vertices[4], vertices[2], vertices[6],
        };

        // ② 6 面 × 2 三角 = 12 三角（顶点索引已是局部 0-23）
        int[] tris =
        {
            0,  1,  2,   2,  1,  3,    // Front
            4,  6,  5,   6,  7,  5,    // Back
            8,  9, 10,  10,  9, 11,    // Left
            12, 13, 14,  14, 13, 15,    // Right
            16, 17, 18,  18, 17, 19,    // Top
            20, 22, 21,  22, 23, 21     // Bottom
        };

        Mesh m = new Mesh
        {
            vertices   = v24,
            triangles  = tris
        };
        
        // 重新计算法线
        m.RecalculateNormals(0f);
        m.RecalculateBounds();
        m.RecalculateTangents();

        
        // 重新计算法线
        /*
        mesh.RecalculateNormals(0f);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        */
        
        /* ---------- ★ 新增：一次性检查三角缠绕 ---------- */
        /*
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int i0 = mesh.triangles[i], i1 = mesh.triangles[i + 1], i2 = mesh.triangles[i + 2];
            Vector3 A = vertices[i0], B = vertices[i1], C = vertices[i2];
            Vector3 n = Vector3.Cross(B - A, C - A).normalized;
            // 这里假设“+Z” 方向为正面，以它为例判断
            if (n.z < 0f)
                Debug.LogWarning($"⚠️  第{i / 3}个三角法线指向 -Z，可能缠绕方向反了");
        }
        /* -------------------------------------------------- */
        
        //return mesh;
        return m;
    }

    public float Time2Beat(float Time, BPMTimingList BPMchanges)
    {
        float Beat = 0;
        for (int i = 0; i <= BPMchanges.Changes.Count - 1; ++i)
        {
            if (i < BPMchanges.Changes.Count - 1)
            {
                if (Time < BPMchanges.Changes[i + 1].StartTime)
                {
                    Beat += (Time - BPMchanges.Changes[i].StartTime) * (BPMchanges.Changes[i].BPM / 60);
                    break;
                }
                else
                {
                    Beat += (BPMchanges.Changes[i + 1].StartTime - BPMchanges.Changes[i].StartTime) * (BPMchanges.Changes[i].BPM / 60);
                }
            }
            else
            {
                Beat += (Time - BPMchanges.Changes[i].StartTime) * (BPMchanges.Changes[i].BPM / 60);
                break;
            }
        }
        return Beat;
    }
    float CalculateTotalBeats()
    {
        if (LevelMusic != null && LevelMusic.clip != null)
        {
            return Time2Beat(LevelMusic.clip.length, bpmTimingList);
        }
        else
        {
            Debug.LogWarning(LevelMusic.clip.length);
            return LevelMusic.clip.length;
        }
    }
}
