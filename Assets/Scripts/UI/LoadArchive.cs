using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class TrackArchive
{
    public string id;
    public List<ChartArchive> charts;
    public bool unlocked;
}

[System.Serializable]
public class ChartArchive
{
    [FormerlySerializedAs("RatingClass")] public int ratingClass;
    [FormerlySerializedAs("Score")] public int score = 0;
    [FormerlySerializedAs("Optimism")] public int optimism = 0;
    public bool unlocked = false;
}

[System.Serializable]
public class PackArchive
{
    public string id;
    public bool unlocked = false;
}

[System.Serializable]
public class Archive
{
    public string playerName;
    public List<string> titles;
    public string displayedTitle;
    public List<string> avatars;
    public string displayedAvatar;
    public int courseModeLevel;
    public int echo;
    public bool isMaster;
    public List<PackArchive> packs;
    public List<TrackArchive> tracks;
}

public class LoadArchive : MonoBehaviour
{
    private Archive archive;

    // 将本地存档同步到 PlayFab
    public void SyncArchiveToPlayFab(Archive archive)
    {
        string jsonData = JsonUtility.ToJson(archive);
        UpdateUserDataRequest request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "archiveData", jsonData }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSyncSuccess, OnSyncFailure);
    }

    // 从 PlayFab 读取存档数据并更新本地存档
    public void LoadArchiveFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadSuccess, OnLoadFailure);
    }

    // PlayFab 存档同步成功回调
    private void OnSyncSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Archive data synced to PlayFab successfully.");
    }

    // PlayFab 存档同步失败回调
    private void OnSyncFailure(PlayFabError error)
    {
        Debug.LogError("Failed to sync archive data to PlayFab: " + error.GenerateErrorReport());
    }

    // PlayFab 读取存档成功回调
    private void OnLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("archiveData"))
        {
            string jsonData = result.Data["archiveData"].Value;
            archive = JsonUtility.FromJson<Archive>(jsonData);
            Debug.Log("Archive data loaded from PlayFab successfully.");
        }
        else
        {
            Debug.Log("No archive data found on PlayFab.");
        }
    }

    // PlayFab 读取存档失败回调
    private void OnLoadFailure(PlayFabError error)
    {
        Debug.LogError("Failed to load archive data from PlayFab: " + error.GenerateErrorReport());
    }
}
