using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldLocationPool : MonoBehaviour
{
    public List<GameObject> HoldComponents = new List<GameObject>();
    public float HitTime;
    public float EndTime;
    public float HitPosition;
    public float EndPosition;
    public float HitTimePos;
    public float EndTimePos;
    public GameObject HoldPlane;
    public GameObject parallelogram;
    public float cutoffX = 0f;
    public GameObject RunTimePlane;
    public int EasingMode;
    public float StartPCT, EndPCT;
    [HideInInspector] public OnPlaying onPlaying;
    [HideInInspector] public bool LastComponent = true;
    public GameObject EffectPrefabP, EffectPrefabp, EffectPrefabG;
    // Start is called before the first frame update
    void Start()
    {
        onPlaying = GameObject.Find("LevelControl").GetComponent<OnPlaying>();
    }

    public void UpdateCutoff(float newCutoffX)
    {
        RunTimePlane.SetActive(false);
        if (parallelogram != null)
        {
            cutoffX = newCutoffX;
            parallelogram.GetComponent<Renderer>().material.SetFloat("_Cutoff", cutoffX);
            float YPos;
            if (EndTimePos != HitTimePos)
            {
                YPos = newCutoffX * ((EndPosition - HitPosition) / (EndTimePos - HitTimePos));
            }
            else
            {
                YPos = newCutoffX * (EndPosition - HitPosition);
            }
            
            RunTimePlane.transform.localPosition = new Vector3 (newCutoffX, YPos, 0);

            if (onPlaying.currentBeat >= HitTime && onPlaying.currentBeat <= EndTime && LastComponent == false)
            {
                RunTimePlane.SetActive(true);
            }
            else
            {
                RunTimePlane.SetActive(false);
            }
            if (onPlaying.currentBeat >= EndTime)
            {
                gameObject.SetActive(false);
            }

        }
        
    }
    public float currentYPos(float newCutoffX)
    {
        float YPos;
        if (EndTimePos != HitTimePos)
        {
            YPos = newCutoffX * ((EndPosition - HitPosition) / (EndTimePos - HitTimePos));
        }
        else
        {
            YPos = newCutoffX * (EndPosition - HitPosition);
        }
        return YPos + transform.localPosition.y;
    }
}
