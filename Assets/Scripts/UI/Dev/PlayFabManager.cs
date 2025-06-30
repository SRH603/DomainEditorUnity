using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public Archive archive;
    
    public void UnlockPack(string packId)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "unlockPack",
            FunctionParameter = new { packId = packId },
            GeneratePlayStreamEvent = true
        };
        
        PlayFabClientAPI.ExecuteCloudScript(request, OnSuccess, OnError);
    }

    public void UnlockChart(string trackId, int ratingClass)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "unlockChart",
            FunctionParameter = new { trackId = trackId, ratingClass = ratingClass },
            GeneratePlayStreamEvent = true
        };
        
        PlayFabClientAPI.ExecuteCloudScript(request, OnSuccess, OnError);
    }

    public void UnlockTrack(string trackId)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "unlockTrack",
            FunctionParameter = new { trackId = trackId},
            GeneratePlayStreamEvent = true
        };
        
        PlayFabClientAPI.ExecuteCloudScript(request, OnSuccess, OnError);
    }
    
    public void UpdateScore(string trackId, int ratingClass, int score, int optimism)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "updateScore",
            FunctionParameter = new { trackId = trackId, ratingClass = ratingClass, score = score, optimism = optimism },
            GeneratePlayStreamEvent = true
        };
        
        PlayFabClientAPI.ExecuteCloudScript(request, OnSuccess, OnError);
    }

    private void OnSuccess(ExecuteCloudScriptResult result)
    {
        string jsonResult = JsonUtility.ToJson(result, true); // 第二个参数为 true 时，格式化输出
        Debug.Log("Cloud Script executed successfully: " + jsonResult);
        //Debug.Log("Cloud Script executed successfully: " + result);
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Cloud Script execution failed: " + error.GenerateErrorReport());
    }
    
    // 示例：加载玩家存档
    public Archive LoadPlayerArchive()
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId
        };

        PlayFabClientAPI.GetUserData(request, OnDataLoaded, OnError);
        return archive;
    }

    private void OnDataLoaded(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("archiveData"))
        {
            string archiveJson = result.Data["archiveData"].Value;
            archive = JsonUtility.FromJson<Archive>(archiveJson);
            Debug.Log("Player archive loaded successfully!");
            // 使用加载的存档数据
        }
        else
        {
            Debug.LogError("No archive data found.");
        }
    }
    
    /// <summary>
    /// 通用的异步方法，用于执行 Cloud Script 并等待完成。
    /// </summary>
    /// <param name="functionName">Cloud Script 名称</param>
    /// <param name="parameters">参数对象</param>
    /// <returns>Task</returns>
    public async Task ExecuteCloudScriptAsync(string functionName, object parameters)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = functionName,
            FunctionParameter = parameters,
            GeneratePlayStreamEvent = true
        };

        var taskCompletionSource = new TaskCompletionSource<bool>();

        PlayFabClientAPI.ExecuteCloudScript(request, (result) => {
            Debug.Log($"Cloud Script '{functionName}' 执行成功: {JsonUtility.ToJson(result, true)}");
            taskCompletionSource.SetResult(true);
        }, (error) => {
            Debug.LogError($"Cloud Script '{functionName}' 执行失败: {error.GenerateErrorReport()}");
            taskCompletionSource.SetException(new System.Exception(error.GenerateErrorReport()));
        });

        await taskCompletionSource.Task;
    }
}