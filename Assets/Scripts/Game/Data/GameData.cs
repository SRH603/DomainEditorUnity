using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "GameData/GameData")]
public class GameData : ScriptableObject
{
    public Info info;
    public Content content;
}

[System.Serializable]
public class Info
{
    public string designer;
    public string bpm;
    public double rating;
    public float offset; // �����Ӻ�����벥��
    public float version;
    public Condition condition;
}

[System.Serializable]
public class Content
{
    public BPMList[] bpmList;
    public JudgmentLine[] judgmentLines;
}

[System.Serializable]
public class BPMList
{
    public Vector3Int startBeat;
    public float bpm;
}

[System.Serializable]
public class JudgmentLine
{
    public double flowSpeed;
    public Note[] notes;
    
    public AnimationCurve positionX;
    public AnimationCurve positionY;
    public AnimationCurve positionZ;
    public AnimationCurve rotationX;
    public AnimationCurve rotationY;
    public AnimationCurve rotationZ;
    public AnimationCurve transparency;
    public AnimationSpeed[] speed;
    // ���캯�����Զ���������ֵ
    public JudgmentLine()
    {
        flowSpeed = 1.0; // ����Ϊ0
        notes = new Note[0]; // ��ձʼ�����
        positionX = new AnimationCurve(); // �����յĶ�������
        positionY = new AnimationCurve(); // �����յĶ�������
        positionZ = new AnimationCurve(); // �����յĶ�������
        rotationX = new AnimationCurve(); // �����յĶ�������
        rotationY = new AnimationCurve(); // �����յĶ�������
        rotationZ = new AnimationCurve(); // �����յĶ�������
        transparency = new AnimationCurve(); // �����յĶ�������
        speed = new AnimationSpeed[1]; // ����ٶ�����
        speed[0] = new AnimationSpeed();
    }
}

[System.Serializable]
public class Note
{
    public int type;
    public double speed;
    public Vector3Int appearBeat;
    public List<NoteData> data = new List<NoteData>(1);

    // ���Note��TypeΪ2�������Data
    public void AddData(Vector3Int hitBeat, double position)
    {
        if (type == 2)
        {
            if (data == null)
            {
                data = new List<NoteData>();
            }
            data.Add(new NoteData(hitBeat, position));
        }
    }
}

[System.Serializable]
public class NoteData
{
    public Vector3Int hitBeat; // ����ʱ�䣬��������ʾ
    public double position;  // ����λ��

    public NoteData()
    {

    }

    public NoteData(Vector3Int hitBeat, double position)
    {
        this.hitBeat = hitBeat;
        this.position = position;
    }
}


[System.Serializable]
public class AnimationSpeed
{
    public double start;
    public double end;
    public Vector3Int startBeat;
    public Vector3Int endBeat;
    public AnimationSpeed()
    {
        start = 0;
        end = 1;
        startBeat = new Vector3Int(0, 0, 0);
        endBeat = new Vector3Int(1, 0, 0);
    }
}
