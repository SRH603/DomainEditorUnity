using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BetaController : MonoBehaviour
{
    public Toggle toggle;
    public GameObject BetaGroup;
    public  OnPlaying PlayingControl;
    public TMP_Dropdown qualityDropdown;
    public int fps;

    // Start is called before the first frame update
    void Start()
    {
        qualityDropdown.onValueChanged.AddListener(delegate {
            ChangeQuality(qualityDropdown.value);
        });

        // 初始化 Dropdown 选项
        qualityDropdown.ClearOptions();
        foreach (string name in QualitySettings.names)
        {
            qualityDropdown.options.Add(new TMP_Dropdown.OptionData(name));
        }

        // 设置当前选中的画质级别
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = fps; //限制帧率测试
        if (toggle.isOn)
        {
            BetaGroup.SetActive(true);
        }
        else
        {
            BetaGroup.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayingControl.Replay();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayingControl.BetaPause();
            Debug.Log("Paused");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayingControl.BetaClick();
            Debug.Log("Playing");
        }

    }
    public void ChangeQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);
    }

}
