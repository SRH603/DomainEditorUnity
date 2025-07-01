using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static GameUtilities.Archive;
using static AutoUnlock;

public class SongSelect : MonoBehaviour
{
    /* ───────────────────────────── 数据容器 ───────────────────────────── */
    #region Data Containers
    //public BoolExpression rootExpression;
    [Header("数据容器")]
    public PacksContainer packsContainer;
    public UnlocksContainer unlocksContainer;
    public ArchivePacksContainer archivePackContainer = new ArchivePacksContainer();

    [Space]
    [HideInInspector] public List<ArchiveTrack> currentSonglist;
    #endregion


    /* ───────────────────────────── 排序设置 ───────────────────────────── */
    #region Sorting
    [Header("排序参数")]
    [HideInInspector] public int sortMethod;          // 0-4 不同排序方式
    [HideInInspector] public int sortOrder;           // 0 ASC | 1 DESC

    public GameObject[] sortMethodTypeface;           // 排序方式字体
    public GameObject[] sortOrderTypeface;            // 0 ASC | 1 DESC
    #endregion


    /* ───────────────────────────── 当前索引 ───────────────────────────── */
    #region Runtime Indices
    [HideInInspector] public int currentChapterIndex;
    [HideInInspector] public int currentTrackIndex;
    [HideInInspector] public int currentDifficultyIndex;
    #endregion


    /* ───────────────────────────── 章节 / 关卡 UI ───────────────────────────── */
    #region Chapter & Level UI
    [Header("章节 / 关卡 UI")]
    public TextMeshProUGUI chapterText;

    public GameObject  levelUIPrefab;
    public GameObject  content;
    [HideInInspector] public List<GameObject> levelContainer;
    [HideInInspector] public GameObject selectedLevel;

    public GameObject[] difficultyButtons;

    [HideInInspector] public bool[] difficultyExist    = new bool[4];
    [HideInInspector] public bool[] difficultyUnlocked = new bool[4];

    [HideInInspector] public int CurrentLevelLocation, CurrentContentLocation;
    #endregion


    /* ───────────────────────────── 歌曲信息 UI ───────────────────────────── */
    #region Song Info UI
    [Header("歌曲信息 UI")]
    public Image Illustration;
    public Image IllustrationLock;

    public TextMeshProUGUI SongTitle;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Optimism;
    public TextMeshProUGUI Artist;
    public TextMeshProUGUI BPM;
    public TextMeshProUGUI Duration;
    public TextMeshProUGUI Level;
    public TextMeshProUGUI Info;

    public Button StartButton;

    public TextMeshProUGUI[] Ratings = new TextMeshProUGUI[4];
    #endregion


    /* ───────────────────────────── 歌曲元数据缓存 ───────────────────────────── */
    #region Song Meta Cache
    [HideInInspector] public string songTitle, artist;
    [HideInInspector] public string bpm;
    [HideInInspector] public string level, info, duration;

    [HideInInspector] public int   score;
    [HideInInspector] public int[] ratings   = new int[4];
    [HideInInspector] public int   optimism;
    [HideInInspector] public bool  unlock;
    #endregion


    /* ───────────────────────────── 统计 / 评分 ───────────────────────────── */
    #region Stats
    [HideInInspector] public Archive archive;
    [HideInInspector] public int     echo;
    [HideInInspector] public float   rating;

    public TMP_Text echoDisplay;
    public TMP_Text ratingDisplay;
    #endregion


    /* ───────────────────────────── 试听 & 资源 ───────────────────────────── */
    #region Audio Preview
    [SerializeField] private PreviewAudioPlayer previewPlayer;

    // 关卡 ID → AudioClip 对照表
    private readonly Dictionary<string, AudioClip> clipDict = new();

    private string currentPlayingId = null;
    #endregion


    /* ───────────────────────────── 其他 ───────────────────────────── */
    #region Misc
    public Color[] levelBarColor;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    private void Init()
    {
        UnlockAll(unlocksContainer);
        currentChapterIndex = PlayerPrefs.GetInt("ChapterIndex", -1);
        currentTrackIndex = PlayerPrefs.GetInt("SongIndex", 0);
        currentDifficultyIndex = PlayerPrefs.GetInt("DifficultyIndex", 0);
        sortMethod = PlayerPrefs.GetInt("Sortord", 0);
        sortOrder = PlayerPrefs.GetInt("SortASC", 0);
        CurrentLevelLocation = PlayerPrefs.GetInt("LevelLocation", 0);
        CurrentContentLocation = PlayerPrefs.GetInt("ContentLocation", 0);
        //archivedContainer = packContainers;
        /*
        if (currentDifficultyIndex == 3)
        {
            currentDifficultyIndex = 2;
        }
        if (currentDifficultyIndex != 0 && currentDifficultyIndex != 1 && currentDifficultyIndex != 2)
        {
            currentDifficultyIndex = 0;
        }
        */
        
        //GetComponent<LoadArchive>().Init();
        //GetComponent<LoadArchive>().SaveLocalArchive();
        //archive = GetComponent<LoadArchive>().archive;
        InitArchivePack();
        archive = LoadLocalArchive();
        LoadEcho();
        
        //LoadPacks();
        LoadConditions();
        LoadArchive();
        LoadRating();
        UpdateChapterUI();
        UpdateButton(sortMethod);
        UpdateSortOrder();
        UpdateDifficultyButton(currentDifficultyIndex);
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        LevelPositionSynchronization();
        
        foreach (var level in levelContainer)
        {
            level.GetComponent<LevelBar>().RefreshRatingClass(currentDifficultyIndex);
        }
    }

