using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameUtilities;
using static GameUtilities.NoteUtilities;
using static GameUtilities.JudgeData;
using System.Reflection;

public class Hold : NoteEntity
{
    [System.Serializable]
    public class HoldColliderData
    {
        public Collider collider;
        public double value;
        public int state = -1;
        public bool tagged = false;
        public bool hitted = false;

        public HoldColliderData(Collider collider, double value)
        {
            this.collider = collider;
            this.value = value;
        }
    }

    [HideInInspector] public double StartTime, EndTime;
    [HideInInspector] public List<HoldColliderData> noteColliderList = new List<HoldColliderData>();

    [HideInInspector] public List<GameObject> HoldComponents = new List<GameObject>();
    [HideInInspector] public float startBeat = -1, endBeat, TotalScore;

    public void UpdateHoldSE()
    {
        StartTime = CalculateIntegratedHitTime(LevelPlayingControl.GetComponent<ReadChart>().gameData.content.bpmList, startBeat);
        EndTime = CalculateIntegratedHitTime(LevelPlayingControl.GetComponent<ReadChart>().gameData.content.bpmList, endBeat);
    }
    public void InitializeHold()
    {
        LevelPlayingControl = GameObject.Find("LevelControl");
        // cpnP
        if (HoldComponents != null && HoldComponents.Count > 0 && startBeat == -1 && HoldComponents[^1].GetComponent<HoldLocationPool>().HitTime != 0)
        {

            startBeat = HoldComponents[0].GetComponent<HoldLocationPool>().HitTime;
            endBeat = HoldComponents[HoldComponents.Count - 1].GetComponent<HoldLocationPool>().HitTime;

            UpdateHoldSE();
        }
        if (AppearTime != -1) Mesh.SetActive(false);
        if (AppearTime == -1) Mesh.SetActive(true);

        HitTime = CalculateIntegratedHitTime(LevelPlayingControl.GetComponent<ReadChart>().gameData.content.bpmList, HitBeat);

        Vector3 Noteposition = this.transform.localPosition;


        // 从startBeat到endBeat，每次增加0.25
        // 很仁慈，没有尾判
        for (float indexBeat = startBeat; indexBeat < endBeat; indexBeat += 0.25f)
        {
            Collider noteCollider = Instantiate(NoteColliderParent, Noteposition, Quaternion.identity, transform.parent);
            Vector3 colliderPosition = noteCollider.transform.position;
            noteCollider.transform.localPosition = new Vector3(0, colliderPosition.y, colliderPosition.z);
            noteCollider.transform.localRotation = Quaternion.identity;
            noteCollider.GetComponent<JudgeCollider>().ParentNote = gameObject;

            double hitTime = CalculateIntegratedHitTime(LevelPlayingControl.GetComponent<ReadChart>().gameData.content.bpmList, indexBeat);

            HoldColliderData colliderData = new HoldColliderData(noteCollider, hitTime);
            noteColliderList.Add(colliderData);
        }
    }

    void Update()
    {
        // Show Note
        if (AppearTime != -1 && HitBeat - LevelPlayingControl.GetComponent<OnPlaying>().offsetTime >= AppearTime) Mesh.SetActive(true);
        if (State != 7) RefreshState();

        // cpnP
        foreach (GameObject HoldCPN in HoldComponents)
        {
            float BaseValue = -transform.localPosition.x - HoldCPN.transform.localPosition.x;
            HoldCPN.GetComponent<HoldLocationPool>().UpdateCutoff(BaseValue);
        }

    }

    
    void RefreshState()
    {
        if (LevelPlayingControl.GetComponent<OnPlaying>().isStart)
        {
            double currentTime = LevelPlayingControl.GetComponent<OnPlaying>().offsetTime;

            if (currentTime - EndTime > 0)
            {
                State = 7;
                Note.SetActive(false);
                HighLightNote.SetActive(false);
            }
            else if (currentTime - StartTime > 0)
            {
                State = 4;
                Note.SetActive(false);
                HighLightNote.SetActive(false);

            }
            else if (currentTime - StartTime >= -optimalJudgmentTime)
            {
                State = 3;
            }

            foreach (var collider in noteColliderList)
            {

                collider.collider.transform.localPosition = new Vector3(0, CurrentYPos(), 0);

                if (currentTime - collider.value >= optimalJudgmentTime) collider.collider.enabled = false;
                else if (currentTime - collider.value >= -optimalJudgmentTime) collider.collider.enabled = true;

                double deltaTime = LevelPlayingControl.GetComponent<OnPlaying>().offsetTime - collider.value;

                // Determine if need to enable judgment

                if (deltaTime >= optimalJudgmentTime) collider.collider.enabled = false;
                else if (deltaTime >= -optimalJudgmentTime) collider.collider.enabled = true;

                // Determining the state with interval

                Dictionary<double, int> judgmentDict = new Dictionary<double, int>
                {
                    { optimalJudgmentTime, 7 }, // miss
                    { 0, 4 }, // late optimal
                    { -optimalJudgmentTime, 3 }, // early optimal
                };

                // Search for state
                foreach (var entry in judgmentDict)
                {
                    if (deltaTime > entry.Key)
                    {
                        collider.state = entry.Value;
                        if (State == 4 && collider.tagged == true) Hit(collider);
                        break;
                    }
                }
                if (collider.state == 7 && collider.hitted == false) // miss
                {
                    LevelPlayingControl.GetComponent<PlayingData>().MissNum += 1;
                    LevelPlayingControl.GetComponent<PlayingData>().Combo = 0;
                    collider.collider.gameObject.SetActive(false);
                    collider.hitted = true;
                    //noteColliderList.Remove(collider);
                }
            }
            

        }
        if (State == 7)
        {
            gameObject.SetActive(false);
        }
    }
    

    public void Hit(HoldColliderData noteCollider)
    {
        noteCollider.collider.gameObject.SetActive(false);
        if (noteCollider.hitted == false)
        {
            noteCollider.hitted = true;
            if (noteCollider.state == 4) Judge("OptimalNum", true, true, OptimalEffectPrefab, noteCollider.collider.transform);
            else if (noteCollider.state == 3) Judge("OptimalNum", true, true, OptimalEffectPrefab, noteCollider.collider.transform);
        }
    }
    public void Judge(string JudgeType, bool Combo, bool isEffect, GameObject Effect, Transform transform)
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

        //gameObject.SetActive(false);
    }
    public void TagElement(Collider noteCollider)
    {
        for (int i = 0; i < noteColliderList.Count; i++)
        {
            HoldColliderData data = noteColliderList[i];

            // 判断当前 HoldColliderData 的 collider 是否与传入的 noteCollider 匹配
            if (data.collider == noteCollider)
            {
                data.tagged = true;
                break;
            }
        }
    }


    public float CurrentYPos()
    {

        foreach (var component in HoldComponents)
        {
            HoldLocationPool pool = component.GetComponent<HoldLocationPool>();
            if (pool != null && LevelPlayingControl.GetComponent<OnPlaying>().currentBeat >= pool.HitTime && LevelPlayingControl.GetComponent<OnPlaying>().currentBeat <= pool.EndTime)
            {
                float BaseValue = -transform.localPosition.x - component.transform.localPosition.x;
                return component.GetComponent<HoldLocationPool>().currentYPos(BaseValue) + transform.localPosition.y;

            }
        }

        return transform.localPosition.y;

    }

}
