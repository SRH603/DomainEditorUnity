using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public static class GameUtilities
{
    [System.Serializable]
    public class JudgmentLineData
    {
        public GameObject LineObject; // 判定线的GameObject
        public double FlowSpeed;
        public List<GameObject> Notes; // 该判定线上的所有notes

        public AnimationCurve positionX;
        public AnimationCurve positionY;
        public AnimationCurve positionZ;
        public AnimationCurve rotationX;
        public AnimationCurve rotationY;
        public AnimationCurve rotationZ;
        public AnimationCurve transparency;
        public AnimationSpeed[] speed;

        public JudgmentLineData(GameObject lineObject, double flowSpeed)
        {
            LineObject = lineObject;
            Notes = new List<GameObject>();
            FlowSpeed = flowSpeed;
        }
    }
    [System.Serializable]
    public class BPMTiming
    {
        public float BPM;       // BPM??
        public float StartTime; // ????????

        public BPMTiming(float bpm, float startTime)
        {
            BPM = bpm;
            StartTime = startTime;
        }
    }

    [System.Serializable]
    public class BPMTimingList
    {
        public List<BPMTiming> Changes = new List<BPMTiming>();

        public void AddChange(float bpm, float startTime)
        {
            Changes.Add(new BPMTiming(bpm, startTime));
        }
    }
    public static class JudgeData // Judgment Time
    {
        public static double badJudgmentTime = 0.110;
        public static double goodJudgmentTime = 0.095;
        public static double perfectJudgmentTime = 0.07;
        public static double optimalJudgmentTime = 0.035;
    }

    public static class NoteUtilities
    {
        public static double CalculateIntegratedHitTime(BPMList[] bpmChanges, double hittime)
        {
            double integratedHittime = 0.0f;


            for (int i = 0; i < bpmChanges.Length; i++)
            {
                double SPB = (1 / bpmChanges[i].bpm) * 60;

                if (FractionToDecimal(bpmChanges[i].startBeat) <= hittime)
                {
                    if (i != bpmChanges.Length - 1)
                    {
                        if (FractionToDecimal(bpmChanges[i + 1].startBeat) >= hittime)
                        {
                            integratedHittime += SPB * (hittime - FractionToDecimal(bpmChanges[i].startBeat));
                            break;
                        }
                        else
                        {
                            integratedHittime += SPB * (FractionToDecimal(bpmChanges[i + 1].startBeat) - FractionToDecimal(bpmChanges[i].startBeat));
                        }
                    }
                    else
                    {
                        integratedHittime += SPB * (hittime - FractionToDecimal(bpmChanges[i].startBeat));
                    }

                }

            }

            return integratedHittime;
        }
    }

    public static class InGameUtilities
    {
        public static double AdvancedCalculateIntegratedSpeed(AnimationSpeed[] animations, double currentTime, double hittime)
        {
            if (hittime >= currentTime)
                return CalculateIntegratedSpeed(animations, currentTime, hittime);
            else return -CalculateIntegratedSpeed(animations, hittime, currentTime);
        }
        public static double CalculateIntegratedSpeed(AnimationSpeed[] animations, double currentTime, double hittime)
        {
            double integratedSpeed = 0.0f;
            bool Within = false, finished = false;
            double lastendtime = 0, lastendspeed = 0;

            foreach (var animation in animations)
            {
                double animationStartTime = FractionToDecimal(animation.startBeat);
                double animationEndTime = FractionToDecimal(animation.endBeat);

                // ????????????????????????????????
                if (Within == false)
                {
                    if (currentTime <= animationEndTime && currentTime >= animationStartTime && hittime <= animationEndTime && hittime >= animationStartTime)
                    {
                        double timeLength = hittime - currentTime;

                        // ????????????????????????
                        double speedAtCurrentTime = Lerp(animation.start, animation.end, (currentTime - animationStartTime) / (animationEndTime - animationStartTime));

                        // ????????????????????????
                        double speedAtEndTime = Lerp(animation.start, animation.end, (hittime - animationStartTime) / (animationEndTime - animationStartTime));

                        // ????????????
                        double averageSpeed = (speedAtCurrentTime + speedAtEndTime) / 2;

                        // ????????????????????????????????????
                        integratedSpeed += averageSpeed * timeLength;
                        Within = false;
                        finished = true;
                        break;
                    }
                    else
                    {
                        if (currentTime <= animationEndTime && currentTime >= animationStartTime)
                        {

                            double timeLength = animationEndTime - currentTime;

                            // ????????????????????????
                            double speedAtCurrentTime = Lerp(animation.start, animation.end, (currentTime - animationStartTime) / (animationEndTime - animationStartTime));

                            // ????????????????????????
                            double speedAtEndTime = Lerp(animation.start, animation.end, (animationEndTime - animationStartTime) / (animationEndTime - animationStartTime));

                            // ????????????
                            double averageSpeed = (speedAtCurrentTime + speedAtEndTime) / 2;

                            // ????????????????????????????????????
                            integratedSpeed += averageSpeed * timeLength;
                            Within = true;
                            lastendtime = animationEndTime;
                            lastendspeed = animation.end;
                            continue;
                        }
                        else
                        {
                            if (currentTime < animationStartTime)
                            {

                                if (hittime >= animationStartTime && hittime <= animationEndTime)
                                {

                                    double timeLength = hittime - animationStartTime;

                                    // ????????????????????????
                                    double speedAtCurrentTime = Lerp(animation.start, animation.end, (animationStartTime - animationStartTime) / (animationEndTime - animationStartTime));

                                    // ????????????????????????
                                    double speedAtEndTime = Lerp(animation.start, animation.end, (hittime - animationStartTime) / (animationEndTime - animationStartTime));

                                    // ????????????
                                    double averageSpeed = (speedAtCurrentTime + speedAtEndTime) / 2;

                                    // ????????????????????????????????????
                                    integratedSpeed += averageSpeed * timeLength;
                                    integratedSpeed += (animationStartTime - currentTime) * lastendspeed;
                                    //integratedSpeed += (animationStartTime - lastendtime) * lastendspeed;
                                    Within = false;
                                    finished = true;
                                    break;
                                }
                                else if (hittime < animationStartTime)
                                {
                                    integratedSpeed = (hittime - currentTime) * lastendspeed;
                                    Within = false;
                                    finished = true;
                                    break;
                                }
                                else
                                {
                                    integratedSpeed += (animationStartTime - currentTime) * lastendspeed;
                                    integratedSpeed += (animationEndTime - animationStartTime) * (animation.end + animation.start) / 2;
                                }
                                Within = true;
                                lastendtime = animationEndTime;
                                lastendspeed = animation.end;
                                continue;

                            }
                        }


                    }
                }
                if (Within == true)
                {
                    if (hittime >= animationStartTime && hittime <= animationEndTime)
                    {

                        double timeLength = hittime - animationStartTime;

                        // ????????????????????????
                        double speedAtCurrentTime = Lerp(animation.start, animation.end, (animationStartTime - animationStartTime) / (animationEndTime - animationStartTime));

                        // ????????????????????????
                        double speedAtEndTime = Lerp(animation.start, animation.end, (hittime - animationStartTime) / (animationEndTime - animationStartTime));

                        // ????????????
                        double averageSpeed = (speedAtCurrentTime + speedAtEndTime) / 2;

                        // ????????????????????????????????????
                        integratedSpeed += averageSpeed * timeLength;
                        integratedSpeed += (animationStartTime - lastendtime) * lastendspeed;
                        Within = false;
                        finished = true;
                        break;
                    }
                    else if (hittime < animationStartTime)
                    {
                        integratedSpeed += (hittime - lastendtime) * lastendspeed;
                        Within = false;
                        finished = true;
                        break;
                    }
                    else
                    {
                        integratedSpeed += (animationEndTime - animationStartTime) * (animation.end + animation.start) / 2;
                        integratedSpeed += (animationStartTime - lastendtime) * lastendspeed;
                    }
                }
                lastendtime = animationEndTime;
                lastendspeed = animation.end;

            }

            if (Within == true)
            {
                integratedSpeed += (hittime - lastendtime) * lastendspeed;
                finished = true;
            }
            if (Within == false && finished == false)
            {
                integratedSpeed = (hittime - currentTime) * lastendspeed;
            }

            return integratedSpeed;
        }
    }

    private static double Lerp(double start, double end, double t)
    {
        return start + (end - start) * t;
    }

    public static double FractionToDecimal(Vector3Int time)
    {
        // 0 + 2 / 1
        if (time[1] == 0)
        {
            return time[0];
        }
        else
        {
            return time[0] + (float)time[2] / time[1];
        }
    }

    public static Transform FindDeepChild(Transform parent, string name)
    {
        // 检查当前层级的子物体
        Transform result = parent.Find(name);
        if (result != null)
        {
            return result;
        }

        // 如果当前层级没有找到，递归检查所有子层级
        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, name);
            if (result != null)
            {
                return result;
            }
        }

        // 如果没有找到，返回null
        return null;
    }

    public static class Archive
    {
        public static global::Archive archive;

        // 存储文件路径
        private static string archiveFilePath = Path.Combine(Application.persistentDataPath, "archive.json");
    
    // 存储档案到本地文件
    public static void SaveLocalArchive(global::Archive archive)
    {
        string jsonData = JsonUtility.ToJson(archive, true);
        File.WriteAllText(archiveFilePath, jsonData);
        Debug.Log("Archive saved locally at: " + archiveFilePath);
    }

    public static void ClearArchive()
    {
        File.WriteAllText(archiveFilePath, "{}");
        Debug.Log("Data Cleared!");
    }

    // 读取本地存档文件
    public static global::Archive LoadLocalArchive()
    {
        if (File.Exists(archiveFilePath))
        {
            string jsonData = File.ReadAllText(archiveFilePath);
            Debug.Log("Archive loaded from local storage.");
            archive = JsonUtility.FromJson<global::Archive>(jsonData);
            return JsonUtility.FromJson<global::Archive>(jsonData);
        }
        else
        {
            Debug.Log("No local archive found, using default values.");
            // 创建一个新的 Archive 对象，并为它填充默认数据
            global::Archive newArchive = new global::Archive
            {
                echo = 0, // 默认值
                packs = new List<PackArchive>(), // 默认空列表
                tracks = new List<TrackArchive>() // 默认空列表
            };

            // 将新创建的 Archive 序列化为 JSON 并保存到文件
            string json = JsonUtility.ToJson(newArchive, true);
            File.WriteAllText(archiveFilePath, json);

            // 返回新创建的 Archive 对象
            return newArchive;
            //return new global::Archive(); // 如果没有找到存档，使用默认空的 Archive 对象
        }
    }
    
    public static void AddEcho(int amount)
    {
        global::Archive archive = LoadLocalArchive();

        archive.echo += amount;
        
        SaveLocalArchive(archive);  // 保存更新后的存档
    }

    public static int GetEcho()
    {
        global::Archive archive = LoadLocalArchive();
        return archive.echo;
    }
    
    public static void UnlockRatingClass2()
    {
        global::Archive archive = LoadLocalArchive();

        archive.isMaster = true;
        
        SaveLocalArchive(archive);  // 保存更新后的存档
    }
    
    public static void UnlockTrack(string trackId)
    {
        global::Archive archive = LoadLocalArchive();
        // 查找是否已经存在该trackId的track
        TrackArchive track = archive.tracks.Find(t => t.id == trackId);
    
        if (track != null)
        {
            if (track.unlocked)
                Debug.Log("The track is already unlocked.");
            else
                track.unlocked = true;
        }
        else
        {
            // 如果没有找到track，创建一个新的track并解锁
            archive.tracks.Add(new TrackArchive { id = trackId, unlocked = true, charts = new List<ChartArchive>() });
        }
        SaveLocalArchive(archive);  // 保存更新后的存档
    }

    public static void UnlockPack(string packId)
    {
        global::Archive archive = LoadLocalArchive();
        // 查找是否已解锁该pack
        PackArchive pack = archive.packs.Find(p => p.id == packId);
    
        if (pack != null)
        {
            if (pack.unlocked)
                Debug.Log("The pack is already unlocked.");
            else
                pack.unlocked = true;
        }
        else
        {
            // 如果没有找到pack，创建并解锁它
            archive.packs.Add(new PackArchive { id = packId, unlocked = true });
        }
        SaveLocalArchive(archive);  // 保存更新后的存档
    }

    public static void UnlockChart(string trackId, int ratingClass)
    {
        global::Archive archive = LoadLocalArchive();
        // 查找track
        TrackArchive track = archive.tracks.Find(t => t.id == trackId);
    
        if (track != null)
        {
            // 查找是否已解锁该ratingClass的chart
            ChartArchive chart = track.charts.Find(c => c.ratingClass == ratingClass);
        
            if (chart != null)
            {
                if (chart.unlocked)
                    Debug.Log("The chart is already unlocked.");
                else
                    chart.unlocked = true;
            }
            else
            {
                // 如果chart不存在，创建一个新的chart并解锁
                track.charts.Add(new ChartArchive { ratingClass = ratingClass, score = 0, optimism = -1, unlocked = true });
            }
        }
        else
        {
            // 如果track不存在，创建track和chart
            archive.tracks.Add(new TrackArchive 
            { 
                id = trackId, 
                unlocked = false, 
                charts = new List<ChartArchive> 
                { 
                    new ChartArchive { ratingClass = ratingClass, score = 0, optimism = -1, unlocked = true } 
                }
            });
        }
        SaveLocalArchive(archive);  // 保存更新后的存档
    }

    public static void UpdateScore(string trackId, int ratingClass, int score, int optimism)
    {
        global::Archive archive = LoadLocalArchive();
        // 查找track
        TrackArchive track = archive.tracks.Find(t => t.id == trackId);
    
        if (track != null)
        {
            // 查找chart
            ChartArchive chart = track.charts.Find(c => c.ratingClass == ratingClass);
        
            if (chart != null)
            {
                if (score > chart.score || chart.optimism > optimism || chart.optimism == -1)
                {
                    // 更新分数和乐观值
                    chart.score = score;
                    chart.optimism = optimism;
                }
            }
            else
            {
                // 如果chart不存在，创建并设置分数和乐观值
                track.charts.Add(new ChartArchive { ratingClass = ratingClass, score = score, optimism = optimism, unlocked = false });
            }
        }
        else
        {
            // 如果track不存在，创建track和chart
            archive.tracks.Add(new TrackArchive 
            { 
                id = trackId, 
                unlocked = false, 
                charts = new List<ChartArchive> 
                { 
                    new ChartArchive { ratingClass = ratingClass, score = score, optimism = optimism, unlocked = false } 
                }
            });
        }
        SaveLocalArchive(archive);  // 保存更新后的存档
        Debug.Log("Score updated");
    }
    
    

    // 将本地存档同步到 PlayFab
    public static void SyncArchiveToPlayFab()
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
    public static void LoadArchiveFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadSuccess, OnLoadFailure);
    }

    // PlayFab 存档同步成功回调
    private static void OnSyncSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Archive data synced to PlayFab successfully.");
    }

    // PlayFab 存档同步失败回调
    private static void OnSyncFailure(PlayFabError error)
    {
        Debug.LogError("Failed to sync archive data to PlayFab: " + error.GenerateErrorReport());
    }

    // PlayFab 读取存档成功回调
    private static void OnLoadSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("archiveData"))
        {
            string jsonData = result.Data["archiveData"].Value;
            archive = JsonUtility.FromJson<global::Archive>(jsonData);
            Debug.Log("Archive data loaded from PlayFab successfully.");
        }
        else
        {
            Debug.Log("No archive data found on PlayFab.");
        }
    }

    // PlayFab 读取存档失败回调
    private static void OnLoadFailure(PlayFabError error)
    {
        Debug.LogError("Failed to load archive data from PlayFab: " + error.GenerateErrorReport());
    }
    }

}