    private void InitArchivePack()
    {
        ArchivePacksContainer archivePacksContainer = new ArchivePacksContainer();
        archivePacksContainer.packs = new List<ArchivePack>();
        int packidx = 0;
        int trackidx = 0;
        foreach (var pack in packsContainer.packs)
        {
            ArchivePack archivePack = new ArchivePack(packidx, pack.id, pack.section, pack.character, pack.name, pack.description);
            archivePack.tracks = new List<ArchiveTrack>();
            foreach (var track in pack.tracks)
            {
                ArchiveTrack archiveTrack = new ArchiveTrack(trackidx, track.id, track.title, track.artist, ConvertTextureToSprite(track.illustration), ConvertTextureToSprite(track.previewIllustration), track.illustrator, track.bpm, track.background, track.songInfo, track.audioPreviewStart, track.audioPreviewEnd, track.version);
                archiveTrack.charts = new List<ArchiveChart>();
                int i = 0;
                foreach (var chart in track.charts)
                {
                    if (chart != null)
                    {
                        ArchiveChart archiveChart = new ArchiveChart(i, chart.info.rating, chart.info.designer);
                        archiveTrack.charts.Add(archiveChart);
                        //Debug.Log(JsonUtility.ToJson(archiveChart));
                    }
                    else
                    {
                        ArchiveChart archiveChart = new ArchiveChart(i, 0, "");
                        archiveTrack.charts.Add(archiveChart);
                    }
                    ++i;
                }

                archiveTrack.packId = pack.id;
                archivePack.tracks.Add(archiveTrack);
                ++trackidx;
                //Debug.Log(JsonUtility.ToJson(archiveTrack));
                clipDict[track.id] = track.track;        // ★ 缓存
            }
            //Debug.Log(archivePacksContainer.packs == null ? "packs is null" : "packs is not null");
            //Debug.Log(archivePack == null ? "archivePack is null" : "archivePack is not null");
            archivePacksContainer.packs.Add(archivePack);
            ++packidx;
            //Debug.Log(JsonUtility.ToJson(archivePack));
            //Debug.Log(JsonUtility.ToJson(archivePacksContainer.packs));
        }
        //Debug.Log(JsonUtility.ToJson(archivePacksContainer));
        archivePackContainer = archivePacksContainer;
    }

