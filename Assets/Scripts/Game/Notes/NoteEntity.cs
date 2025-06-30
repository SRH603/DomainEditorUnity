using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteEntity : MonoBehaviour
{
    [HideInInspector] public GameObject LevelPlayingControl;
    public CapsuleCollider NoteColliderParent;
    public GameObject Mesh;
    public GameObject Note, HighLightNote;
    public GameObject OptimalEffectPrefab, PerfectEffectPrefab, GoodEffectPrefab;
    public AudioSource audioSource;
    public AudioClip hitSound;

    [HideInInspector] public double HitBeat; //应该是HitBeat （要修改）
    [HideInInspector] public double HitTime;
    [HideInInspector] public double AppearTime;
    [HideInInspector] public double Speed;
    [HideInInspector] public int State = -1; //-1 not loaded, 0 bad, 1 early good, 2 early p, 3 early P, 4 late P, 5 late p, 6 late good, 7 miss


    public void HighLight()
    {
        Note.SetActive(false);
        HighLightNote.SetActive(true);
    }
    public void UnHighLight()
    {
        Note.SetActive(true);
        HighLightNote.SetActive(false);
    }
}
