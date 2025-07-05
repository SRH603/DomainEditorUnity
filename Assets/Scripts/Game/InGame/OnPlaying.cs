using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static GameUtilities.InGameUtilities;
using static GameUtilities;

public class OnPlaying : MonoBehaviour
{
    private GenerateLevel generateLevel;
    private PlayingData playingData;
    public float currentTime = 0;
    public float offset, offsetTime;
    public float currentAudioTime = 0;
    public float currentBeat = 0;
    [HideInInspector]public float nowBeatHalfInt = 0;
    
    public float currentBPM = -1; // Only for display
    public List<JudgmentLineData> allLines = new List<JudgmentLineData>();
    public bool isStart = false;
    public AudioSource LevelMusic;
    [HideInInspector] public bool HoldGenerateEffectTick = false;
    public BPMTimingList bpmTimingList = new BPMTimingList();
    public GameObject EndUI;
    public Button pauseButton;
    public GameObject pauseGroup;

    //public TMP_Text text;

    public void LoadLevel()
    {
        generateLevel = GetComponent<GenerateLevel>();
        playingData = GetComponent<PlayingData>();
        
        offset = GetComponent<ReadChart>().gameData.info.offset + PlayerPrefs.GetFloat("audioOffset",0);

        double generatingtime = 0;

        int i = 0;
        foreach (var bpmChanges in generateLevel.gameData.content.bpmList)
        {

            bpmTimingList.Changes.Add(new BPMTiming(bpmChanges.bpm, (float)generatingtime));
            if (generateLevel.gameData.content.bpmList.Length == i + 1)
            {
                break;
            }
            else
            {
                generatingtime += (FractionToDecimal(generateLevel.gameData.content.bpmList[i + 1].startBeat) - FractionToDecimal(bpmChanges.startBeat)) / (bpmChanges.bpm / 60);
            }

            ++i;
        }

        if (currentBPM == -1)
        {
            currentBPM = GetComponent<GenerateLevel>().gameData.content.bpmList[0].bpm;
        }

        allLines = generateLevel.AllJudgementLines;

        LevelMusic.enabled = true;
        LevelMusic.Pause();
        
        Invoke(nameof(BetaClick), 1.5f);
    }

    void Update()
    {
        //text.text = currentTime + " " + currentAudioTime + " " + currentBeat + " " + currentBPM;
        //if (Input.GetMouseButtonDown(0)) Debug.Log(currentBeat);
        currentAudioTime = LevelMusic.time;

        if (isStart == true) ChartPlaying();
        
        // End
        if (generateLevel != null && currentBeat >= generateLevel.totalBeats)
        {
            BetaPause();
            EndUI.SetActive(true);
            playingData.DisplayFinal();
            this.enabled = false;
        }


    }

    void ChartPlaying()
    {
        MoveNotesTowardsJudgementLine();
        currentTime += Time.deltaTime;
        offsetTime = currentTime - offset;
        currentBeat = Time2Beat(offsetTime, bpmTimingList);
        LegacyHold();
        currentBPM = BPMControl(currentBPM);
        foreach (var Lines in allLines)
        {
            UpdateJudgementLineAnimation(Lines, currentBeat);
        }
        
    }

    public float Time2Beat(float Time, BPMTimingList bpmTimingList)
    {
        float Beat = 0;
        for (int i = 0; i <= bpmTimingList.Changes.Count - 1; ++i)
        {
            if (i < bpmTimingList.Changes.Count - 1)
            {
                if (Time < bpmTimingList.Changes[i + 1].StartTime)
                {
                    Beat += (Time - bpmTimingList.Changes[i].StartTime) * (bpmTimingList.Changes[i].BPM / 60);
                    break;
                }
                else
                {
                    Beat += (bpmTimingList.Changes[i + 1].StartTime - bpmTimingList.Changes[i].StartTime) * (bpmTimingList.Changes[i].BPM / 60);
                }
            }
            else
            {
                Beat += (Time - bpmTimingList.Changes[i].StartTime) * (bpmTimingList.Changes[i].BPM / 60);
                break;
            }
        }
        return Beat;
    }
    void MoveNotesTowardsJudgementLine()
    {
        if (allLines != null)
        {
            foreach (var line in allLines)
            {
                foreach (var note in line.Notes)
                {
                    double BeatBasedDistance = AdvancedCalculateIntegratedSpeed(line.speed, currentBeat, (float)note.GetComponent<NoteEntity>().HitBeat);
                    double FinalDistance = BeatBasedDistance * note.GetComponent<NoteEntity>().Speed;
                    note.transform.localPosition = new Vector3((float)FinalDistance, note.transform.localPosition.y, note.transform.localPosition.z);
                }
            }
        }
    }
    float BPMControl(float thisBPM) // Only for display
    {
        foreach (var ChangedBPM in bpmTimingList.Changes)
        {

            if (offsetTime <= ChangedBPM.StartTime)
            {
                return thisBPM;
            }
            else
            {
                thisBPM = ChangedBPM.BPM;
            }
        }
        return thisBPM;
    }

