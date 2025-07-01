using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameUtilities.Archive;

public class PlayingData : MonoBehaviour
{
    private GenerateLevel generateLevel;

    public int totalNotes, Combo, MaxCombo, Score, OptimalNum, EarlyPerfectNum, LatePerfectNum, EarlyGoodNum, LateGoodNum, MissNum, BadNum;
    public int JudgedNotes = 0;
    public TextMeshProUGUI ScoreDisplayer, ComboDisplayer, TitleDisplayer, RatingDisplayer, OptimismDisplayer;
    public TextMeshProUGUI FinalScore, FinalAcc, FinalLevel, Difference, Title, Artist, FinalRating, Perfect, Good, Bad, Miss, EarlyP, LateP, EarlyG, LateG, FinalCombo, Award;



    // Start is called before the first frame update
    void Start()
    {
        generateLevel = this.GetComponent<GenerateLevel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generateLevel.trackData.title != "")
        {
            TitleDisplayer.text = generateLevel.trackData.title;
            if (PlayerPrefs.GetInt("DifficultyIndex", -1) == 0)
                RatingDisplayer.text = "EZ Lv." + ((int)generateLevel.gameData.info.rating).ToString();
            if (PlayerPrefs.GetInt("DifficultyIndex", -1) == 1)
                RatingDisplayer.text = "HD Lv." + ((int)generateLevel.gameData.info.rating).ToString();
            if (PlayerPrefs.GetInt("DifficultyIndex", -1) == 2)
                RatingDisplayer.text = "IN Lv." + ((int)generateLevel.gameData.info.rating).ToString();
            if (PlayerPrefs.GetInt("DifficultyIndex", -1) == 3)
                RatingDisplayer.text = "AT Lv." + ((int)generateLevel.gameData.info.rating).ToString();
        }

        JudgedNotes = OptimalNum + EarlyPerfectNum + LatePerfectNum + EarlyGoodNum + LateGoodNum + MissNum + BadNum;
        if (Combo > MaxCombo)
            MaxCombo = Combo;
        if (generateLevel.NotesNum > 0)
        {
            double OptimalScore, PerfectScore, GoodScore, ComboScore;
            OptimalScore = (1000000 / (double)generateLevel.NotesNum + 0) * OptimalNum;
            PerfectScore = 1000000 / (double)generateLevel.NotesNum * (EarlyPerfectNum + LatePerfectNum);
            GoodScore = 1000000 / (double)generateLevel.NotesNum * (EarlyGoodNum + LateGoodNum) * 0.5;
            ComboScore = 50000 / (double)generateLevel.NotesNum * MaxCombo;
            Score = (int)(OptimalScore + PerfectScore + GoodScore);

        }

        if (ScoreDisplayer != null)
        {
            ScoreDisplayer.text = Score.ToString("D7");
            ComboDisplayer.text = Combo.ToString() + "\nCombo";
            if(Combo < 3)
            {
                ComboDisplayer.gameObject.SetActive(false);
            }
            else
            {
                ComboDisplayer.gameObject.SetActive(true);
            }
        }
    }

    string LevelJudge()
    {
        if (Score >= 1000000)
            return "P";
        else if (Score >= 990000)
            return "S+";
        else if (Score >= 980000)
            return "S";
        else if (Score >= 960000)
            return "A";
        else if (Score >= 900000)
            return "B";
        else if (Score >= 800000)
            return "C";
        else 
            return "F";
    }

    public void DisplayFinal()
    {
        FinalScore.text = Score.ToString("D7");
        FinalAcc.text = ((int)(100 / (float)generateLevel.NotesNum * (OptimalNum + EarlyPerfectNum + LatePerfectNum + (EarlyGoodNum + LateGoodNum) * 0.5))).ToString() + "%";
        FinalLevel.text = LevelJudge();
        int lastScore = 0;
        foreach (var track in LoadLocalArchive().tracks)
        {
            if (track.id == generateLevel.trackData.id)
            {
                foreach (var chart in track.charts)
                {
                    if (chart.ratingClass == PlayerPrefs.GetInt("DifficultyIndex", -1))
                    {
                        lastScore = chart.score;
                    }
                }
            }
        }
        if (Score - lastScore >= 0)
            Difference.text = "+" + (Score - lastScore);
        else
            Difference.text = (Score - lastScore).ToString();
        Title.text = TitleDisplayer.text;
        Artist.text = "Artist";
        FinalRating.text = RatingDisplayer.text;
        Perfect.text = (OptimalNum + EarlyPerfectNum + LatePerfectNum).ToString();
        Good.text = (EarlyGoodNum + LateGoodNum).ToString();
        Bad.text = BadNum.ToString();
        Miss.text = MissNum.ToString();
        EarlyP.text = EarlyPerfectNum.ToString();
        LateP.text = LatePerfectNum.ToString();
        EarlyG.text = EarlyGoodNum.ToString();
        LateG.text = LateGoodNum.ToString();
        FinalCombo.text = MaxCombo.ToString();
        Award.text = "+" + (Score / 10000) + " Echos";
        OptimismDisplayer.text = (generateLevel.NotesNum - OptimalNum).ToString();
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        UpdateScore(generateLevel.trackData.id, PlayerPrefs.GetInt("DifficultyIndex", -1), Score, (generateLevel.NotesNum - OptimalNum));
        AddEcho(Score / 10000);
    }
    public void BackToSelect()
    {
        SceneLoader.Load("LevelSelect");
    }

    public void Replay()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneLoader.Load(currentSceneName);
        //SceneManager.LoadScene(currentSceneName);
    }

}