    private Sprite ConvertTextureToSprite(Texture texture)
    {
        // 检查 Texture 是否可以转换为 Texture2D
        if (texture is Texture2D texture2D)
        {
            return Sprite.Create(
                texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f) // 锚点为中心
            );
        }
        else
        {
            Debug.LogError("The provided Texture is not a Texture2D. Conversion failed.");
            return null;
        }
    }

    public void BuyTrack()
    {
        if (selectedLevel.GetComponent<LevelBar>().packCondition.type == ConditionType.Currency)
        {
            if (GetEcho() >= selectedLevel.GetComponent<LevelBar>().packCondition.amount)
            {
                AddEcho(-selectedLevel.GetComponent<LevelBar>().packCondition.amount);
                UnlockPack(selectedLevel.GetComponent<LevelBar>().packId);
            }
            Init();
        }
        else if (selectedLevel.GetComponent<LevelBar>().trackCondition.type == ConditionType.Currency)
        {
            if (GetEcho() >= selectedLevel.GetComponent<LevelBar>().trackCondition.amount)
            {
                AddEcho(-selectedLevel.GetComponent<LevelBar>().trackCondition.amount);
                UnlockTrack(selectedLevel.GetComponent<LevelBar>().id);
            }
            Init();
        }
        else if (selectedLevel.GetComponent<LevelBar>().chartCondition.type == ConditionType.Currency)
        {
            if (GetEcho() >= selectedLevel.GetComponent<LevelBar>().chartCondition.amount)
            {
                AddEcho(-selectedLevel.GetComponent<LevelBar>().chartCondition.amount);
                UnlockChart(selectedLevel.GetComponent<LevelBar>().id, currentDifficultyIndex);
            }
            Init();
        }
    }

    private void LoadRating()
    {
        List<float> ratingList = new List<float>();
        foreach (var pack in archivePackContainer.packs)
        {
            foreach (var track in pack.tracks)
            {
                foreach (var chart in track.charts)
                {
                    ratingList.Add((float)Math.Round(((double)chart.GetScore() / 1000000 + Mathf.Max(0, (100 - chart.GetOptimism()) / 1000)) * chart.rating, 4));
                    //Debug.Log((float)Math.Round(((double)chart.GetScore() / 1000000 + Mathf.Max(0, (100 - chart.GetOptimism()) / 1000)) * chart.rating, 4));
                }
            }
        }
        ratingList.Sort((a, b) => b.CompareTo(a));  // 逆序比较，b在前表示从大到小排序
        float rating = 0;
        int i = 0;
        foreach (var rate in ratingList)
        {
            if (i < 20)
            {
                rating += rate;
            }

            ++i;
        }
        this.rating = rating / (i + 1);
        ratingDisplay.text = this.rating.ToString("F2");
        if (rating >= 11) UnlockRatingClass2();
    }
    
    private void LoadEcho()
    {
        echo = archive.echo;
        echoDisplay.text = echo.ToString();
    }

    private void LoadConditions()
    {
        foreach (var trackUnlocks in unlocksContainer.tracks)
        {
            foreach (var pack in archivePackContainer.packs)
            {
                bool finished = false;
                foreach (var track in pack.tracks)
                {
                    if (trackUnlocks.trackId == track.id)
                    {
                        track.condition = trackUnlocks.condition;
                        finished = true;
                        break;
                    }
                }
                if (finished) break;
            }
        }
        foreach (var chartUnlocks in unlocksContainer.charts)
        {
            foreach (var pack in archivePackContainer.packs)
            {
                bool finished = false;
                foreach (var track in pack.tracks)
                {
                    if (chartUnlocks.trackId == track.id)
                    {
                        foreach (var chart in track.charts)
                        {
                            if (chart.ratingClass == chartUnlocks.ratingClass)
                            {
                                chart.condition = chartUnlocks.condition;
                                finished = true;
                                break;
                            }
                        }
                    }
                    if (finished) break;
                }
                if (finished) break;
            }
        }
        foreach (var packUnlocks in unlocksContainer.packs)
        {
            foreach (var pack in archivePackContainer.packs)
            {
                if (packUnlocks.packId == pack.id)
                {
                    pack.condition = packUnlocks.condition;
                    break;
                }
            }
        }
    }

    public void DownLoadArchive()
    {
        archive = GetComponent<PlayFabManager>().LoadPlayerArchive();
    }
    
    private void LoadArchive()
    {
        foreach (var trackArchive in archive.tracks)
        {
            foreach (var pack in archivePackContainer.packs)
            {
                foreach (var track in pack.tracks)
                {
                    if (trackArchive.id == track.id)
                    {
                        track.SetUnlocked(trackArchive.unlocked);
                        //Debug.Log(track.title);
                        foreach (var chartArchive in trackArchive.charts)
                        {
                            track.charts[chartArchive.ratingClass].SetScore(chartArchive.score);
                            track.charts[chartArchive.ratingClass].SetOptimism(chartArchive.optimism);
                            track.charts[chartArchive.ratingClass].SetUnlocked(chartArchive.unlocked);
                        }
                    }
                }
            }
            
        }
        
        foreach (var packArchive in archive.packs)
        {
            foreach (var pack in archivePackContainer.packs.Where(pack => pack.id == packArchive.id))
            {
                pack.SetUnlocked(packArchive.unlocked);
                break;
            }
        }
    }

    private void UpdateButton(int buttonIndex)
    {
        for (var i = 0; i <= sortMethodTypeface.Length - 1; ++i)
        {
            sortMethodTypeface[i].gameObject.SetActive(i == buttonIndex);
        }
    }

    public void SortMethodClick()
    {
        if (sortMethod == sortMethodTypeface.Length - 1)
        {
            sortMethod = 0;
        }
        else
        {
            ++sortMethod;
        }

        UpdateButton(sortMethod);
        LevelSynchronization();
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        PlayerPrefs.SetInt("Sortord", sortMethod);
        LevelPositionSynchronization();
    }

    private void UpdateDifficultyButton(int buttonIndex)
    {
        for (var i = 0; i <= difficultyButtons.Length - 1; ++i)
        {
            difficultyButtons[i].gameObject.GetComponent<DifficultyButton>().Select.SetActive(i == buttonIndex);
        }
        PlayerPrefs.SetInt("DifficultyIndex", currentDifficultyIndex);

        foreach (var level in levelContainer)
        {
            level.GetComponent<LevelBar>().RefreshRatingClass(currentDifficultyIndex);
        }
    }

    public void DifficultyClick(int index)
    {
        currentDifficultyIndex = index;
        LevelSynchronization();
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        UpdateDifficultyButton(currentDifficultyIndex);
        LevelPositionSynchronization();
    }

    public void SortOrderClick()
    {
        sortOrder = sortOrder == 0 ? 1 : 0;

        UpdateSortOrder();
        LevelSynchronization();
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        PlayerPrefs.SetInt("SortASC", sortOrder);
        LevelPositionSynchronization();
    }

    private void UpdateSortOrder()
    {
        if (sortOrder == 0)
        {
            sortOrderTypeface[0].SetActive(true);
            sortOrderTypeface[1].SetActive(false);
        }
        else
        {
            sortOrderTypeface[1].SetActive(true);
            sortOrderTypeface[0].SetActive(false);
        }
    }    

    private void RefreshSongList(int chapterID, int sortMet, int ratingClass, int sortOrd)
    {
        if (chapterID == -1)
        {
            currentSonglist = new List<ArchiveTrack>();
            foreach (var pack in archivePackContainer.packs)
            {
                foreach (var song in pack.tracks)
                {
                    //song.packId = pack.id;
                    currentSonglist.Add(song);
                }
            }
        }
        else
        {
            currentSonglist = archivePackContainer.packs[chapterID].tracks;
        }
        currentSonglist = SortSonglist(currentSonglist, sortMet, ratingClass, sortOrd);
        
        if (currentSonglist.Count == 0 && currentDifficultyIndex == 3)
        {
            --currentDifficultyIndex;

            UpdateDifficultyButton(currentDifficultyIndex);
            LevelSynchronization();
            RefreshSongListWithLevel(currentChapterIndex, this.sortMethod, currentDifficultyIndex, sortOrder);
            LevelPositionSynchronization();

            return;
        }
        GenerateSonglistUI();
        RefreshSongSelected();
    }

    private void RefreshSongListWithLevel(int chapterID, int sortMet, int ratingClass, int sortOrd)
    {
        if (chapterID == -1)
        {
            currentSonglist = new List<ArchiveTrack>();
            foreach (var pack in archivePackContainer.packs)
                foreach (var song in pack.tracks)
                    currentSonglist.Add(song);
        }
        else
        {
            currentSonglist = archivePackContainer.packs[chapterID].tracks;
        }
        currentSonglist = SortSonglist(currentSonglist, sortMet, ratingClass, sortOrd);
        
        GenerateSonglistUI();
        RefreshSongSelected();
    }

    void RefreshDifficultyExist()
    {
        int index = 0;
        difficultyExist = selectedLevel.GetComponent<LevelBar>().DifficultyExist;
        foreach (var exist in difficultyExist)
        {
            difficultyButtons[index].SetActive(exist);
            ++index;
        }

        index = 0;
        difficultyUnlocked = selectedLevel.GetComponent<LevelBar>().DifficultyUnlocked;
        foreach (var exist in difficultyUnlocked)
        {
            difficultyButtons[index].GetComponent<DifficultyButton>().Lock.SetActive(!exist);
            ++index;
        }
    }

    private List<ArchiveTrack> SortSonglist(List<ArchiveTrack> songs, int sortMet, int ratingClass, int asc)
    {
        List<ArchiveTrack> sortedList = new List<ArchiveTrack>();
        // ??????????ratingClass????RatingClass??Difficulty??????Song
        List<ArchiveTrack> filteredSongs = songs.Where(song => song.charts.Any(d => d.ratingClass == ratingClass)).ToList();

        if (sortMet == 0)
        {
            // ??idx????
            sortedList = new List<ArchiveTrack>(filteredSongs);
            if (asc == 0)
                sortedList.Sort((a, b) => a.idx.CompareTo(b.idx));
            else
                sortedList.Sort((a, b) => b.idx.CompareTo(a.idx));
        }
        if (sortMet == 1)
        {
            // ??id????
            sortedList = new List<ArchiveTrack>(filteredSongs);
            if (asc == 0)
                sortedList.Sort((a, b) => string.Compare(a.id, b.id, StringComparison.Ordinal));
            else
                sortedList.Sort((a, b) => string.Compare(b.id, a.id, StringComparison.Ordinal));

        }
        if (sortMet == 2)
        {
            // ??????
            if (asc == 0)
            {
                // ????????
                sortedList = filteredSongs
                    .OrderBy(song =>
                    {
                        var difficulty = song.charts.FirstOrDefault(d => d.ratingClass == ratingClass);
                        return difficulty?.rating ?? int.MaxValue;
                    })
                    .ThenBy(song => song.id) // ??????????
                    .ToList();
            }
            else
            {
                // ????????
                sortedList = filteredSongs
                    .OrderByDescending(song =>
                    {
                        var difficulty = song.charts.FirstOrDefault(d => d.ratingClass == ratingClass);
                        return difficulty?.rating ?? int.MinValue;
                    })
                    .ThenByDescending(song => song.id) // ??????????
                    .ToList();
            }

        }
        return sortedList;
    }

    private void GenerateSonglistUI()
    {
        //CurrentLevelLocation = (int)(SelectedLevel.GetComponent<RectTransform>().rect.height);
        int location = 0;
        if (levelContainer != null)
        {
            foreach (var song in levelContainer) // ??????
            {
                Destroy(song);
            }

        }
        levelContainer = new List<GameObject>();
        foreach (var song in currentSonglist)
        {
            GameObject clonedObject = Instantiate(levelUIPrefab, content.transform, true);
            levelContainer.Add(clonedObject);
            clonedObject.SetActive(true);

            // ??????????

            // ??????????????RectTransform
            RectTransform rectTransform = clonedObject.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                // ????RectTransform??????
                rectTransform.anchoredPosition = new Vector2(0, -90 * location); // ????
                rectTransform.sizeDelta = new Vector2(300, 90); // ????
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one; // ????
            }

            clonedObject.GetComponent<LevelBar>().idx = song.idx;
            clonedObject.GetComponent<LevelBar>().difficulty = currentDifficultyIndex;
            clonedObject.GetComponent<LevelBar>().LevelName = song.title;
            clonedObject.GetComponent<LevelBar>().score = song.charts[currentDifficultyIndex].GetScore();
            clonedObject.GetComponent<LevelBar>().packUnlocked = GetPackUnlocked(song.packId);
            clonedObject.GetComponent<LevelBar>().trackUnlocked = song.GetUnlocked();
            clonedObject.GetComponent<LevelBar>().chartUnlocked = song.charts[currentDifficultyIndex].GetUnlocked();

            if (GetPackUnlocked(song.packId) && song.GetUnlocked() && song.charts[currentDifficultyIndex].GetUnlocked() == false)
            {
                clonedObject.GetComponent<LevelBar>().unlockCondition = TruncateText(GenerateConditionString(song.charts[currentDifficultyIndex].condition), 80);
                //Debug.Log("wgvewvwe"+song.title);
            }
            else if (GetPackUnlocked(song.packId) && song.GetUnlocked() == false)
            {
                clonedObject.GetComponent<LevelBar>().unlockCondition = TruncateText(GenerateConditionString(song.condition), 120);
                //Debug.Log("sb"+song.title);
            }
            else if (!GetPackUnlocked(song.packId))
            {
                clonedObject.GetComponent<LevelBar>().unlockCondition = TruncateText(GenerateConditionString(GetPack(song.packId).condition), 120);
            }
                

            if (song.condition != null)
            {
                clonedObject.GetComponent<LevelBar>().trackCondition = song.condition;
            }
            if (song.charts[currentDifficultyIndex].condition != null)
            {
                clonedObject.GetComponent<LevelBar>().chartCondition = song.charts[currentDifficultyIndex].condition;
            }
            if (GetPack(song.packId).condition != null)
            {
                clonedObject.GetComponent<LevelBar>().packCondition = GetPack(song.packId).condition;
            }
            
            
            clonedObject.GetComponent<LevelBar>().artist = song.artist;
            clonedObject.GetComponent<LevelBar>().bPM = song.bpm;
            clonedObject.GetComponent<LevelBar>().info = song.songInfo;
            clonedObject.GetComponent<LevelBar>().duration = song.duration;
            clonedObject.GetComponent<LevelBar>().optimism = song.charts[currentDifficultyIndex].GetOptimism();
            clonedObject.GetComponent<LevelBar>().id = song.id;
            clonedObject.GetComponent<LevelBar>().packId = song.packId;
            clonedObject.GetComponent<LevelBar>().illustrator = song.illustrator;
            clonedObject.GetComponent<LevelBar>().charter = song.charts[currentDifficultyIndex].designer;
            
            clonedObject.GetComponent<LevelBar>().coverImage.sprite = song.previewIllustration;
            //太卡了，还是自己裁剪
            //ApplyCenterCrop(clonedObject.GetComponent<LevelBar>().coverImage, song.previewIllustration);
            clonedObject.GetComponent<LevelBar>().coverImage.GetComponent<AlphaMaskOverlay>().Apply();
            
            clonedObject.GetComponent<LevelBar>().illustration = song.illustration;

            for (int i = 0;i <= song.charts.Count - 1;i++)
                clonedObject.GetComponent<LevelBar>().Ratings[i] = (int)song.charts[i].rating;
            foreach (var difficulty in song.charts)
            {
                if (difficulty.ratingClass == currentDifficultyIndex)
                    clonedObject.GetComponent<LevelBar>().LevelRating = ((int)difficulty.rating).ToString();
            }
            foreach (var ratingClass in song.charts)
            {
                if (ratingClass.ratingClass == 0)
                {
                    clonedObject.GetComponent<LevelBar>().DifficultyExist[0] = true;
                    if (ratingClass.GetUnlocked() && song.GetUnlocked())
                        clonedObject.GetComponent<LevelBar>().DifficultyUnlocked[0] = true;
                }
                if (ratingClass.ratingClass == 1)
                {
                    clonedObject.GetComponent<LevelBar>().DifficultyExist[1] = true;
                    if (ratingClass.GetUnlocked() && song.GetUnlocked())
                        clonedObject.GetComponent<LevelBar>().DifficultyUnlocked[1] = true;

                }
                if (ratingClass.ratingClass == 2)
                {
                    clonedObject.GetComponent<LevelBar>().DifficultyExist[2] = true;
                    if (ratingClass.GetUnlocked() && song.GetUnlocked())
                        clonedObject.GetComponent<LevelBar>().DifficultyUnlocked[2] = true;

                }
                if (ratingClass.ratingClass == 3)
                {
                    clonedObject.GetComponent<LevelBar>().DifficultyExist[3] = true;
                    if (ratingClass.GetUnlocked() && song.GetUnlocked())
                        clonedObject.GetComponent<LevelBar>().DifficultyUnlocked[3] = true;

                }

            }
            clonedObject.GetComponent<LevelBar>().UpdateInfo();
            clonedObject.GetComponent<LevelBar>().mainBar.color = levelBarColor[currentDifficultyIndex];

            location++;
        }
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 90 * levelContainer.Count);
        
    }

    private bool GetPackUnlocked(string packId)
    {
        foreach (var pack in archivePackContainer.packs)
        {
            if (packId == pack.id)
            {
                //Debug.Log(pack.GetUnlocked());
                return pack.GetUnlocked();
            }
        }
        return false;
    }

    private ArchivePack GetPack(string packId)
    {
        foreach (var pack in archivePackContainer.packs)
        {
            if (packId == pack.id)
            {
                return pack;
            }
        }
        return null;
    }
    
    public static void ApplyCenterCrop(Image img, Sprite originalSprite)
    {
        if (img == null || originalSprite == null) return;

        Texture2D sourceTex = originalSprite.texture;
        Rect texRect = originalSprite.textureRect;

        // 创建 RenderTexture，读取 Sprite.rect 区域
        int texW = Mathf.RoundToInt(texRect.width);
        int texH = Mathf.RoundToInt(texRect.height);

        RenderTexture rt = RenderTexture.GetTemporary(sourceTex.width, sourceTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sourceTex, rt);

        RenderTexture.active = rt;

        Texture2D readableTex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(texRect.x, texRect.y, texW, texH), 0, 0);
        readableTex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        // 从 readableTex 中进行居中裁切
        float viewW = img.rectTransform.rect.width;
        float viewH = img.rectTransform.rect.height;

        float texRatio = (float)texW / texH;
        float viewRatio = viewW / viewH;

        int cropW, cropH;

        if (texRatio > viewRatio)
        {
            cropH = texH;
            cropW = Mathf.RoundToInt(cropH * viewRatio);
        }
        else
        {
            cropW = texW;
            cropH = Mathf.RoundToInt(cropW / viewRatio);
        }

        int cropX = (texW - cropW) / 2;
        int cropY = (texH - cropH) / 2;

        Color[] pixels = readableTex.GetPixels(cropX, cropY, cropW, cropH);
        Texture2D finalTex = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
        finalTex.SetPixels(pixels);
        finalTex.Apply();

        Sprite newSprite = Sprite.Create(finalTex, new Rect(0, 0, cropW, cropH), new Vector2(0.5f, 0.5f), originalSprite.pixelsPerUnit);
        img.sprite = newSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
    }



    
    string TruncateText(string text, int maxLength)
    {
        if (text.Length > maxLength)
        {
            // 如果字符串长度超过最大长度，则截断并添加 "..."
            return text.Substring(0, maxLength) + "...";
        }
        else
        {
            // 如果字符串长度没有超过最大长度，直接返回原文本
            return text;
        }
    }

    private string GenerateConditionString(Condition condition)
    {
        StringBuilder result = new StringBuilder();

        if (condition == null)
            return "";

        string trackName = "";
        foreach (var pack in packsContainer.packs)
        {
            foreach (var track in pack.tracks)
            {
                if (condition.otherTrackId == track.id)
                {
                    trackName = track.title;
                }
            }
        }

        string ratingClassName = "";
        if (condition.ratingClass == 0)
        {
            ratingClassName = "EZ";
        }
        if (condition.ratingClass == 1)
        {
            ratingClassName = "HD";
        }
        if (condition.ratingClass == 2)
        {
            ratingClassName = "MS";
        }
        if (condition.ratingClass == 3)
        {
            ratingClassName = "IN";
        }
        switch (condition.type)
        {
            case ConditionType.Null:
                result.AppendLine("");
                break;
            
            case ConditionType.Currency:
                result.AppendLine($"Requires [{condition.amount}] Echoes.");
                break;

            case ConditionType.OtherTrack:
                    
                result.AppendLine($"Unlock by getting [{condition.targetScore}] or higher in any Difficulty of '{trackName}'.");
                break;
                
            case ConditionType.OtherChart:
                    
                result.AppendLine($"Unlock by getting [{condition.targetScore}] or higher in '{ratingClassName}' of '{trackName}'.");
                break;

            case ConditionType.Pack:
                result.AppendLine($"Requires {condition.packId} to unlock.");
                break;
            
            case ConditionType.GeneralRatingClass2:
                result.AppendLine($"Get 800000+ in Hard of {trackName}");
                break;

            default:
                result.AppendLine("Unknown condition type.");
                break;
        }

        return result.ToString();
    }

    public void ChapterSwitch(bool up)
    {
        if(up)
        {
            if (currentChapterIndex == archivePackContainer.packs.Count - 1)
            {
                currentChapterIndex = -1;
            }
            else
            {
                ++currentChapterIndex;
            }
        }
        else
        {
            if (currentChapterIndex == -1)
            {
                currentChapterIndex = archivePackContainer.packs.Count - 1;
            }
            else
            {
                --currentChapterIndex;
            }
        }
        UpdateChapterUI();
        LevelSynchronization();
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        PlayerPrefs.SetInt("ChapterIndex", currentChapterIndex);
        LevelPositionSynchronization();
    }
    
    public void ChapterSelect(int index)
    {
        currentChapterIndex = index;
        UpdateChapterUI();
        LevelSynchronization();
        RefreshSongList(currentChapterIndex, sortMethod, currentDifficultyIndex, sortOrder);
        PlayerPrefs.SetInt("ChapterIndex", currentChapterIndex);
        LevelPositionSynchronization();
    }

    void UpdateChapterUI()
    {
        chapterText.text = ConvertToVerticalText(currentChapterIndex == -1 ? "All" : archivePackContainer.packs[currentChapterIndex].name.en);
    }

    private string ConvertToVerticalText(string input)
    {
        // ???? StringBuilder ????????????????????
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var c in input)
        {
            sb.Append(c);
            sb.Append('\n'); // ??????????
        }

        return sb.ToString();
    }

    public void SelectSong(int index)
    {
        currentTrackIndex = index;
        PlayerPrefs.SetInt("SongIndex", currentTrackIndex);
        RefreshSongSelected();
    }

    private void RefreshSongSelected()
    {
        if (levelContainer == null || levelContainer.Count == 0)
        {
            Debug.LogWarning("LevelContainer is null or empty.");
            return;
        }
        foreach (var song in levelContainer)
        {
            if (song != null)
                if (song.GetComponent<LevelBar>().idx == currentTrackIndex)
                {
                    SelectSong(song);
                    RefreshDifficultyExist();
                    RefreshSongUI();
                    PlayPreview();
                    return;
                }
        }
        if (levelContainer[0] != null)
            currentTrackIndex = levelContainer[0].GetComponent<LevelBar>().idx;
        // ??????????????????????????????????????
        foreach (var song in levelContainer)
        {
            if (song != null)
                if (song.GetComponent<LevelBar>().idx == currentTrackIndex)
                {
                    SelectSong(song);
                    RefreshDifficultyExist();
                    RefreshSongUI();
                    PlayPreview();
                    return;
                }
        }
        
    }

    void PlayPreview()
    {
        string id = selectedLevel.GetComponent<LevelBar>().id;

        if (clipDict.TryGetValue(id, out var clip))
        {
            // === 新增判断：同曲且正在播放 → 不重播 ===
            bool needReplay = previewPlayer.CurrentClip != clip || !previewPlayer.IsPlaying;

            if (needReplay)
            {
                var td = packsContainer.packs
                    .SelectMany(p => p.tracks)
                    .First(t => t.id == id);

                previewPlayer.PlayPreview(
                    clip,
                    td.audioPreviewStart,
                    td.audioPreviewEnd);

                currentPlayingId = id;   // 记录当前曲目
            }
        }
        else
        {
            previewPlayer.StopPreview();
            currentPlayingId = null;
            Debug.LogWarning($"找不到 {id} 的 AudioClip");
        }

    }

    void RefreshSongUI()
    {
        LevelBar levelBar = selectedLevel.GetComponent<LevelBar>();
        ratings = levelBar.Ratings;
        for (int i = 0; i <= Ratings.Count() - 1; ++i)
        {
            if (/*levelBar.DifficultyUnlocked[i] && */levelBar.DifficultyExist[i])
            {
                Ratings[i].text = ratings[i].ToString();

            }
        }
        songTitle = levelBar.LevelName;
        score = levelBar.score;
        unlock = (levelBar.trackUnlocked && levelBar.chartUnlocked && levelBar.packUnlocked);
        artist = levelBar.artist;
        bpm = levelBar.bPM;
        info = levelBar.info;
        duration = levelBar.duration;
        optimism = levelBar.optimism;

        if (unlock)
        {
            StartButton.interactable = true;
        }
        else
        {
            StartButton.interactable = false;
        }

        SongTitle.text = songTitle.ToString();
        Score.text = score.ToString("D7");
        if (score >= 1000000)
        {
            Level.text = "♾";
        }
        else if (score >= 990000)
        {
            Level.text = "IIS";
        }
        else if (score >= 980000)
        {
            Level.text = "IS";
        }
        else if (score >= 970000)
        {
            Level.text = "S";
        }
        else if (score >= 950000)
        {
            Level.text = "A";
        }
        else if (score >= 920000)
        {
            Level.text = "B";
        }
        else if (score >= 880000)
        {
            Level.text = "C";
        }
        else if (score >= 800000)
        {
            Level.text = "F";
        }
        else
        {
            Level.text = "-";
        }
        if (optimism <= 100)
        {
            Level.color = Color.white;
        }
        else if (optimism <= 20)
        {
            Level.color = Color.green;
        }
        else if (optimism <= 0)
        {
            Level.color = Color.cyan;
        }
        IllustrationLock.gameObject.SetActive(!unlock);
        Artist.text = artist;
        BPM.text = bpm.ToString();
        Info.text = info;
        Duration.text = duration;
        Optimism.text = optimism.ToString();

        // ????????
        //Texture2D texture = Resources.Load<Texture2D>("level/" + levelBar.id + "/illustration");
        Illustration.sprite = levelBar.illustration;

        /*
        if (texture != null)
        {
            // ????????Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // ??Sprite??????????Image
            Illustration.sprite = sprite;
        }
        else
        {
            Illustration.sprite = null;
            //Debug.LogError("Failed to load texture from Resources.");
        }
        */
    }

    void SelectSong(GameObject song)
    {
        if (selectedLevel != null)
        {
            selectedLevel.GetComponent<LevelBar>().Pointer.SetActive(false);
            selectedLevel.GetComponent<LevelBar>().Unselect();
        }
        selectedLevel = song;
        selectedLevel.GetComponent<LevelBar>().Pointer.SetActive(true);
        selectedLevel.GetComponent<LevelBar>().Select();
    }

    public void LevelSynchronization()
    {
        CurrentContentLocation = (int)(content.GetComponent<RectTransform>().localPosition.y);
        //if (selectedLevel != null)
            CurrentLevelLocation = (int)(selectedLevel.GetComponent<RectTransform>().localPosition.y);
        PlayerPrefs.SetInt("LevelLocation", CurrentLevelLocation);
        PlayerPrefs.SetInt("ContentLocation", CurrentContentLocation);
    }

    private void LevelPositionSynchronization()
    {
        // Debug.Log("Original" + CurrentContentLocation);
        // Debug.Log("Original" + CurrentLevelLocation);
        // Debug.Log("Original" + (int)SelectedLevel.GetComponent<RectTransform>().localPosition.y);
        if (selectedLevel != null)
            CurrentContentLocation = CurrentContentLocation + CurrentLevelLocation - (int)(selectedLevel.GetComponent<RectTransform>().localPosition.y);
        Vector2 newContentLocation = new Vector2(content.GetComponent<RectTransform>().localPosition.x, CurrentContentLocation);
        // Debug.Log(NewContentLocation);
        // Debug.Log("New" + CurrentContentLocation);
        // Debug.Log("New" + CurrentLevelLocation);

        content.GetComponent<RectTransform>().localPosition = newContentLocation;

        PlayerPrefs.SetInt("LevelLocation", CurrentLevelLocation);
        PlayerPrefs.SetInt("ContentLocation", CurrentContentLocation);

    }

    public void StartLevel()
    {
        previewPlayer.StopPreview();
        LevelBar lb   = selectedLevel.GetComponent<LevelBar>();
        string   t    = lb.LevelName;              // 标题
        string   a    = lb.artist;                 // 艺术家
        string   ch   = lb.charter;            // EZ / HD / …（你已有这个字段）
        string   illu = lb.illustrator;   // 插画者名
        Sprite   pic  = lb.illustration;           // 封面

        // ② 注入 Overlay
        var ov = PlayingSceneLoadingOverlay.Instance;
        if (ov != null)
            ov.AssignInfo(t, a, ch, illu, pic);
        
        PlayerPrefs.SetString("PlayingID", lb.id);
        PlayingSceneLoader.Load("ChartPlaying");
        //SceneManager.LoadScene("ChartPlaying");
    }
    
    private void OnDisable()
    {
        // 防止场景卸载时仍在播放
        if (previewPlayer != null)
            previewPlayer.StopPreview(0f);   // 传 0 也行，关键是触发立即停止逻辑
    }

}
