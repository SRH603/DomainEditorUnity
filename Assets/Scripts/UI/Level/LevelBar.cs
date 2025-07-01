using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class LevelBar : MonoBehaviour
{
    [FormerlySerializedAs("songselect")] public SongSelect songSelect;

    public bool selected;
    public int idx = -1;
    public string id, packId;
    public int difficulty = -1;
    public string LevelName, LevelRating;
    public TextMeshProUGUI Score; // ���еȼ�
    public int score = 0;
    public bool trackUnlocked = false, chartUnlocked = false, packUnlocked = false;
    public GameObject lockcover;

    public Image D0, D1, D2, D3;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Rating;
    public TMP_Text unlockConditionText;
    public GameObject unlockConditionTextHolder;
    public GameObject Pointer;
    public bool[] DifficultyExist = new bool[4];
    public bool[] DifficultyUnlocked = new bool[4];
    public int[] Ratings = new int[] { -1, -1, -1, -1 };
    public string artist, bPM, level, info, unlockCondition;
    public string duration;
    public int optimism;
    public TMP_Text Grade;
    public Button conditionButton;
    public GameObject ConditionPanel, ConditionBuyButton;
    public Condition trackCondition = new Condition();
    public Condition chartCondition = new Condition();
    public Condition packCondition = new Condition();
    public TMP_Text ConditionText;
    public Image coverImage;
    public Image mainBar;
    [HideInInspector] public Sprite illustration;
    public string illustrator, charter;
    
    // Start is called before the first frame update
    void Start()
    {
        RefreshRatingClass(songSelect.currentDifficultyIndex);
    }
    
    public void ConditionClick()
    {
        bool displayBuyTrack = trackCondition.type == ConditionType.Currency;
        bool displayBuyPack = packCondition.type == ConditionType.Currency;
        bool displayBuyChart = chartCondition.type == ConditionType.Currency;
        if (!packUnlocked)
        {
            ConditionBuyButton.SetActive(displayBuyPack);
        }
        else if (!trackUnlocked)
        {
            ConditionBuyButton.SetActive(displayBuyTrack);
        }
        else if (!chartUnlocked)
        {
            ConditionBuyButton.SetActive(displayBuyChart);
        }
        
        ConditionPanel.SetActive(true);
        ConditionText.text = unlockCondition;
    }

    public void Select()
    {
        selected = true;
        RefreshConditionButton();
    }

    public void RefreshConditionButton()
    {
        if (!(trackUnlocked && chartUnlocked && packUnlocked))
        {
            //conditionButton.GetComponent<Button>().enabled = true;
            conditionButton.gameObject.SetActive(true);
            //Debug.Log("111");
        }
        else
        {
            //conditionButton.GetComponent<Button>().enabled = false;
            conditionButton.gameObject.SetActive(false);
        }
    }

    public void Unselect()
    {
        selected = false;
        //conditionButton.GetComponent<Button>().enabled = false;
        conditionButton.gameObject.SetActive(false);
    }

    public void RefreshRatingClass(int ratingClass)
    {
        //RefreshConditionButton();
        if (ratingClass == 0)
        {
            D0.gameObject.SetActive(true);
            D1.gameObject.SetActive(false);
            D2.gameObject.SetActive(false);
            D3.gameObject.SetActive(false);
        }
        else if (ratingClass == 1)
        {
            D0.gameObject.SetActive(false);
            D1.gameObject.SetActive(true);
            D2.gameObject.SetActive(false);
            D3.gameObject.SetActive(false);
        }
        else if (ratingClass == 2)
        {
            D0.gameObject.SetActive(false);
            D1.gameObject.SetActive(false);
            D2.gameObject.SetActive(true);
            D3.gameObject.SetActive(false);
        }
        else if (ratingClass == 3)
        {
            D0.gameObject.SetActive(false);
            D1.gameObject.SetActive(false);
            D2.gameObject.SetActive(false);
            D3.gameObject.SetActive(true);
        }
    }

    public void UpdateInfo()
    {
        //RefreshConditionButton();
        if (score >= 1000000)
        {
            Grade.text = "♾";
        }
        else if (score >= 990000)
        {
            Grade.text = "IIS";
        }
        else if (score >= 980000)
        {
            Grade.text = "IS";
        }
        else if (score >= 970000)
        {
            Grade.text = "S";
        }
        else if (score >= 950000)
        {
            Grade.text = "A";
        }
        else if (score >= 920000)
        {
            Grade.text = "B";
        }
        else if (score >= 880000)
        {
            Grade.text = "C";
        }
        else if (score >= 800000)
        {
            Grade.text = "F";
        }
        else
        {
            Grade.text = "-";
        }
        Name.text = LevelName;
        Rating.text = LevelRating;
        Score.text = score.ToString("D7");
        lockcover.SetActive(!(trackUnlocked && chartUnlocked && packUnlocked));
        if (!(trackUnlocked && chartUnlocked && packUnlocked))
        {
            unlockConditionTextHolder.SetActive(true);
            unlockConditionText.text = unlockCondition;
        }
        else
        {
            unlockConditionTextHolder.gameObject.SetActive(false);
            unlockConditionText.text = unlockCondition;
        }


    }

    public void Click()
    {
        songSelect.SelectSong(idx);
    }

}
