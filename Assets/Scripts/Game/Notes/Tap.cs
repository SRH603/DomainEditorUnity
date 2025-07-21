using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static GameUtilities.JudgeData;
using static GameUtilities.NoteUtilities;

public class Tap : NoteEntity
{
    [HideInInspector] public CapsuleCollider NoteCollider;
    [HideInInspector] public bool Hitted = false;


    public void Start()
    {
        //Debug.Log(this.gameObject.activeSelf);
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

            if (deltaTime >= goodJudgmentTime) NoteCollider.enabled = false;
            else if (deltaTime >= -badJudgmentTime) NoteCollider.enabled = true;

            // Determining the state with interval

            Dictionary<double, int> judgmentDict = new Dictionary<double, int>
            {
                { goodJudgmentTime, 7 }, // miss
                { perfectJudgmentTime, 6 }, // late good
                { optimalJudgmentTime, 5 }, // late perfect
                { 0, 4 }, // late optimal
                { -optimalJudgmentTime, 3 }, // early optimal
                { -perfectJudgmentTime, 2 }, // early perfect
                { -goodJudgmentTime, 1 }, // early good
                { -badJudgmentTime, 0 } // bad
            };

            // Search for state
            foreach (var entry in judgmentDict)
            {
                if (deltaTime > entry.Key)
                {
                    State = entry.Value;
                    break;
                }
            }

        }

        if (State == 7) // miss
        {
            LevelPlayingControl.GetComponent<PlayingData>().MissNum += 1;
            LevelPlayingControl.GetComponent<PlayingData>().Combo = 0;
            //Debug.Log("漏了一个Tap" + HitBeat);
            //gameObject.SetActive(false);
        }
    }

    public void Hit()
    {
        NoteCollider.gameObject.SetActive(false);
        
        if (Hitted == false)
        {
            Hitted = true;
            if (State == 6) Judge("LateGoodNum", true, true, GoodEffectPrefab);
            else if (State == 5) Judge("LatePerfectNum", true, true, PerfectEffectPrefab);
            else if (State == 4) Judge("OptimalNum", true, true, OptimalEffectPrefab);
            else if (State == 3) Judge("OptimalNum", true, true, OptimalEffectPrefab);
            else if (State == 2) Judge("EarlyPerfectNum", true, true, PerfectEffectPrefab);
            else if (State == 1) Judge("EarlyGoodNum", true, true, GoodEffectPrefab);
            else if (State == 0) Judge("BadNum", false, false, new GameObject());
            if (State != 0) audioSource.PlayOneShot(hitSound);
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