    void LegacyHold()
    {
        float originalBeat = nowBeatHalfInt;
        nowBeatHalfInt = (int)(currentBeat * 4);
        nowBeatHalfInt /= 4;
        if (nowBeatHalfInt > originalBeat)
        {
            HoldGenerateEffectTick = true;
        }
        else
        {
            HoldGenerateEffectTick = false;
        }
    }

    public void UpdateJudgementLineAnimation(JudgmentLineData judgementLine, float currentBeat)
    {
        if (judgementLine == null) return;
        Vector3 position = new Vector3(judgementLine.positionX.Evaluate(currentBeat), judgementLine.positionY.Evaluate(currentBeat), judgementLine.positionZ.Evaluate(currentBeat));
        judgementLine.LineObject.transform.localPosition = position;

        Quaternion rotation = Quaternion.Euler(judgementLine.rotationX.Evaluate(currentBeat), judgementLine.rotationY.Evaluate(currentBeat), judgementLine.rotationZ.Evaluate(currentBeat));
        judgementLine.LineObject.transform.localRotation = rotation;

        Renderer renderer = judgementLine.LineObject.transform.Find("JudgementLine").GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.sharedMaterial.color;
            color.a = judgementLine.transparency.Evaluate(currentBeat);
            renderer.sharedMaterial.color = color;
        }
        else
        {
            Debug.LogWarning("Cannot find Renderer");
        }
        judgementLine.FlowSpeed = (float)ReturnJudgementLineSpeedAnimation(currentBeat, judgementLine);
    }

    // ????????????????????????????????????????
    private float CalculateProgress(float currentBeat, Vector3Int startTime, Vector3Int endTime)
    {
        double startBeat = FractionToDecimal(startTime);
        double endBeat = FractionToDecimal(endTime);
        return (float)((currentBeat - startBeat) / (endBeat - startBeat));
    }

    

    public double ReturnJudgementLineSpeedAnimation(double currentBeat, JudgmentLineData judgementLine)
    {
        AnimationSpeed lastAnimSpeed = null;
        bool animationUpdated = false;
        float returnvalue = 0;

        // ????????????????
        foreach (var animSpeed in judgementLine.speed)
        {
            if (currentBeat >= FractionToDecimal(animSpeed.endBeat))
            {
                lastAnimSpeed = animSpeed; // ????????????????????
            }
            if (currentBeat <= FractionToDecimal(animSpeed.startBeat))
            {
                continue;  // ??????????????????????????????????????????????????
            }
            float animStartTime = (float)FractionToDecimal(animSpeed.startBeat);
            float animEndTime = (float)FractionToDecimal(animSpeed.endBeat);

            // ??????????????????????????????????????????
            if (currentBeat >= animStartTime && currentBeat <= animEndTime)
            {
                float progress = CalculateProgress((float)currentBeat, animSpeed.startBeat, animSpeed.endBeat);
                float currentSpeed = Mathf.Lerp((float)animSpeed.start, (float)animSpeed.end, progress);
                // ?????????????????? NoteFlowSpeed ?????????? judgementLine ????
                

                animationUpdated = true;
                returnvalue = currentSpeed;
                break;  // ??????????????????????????????????????
            }
        }

        // ??????????????????????????????????????????????????????????????????????????????????????????????
        if (lastAnimSpeed != null && !animationUpdated && currentBeat > FractionToDecimal(lastAnimSpeed.endBeat))
        {
            // ????????????????????????????????
            returnvalue = (float)lastAnimSpeed.end;
        }
        return returnvalue;
    }
    
    /// <summary>
    /// 当应用暂停（切到后台）／恢复（回到前台）时被调用
    /// </summary>
    /// <param name="pauseStatus">true = 应用已暂停／进入后台；false = 应用已恢复／回到前台</param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 应用切到后台，暂停游戏
            Pause();
            //Debug.Log("应用已暂停，游戏已暂停");
        }
        else
        {
            // 应用回到前台，根据需求决定是否自动恢复
            // 若不想自动恢复，可留空或弹出“继续/重试”界面
            //BetaClick();
            //Debug.Log("应用已恢复，游戏已继续");
        }
    }

    /// <summary>
    /// 当应用失去／获得焦点时被调用（某些平台会优先调用 OnApplicationFocus）
    /// </summary>
    /// <param name="hasFocus">true = 获得焦点；false = 失去焦点</param>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // 也可以在这里做一遍，作为保险
            Pause();
            //Debug.Log("应用失去焦点，游戏已暂停");
        }
        else
        {
            // 恢复
            //BetaClick();
            //Debug.Log("应用重新获得焦点，游戏已继续");
        }
    }


    public void BetaClick()
    {
        if (isStart == false) isStart = true;
        pauseGroup.SetActive(false);
        pauseButton.interactable = true;
        //Auto = true;
        LevelMusic.UnPause();
    }
    public void BetaPause()
    {
        isStart = false;
        pauseButton.interactable = false;
        LevelMusic.Pause();
        //Auto = true;
    }
    
    public void Pause()
    {
        pauseGroup.SetActive(true);
        pauseButton.interactable = false;
        isStart = false;
        LevelMusic.Pause();
        //Auto = true;
    }
    public void Replay()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
