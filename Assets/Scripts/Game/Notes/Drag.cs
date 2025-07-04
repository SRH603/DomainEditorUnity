using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static GameUtilities.JudgeData;
using static GameUtilities.NoteUtilities;

public class Drag : NoteEntity
{
    [HideInInspector] public CapsuleCollider NoteCollider;
    [HideInInspector] public bool Tagged = false;
    [HideInInspector] public bool Hitted = false;


    void Start()
    {
        LevelPlayingControl = GameObject.Find("LevelControl");

        if (AppearTime == -1) Mesh.SetActive(true);
        else Mesh.SetActive(false);

        HitTime = CalculateIntegratedHitTime(LevelPlayingControl.GetComponent<ReadChart>().gameData.content.bpmList, HitBeat);

        ColliderProcessing();
    }
    void Update()
    {
        // Show Note
        if (AppearTime != -1 && HitBeat - LevelPlayingControl.GetComponent<OnPlaying>().offsetTime >= AppearTime) Mesh.SetActive(true);
        if (State != 7) RefreshState();
    }
    void ColliderProcessing()
    {
        Vector3 Noteposition = this.transform.localPosition;
        NoteCollider = Instantiate(NoteColliderParent, Noteposition, Quaternion.identity, transform.parent);
        Vector3 position = NoteCollider.transform.position;
        NoteCollider.transform.localPosition = new Vector3(0, position.y, position.z);
        NoteCollider.transform.localRotation = Quaternion.identity;
        NoteCollider.GetComponent<JudgeCollider>().ParentNote = gameObject;
    }
    void RefreshState()
    {
        if (LevelPlayingControl.GetComponent<OnPlaying>().isStart)
        {
            
            double deltaTime = LevelPlayingControl.GetComponent<OnPlaying>().offsetTime - HitTime;

            // Determine if need to enable judgment

            if (deltaTime >= perfectJudgmentTime) NoteCollider.enabled = false;
            else if (deltaTime >= -perfectJudgmentTime) NoteCollider.enabled = true;

            // Determining the state with interval

            Dictionary<double, int> judgmentDict = new Dictionary<double, int>
            {
                { perfectJudgmentTime, 7 }, // miss
                { 0, 4 }, // late optimal
                { -perfectJudgmentTime, 3 }, // early optimal
            };

            // Search for state
            foreach (var entry in judgmentDict)
            {
                if (deltaTime > entry.Key)
                {
                    State = entry.Value;
                    if (State == 4 && Tagged == true) Hit();
                    break;
                }
            }

        }

        if (State == 7) // miss
        {
            LevelPlayingControl.GetComponent<PlayingData>().MissNum += 1;
            LevelPlayingControl.GetComponent<PlayingData>().Combo = 0;
            gameObject.SetActive(false);
        }
    }

    public void Hit()
    {
        NoteCollider.gameObject.SetActive(false);

        if (Hitted == false)
        {
            Hitted = true;
            if (State == 4) Judge("OptimalNum", true, true, OptimalEffectPrefab);
            else if (State == 3) Judge("OptimalNum", true, true, OptimalEffectPrefab);
            audioSource.PlayOneShot(hitSound);
            Note.SetActive(false);
        }
    }
    public void Judge(string JudgeType, bool Combo, bool isEffect, GameObject Effect)
    {
        PlayingData playingData = LevelPlayingControl.GetComponent<PlayingData>();
        FieldInfo fieldInfo = playingData.GetType().GetField(JudgeType);

        if (fieldInfo != null) // Reflection
        {
            int currentValue = (int)fieldInfo.GetValue(playingData);
            fieldInfo.SetValue(playingData, currentValue + 1);
        }

        if (Combo) LevelPlayingControl.GetComponent<PlayingData>().Combo += 1;
        else LevelPlayingControl.GetComponent<PlayingData>().Combo = 0;

        if (isEffect)
        {
            GameObject noteInstance = Instantiate(Effect, transform.position, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }
}
